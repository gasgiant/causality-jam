using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{

	public List<SoundLibraryAsset> sondLibraryAssets;
	SoundGroup[] soundGroups;

	Dictionary<string, SoundGroup> groupDictionary = new Dictionary<string, SoundGroup>();

	public void ImportSoundGroups()
	{
		foreach (SoundLibraryAsset asset in sondLibraryAssets)
		{
			soundGroups = asset.soundGroups.ToArray();
			foreach (SoundGroup soundGroup in soundGroups)
			{
				groupDictionary.Add(soundGroup.groupID, soundGroup);
			}
		}
	}

	public List<string> GetKeys()
	{
		List<string> list = new List<string>();
		foreach (var item in groupDictionary)
		{
			list.Add(item.Key);
		}
		return list;
	}

	public SoundGroup GetSoundGroupFromName(string name)
	{
		if (groupDictionary.ContainsKey(name))
		{
			return groupDictionary[name];
		}
		return null;
	}
}

[System.Serializable]
public class SoundGroup
{
	public string groupID;
	public float volume;
	public float deadTime;
	public AudioClip[] sounds;

	public bool randomize;
	public float pitchRndValue;
	public float volumeRndValue;

	public bool interruptible;
	public int maxSoursesCount;
	public bool loop;

	public float fadeOutTime;

	public SoundLoopMode loopMode;
	public int maxLoopCount;
	public float fadeStartTime;
	public float fadeDuration;

	public AudioClip GetClip()
	{
		if (sounds.Length < 1) return null;
		return sounds[Random.Range(0, sounds.Length)];
	}

}

public enum SoundLoopMode { Simple, Crossfade }