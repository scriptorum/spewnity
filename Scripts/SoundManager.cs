using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Spewnity
{
	public class SoundManager : MonoBehaviour
	{
		public static SoundManager instance = null;
		private Dictionary<string, int> nameToSound = new Dictionary<string, int>();
		private AudioSource defaultSource;
		private AudioSource musicSource;
		private AudioSource fxSource;
		public Sound[] sounds;

		void Awake()
		{
			if(instance == null) instance = this;
			else if(instance != this) Destroy(gameObject);

			DontDestroyOnLoad(gameObject);

			for(int i = 0; i < sounds.Length; i++)
			{
				Sound snd = sounds[i];
				nameToSound.Add(snd.name, i);
				if(snd.source == null)
				{
					switch(snd.sourceType)
					{
						case SourceType.Default: 
							if(defaultSource == null)
							{
								defaultSource = gameObject.AddComponent<AudioSource>();
								defaultSource.playOnAwake = false;
							}
							snd.source = defaultSource;
							break;

						case SourceType.Music:
							if(musicSource == null)
							{
								musicSource = gameObject.AddComponent<AudioSource>();
								musicSource.playOnAwake = false;
							}
							snd.source = musicSource;
							break;

						case SourceType.FX:
							if(fxSource == null)
							{
								fxSource = gameObject.AddComponent<AudioSource>();
								fxSource.playOnAwake = false;
							}
							snd.source = fxSource;
							break;

						case SourceType.Custom: 
							throw new UnityException("Custom Audio Source must be defined for sound " + snd.name);

						default: 
							throw new UnityException("Error with sound " + snd.name + "; SourceType " + snd.sourceType + " not implemented");
					}
				}
				else snd.source.playOnAwake = false;
			}				
		}
			
		// Returns the Sound object by name
		Sound getSound(string name)
		{
			if(!nameToSound.ContainsKey(name))
				throw new UnityException("Cannot play sound '" + name + "': not found");
			Sound snd = sounds[nameToSound[name]];
			return snd;
		}

		AudioSource getSource(string name)
		{
			return getSource(getSound(name));
		}

		// Returns the AudioSource used by the supplied sound
		AudioSource getSource(Sound snd)
		{
			return snd.source;
		}

		public void play(string name)
		{			
			Sound snd = getSound(name);

			float pitch = 1 + snd.pitchOffset + Random.Range(-snd.pitchVariation, snd.pitchVariation);
			float volume = 1 + snd.volumeOffset + Random.Range(-snd.volumeVariation, snd.volumeVariation);
			float pan =  snd.panOffset + Random.Range(-snd.panVariation, snd.panVariation);

			playAs(snd, pitch, volume, pan, snd.looping);
		}

		public void playAs(string name, float pitch = 1.0f, float volume = 1.0f, float pan = 0.0f, bool loop = false)
		{
			playAs(getSound(name), pitch, volume, pan, loop);
		}

		public void playAs(Sound snd, float pitch = 1.0f, float volume = 1.0f, float pan = 0.0f, bool loop = false)
		{
			AudioSource src = getSource(snd);

			if(snd.clips.Length <= 0)
				throw new UnityException("Cannot play sound '" + name + "': no AudioClips defined");
			
			if(src == null) 
				throw new UnityException("Cannot play sound '" + name + "': no AudioSource connected");

			var clipId = Random.Range(0, snd.clips.Length);
			src.clip = snd.clips[clipId];
			if(src.clip == null)
				throw new UnityException("Cannot play sound '" + name + "': clip '" + clipId + "' not defined");

			src.pitch = pitch;
			src.volume = volume;
			src.panStereo = pan;
			src.loop = loop;
			src.Play();
		}

		public void stop()
		{
			foreach(Sound snd in sounds)
				snd.source.Stop();
		}

		public void stop(string name)
		{
			getSource(name).Stop();
		}
	}

	[System.Serializable]
	public class Sound
	{
		[Tooltip("This is the name you pass to play()")]
		public string name;

		[Tooltip("If more than one, clip will be chosen at random")]
		public AudioClip[] clips;

		[Tooltip("Initial pitch is 1 plus this pitch offset")]
		public float pitchOffset;

		[Tooltip("Final pitch will be adjusted randomly +/- this value")]
		public float pitchVariation;

		[Tooltip("Initial volume is 1 plus this volume offset (should be negative)")]
		public float volumeOffset;
		// lame, this should be initial volume with default of 1.0f

		[Tooltip("Final volume will be adusted randomly +/- this value")]
		public float volumeVariation;

		[Tooltip("Inital sound panning (-1 to 1)")]
		public float panOffset;

		[Tooltip("Final panning will be adusted randomly +/- this value")]
		public float panVariation;

		[Tooltip("Whether to loop the sound when played")]
		public bool looping;

		[Tooltip("Default, Music and FX are predefined audio sources. Custom will throw an error if custom source is not defined")]
		public SourceType sourceType;

		[Tooltip("If supplied, this custom Audio Source will be used, regardless of SourceType.")]
		public AudioSource source;
	}

	public enum SourceType
	{
		Default,
		Music,
		FX,
		Pool, // TODO
		Custom
	}
}

