using UnityEngine;

public class SequenceView : MonoBehaviour
{
	[SerializeField, NonReorderable] private Sprite[] diceSprites;
	[SerializeField] private float dieSize = 1f;
	[SerializeField] private float spacing = 0.2f;
	private SpriteRenderer[] renderers;
	private int diceCount;
	private DiceSequence diceSequence;

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
	}

    void Update()
    {
		for (int i = 0; i < diceCount; i++)
		{
			int die = diceSequence.PeekDie(i);
			renderers[i].sprite = diceSprites[die - 1];
			renderers[i].transform.localPosition = (dieSize + spacing) * i * Vector3.down;
		}
	}
}
