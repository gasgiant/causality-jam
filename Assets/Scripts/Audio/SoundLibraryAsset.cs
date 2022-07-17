using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Library Asset", menuName = "Audio Library Asset")]
public class SoundLibraryAsset : ScriptableObject
{

	public List<SoundGroup> soundGroups;

	public bool editMode;
}