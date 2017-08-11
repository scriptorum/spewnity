using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

/**
 * TODO Support global volume setting.
 */

namespace Spewnity
{
    public class SoundManager : MonoBehaviour
    {
        public const float SEMITONE = 1.0594630943592952646f;
        public static SoundManager instance = null;

        public int maxPoolSize = 32;
        public int minPoolSize = 1;
        [Tooltip("If supplied and not specified per-sound, the default AudioMixerGroup for pool AudioSources")]
        public AudioMixerGroup defaultAudioMixerGroup;
        public Sound[] sounds;
        [Tooltip("If true, maintains one instance of SoundManager that survives scene changes.")]
        public bool dontDestroyOnLoad = false;

        private Dictionary<string, int> nameToSound = new Dictionary<string, int>();
        private List<AudioSource> openPool;
        private List<AudioSource> busyPool;

        [HideInInspector]
        public bool soundsInitialized = false;

#if UNITY_EDITOR
        [ExecuteInEditMode]
        void OnValidate()
        {
            if (sounds.Length == 0) soundsInitialized = false;
            else if (!soundsInitialized)
            {
                Sound sound = sounds[0];
                sound.volume = 1f;
                sound.pitch = 1f;
                sound.usePool = true;
                sound.livePreview = false;
                sound.multi = SoundMulti.Multiple;
                soundsInitialized = true;
            }

#if DEBUG
            foreach(Sound sound in sounds)
            {
                if (sound.usePool && sound.looping) Debug.Log(sound.name + " is looping and pooling. Both simultaneously are not recommended.");
            }
#endif

            bool previewing = false;
            foreach(Sound snd in sounds)
            {
                if (snd.livePreview)
                {
                    AudioSource source = gameObject.GetComponent<AudioSource>();
                    if (source == null)
                        source = gameObject.AddComponent<AudioSource>();
                    snd.livePreview = false;
                    source.playOnAwake = false;
                    source.outputAudioMixerGroup = snd.group == null ? defaultAudioMixerGroup : snd.group;
                    source.clip = snd.clips.Rnd();
                    source.pitch = snd.GetPitch();
                    source.volume = snd.GetVolume();
                    source.panStereo = snd.GetPan();
                    source.loop = false;
                    source.Play();
                    previewing = true;
                }
            }
            if (!previewing)
            {
                AudioSource source = GetComponent<AudioSource>();
                if (source != null)
                    UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(source); };
            }
        }
#endif
        void Awake()
        {
            if (dontDestroyOnLoad || (instance != null && instance.dontDestroyOnLoad))
            {
                if (instance == null)
                {
                    instance = this;
                    DontDestroyOnLoad(gameObject);
                    instance.dontDestroyOnLoad = true;
                }
                else if (instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else instance = this;

            for (int i = 0; i < sounds.Length; i++)
            {
                Sound snd = sounds[i];
                nameToSound.Add(snd.name, i);
                if (snd.usePool == false && snd.source == null)
                {
                    snd.source = gameObject.AddComponent<AudioSource>();
                }
                if (snd.source != null) snd.source.playOnAwake = false;
            }

            // Set up pool
            openPool = new List<AudioSource>();
            busyPool = new List<AudioSource>();

            while (GetSourceCount() < minPoolSize)
            {
                if (EnlargePool() == false)
                {
                    Debug.Log("Error creating beyond " + GetSourceCount() + " audio sources.");
                    break;
                }
            }
        }

        public int GetSourceCount()
        {
            return openPool.Count + busyPool.Count;
        }

        private bool EnlargePool()
        {
            if (GetSourceCount() >= maxPoolSize) return false;

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            openPool.Add(source);

            return true;
        }

        // Returns the Sound object associated with the supplied name.
        // If the sound is already playing, altering this object will have no affect.
        // In that case, manipulating the AudioSource directly. (See GetSource.)
        public Sound GetSound(string soundName)
        {
            if (!nameToSound.ContainsKey(soundName)) throw new UnityException("Cannot find sound named '" + soundName + "'");
            Sound sound = sounds[nameToSound[soundName]];
            return sound;
        }

        // Returns the AudioSource associated with this sound.
        // Use this to manipulate the AudioSource directly, for example to call PlayOneShot().
        // Take care if Sound.usePool is true, as the AudioSource will be thrown back into the pool
        // when the sound is done playing.
        public AudioSource GetSource(string soundName)
        {
            Sound sound = GetSound(soundName);
            return sound.source;
        }

        // Plays the Sound associated with the supplied name.
        public Sound Play(string name, System.Action<Sound> onComplete = null)
        {
            Sound sound = GetSound(name);
            return Play(sound, onComplete);
        }

        // Plays the Sound.
        public Sound Play(Sound sound, System.Action<Sound> onComplete = null)
        {
            return PlayAs(sound, sound.GetPitch(), sound.GetVolume(), sound.GetPan(), sound.looping, onComplete, null);
        }

        // Plays a sound at a position in space
        // See PlayAt(Sound...)
        public void PlayAt(string name, Vector3 position, System.Action<Sound> onComplete = null)
        {
            Sound sound = GetSound(name);
            PlayAt(sound, position, onComplete);
        }

        // Plays a sound at a position in space. This uses AudioSource.PlayClipAtPoint().
        // Note this method doesn't support changes in pitch, panning, or start delays.
        // A better method may be to supply your own AudioSource and attach it to a GameObject,
        // or calculate the panning/volume yourself.
        public void PlayAt(Sound sound, Vector3 position, System.Action<Sound> onComplete = null)
        {
            PlayAs(sound, sound.GetPitch(), sound.GetVolume(), sound.GetPan(), sound.looping, onComplete, position);
        }

        public Sound PlayAs(string name, float pitch = 1.0f, float volume = 1.0f, float pan = 0.0f,
            bool loop = false, System.Action<Sound> onComplete = null, Vector3? position = null)
        {
            Sound sound = GetSound(name);
            PlayAs(sound, pitch, volume, pan, loop, onComplete, position);
            return sound;
        }

        public Sound PlayAs(Sound sound, float pitch = 1.0f, float volume = 1.0f, float pan = 0.0f,
            bool loop = false, System.Action<Sound> onComplete = null, Vector3? position = null)
        {
            if (sound.clips.Length <= 0) throw new UnityException("Cannot play sound '" + sound.name + "': no AudioClips defined");

            // Do not play over sound already playing if Multi = Deny
            if (sound.multi == SoundMulti.Deny && sound.source != null && sound.source.isPlaying)
                return sound;

            // Stop old sound and replace with new sound if sound already playing and Multi = TakeOver
            if (sound.multi == SoundMulti.TakeOver && sound.source != null && sound.source.isPlaying)
                sound.source.Stop();

            else if (sound.usePool && position == null)
            {
                if (openPool.Count == 0)
                {
                    if (EnlargePool() == false)
                        Debug.Log("Cannot play sound " + sound.name + "; no open audio sources remaining in pool");
                }
                sound.source = openPool[0];
                openPool.RemoveAt(0);
                busyPool.Add(sound.source);
            }

            int clipId = Random.Range(0, sound.clips.Length);
            AudioClip clip = sound.clips[clipId];
            if (clip == null) throw new UnityException("Cannot play sound '" + sound.name + "': clip '" + clipId + "' not defined");

            if (position != null)
            {
                AudioSource.PlayClipAtPoint(sound.clips[clipId], (Vector3) position, volume);
                return null;
            }

            if (sound.source == null) throw new UnityException("Cannot play sound '" + sound.name + "': no AudioSource connected");

            sound.source.clip = sound.clips[clipId];
            sound.source.pitch = pitch;
            sound.source.volume = volume;
            sound.source.panStereo = pan;
            sound.source.loop = loop;
            sound.source.outputAudioMixerGroup = sound.group == null ? defaultAudioMixerGroup : sound.group;

            if (sound.delay > 0)
            {
                sound.source.PlayDelayed(sound.delay);
                sound.delay = 0;
            }
            else sound.source.Play();

            if (sound.usePool || onComplete != null) StartCoroutine(OnSoundComplete(sound, onComplete));

            return sound;
        }

        private IEnumerator OnSoundComplete(Sound sound, System.Action<Sound> onComplete)
        {
            // Wait for sound to (theoretically) be over
            AudioSource source = sound.source;
            yield return new WaitForSeconds(source.clip.length);

            // Notify callback, if supplied
            if (onComplete != null) onComplete.Invoke(sound);

            // Move audio source from busy to open pool
            if (sound.usePool)
            {
                busyPool.Remove(source);
                openPool.Add(source);
            }
        }

        public void Stop()
        {
            // Stop all sounds from playing
            foreach(Sound sound in sounds)
            {
                if (sound.source != null) sound.source.Stop();
            }

            // If using pooling or callbacks, coroutines may still be running - prevent them from finishing
            StopAllCoroutines();

            // If using pooling, since we stopped the callbacks, audio sources may be falsely listed as busy
            foreach(AudioSource source in busyPool) openPool.Add(source);
            busyPool.Clear();
        }

        // Stops a specific sound from playing. If pooling was used, only stops the last instance
        // of the sound, and there will be a delay before the AudioSource is released back to the pool.
        // If callbacks were used during play(), this will not prevent them from happening.
        public void Stop(string name)
        {
            GetSource(name).Stop();
        }

        // Stops a specific sound from playing, but fades it out gradually first.
        // See Stop().
        public void FadeOut(string name, float fadeTime, AnimationCurve curve = null)
        {
            AudioSource source = GetSource(name);

            // If fade time exceeds actual time remaining, reduce the fade time
            if (source.loop == false)
            {
                float timeRemaining = source.clip.length - source.time;
                if (fadeTime > timeRemaining) fadeTime = timeRemaining;
            }
            StartCoroutine(source.volume.LerpFloat(0.0f, fadeTime, (f) => source.volume = f, curve, () => source.Stop()));
        }

        // Fades in a specific sound gradually. If the sound is not playing already, it is started.
        public void FadeIn(string name, float fadeTime, float volume = 1.0f, AnimationCurve curve = null)
        {
            // Get audio source, start it if not playing
            AudioSource source = GetSource(name);
            if (!source.isPlaying) Play(name);

            StartCoroutine(source.volume.LerpFloat(volume, fadeTime, (f) => source.volume = f, curve));
        }
    }

    [System.Serializable]
    public class Sound
    {
        [Tooltip("This is the name you pass to play()")]
        public string name;

        [Tooltip("Select in the inspector during editing to play the sound once (without looping); has no effect on runtime")]
        public bool livePreview;

        [Tooltip("If more than one, clip will be chosen at random")]
        public AudioClip[] clips;

        [Tooltip("Base pitch is 1.0")]
        public float pitch;

        [Tooltip("Final pitch will be adjusted randomly +/- this value")]
        public float pitchVariation;

        [Tooltip("Base volume is 1.0")]
        public float volume;

        [Tooltip("Final volume will be adusted randomly +/- this value")]
        public float volumeVariation;

        [Tooltip("Inital sound panning (-1 to 1)")]
        public float pan;

        [Tooltip("Final panning will be adusted randomly +/- this value")]
        public float panVariation;

        [Tooltip("Whether to loop the sound when played")]
        public bool looping;

        [Tooltip("If true, uses internal audio source pooling. If false, generates its own audio source. Ignored if a custom audio source is supplied.")]
        public bool usePool;

        [Tooltip("Controls behavior when same sound is playing: Multiple (allow multiples), TakeOver (new sound replaces current), Deny (new sound is denied)")]
        public SoundMulti multi;

        [Tooltip("If nonzero, delays the next playing of the sound by this number of second. This property is then reset to 0.")]
        [HideInInspector]
        public float delay;

        [Tooltip("If usePool is false and this is supplied, will use this custom Audio Source.")]
        public AudioSource source;
        [Tooltip("If supplied, the Audio Source will use this Audio Mixer Group.")]
        public AudioMixerGroup group;

        // Returns valid pitch/volume/pan within the range of variation
        public float GetPitch()
        {
            return pitch + Random.Range(-pitchVariation, pitchVariation);
        }

        public float GetVolume()
        {
            return volume + Random.Range(-volumeVariation, volumeVariation);
        }

        public float GetPan()
        {
            return pan + Random.Range(-panVariation, panVariation);
        }
    }

    [System.Serializable]
    public enum SoundMulti
    {
        Multiple,
        TakeOver,
        Deny
    }
}