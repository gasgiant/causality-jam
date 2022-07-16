using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy")]
public class EnemyConfig : ScriptableObject
{
	public string displayName;
	public Sprite sprite;
	public int maxHp = 10;
	public VerbType[] verbs;
}
