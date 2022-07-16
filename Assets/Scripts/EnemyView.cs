using UnityEngine;
using TMPro;

public class EnemyView : MonoBehaviour
{
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private HealthView healthView;

	public void Display(Enemy enemy)
	{
		gameObject.SetActive(true);
		spriteRenderer.sprite = enemy.config.sprite;
		nameText.text = enemy.config.displayName;
		healthView.Display(enemy.health);
	}
}
