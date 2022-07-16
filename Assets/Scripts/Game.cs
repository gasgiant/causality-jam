using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	[SerializeField] int diceCount = 12;

	public static Game Instance;

	public int DiceCount => diceCount;
	public DiceSequence DiceSequence { get; private set; }

	private void Awake()
	{
		Instance = this;
		DiceSequence = new DiceSequence(0, diceCount);
	}


	public static void DealDamage(Damage dmg, ref Health health)
	{
		health.block -= dmg.value;
		if (health.block < 0)
		{
			health.hp += health.block;
			health.block = 0;
		}
		if (health.hp < 0)
			health.hp = 0;
	}
}

public class DiceSequence
{
	private readonly System.Random random;
	private readonly int capacity;
	private readonly int[] data;

	private int queueIndex;

	public DiceSequence(int seed, int capacity)
	{
		random = new System.Random(seed);
		this.capacity = capacity;
		data = new int[capacity];

		for (int i = 0; i < capacity; i++)
		{
			GenerateDie();
		}
	}

	public int ConsumeDie()
	{
		int die = data[queueIndex];
		GenerateDie();
		return die;
	}

	public int PeekDie(int displayIndex)
	{
		Debug.Assert(displayIndex < capacity, "Trying to peek die over capacity!");
		int index = queueIndex + displayIndex;
		if (index >= capacity)
			index -= capacity;
		return data[index];
	}

	private void GenerateDie()
	{
		data[queueIndex] = random.Next(1, 7);
		queueIndex += 1;
		if (queueIndex >= capacity)
			queueIndex = 0;
	}
}

public struct Health
{
	public int hp;
	public int block;
}

public struct Damage
{
	public int value;
}
