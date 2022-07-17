using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestSite : MonoBehaviour
{
	public HideScreenImageEffect hideScreen;
	public Button healButton;
	public TextMeshProUGUI healText;
	public ItemView[] itemViews;
	public ItemView treasureView;
	public AnimationCurve curve;

	public GameObject takeButton;
	public GameObject replaceButtons;

	private Verb treasure;
	bool healPressed;
	bool takePressed;
	int replaceIndex = -1;

	public void Take()
	{
		takePressed = true;
	}

	public void Take(int i)
	{
		takePressed = true;
		replaceIndex = i;
	}

	private void OnEnable()
	{
		hideScreen.SetValue(1);
		treasure = Game.Instance.GetTreasure();

		if (Game.Items.Count >= 4)
		{
			replaceButtons.SetActive(true);
			takeButton.SetActive(false);
		}
		else
		{
			replaceButtons.SetActive(false);
			takeButton.SetActive(true);
		}

		int die0 = Game.Instance.DiceSequence.PeekDie(0);
		int die1 = Game.Instance.DiceSequence.PeekDie(1);
		int die2 = Game.Instance.DiceSequence.PeekDie(2);
		healText.text = $"Heal for\n{Verb.DiceText(die0)} {Verb.DiceText(die1)} {Verb.DiceText(die2)}";

		foreach (var item in itemViews)
		{
			item.gameObject.SetActive(false);
		}

		for (int i = 0; i < Game.Items.Count; i++)
		{
			itemViews[i].gameObject.SetActive(true);
			Game.Items[i].Init();
		}

		StartCoroutine(Tick());
	}

	IEnumerator Tick()
	{
		for (int i = 0; i < Game.Items.Count; i++)
		{
			itemViews[i].Display(Game.Items[i]);
		}
		treasureView.Display(treasure);
		yield return new WaitForSeconds(hideScreen.Hide());
		while (true)
		{
			for (int i = 0; i < Game.Items.Count; i++)
			{
				itemViews[i].Display(Game.Items[i]);
			}
			treasureView.Display(treasure);

			if (healPressed)
			{
				healPressed = false;
				takeButton.SetActive(false);
				replaceButtons.SetActive(false);
				healButton.interactable = false;
				int die0 = Game.Instance.DiceSequence.ConsumeDie();
				int die1 = Game.Instance.DiceSequence.ConsumeDie();
				int die2 = Game.Instance.DiceSequence.ConsumeDie();
				Encounter.Heal(die0 + die1 + die2, ref Game.Player.health);
				yield return new WaitForSeconds(0.5f);
				yield return Continue();
			}

			if (takePressed)
			{
				takePressed = false;
				takeButton.SetActive(false);
				replaceButtons.SetActive(false);
				healButton.interactable = false;

				Vector3 targetPos;
				if (replaceIndex >= 0)
					targetPos = itemViews[replaceIndex].transform.position;
				else
					targetPos = itemViews[Game.Items.Count].transform.position;

				Vector3 initialTreasurePos = treasureView.transform.position;

				float t = 0;
				while (t < 1)
				{
					t += Time.deltaTime / 0.5f;
					treasureView.transform.position = 
						Vector3.Lerp(initialTreasurePos, targetPos, curve.Evaluate(t));
					if (replaceIndex >= 0)
					{
						itemViews[replaceIndex].transform.position =
							Vector3.Lerp(targetPos, targetPos + Vector3.down * 5, curve.Evaluate(t));
					}
					yield return null;
				}
				treasureView.transform.position = targetPos;
				if (replaceIndex >= 0)
					Game.Items[replaceIndex] = treasure;
				else
					Game.Items.Add(treasure);
				yield return Continue();
			}

			yield return null;
		}
	}

	IEnumerator Continue()
	{
		yield return new WaitForSeconds(0.5f);
		float hideTime = hideScreen.Show();
		yield return new WaitForSeconds(hideTime + 0.1f);
		Game.Instance.NextFloor();
	}

	public void Heal()
	{
		healPressed = true;
	}
}
