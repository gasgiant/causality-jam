using UnityEngine;
using TMPro;

public class HealthView : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI hpText;
	[SerializeField] private TextMeshProUGUI blockText;

	public void Display(Health health)
	{
		hpText.text = $"{health.hp}/{health.maxHp}";
		blockText.text = $"{health.block}";

		blockText.gameObject.SetActive(health.block > 0);
	}
}
