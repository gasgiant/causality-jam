using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
	private System.Random rand;

	public static Game Instance { get; private set; }
	public static Player Player => Instance.player;
	public static Encounter CurrentEncounter => Instance.currentEncounter;

	public GameObject healParticle;
	[SerializeField] private GameObject sequenceView;
	[SerializeField] private int debugStartEnc = 0;
	[SerializeField] private int diceCount = 12;
	[SerializeField] private int startHp = 36;
	[SerializeField] private int startEnergy = 3;
	[SerializeField] private EncounterPool[] encounterPools;
	[SerializeField] private EncounterConfig boss;

	private EncounterConfig GetNextEncounter()
	{
		if (currentEncounterIndex + debugStartEnc < 1)
			return encounterPools[0].GetEncounterConfig();

		if (currentEncounterIndex + debugStartEnc < 4)
			return encounterPools[1].GetEncounterConfig();


		if (currentEncounterIndex + debugStartEnc < 7)
			return encounterPools[2].GetEncounterConfig();


		if (currentEncounterIndex + debugStartEnc < 8)
			return encounterPools[3].GetEncounterConfig();

		if (currentEncounterIndex == 8)
			return boss;

		return encounterPools[encounterPools.Length - 1].GetEncounterConfig();
	}
	
	public int DiceCount => diceCount;
	public DiceSequence DiceSequence { get; private set; }

	private Player player;
	private Encounter currentEncounter;
	private int currentEncounterIndex;
	private float timeToStartEncounter;

	private int currentFloorIndex;

	private int currentTreasureIndex;

	private List<Verb> items = new List<Verb>();
	private Verb[] treasures;

	private int[] restFloors = { 2, 6, 10 };
	private bool IsTreasureRoom()
	{
		for (int i = 0; i < restFloors.Length; i++)
		{
			if (currentFloorIndex == restFloors[i])
				return true;
		}

		return false;
	}
	//private bool IsTreasureRoom() => currentFloorIndex % 2 == 1;

	public static List<Verb> Items => Instance.items;
	private bool gameStarted = false;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		int seed = Random.Range(0, 10000);

		AudioManager.Instance.PlayMenuMusic();
		DiceSequence = new DiceSequence(seed, diceCount);
		player = new Player(startHp, startEnergy);

		rand = new System.Random(seed);

		treasures = new Verb[]
		{
			new Bite(),
			new Armor(),
			new Stash(),
			new Warhammer(),
			new Zweihander(),
			new OakShield(),
			new Bomb(),
			new Cannon()
		};
	}

	public Verb GetTreasure()
	{
		var treasure = treasures[currentTreasureIndex];
		currentTreasureIndex = (currentTreasureIndex + 1) % treasures.Length;
		return treasure;
	}

	public void NewGame()
	{
		rand.Shuffle(treasures);
		sequenceView.SetActive(true);
		AudioManager.Instance.PlayBattleMusic();
		int seed = Random.Range(0, 100000);

		//DiceSequence = new DiceSequence(seed, diceCount);

		for (int i = 0; i < encounterPools.Length; i++)
		{
			encounterPools[i].Setup(seed + i);
		}

		gameStarted = true;
		items.Clear();
		items.Add(new Sword());
		items.Add(new Shield());
		items.Add(new Wait());
		//items.Add(new Cannon());

		if (currentEncounter)
		{
			currentEncounter.StopAllCoroutines();
			currentEncounter = null;
		}
		currentEncounterIndex = 0;
		currentFloorIndex = 0;
		timeToStartEncounter = Time.time + 0.05f;
		player = new Player(startHp, startEnergy);
		SceneManager.LoadScene(1);
	}

	public void NextFloor()
	{
		if (currentEncounter)
		{
			currentEncounter.StopAllCoroutines();
			currentEncounter = null;
		}
		currentFloorIndex += 1;
		ResetPlayer();

		if (IsTreasureRoom())
		{
			SceneManager.LoadScene(2);
		}
		else
		{
			Instance.currentEncounterIndex += 1;
			Instance.timeToStartEncounter = Time.time + 0.05f;
			SceneManager.LoadScene(1);
		}
	}

	private void ResetPlayer()
	{
		var health = player.health;
		health.block = 0;
		player.health = health;

		var energy = player.energy;
		energy.current = energy.max;
		energy.extraNextTurn = 0;
		player.energy = energy;
	}

	

	private void Update()
	{
		if (gameStarted && !IsTreasureRoom() && currentEncounter == null && Time.time > timeToStartEncounter)
		{
			currentEncounter = FindObjectOfType<Encounter>();
			currentEncounter.Begin(GetNextEncounter());
		}
	}
}
