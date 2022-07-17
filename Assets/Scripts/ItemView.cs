using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private int index;

	[SerializeField] private Color[] incactive;
	[SerializeField] private Color[] active;
	[SerializeField] private Image[] backgrounds;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private TextMeshProUGUI costText;
	[SerializeField] private TextMeshProUGUI usesText;

	Vector3 scaleVel;
	bool pointerOver;

	private bool IsHighlighted() => index == Game.CurrentEncounter.HighlightedItemIndex;

	public void Display(Verb verb)
	{
		nameText.text = verb.Name();
		descriptionText.text = verb.Description(IsHighlighted(), 0, Game.Instance.DiceSequence);
		costText.text = verb.EnergyCost().ToString();
		costText.color = Game.Player.energy.current < verb.EnergyCost() ? Color.red : Color.white;

		if (verb.MaxUses() > 0)
		{
			usesText.gameObject.SetActive(true);
			usesText.text = verb.uses.ToString();
			usesText.color = verb.uses > 0 ? Color.white : Color.red;

		}
		else
		{
			usesText.gameObject.SetActive(false);
		}
	}
	

	public void OnPointerEnter(PointerEventData eventData)
	{
		pointerOver = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		pointerOver = false;
		Game.CurrentEncounter.TrySetHighlightedItem(-1);
	}

	private void Update()
	{
		if (!Game.CurrentEncounter) return;

		if (pointerOver)
		{
			Game.CurrentEncounter.TrySetHighlightedItem(index);
		}

		if (Game.CurrentEncounter.WaitingForItemConfirm && index == Game.CurrentEncounter.HighlightedItemIndex)
		{
			for (int i = 0; i < backgrounds.Length; i++)
			{
				backgrounds[i].color = active[i];
			}
		}
		else
		{
			for (int i = 0; i < backgrounds.Length; i++)
			{
				backgrounds[i].color = incactive[i];
			}
		}

		Vector3 targetScale = IsHighlighted() ? Vector3.one * 1.15f : Vector3.one;
		transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVel, 0.05f);
	}
}
