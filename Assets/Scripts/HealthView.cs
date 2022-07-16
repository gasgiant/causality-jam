using UnityEngine;
using TMPro;

public class HealthView : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI hpText;
	[SerializeField] private TextMeshProUGUI blockText;

	private void OnEnable()
	{
		var h = new Health();
		h.maxHp = 20;
		h.hp = 10;
		h.block = 30;
		DisplayHealth(h);
	}

	public void DisplayHealth(Health health)
	{
		hpText.text = $"{health.hp}/{health.maxHp}";

		if (health.block > 0)
		{
			blockText.gameObject.SetActive(true);
			blockText.text = $"{health.block}";
		}
		else
		{
			blockText.gameObject.SetActive(false);
		}
	}
}
