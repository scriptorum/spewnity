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
		public float recheckInterval = 0.2f;
		public AudioMixerGroup output;
		public Sound[] sounds;

		private Dictionary<string, int> nameToSound = new Dictionary<string, int>();
		private List<AudioSource> openPool;
		private List<AudioSource> busyPool;
		private WaitForSeconds closeSourceCheckDelay;

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
				soundsInitialized = true;
			}

			foreach(Sound sound in sounds)
			{
				if(sound.usePool && sound.looping) Debug.Log("Not recommended to use the audio source pool for a looping sound");
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
			closeSourceCheckDelay = new WaitForSeconds(recheckInterval);

			while(getSourceCount() < minPoolSize)
			{
				if(enlargePool() == false)
				{
					Debug.Log("Error creating beyond " + getSourceCount() + " audio sources.");
					break;
				}
			}
		}

		public int getSourceCount()
		{
			return openPool.Count + busyPool.Count;
		}


		private bool enlargePool()
		{
			if(getSourceCount() >= maxPoolSize) return false;

			AudioSource source = gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.outputAudioMixerGroup = output;
			openPool.Add(source);

			return true;
		}
			
		// Returns the Sound object associated with the supplied name.
		Sound getSound(string name)
		{
			if(!nameToSound.ContainsKey(name)) throw new UnityException("Cannot play sound '" + name + "': not found");
			Sound snd = sounds[nameToSound[name]];
			return snd;
		}

		// Plays the Sound associated with the supplied name.
		public void play(string name)
		{			
			Sound snd = getSound(name);

			float pitch = snd.pitch + Random.Range(-snd.pitchVariation, snd.pitchVariation);
			float volume = snd.volume + Random.Range(-snd.volumeVariation, snd.volumeVariation);
			float pan = snd.pan + Random.Range(-snd.panVariation, snd.panVariation);

			playAs(snd, pitch, volume, pan, snd.looping);
		}

		public void playAs(string name, float pitch = 1.0f, float volume = 1.0f, float pan = 0.0f, bool loop = false)
		{
			playAs(getSound(name), pitch, volume, pan, loop);
		}

		public void playAs(Sound sound, float pitch = 1.0f, float volume = 1.0f, float pan = 0.0f, bool loop = false)
		{
			if(sound.clips.Length <= 0) throw new UnityException("Cannot play sound '" + name + "': no AudioClips defined");

			int clipId = Random.Range(0, sound.clips.Length);
			AudioClip clip = sound.clips[clipId];
			if(clip == null) throw new UnityException("Cannot play sound '" + name + "': clip '" + clipId + "' not defined");

			if(sound.usePool)
			{
				if(openPool.Count == 0)
				{
					if(enlargePool() == false) throw new UnityException("Cannot play sound " + sound.name + "; no open audio sources remaining in pool");
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
			sound.source.Play();
			
			if(sound.usePool) StartCoroutine(closeSourceAfterPlaying(sound.source));
		}

		private IEnumerator closeSourceAfterPlaying(AudioSource source)
		{
			yield return new WaitForSeconds(source.clip.length);

			while(source.isPlaying) yield return closeSourceCheckDelay;

			busyPool.Remove(source);
			openPool.Add(source);
		}

		public void stop()
		{
			foreach(Sound sound in sounds) sound.source.Stop();
		}

		public void stop(string name)
		{
			Sound sound = getSound(name);
			sound.source.Stop();
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
		// lame, this should be initial volume with default of 1.0f

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

		[Tooltip("If usePool is false and this is supplied, will use this custom Audio Source.")]
		public AudioSource source;
	}
}

