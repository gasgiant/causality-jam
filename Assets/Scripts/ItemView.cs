using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private int index;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private TextMeshProUGUI costText;

	Vector3 scaleVel;

	private bool IsHighlighted() => index == Game.CurrentEncounter.HighlightedItemIndex;

	public void Display(Verb verb)
	{
		nameText.text = verb.Name();
		descriptionText.text = verb.Description(IsHighlighted(), 0, Game.Instance.DiceSequence);
		costText.text = verb.EnergyCost().ToString();
	}
	

	public void OnPointerEnter(PointerEventData eventData)
	{
		Game.CurrentEncounter.TrySetHighlightedItem(index);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Game.CurrentEncounter.TrySetHighlightedItem(-1);
	}

	private void Update()
	{
		if (!Game.CurrentEncounter) return;
		Vector3 targetScale = IsHighlighted() ? Vector3.one * 1.15f : Vector3.one;
		transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleVel, 0.05f);
	}
}
