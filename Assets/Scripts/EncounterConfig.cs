using UnityEngine;

[CreateAssetMenu(fileName = "New Encounter", menuName = "Encounter")]
public class EncounterConfig : ScriptableObject
{
	public EnemyConfig[] enemies;
}
