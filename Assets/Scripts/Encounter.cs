using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour
{
	[SerializeField] PointerArrow pointerArrow;
	[SerializeField] PlayerView playerView;
	[SerializeField, NonReorderable] ItemView[] itemViews;
	[SerializeField, NonReorderable] EnemyView[] enemyViews;
	public enum GameState { None, PlayerTurn, EnemyTurn }

	public GameState currentGameState = GameState.PlayerTurn;
	public GameState nextGameState = GameState.None;
	
	private List<Enemy> enemies = new List<Enemy>();
	private List<Verb> items = new List<Verb>();

	public int SelectedEnemy = -1;
	public int HighlightedItemIndex { get; private set; } = -1;
	public bool blockSetHighlightedItem = false;
	public bool IsSelectingTarget { get; private set; } = false;

	public void TrySetHighlightedItem(int index)
	{
		if (!blockSetHighlightedItem)
			HighlightedItemIndex = index;
	}

	private void OnEnable()
	{
		pointerArrow.gameObject.SetActive(false);

		foreach (var view in itemViews)
		{
			view.gameObject.SetActive(false);
		}

		foreach (var view in enemyViews)
		{
			view.gameObject.SetActive(false);
		}
	}

	public void Begin(EncounterConfig config)
	{
		

		StartEncounter(config);

		items.Clear();
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
				if (Input.GetKeyDown(KeyCode.Mouse0) && HighlightedItemIndex >= 0)
				{
					yield return UseItem(items[HighlightedItemIndex]);
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
					var action = enemies[i].nextAction;
					enemies[i].nextAction = null;
					enemies[i].view.Attack();
					yield return DoVerb(action, -1);
					UpdateViews();
					yield return new WaitForSeconds(1f);
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

				RestoreEnergy(ref Game.Player.energy);
			}

			if (nextGameState == GameState.EnemyTurn)
			{
				nextGameState = GameState.None;
				currentGameState = GameState.EnemyTurn;
			}

			yield return null;
		}
	}

	private IEnumerator UseItem(Verb item)
	{
		if (item.EnergyCost() <= Game.Player.energy.current)
		{
			blockSetHighlightedItem = true;
			if (item.IsTargetable())
			{
				IsSelectingTarget = true;
				pointerArrow.gameObject.SetActive(true);
			}
			while (Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1))
			{
				UpdateViews();
				pointerArrow.Display(itemViews[HighlightedItemIndex].transform.position);
				yield return null;
			}

			pointerArrow.gameObject.SetActive(false);
			blockSetHighlightedItem = false;
			HighlightedItemIndex = -1;
			bool valid = !Input.GetKey(KeyCode.Mouse1);

			if (IsSelectingTarget)
			{
				IsSelectingTarget = false;
				valid &= SelectedEnemy >= 0 && SelectedEnemy < enemies.Count;
			}

			if (valid)
				yield return DoVerb(item, SelectedEnemy);
		}
	}

	private IEnumerator DoVerb(Verb verb, int target)
	{
		verb.Execute(Game.Instance.DiceSequence, target, -1, Game.Player, enemies);

		for (int i = enemies.Count - 1; i >= 0; i--)
		{
			if (enemies[i].health.IsDead)
			{
				enemies[i].view.gameObject.SetActive(false);
				enemies.RemoveAt(i);
			}
		}

		if (Game.Player.health.IsDead)
		{
			UpdateViews();
			while (true)
			{
				yield return null;
			}
		}

		if (enemies.Count == 0)
		{
			UpdateViews();
			yield return new WaitForSeconds(1);
			Game.StartNextEncounter();
		}
	}

	private void UpdateViews()
	{
		for (int i = 0; i < items.Count; i++)
		{
			var view = itemViews[i];
			view.Display(items[i]);
		}

		int previewStartIndex = HighlightedItemIndex >= 0 ? 
			items[HighlightedItemIndex].DiceCount() : 0;

		for (int i = 0; i < enemies.Count; i++)
		{
			var enemy = enemies[i];
			enemy.view.Display(enemy, i, previewStartIndex);
			if (enemy.nextAction != null)
			{
				previewStartIndex += enemies[i].nextAction.DiceCount();
			}
		}
	
		playerView.Display(Game.Player);
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
	public int DiceConsumed { get; private set; }

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
		DiceConsumed += 1;
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

	public bool IsDead => hp <= 0;
}

public struct Damage
{
	public int value;

	public Damage(int value)
	{
		this.value = value;
	}
}


