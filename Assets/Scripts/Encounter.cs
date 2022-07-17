using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Encounter : MonoBehaviour
{
	[SerializeField] ParticleSystem veil;
	[SerializeField] FadeCanvasGroup deathScreen;
	[SerializeField] HideScreenImageEffect hideScreen;
	[SerializeField] Button passTurnButton;
	[SerializeField] PointerArrow pointerArrow;
	[SerializeField] PlayerView playerView;
	[SerializeField] GameObject shieldParticle;
	
	[SerializeField, NonReorderable] ItemView[] itemViews;
	[SerializeField] EnemyView[] enemyViews;
	public enum GameState { None, PlayerTurn, EnemyTurn }

	public GameState currentGameState = GameState.PlayerTurn;
	public GameState nextGameState = GameState.None;
	
	private List<Enemy> enemies = new List<Enemy>();
	

	public PlayerView PlayerView => playerView;
	public int SelectedEnemy = -1;
	public int HighlightedItemIndex { get; private set; } = -1;
	public bool blockSetHighlightedItem = false;
	public bool IsSelectingTarget { get; private set; } = false;
	public bool WaitingForItemConfirm { get; private set; } = false;

	public int EnemyTurnStart { get; private set; }
	public int EnemyTurnEnd { get; private set; }
	bool passTurnPressed;
	bool restartPressed = false;

	public float veilDelayTime;

	public int veilCountdown;
	public void CastVeil()
	{
		veilCountdown = 3;
		var emission = veil.emission;
		emission.rateOverTimeMultiplier = 15;
		veilDelayTime = Time.time + 2;
	}

	public void TrySetHighlightedItem(int index)
	{
		if (!blockSetHighlightedItem)
			HighlightedItemIndex = index;
	}

	public void PassTurn()
	{
		passTurnPressed = true;
	}

	public void Restart()
	{
		restartPressed = true;
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
		hideScreen.SetValue(1);
		StartEncounter(config);

		for (int i = 0; i < Game.Items.Count; i++)
		{
			itemViews[i].gameObject.SetActive(true);
			Game.Items[i].Init();
		}

		UpdateViews();

		StartCoroutine(Tick());
	}

	private IEnumerator Tick()
	{
		yield return new WaitForSeconds(hideScreen.Hide());

		while (true)
		{
			// states
			if (currentGameState == GameState.PlayerTurn)
			{
				if (Input.GetKeyDown(KeyCode.Return))
				{
					float hideTime = hideScreen.Show();
					yield return new WaitForSeconds(hideTime + 0.5f);
					Game.Instance.NextFloor();
				}

				if (Input.GetKeyDown(KeyCode.Mouse0) && HighlightedItemIndex >= 0)
				{
					yield return UseItem(Game.Items[HighlightedItemIndex]);
				}


				UpdateViews();
				if (Input.GetKeyDown(KeyCode.Space) || passTurnPressed)
				{
					passTurnPressed = false;
					passTurnButton.interactable = false;
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
					yield return DoVerb(action, -1, i);
					if (action.GetType() == typeof(Veil))
					{
						yield return new WaitForSeconds(2f);
					}
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
				passTurnButton.interactable = true;
				ClearBlock(ref Game.Player.health);

				foreach (var enemy in enemies)
				{
					enemy.PickAction();
				}
				foreach (var item in Game.Items)
				{
					if (item.InitPerTurn())
						item.Init();
				}

				RestoreEnergy(ref Game.Player.energy);
			}

			if (nextGameState == GameState.EnemyTurn)
			{
				veilCountdown -= 1;
				if (veilCountdown <= 0)
				{
					var emission = veil.emission;
					emission.rateOverTime = 0;
				}

				nextGameState = GameState.None;
				currentGameState = GameState.EnemyTurn;

				for (int i = 0; i < enemies.Count; i++)
				{
					ClearBlock(ref enemies[i].health);
				}
				UpdateViews();
				yield return new WaitForSeconds(0.2f);
			}

			yield return null;
		}
	}

	private IEnumerator UseItem(Verb item)
	{
		if (item.EnergyCost() <= Game.Player.energy.current 
			&& (item.MaxUses() < 0 || item.uses > 0))
		{
			blockSetHighlightedItem = true;
			if (item.IsTargetable())
			{
				IsSelectingTarget = true;
				pointerArrow.gameObject.SetActive(true);
			}

			WaitingForItemConfirm = true;
			while (Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse1))
			{
				UpdateViews();
				pointerArrow.Display(itemViews[HighlightedItemIndex].transform.position);
				yield return null;
			}
			WaitingForItemConfirm = false;
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
			{
				yield return DoVerb(item, SelectedEnemy, -1);
				yield return new WaitForSeconds(0.3f);
			}
		}
	}

	private IEnumerator DoVerb(Verb verb, int target, int selfIndex)
	{
		verb.Execute(Game.Instance.DiceSequence, target, selfIndex, Game.Player, enemies);

		for (int i = enemies.Count - 1; i >= 0; i--)
		{
			if (enemies[i].health.IsDead)
			{
				enemies[i].view.OnDie();
				enemies[i].view.gameObject.SetActive(false);
				enemies.RemoveAt(i);
			}
		}

		if (Game.Player.health.IsDead)
		{
			UpdateViews();
			yield return new WaitForSeconds(1);
			float hideTime = hideScreen.Show();
			yield return new WaitForSeconds(0.5f);
			deathScreen.Show();
			while (!restartPressed)
			{
				yield return null;
			}
			restartPressed = false;
			Game.Instance.NewGame();
		}

		if (enemies.Count == 0)
		{
			UpdateViews();
			yield return new WaitForSeconds(1);
			float hideTime = hideScreen.Show();
			yield return new WaitForSeconds(hideTime + 0.5f);
			Game.Instance.NextFloor();
		}
	}

	private void UpdateViews()
	{
		for (int i = 0; i < Game.Items.Count; i++)
		{
			var view = itemViews[i];
			view.Display(Game.Items[i]);
		}

		int previewStartIndex = HighlightedItemIndex >= 0 ?
			Game.Items[HighlightedItemIndex].DiceCount() : 0;

		EnemyTurnStart = previewStartIndex;

		for (int i = 0; i < enemies.Count; i++)
		{
			var enemy = enemies[i];
			enemy.view.Display(enemy, i, previewStartIndex);
			if (enemy.nextAction != null)
			{
				previewStartIndex += enemies[i].nextAction.DiceCount();
			}
		}

		EnemyTurnEnd = previewStartIndex;
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

	public static void Heal(int amount, ref Health health, Vector3 targetPosition)
	{
		health.hp = Mathf.Min(health.hp + amount, health.maxHp);
		Instantiate(Game.Instance.healParticle, targetPosition, Quaternion.identity);
	}

	public static void AddBlock(int amount, ref Health health, Vector3 targetPosition)
	{
		health.block += amount;
		Instantiate(Game.CurrentEncounter.shieldParticle, targetPosition, Quaternion.identity);
	}

	public static void ClearBlock(ref Health health)
	{
		health.block = 0;
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

	public static int DieSpriteInvertedeIndex(int die)
	{
		if (die <= 0)
			return 3;

		return die + 6;
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

	bool openerPlayer = false;

	public Enemy(EnemyConfig config, EnemyView view)
	{
		this.config = config;
		this.view = view;
		health.maxHp = config.maxHp;
		health.hp = config.maxHp;
	}

	public void PickAction()
	{
		if (config.opener != EnemyAbility.None && !openerPlayer)
		{
			openerPlayer = true;
			nextAction = Verb.Make(config.opener);
		}
		else
		{
			nextAction = Verb.Make(config.verbs[Random.Range(0, config.verbs.Length)]);
		}
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


