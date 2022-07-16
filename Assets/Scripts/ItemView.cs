using UnityEngine;
using TMPro;

public class ItemView : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	[SerializeField] private TextMeshProUGUI costText;

	public void Display(Verb verb)
	{
		nameText.text = verb.Name();
		descriptionText.text = verb.Description(false, -1, Game.Instance.DiceSequence);
		costText.text = verb.EnergyCost().ToString();
	}
}
