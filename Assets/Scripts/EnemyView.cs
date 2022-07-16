using UnityEngine;
using TMPro;
using System.Collections;

public class EnemyView : MonoBehaviour
{
	[SerializeField] private SelectionCatcher selectionCatcher;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private HealthView healthView;
	[SerializeField] private EnemyActionView actionView;

	private float idleFreq = 5;
	private float idleAmount = 0.1f;

	public void Display(Enemy enemy, int index, int previewIndex)
	{
		if (enemy == null) return;
		idleFreq = enemy.config.idleFreq;
		idleAmount = enemy.config.idleAmount;

		gameObject.SetActive(true);
		spriteRenderer.sprite = enemy.config.sprite;
		nameText.text = enemy.config.displayName;
		healthView.Display(enemy.health);

		if (enemy.nextAction != null)
			actionView.Display(enemy.nextAction.Description(previewIndex >= 0, previewIndex, Game.Instance.DiceSequence));
		else
			actionView.Display("");

		if (selectionCatcher.IsSelected && Game.CurrentEncounter.IsSelectingTarget)
		{
			Game.CurrentEncounter.SelectedEnemy = index;
			spriteRenderer.color = Color.red;
		}
		else
		{
			if (Game.CurrentEncounter.SelectedEnemy == index)
			{
				Game.CurrentEncounter.SelectedEnemy = -1;
			}
			spriteRenderer.color = Color.white;
		}
	}

	private void Update()
	{
		float add = (Mathf.Sin(Time.time * idleFreq) + 1) * 0.5f * idleAmount;
		spriteRenderer.transform.localScale = new Vector3(2, 2 + add, 2);
	}

	public void Flash()
	{
		StartCoroutine(Flash(0.05f, 0.05f, 0.1f));
	}

	private IEnumerator Flash(float upTime, float plato, float downTime)
	{
		Material mat = spriteRenderer.material;
		int id = Shader.PropertyToID("_Flash");

		float t = 0;
		while (t < 1)
		{
			t += Time.deltaTime / upTime;
			mat.SetFloat(id, t);
			yield return null;
		}
		mat.SetFloat(id, 1);
		
		yield return new WaitForSeconds(plato);

		t = 0;
		while (t < 1)
		{
			t += Time.deltaTime / upTime;
			mat.SetFloat(id, 1 - t);
			yield return null;
		}
		mat.SetFloat(id, 0);
	}
}
