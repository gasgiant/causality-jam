using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public EncounterConfig encounterConfig;

	[SerializeField] int diceCount = 12;
	[SerializeField] PlayerView playerView;
	[SerializeField, NonReorderable] EnemyView[] enemyViews;

	public static Game Instance;

	public int DiceCount => diceCount;
	public DiceSequence DiceSequence { get; private set; }

	private Player player;
	private List<Enemy> enemies = new List<Enemy>();

	private void Awake()
	{
		Instance = this;
		DiceSequence = new DiceSequence(0, diceCount);

		player = new Player(72, 3);
		playerView.Display(player);
		StartEncounter(encounterConfig);
	}

	private void StartEncounter(EncounterConfig encounterConfig)
	{
		enemies.Clear();
		foreach (var view in enemyViews)
		{
			view.gameObject.SetActive(false);
		}

		for (int i = 0; i < encounterConfig.enemies.Length; i++)
		{
			enemies.Add(new Enemy(encounterConfig.enemies[i], enemyViews[i]));
		}
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

public class Player
{
	public Health health;
	public Energy energy;
	public Player(int maxHp, int maxEnergy)
	{
		health.maxHp = maxHp;
		health.hp = maxHp;

		energy.max = maxEnergy;
		energy.current = maxEnergy;
	}
}

public class Enemy
{
	public Health health;
	public EnemyConfig config;
	public EnemyView view;

	public Enemy(EnemyConfig config, EnemyView view)
	{
		this.config = config;
		this.view = view;
		health.maxHp = config.maxHp;
		health.hp = config.maxHp;
		view.Display(this);
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

public struct Energy
{
	public int max;
	public int current;
}

public struct Health
{
	public int maxHp;
	public int hp;
	public int block;
}

public struct Damage
{
	public int value;
}

public enum DieSpriteIndex
{
	One = 0,
	Two = 1,
	Three = 2,
	Empty = 3,
	Four = 4,
	Five = 5,
	Six = 6
}
