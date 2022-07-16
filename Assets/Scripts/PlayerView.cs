using UnityEngine;
using TMPro;

public class PlayerView : MonoBehaviour
{
	[SerializeField] private HealthView healthView;
	[SerializeField] private TextMeshProUGUI energyText;

	public void Display(Player player)
	{
		healthView.Display(player.health);
		energyText.text = $"{player.energy.current}/{player.energy.max}";
	}
}
