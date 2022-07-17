using UnityEngine;

[CreateAssetMenu(fileName = "Pool", menuName = "Encounter Pool")]
public class EncounterPool : ScriptableObject
{
	public System.Random rng;
	public EncounterConfig[] encounters;

	int index;

	public void Setup(int seed)
	{
		rng = new System.Random(seed);
		rng.Shuffle(encounters);
		index = 0;
	}

	public EncounterConfig GetEncounterConfig()
	{
		var enc = encounters[index];

		index += 1;
		if (index >= encounters.Length)
		{
			index = 0;
			rng.Shuffle(encounters);
		}

		return enc;
	}
}

static class RandomExtensions
{
	public static void Shuffle<T>(this System.Random rng, T[] array)
	{
		int n = array.Length;
		while (n > 1)
		{
			int k = rng.Next(n--);
			T temp = array[n];
			array[n] = array[k];
			array[k] = temp;
		}
	}
}
