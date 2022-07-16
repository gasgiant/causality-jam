using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
	public static Game Instance { get; private set; }
	public static Player Player => Instance.player;
	public static Encounter CurrentEncounter => Instance.currentEncounter;

	[SerializeField] private int diceCount = 12;
	[SerializeField] private EncounterConfig[] encounters;
	
	public int DiceCount => diceCount;
	public DiceSequence DiceSequence { get; private set; }

	private Player player;
	private Encounter currentEncounter;
	private int currentEncounterIndex;
	private float timeToStartEncounter;

	public static void StartNextEncounter()
	{
		Instance.currentEncounter.StopAllCoroutines();
		Instance.currentEncounter = null;
		SceneManager.LoadScene(0);
		Instance.currentEncounterIndex += 1;
		Instance.timeToStartEncounter = Time.time + 0.05f;
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
		player = new Player(36, 3);

		
	}

	private void Update()
	{
		if (currentEncounter == null && Time.time > timeToStartEncounter)
			StartEncounter(currentEncounterIndex);
	}

	private void StartEncounter(int index)
	{
		currentEncounter = FindObjectOfType<Encounter>();
		currentEncounter.Begin(encounters[index % encounters.Length]);
	}
}
