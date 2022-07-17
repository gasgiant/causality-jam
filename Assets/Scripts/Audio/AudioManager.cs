using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioSourseType { Music, SFX }

public class SoundRequest
{
	public static int runningID = 0;
	public int ID;

	public SoundRequest()
	{
		runningID = (runningID + 1) % 10000;
		ID = runningID;
	}
}

public class SourseKeeper
{
	public AudioSourseType type;
	public AudioSource sourse;
	float voulumeMultiplier;
	float baseVolume;
	public bool buisy;
	public GameObject gameObject;

	public float VoulumeMultiplier { get { return voulumeMultiplier; } }

	public SourseKeeper(AudioSourseType _type, AudioSource _sourse, float _baseVolume, GameObject _gameObject)
	{
		type = _type;
		sourse = _sourse;
		baseVolume = _baseVolume;
		gameObject = _gameObject;
		voulumeMultiplier = 1;
	}

	public void SetMultiplier(float multiplier)
	{
		voulumeMultiplier = multiplier;
		sourse.volume = baseVolume * voulumeMultiplier;
	}

	public void SetBaseVolume(float volume)
	{
		baseVolume = volume;
		sourse.volume = baseVolume * voulumeMultiplier;
	}
}

public class SoundPlayer
{
	public int ID;
	public string groupID;
	public SourseKeeper sourseKeeper;
	public int sourseIndex;
	public SourseKeeper secondSourseKeeper;
	public int secondSourseIndex;
	public float duration;

	public bool interruptible;
	public Coroutine playCoroutine;

	public float fadeOutTime;
	public bool isStopping;


	public bool crossFadeLoop;


	public SoundPlayer(int _ID, string _groupID, SourseKeeper _sourse, int _index)
	{
		ID = _ID;
		groupID = _groupID;
		sourseKeeper = _sourse;
		sourseIndex = _index;
		crossFadeLoop = false;
	}

	public SoundPlayer(int _ID, string _groupID, SourseKeeper _sourse, int _index, SourseKeeper _secondSourse, int _secondIndex)
	{
		ID = _ID;
		groupID = _groupID;
		sourseKeeper = _sourse;
		sourseIndex = _index;
		secondSourseKeeper = _secondSourse;
		secondSourseIndex = _secondIndex;
		crossFadeLoop = true;
	}
}

public class AudioManager : MonoBehaviour
{

	public static AudioManager Instance;

	public int SoundCount;

	private float masterVolumePercent;
	public float MasterVolumePercent { get { return masterVolumePercent; } }
	private float sfxVolumePercent;
	public float SfxVolumePercent { get { return sfxVolumePercent; } }
	private float musicVolumePercent;
	public float MusicVolumePercent { get { return musicVolumePercent; } }

	SoundLibrary library;

	public AudioClip menuMusic;
	public AudioClip[] battleMusics;


	AudioSource sfxSource;
	SourseKeeper[] musicKeepers;
	public int activeMusicSourceIndex;

	SourseKeeper[] sourseKeepers;
	const int cyclingSoursesNumber = 64;

	List<SourseKeeper> allSourseKeepers;

	Dictionary<int, SoundPlayer> soundPlayersDictionary;

	Dictionary<string, int> soundPlayersOfGroupCount;
	Dictionary<string, float> nextTimeAllowed;

	void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		library = FindObjectOfType<SoundLibrary>();
		if (library == null) Debug.LogError("Sound library not found!");
		library.ImportSoundGroups();

		masterVolumePercent = 1;// PlayerPrefs.GetFloat("masterVolumePercent", 1f);
		musicVolumePercent = PlayerPrefs.GetFloat("musicVolumePercent", 1f);
		sfxVolumePercent = PlayerPrefs.GetFloat("sfxVolumePercent", 1f);

		GameObject newAudioSourceGO;
		AudioSource newAudioSource;
		SourseKeeper newSourseKeeper;

		allSourseKeepers = new List<SourseKeeper>();

		int musicSoursesNumber = 2;
		musicKeepers = new SourseKeeper[musicSoursesNumber];
		for (int i = 0; i < musicSoursesNumber; i++)
		{
			newAudioSourceGO = new GameObject("Music source " + (i + 1));
			newAudioSource = newAudioSourceGO.AddComponent<AudioSource>();
			newAudioSource.loop = true;
			newAudioSource.playOnAwake = false;
			newSourseKeeper = new SourseKeeper(AudioSourseType.Music, newAudioSource, masterVolumePercent * musicVolumePercent, newAudioSourceGO);
			musicKeepers[i] = newSourseKeeper;
			allSourseKeepers.Add(newSourseKeeper);
			newAudioSourceGO.transform.parent = transform;
		}

		sourseKeepers = new SourseKeeper[cyclingSoursesNumber];
		for (int i = 0; i < cyclingSoursesNumber; i++)
		{
			newAudioSourceGO = new GameObject("Sound player source " + (i + 1));
			newAudioSource = newAudioSourceGO.AddComponent<AudioSource>();
			newAudioSource.playOnAwake = false;
			newSourseKeeper = new SourseKeeper(AudioSourseType.SFX, newAudioSource, masterVolumePercent * sfxVolumePercent, newAudioSourceGO);
			allSourseKeepers.Add(newSourseKeeper);
			sourseKeepers[i] = newSourseKeeper;
			newAudioSourceGO.transform.parent = transform;
			newAudioSourceGO.SetActive(false);
		}

		newAudioSourceGO = new GameObject("sfx source");
		sfxSource = newAudioSourceGO.AddComponent<AudioSource>();
		sfxSource.playOnAwake = false;
		newSourseKeeper = new SourseKeeper(AudioSourseType.SFX, sfxSource, masterVolumePercent * sfxVolumePercent, newAudioSourceGO);
		allSourseKeepers.Add(newSourseKeeper);
		newAudioSourceGO.transform.parent = transform;

		soundPlayersDictionary = new Dictionary<int, SoundPlayer>();

		soundPlayersOfGroupCount = new Dictionary<string, int>();
		nextTimeAllowed = new Dictionary<string, float>();

		foreach (string key in library.GetKeys())
		{
			soundPlayersOfGroupCount.Add(key, 0);
			nextTimeAllowed.Add(key, 0);
		}
	}

	public void PlayMenuMusic()
	{
		PlayMusic(menuMusic);
	}

	public void PlayBattleMusic()
	{
		PlayMusic(battleMusics[Random.Range(0, battleMusics.Length)]);
	}

	void Update()
	{
		SoundCount = 0;

		foreach (var item in soundPlayersDictionary)
		{
			SoundCount += 1;
		}
	}

	public void ChangeMasterVolume(float master)
	{
		ChangeVolume(master, musicVolumePercent, sfxVolumePercent);
	}

	public void ChangeMusicVolume(float music)
	{
		ChangeVolume(masterVolumePercent, music, sfxVolumePercent);
	}

	public void ChangeSFXVolume(float sfx)
	{
		ChangeVolume(masterVolumePercent, musicVolumePercent, sfx);
	}

	public void ChangeVolume(float master, float music, float sfx)
	{
		//masterVolumePercent = master;
		musicVolumePercent = music;
		sfxVolumePercent = sfx;

		//PlayerPrefs.SetFloat("masterVolumePercent", masterVolumePercent);
		PlayerPrefs.SetFloat("musicVolumePercent", musicVolumePercent);
		PlayerPrefs.SetFloat("sfxVolumePercent", sfx);

		foreach (SourseKeeper keeper in allSourseKeepers)
		{
			switch (keeper.type)
			{
				case AudioSourseType.Music:
					keeper.SetBaseVolume(master * music);
					break;
				case AudioSourseType.SFX:
					keeper.SetBaseVolume(master * sfx);
					break;
			}
		}
	}

	public bool IsSoundGroupExist(string tag)
	{
		return library.GetSoundGroupFromName(tag) == null;
	}

	public bool IsSoundInterruptible(string tag)
	{
		SoundGroup group = library.GetSoundGroupFromName(tag);
		if (group != null)
			return group.interruptible;
		else return false;
	}

	int GetVacantAudioSourseIndex()
	{
		int i;
		for (i = 0; i < cyclingSoursesNumber; i++)
		{
			if (!sourseKeepers[i].buisy)
			{
				sourseKeepers[i].buisy = true;
				sourseKeepers[i].gameObject.SetActive(true);
				return i;
			}
		}
		Debug.LogError("All sound sourses are buisy!");
		return -1;
	}

	void DeactivateSourse(int index)
	{
		sourseKeepers[index].buisy = false;
		sourseKeepers[index].sourse.time = 0;
		sourseKeepers[index].gameObject.SetActive(false);
	}

	public void PlaySound(string soundName)
	{
		SoundGroup group = library.GetSoundGroupFromName(soundName);
		if (group == null)
		{
			Debug.LogError("Soundgroup '" + soundName + "' not found!");
			return;
		}

		int index;

		if (group.deadTime > 0 && nextTimeAllowed[group.groupID] > Time.unscaledTime)
			return;

		if (group.randomize)
		{
			index = GetVacantAudioSourseIndex();
			if (index >= 0)
			{
				if (group.deadTime > 0)
					nextTimeAllowed[group.groupID] = Time.unscaledTime + group.deadTime;

				SoundPlayer soundPlayer = new SoundPlayer((new SoundRequest()).ID, group.groupID, sourseKeepers[index], index);
				soundPlayer.sourseKeeper.sourse.loop = false;
				soundPlayer.sourseKeeper.SetMultiplier(1);
				soundPlayer.sourseKeeper.sourse.pitch = 1 + (Random.value - 0.5f) * group.pitchRndValue;
				AudioClip clip = group.GetClip();
				soundPlayer.interruptible = false;
				soundPlayer.duration = clip.length / soundPlayer.sourseKeeper.sourse.pitch;
				soundPlayersDictionary.Add(soundPlayer.ID, soundPlayer);

				soundPlayer.playCoroutine = StartCoroutine(AwaitStopRoutine(soundPlayer));
				soundPlayer.sourseKeeper.sourse.PlayOneShot(clip, group.volume * (1 - Random.value * group.volumeRndValue));
			}
		}
		else
		{
			if (group.deadTime > 0)
				nextTimeAllowed[group.groupID] = Time.unscaledTime + group.deadTime;
			sfxSource.PlayOneShot(group.GetClip(), masterVolumePercent * sfxVolumePercent * group.volume);
		}
	}

	public SoundRequest StartSound(string soundName)
	{
		SoundGroup group = library.GetSoundGroupFromName(soundName);
		if (group == null)
		{
			Debug.LogError("Soundgoup '" + soundName + "' not found!");
			return null;
		}

		if (group.deadTime > 0 && nextTimeAllowed[group.groupID] > Time.unscaledTime)
			return null;

		if (soundPlayersOfGroupCount[group.groupID] >= group.maxSoursesCount)
			return null;

		int index = GetVacantAudioSourseIndex();
		if (index < 0)
		{
			return null;
		}

		SoundRequest request = new SoundRequest();
		SoundPlayer soundPlayer;

		if (group.loop && group.loopMode == SoundLoopMode.Crossfade)
		{
			int index2 = GetVacantAudioSourseIndex();
			if (index2 < 0)
			{
				DeactivateSourse(index);
				return null;
			}

			if (group.deadTime > 0)
				nextTimeAllowed[group.groupID] = Time.unscaledTime + group.deadTime;

			soundPlayersOfGroupCount[group.groupID] += 1;
			soundPlayer = new SoundPlayer(request.ID, group.groupID, sourseKeepers[index], index, sourseKeepers[index2], index2);
			soundPlayer.interruptible = true;
			soundPlayer.sourseKeeper.sourse.clip = group.GetClip();

			soundPlayer.fadeOutTime = group.fadeOutTime;
			soundPlayer.sourseKeeper.sourse.loop = false;
			soundPlayer.secondSourseKeeper.sourse.loop = false;

			if (group.randomize)
			{
				soundPlayer.sourseKeeper.sourse.pitch = 1 + (Random.value - 0.5f) * group.pitchRndValue;
				soundPlayer.sourseKeeper.SetMultiplier(group.volume * (1 - Random.value * group.volumeRndValue));
			}
			else
			{
				soundPlayer.sourseKeeper.sourse.pitch = 1;
				soundPlayer.sourseKeeper.SetMultiplier(group.volume);
			}

			soundPlayer.secondSourseKeeper.sourse.clip = soundPlayer.sourseKeeper.sourse.clip;
			soundPlayer.secondSourseKeeper.sourse.pitch = soundPlayer.sourseKeeper.sourse.pitch;
			soundPlayer.secondSourseKeeper.SetMultiplier(soundPlayer.sourseKeeper.VoulumeMultiplier);

			soundPlayer.duration = soundPlayer.sourseKeeper.sourse.clip.length / soundPlayer.sourseKeeper.sourse.pitch * group.maxLoopCount;

			soundPlayer.crossFadeLoop = true;
			soundPlayer.playCoroutine = StartCoroutine(LoopCrossFade(soundPlayer, group.fadeStartTime, group.fadeDuration));

			soundPlayersDictionary.Add(request.ID, soundPlayer);
		}
		else
		{
			if (group.deadTime > 0)
				nextTimeAllowed[group.groupID] = Time.unscaledTime + group.deadTime;

			soundPlayersOfGroupCount[group.groupID] += 1;
			soundPlayer = new SoundPlayer(request.ID, group.groupID, sourseKeepers[index], index);
			soundPlayer.interruptible = group.interruptible;
			soundPlayer.sourseKeeper.sourse.clip = group.GetClip();

			soundPlayer.fadeOutTime = group.fadeOutTime;
			soundPlayer.sourseKeeper.sourse.loop = group.loop;

			if (group.randomize)
			{
				soundPlayer.sourseKeeper.sourse.pitch = 1 + (Random.value - 0.5f) * group.pitchRndValue;
				soundPlayer.sourseKeeper.SetMultiplier(group.volume * (1 - Random.value * group.volumeRndValue));
			}
			else
			{
				soundPlayer.sourseKeeper.sourse.pitch = 1;
				soundPlayer.sourseKeeper.SetMultiplier(group.volume);
			}

			if (group.loop)
			{
				soundPlayer.duration = soundPlayer.sourseKeeper.sourse.clip.length / soundPlayer.sourseKeeper.sourse.pitch * group.maxLoopCount;
			}
			else
			{
				soundPlayer.duration = soundPlayer.sourseKeeper.sourse.clip.length / soundPlayer.sourseKeeper.sourse.pitch;
			}

			soundPlayersDictionary.Add(request.ID, soundPlayer);

			soundPlayer.playCoroutine = StartCoroutine(AwaitStopRoutine(soundPlayer));
			soundPlayer.sourseKeeper.sourse.Play();
		}
		return request;
	}

	IEnumerator AwaitStopRoutine(SoundPlayer soundPlayer)
	{
		yield return new WaitForSecondsRealtime(soundPlayer.duration);
		StopSound(soundPlayer);
	}

	IEnumerator LoopCrossFade(SoundPlayer soundPlayer, float startTime, float crossFadeDuration)
	{
		soundPlayer.sourseKeeper.sourse.Play();
		SourseKeeper[] twoKeepers = new SourseKeeper[2];
		twoKeepers[0] = soundPlayer.sourseKeeper;
		twoKeepers[1] = soundPlayer.secondSourseKeeper;

		float percent = 0;
		float length = soundPlayer.sourseKeeper.sourse.clip.length / soundPlayer.sourseKeeper.sourse.pitch;
		float timeWhenFree = Time.unscaledTime + soundPlayer.duration;
		int activeInd = 0;
		float startVolumeMult = soundPlayer.sourseKeeper.VoulumeMultiplier;

		yield return new WaitForSecondsRealtime(Mathf.Clamp(length - crossFadeDuration, 0, 10000));

		float waitTime = Mathf.Clamp(length - 2 * crossFadeDuration - startTime, 0, 10000);
		while (timeWhenFree > Time.unscaledTime)
		{
			twoKeepers[1 - activeInd].sourse.time = startTime;
			twoKeepers[1 - activeInd].sourse.Play();
			percent = 0;
			while (percent < 1)
			{
				percent += Time.unscaledDeltaTime / crossFadeDuration;
				twoKeepers[activeInd].SetMultiplier(Mathf.Lerp(startVolumeMult, 0, percent));
				twoKeepers[1 - activeInd].SetMultiplier(Mathf.Lerp(0, startVolumeMult, percent));
				yield return null;
			}
			twoKeepers[activeInd].SetMultiplier(0);
			twoKeepers[1 - activeInd].SetMultiplier(startVolumeMult);
			twoKeepers[activeInd].sourse.Stop();
			activeInd = 1 - activeInd;
			yield return new WaitForSecondsRealtime(waitTime);
		}
		StopSound(soundPlayer);
	}

	public void StopSound(SoundRequest request, float delay = 0)
	{
		if (request != null && soundPlayersDictionary.ContainsKey(request.ID))
			StopSound(soundPlayersDictionary[request.ID], delay);
	}

	public void StopSound(SoundPlayer soundPlayer, float delay = 0)
	{
		StopCoroutine(soundPlayer.playCoroutine);
		if (soundPlayer.interruptible)
		{
			if (!soundPlayer.isStopping)
			{
				soundPlayer.isStopping = true;
				StartCoroutine(StopSoundRoutine(soundPlayer, delay));
			}
		}
		else
		{
			StopSoundImmediately(soundPlayer);
		}
	}

	public void StopSoundImmediately(SoundPlayer soundPlayer)
	{
		soundPlayer.sourseKeeper.sourse.Stop();
		DeactivateSourse(soundPlayer.sourseIndex);
		if (soundPlayer.crossFadeLoop)
		{
			soundPlayer.secondSourseKeeper.sourse.Stop();
			DeactivateSourse(soundPlayer.secondSourseIndex);
		}
		soundPlayersDictionary.Remove(soundPlayer.ID);
	}

	IEnumerator StopSoundRoutine(SoundPlayer soundPlayer, float delay)
	{
		yield return new WaitForSeconds(delay);
		float duration = soundPlayer.fadeOutTime;
		float percent = 0;
		float startVolume = soundPlayer.sourseKeeper.VoulumeMultiplier;

		float startVolume2 = 0;
		if (soundPlayer.crossFadeLoop) startVolume2 = soundPlayer.secondSourseKeeper.VoulumeMultiplier;

		if (duration > 0)
		{
			while (percent < 1)
			{
				percent += Time.deltaTime / duration;
				soundPlayer.sourseKeeper.SetMultiplier(Mathf.Lerp(startVolume, 0, percent));
				if (soundPlayer.crossFadeLoop) soundPlayer.secondSourseKeeper.SetMultiplier(Mathf.Lerp(startVolume2, 0, percent));
				yield return null;
			}
		}
		soundPlayersOfGroupCount[soundPlayer.groupID] -= 1;
		StopSoundImmediately(soundPlayer);
	}

	public void PlayMusic(AudioClip clip, float fadeDuration = 1)
	{
		activeMusicSourceIndex = 1 - activeMusicSourceIndex;
		musicKeepers[activeMusicSourceIndex].sourse.clip = clip;
		musicKeepers[activeMusicSourceIndex].sourse.Play();

		StartCoroutine(AnimateMusicCrossfade(fadeDuration));
	}

	IEnumerator AnimateMusicCrossfade(float duration)
	{
		float percent = 0;

		while (percent < 1)
		{
			percent += Time.unscaledDeltaTime * 1 / duration;
			musicKeepers[activeMusicSourceIndex].SetMultiplier(Mathf.Lerp(0, 1, percent));
			musicKeepers[1 - activeMusicSourceIndex].SetMultiplier(Mathf.Lerp(1, 0, percent));
			yield return null;
		}
	}
}