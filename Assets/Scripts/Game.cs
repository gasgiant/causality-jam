using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
	public EncounterConfig encounterConfig;

	[SerializeField] int diceCount = 12;
	[SerializeField] PlayerView playerView;
	[SerializeField, NonReorderable] ItemView[] itemViews;
	[SerializeField, NonReorderable] EnemyView[] enemyViews;

	public static Game Instance;
	public enum GameState { None, PlayerTurn, EnemyTurn }
	

	public int DiceCount => diceCount;
	public DiceSequence DiceSequence { get; private set; }

	public GameState currentGameState = GameState.PlayerTurn;
	public GameState nextGameState = GameState.None;
	private Player player;
	private List<Enemy> enemies = new List<Enemy>();
	private List<Verb> items = new List<Verb>();

	private void Awake()
	{
		Instance = this;
		DiceSequence = new DiceSequence(1, diceCount);

		player = new Player(72, 3);
		StartEncounter(encounterConfig);

		items.Add(Verb.Make(VerbType.Sword));
		itemViews[0].gameObject.SetActive(true);
		items.Add(Verb.Make(VerbType.Wait));
		itemViews[1].gameObject.SetActive(true);

		UpdateViews();

		StartCoroutine(Tick());
	}

	private IEnumerator Tick()
	{
		while (true)
		{
			// states
			if (currentGameState == GameState.PlayerTurn)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					UseItem(items[0]);
				}

				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					UseItem(items[1]);
				}


				UpdateViews();
				if (Input.GetKeyDown(KeyCode.Space))
				{
					nextGameState = GameState.EnemyTurn;
				}
			}

			if (currentGameState == GameState.EnemyTurn)
			{
				for (int i = 0; i < enemies.Count; i++)
				{
					yield return new WaitForSeconds(0.5f);
					enemies[i].nextAction.Execute(DiceSequence, -1, i, player, enemies);
					enemies[i].nextAction = null;
					UpdateViews();
					yield return new WaitForSeconds(0.5f);
				}
				nextGameState = GameState.PlayerTurn;
			}

			// transitions
			if (nextGameState == GameState.PlayerTurn)
			{
				nextGameState = GameState.None;
				currentGameState = GameState.PlayerTurn;

				foreach (var enemy in enemies)
				{
					enemy.PickAction();
				}

				RestoreEnergy(ref player.energy);
			}

			if (nextGameState == GameState.EnemyTurn)
			{
				nextGameState = GameState.None;
				currentGameState = GameState.EnemyTurn;
			}

			yield return null;
		}
	}

	private void UseItem(Verb item)
	{
		if (item.EnergyCost() <= player.energy.current)
			item.Execute(DiceSequence, 0, -1, player, enemies);
	}

	private void UpdateViews()
	{
		int previewStartIndex = 0;

		for (int i = 0; i < items.Count; i++)
		{
			itemViews[i].Display(items[i]);
		}


		for (int i = 0; i < enemies.Count; i++)
		{
			var enemy = enemies[i];
			enemy.view.Display(enemy, previewStartIndex);
			if (enemy.nextAction != null)
			{
				previewStartIndex += enemies[i].nextAction.DieCount();
			}
		}
	
		playerView.Display(player);
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
			var enemy = new Enemy(encounterConfig.enemies[i], enemyViews[i]);

			enemies.Add(enemy);
			enemyViews[i].gameObject.SetActive(true);
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

	public static void AddExtraEnergyNextTurn(int amount, ref Energy energy)
	{
		energy.extraNextTurn += amount;
	}

	public static void SpendEnergy(int amount, ref Energy energy)
	{
		energy.current -= amount;
		if (energy.current < 0)
			energy.current = 0;
	}

	public static void RestoreEnergy(ref Energy energy)
	{
		energy.current = energy.max + energy.extraNextTurn;
		energy.extraNextTurn = 0;
	}

	public static int DieSpriteIndex(int die)
	{
		if (die == 1)
			return 0;
		if (die == 2)
			return 1;
		if (die == 3)
			return 2;
		if (die == 4)
			return 4;
		if (die == 5)
			return 5;
		if (die == 6)
			return 6;

		return 3;
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
	public Verb nextAction;

	public Enemy(EnemyConfig config, EnemyView view)
	{
		this.config = config;
		this.view = view;
		health.maxHp = config.maxHp;
		health.hp = config.maxHp;
	}

	public void PickAction()
	{
		nextAction = Verb.Make(config.verbs[Random.Range(0, config.verbs.Length)]); 
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
	public int extraNextTurn;
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

	public Damage(int value)
	{
		this.value = value;
	}
}


