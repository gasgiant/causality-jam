using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
	public static Game Instance { get; private set; }
	public static Player Player => Instance.player;
	public static Encounter CurrentEncounter => Instance.currentEncounter;

	[SerializeField] private int debugStartEnc = 0;
	[SerializeField] private int diceCount = 12;
	[SerializeField] private int startHp = 36;
	[SerializeField] private int startEnergy = 3;
	[SerializeField] private EncounterPool[] encounterPools;

	private EncounterConfig GetNextEncounter()
	{
		if (currentEncounterIndex + debugStartEnc < 2)
			return encounterPools[0].GetEncounterConfig();
		if (currentEncounterIndex + debugStartEnc < 5)
			return encounterPools[1].GetEncounterConfig();

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
	private List<Verb> treasures = new List<Verb>();

	private bool IsTreasureRoom() => currentFloorIndex % 4 == 3;

	public static List<Verb> Items => Instance.items;
	private bool gameStarted = false;

	public Verb GetTreasure()
	{
		var treasure = treasures[currentTreasureIndex];
		currentTreasureIndex = (currentTreasureIndex + 1) % treasures.Count;
		return treasure;
	}

	public void NewGame()
	{
		int seed = 1;

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

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		DiceSequence = new DiceSequence(1, diceCount);
		player = new Player(startHp, startEnergy);

		treasures.Add(new Wrath());
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
