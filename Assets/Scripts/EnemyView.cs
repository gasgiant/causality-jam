using UnityEngine;
using TMPro;

public class EnemyView : MonoBehaviour
{
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private HealthView healthView;
	[SerializeField] private EnemyActionView actionView;

	public void Display(Enemy enemy, int previewIndex)
	{
		if (enemy == null) return;
		gameObject.SetActive(true);
		spriteRenderer.sprite = enemy.config.sprite;
		nameText.text = enemy.config.displayName;
		healthView.Display(enemy.health);

		if (enemy.nextAction != null)
			actionView.Display(enemy.nextAction.Description(previewIndex >= 0, previewIndex, Game.Instance.DiceSequence));
		else
			actionView.Display("");
	}
}
