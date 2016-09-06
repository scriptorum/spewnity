using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace Spewnity
{
	public class SoundManager : MonoBehaviour
	{
		public static SoundManager instance = null;

		public int maxPoolSize = 32;
		public int minPoolSize = 1;
		public AudioMixerGroup output;
		public Sound[] sounds;

		private Dictionary<string, int> nameToSound = new Dictionary<string, int>();
		private List<AudioSource> openPool;
		private List<AudioSource> busyPool;

		[HideInInspector]
		public bool soundsInitialized = false;

		[ExecuteInEditMode]
		void OnValidate()
		{
			if(sounds.Length == 0) soundsInitialized = false;
			else if(!soundsInitialized)
			{
				Sound sound = sounds[0];
				sound.volume = 1f;
				sound.pitch = 1f;
				sound.usePool = true;
				soundsInitialized = true;
			}

			#if DEBUG
			foreach(Sound sound in sounds)
			{
				if(sound.usePool && sound.looping) Debug.Log(sound.name + " is looping and pooling. Both simultaneously are not recommended.");
			}
			#endif

			// Update all pool source in case mixer changed
			List<List<AudioSource>> pools = new List<List<AudioSource>> {
				busyPool,
				openPool
			};
			foreach(List<AudioSource> pool in pools)
			{
				if(pool == null) continue;
				foreach(AudioSource source in pool)
				{
					source.outputAudioMixerGroup = output;
				}
			}			
		}

		void Awake()
		{
			if(instance == null) instance = this;
			else if(instance != this) Destroy(gameObject);

			DontDestroyOnLoad(gameObject);

			for(int i = 0; i < sounds.Length; i++)
			{
				Sound snd = sounds[i];
				nameToSound.Add(snd.name, i);
				if(snd.usePool == false && snd.source == null)
				{
					snd.source = gameObject.AddComponent<AudioSource>();
				}
				if(snd.source != null) snd.source.playOnAwake = false;
			}

			// Set up pool
			openPool = new List<AudioSource>();
			busyPool = new List<AudioSource>();

			while(GetSourceCount() < minPoolSize)
			{
				if(EnlargePool() == false)
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
			if(GetSourceCount() >= maxPoolSize) return false;

			AudioSource source = gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.outputAudioMixerGroup = output;
			openPool.Add(source);

			return true;
		}
			
		// Returns the Sound object associated with the supplied name.
		// If the sound is already playing, altering this object will have no affect.
		// In that case, manipulating the AudioSource directly. (See GetSource.)
		public Sound GetSound(string soundName)
		{
			if(!nameToSound.ContainsKey(soundName)) throw new UnityException("Cannot find sound named '" + soundName + "'");
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
			float pitch = sound.pitch + Random.Range(-sound.pitchVariation, sound.pitchVariation);
			float volume = sound.volume + Random.Range(-sound.volumeVariation, sound.volumeVariation);
			float pan = sound.pan + Random.Range(-sound.panVariation, sound.panVariation);
			return PlayAs(sound, pitch, volume, pan, sound.looping, onComplete);
		}

		public Sound PlayAs(string name, float pitch = 1.0f, float volume = 1.0f, float pan = 0.0f, bool loop = false, System.Action<Sound> onComplete = null)
		{
			Sound sound = GetSound(name);
			PlayAs(sound, pitch, volume, pan, loop, onComplete);
			return sound;
		}

		public Sound PlayAs(Sound sound, float pitch = 1.0f, float volume = 1.0f, float pan = 0.0f, bool loop = false, System.Action<Sound> onComplete = null)
		{
			if(sound.clips.Length <= 0) throw new UnityException("Cannot play sound '" + name + "': no AudioClips defined");

			int clipId = Random.Range(0, sound.clips.Length);
			AudioClip clip = sound.clips[clipId];
			if(clip == null) throw new UnityException("Cannot play sound '" + name + "': clip '" + clipId + "' not defined");

			if(sound.usePool)
			{
				if(openPool.Count == 0)
				{
					if(EnlargePool() == false) throw new UnityException("Cannot play sound " + sound.name + "; no open audio sources remaining in pool");
				}							
				sound.source = openPool[0];
				openPool.RemoveAt(0);
				busyPool.Add(sound.source);
			}

			if(sound.source == null) throw new UnityException("Cannot play sound '" + name + "': no AudioSource connected");

			sound.source.clip = sound.clips[clipId];
			sound.source.pitch = pitch;
			sound.source.volume = volume;
			sound.source.panStereo = pan;
			sound.source.loop = loop;

			if(sound.delay > 0)
			{
				sound.source.PlayDelayed(sound.delay);
				sound.delay = 0;
			}
			else sound.source.Play();
			
			if(sound.usePool || onComplete != null) StartCoroutine(OnSoundComplete(sound, onComplete));

			return sound;
		}

		private IEnumerator OnSoundComplete(Sound sound, System.Action<Sound> onComplete)
		{
			// Wait for sound to (theoretically) be over
			AudioSource source = sound.source;
			yield return new WaitForSeconds(source.clip.length);

			// Notify callback, if supplied
			if(onComplete != null) onComplete.Invoke(sound);

			// Move audio source from busy to open pool
			if(sound.usePool)
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
				if(sound.source != null) sound.source.Stop();
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
			if(source.loop == false)
			{
				float timeRemaining = source.clip.length - source.time;
				if(fadeTime > timeRemaining) fadeTime = timeRemaining;
			}
			StartCoroutine(source.volume.LerpFloat(0.0f, fadeTime, (f) => source.volume = f, curve, () => source.Stop()));
		}

		// Fades in a specific sound gradually. If the sound is not playing already, it is started.
		public void FadeIn(string name, float fadeTime, float volume = 1.0f, AnimationCurve curve = null)
		{
			// Get audio source, start it if not playing
			AudioSource source = GetSource(name);
			if(!source.isPlaying) Play(name);

			StartCoroutine(source.volume.LerpFloat(volume, fadeTime, (f) => source.volume = f, curve));
		}
	}

	[System.Serializable]
	public class Sound
	{
		[Tooltip("This is the name you pass to play()")]
		public string name;

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

		[Tooltip("If nonzero, delays the next playing of the sound by this number of second. This property is then reset to 0.")]
		[HideInInspector]
		public float delay;

		[Tooltip("If usePool is false and this is supplied, will use this custom Audio Source.")]
		public AudioSource source;
	}
}

