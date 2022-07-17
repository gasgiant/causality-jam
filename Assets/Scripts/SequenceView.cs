using System.Collections;
using UnityEngine;

public class SequenceView : MonoBehaviour
{
	[SerializeField, NonReorderable] private Sprite[] diceSprites;
	[SerializeField] private float dieSize = 1f;
	[SerializeField] private float spacing = 0.2f;
	[SerializeField] private AnimationCurve moveCurve;
	[SerializeField] private Color enemyHighlight;
	[SerializeField] private Color playerHighlight;
	private SpriteRenderer[] renderers;
	private int diceCount;
	private DiceSequence diceSequence;
	private int previousDiceConsumed;
	private Vector3 offset;

	void Start()
    {
		diceCount = Game.Instance.DiceCount;
		diceSequence = Game.Instance.DiceSequence;
		renderers = new SpriteRenderer[diceCount];
		for (int i = 0; i < diceCount; i++)
		{
			var go = new GameObject("Dice " + i);
			go.transform.SetParent(transform);
			renderers[i] = go.AddComponent<SpriteRenderer>();
		}

		previousDiceConsumed = diceSequence.DiceConsumed;

		StartCoroutine(Tick());
	}

	IEnumerator Tick()
	{
		while (true)
		{
			if (previousDiceConsumed != diceSequence.DiceConsumed)
			{
				while (Game.CurrentEncounter && Time.time < Game.CurrentEncounter.veilDelayTime)
				{
					yield return null;
				}

				int diff = diceSequence.DiceConsumed - previousDiceConsumed;
				previousDiceConsumed = diceSequence.DiceConsumed;

				float t = 0;
				while (t < 1)
				{
					t += Time.deltaTime / 0.1f;
					for (int i = 0; i < diff; i++)
					{
						renderers[i].transform.localPosition = DiePosition(i) + Vector3.left * 2 * t;
						renderers[i].color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), t);
					}
					yield return null;
				}

				
				Vector3 initialOffset = DiePosition(diff) - DiePosition(0);
				offset = initialOffset;
				for (int i = 0; i < diceCount; i++)
				{
					renderers[i].color = Color.white;
				}

				t = 0;
				while (t < 1)
				{
					t += Time.deltaTime / 0.3f;
					offset = Vector3.Lerp(initialOffset, Vector3.zero, moveCurve.Evaluate(t));
					UpdateSpritesAndPositions();
					yield return null;
				}
				offset = Vector3.zero;
			}

			UpdateSpritesAndPositions();

			yield return null;
		}
	}

	private void UpdateSpritesAndPositions()
	{
		int enemyStart = Game.CurrentEncounter != null ? Game.CurrentEncounter.EnemyTurnStart : 0;
		int enemyEnd = Game.CurrentEncounter != null ? Game.CurrentEncounter.EnemyTurnEnd : 0;

		for (int i = 0; i < diceCount; i++)
		{
			int die = diceSequence.PeekDie(i);
			renderers[i].sprite = diceSprites[die - 1];
			renderers[i].transform.localPosition = DiePosition(i);
			renderers[i].color = Color.white;
			if (i < enemyStart)
			{
				renderers[i].color = playerHighlight;
				renderers[i].transform.localScale = Vector3.one * 1.2f;
			}
			else
			{
				renderers[i].transform.localScale = Vector3.one;
			}

			if (i >= enemyStart && i < enemyEnd)
			{
				renderers[i].color = enemyHighlight;
			}
		}
	}

	private Vector3 DiePosition(int index) => (dieSize + spacing) * index * Vector3.down + offset;
}
