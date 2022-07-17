using UnityEngine;
using TMPro;
using System.Collections;
using CameraShake;

public class EnemyView : MonoBehaviour
{
	[SerializeField] private GameObject bloodParticles;
	[SerializeField] private SelectionCatcher selectionCatcher;
	[SerializeField] public SpriteRenderer spriteRenderer;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private HealthView healthView;
	[SerializeField] private EnemyActionView actionView;

	private float idleFreq = 5;
	private float idleAmount = 0.1f;

	public void OnDie()
	{
		Instantiate(bloodParticles,
			spriteRenderer.transform.position + Vector3.up, Quaternion.identity);
	}

	public void Display(Enemy enemy, int index, int previewIndex)
	{
		if (!Game.CurrentEncounter) return;
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

	public void Attack()
	{
		StartCoroutine(MoveAndFlash(Color.red, 0.05f, 0.1f, 0.05f, Vector3.left, Quaternion.identity));
	}

	public void TakeHit()
	{
		StartCoroutine(MoveAndFlash(Color.white, 0.05f, 0.05f, 0.05f, Vector3.right * 0.2f, Quaternion.AngleAxis(-10, Vector3.forward)));
	}

	private IEnumerator MoveAndFlash(Color color, float upTime, float plato, float downTime, Vector3 offset, Quaternion rotation)
	{
		Transform tr = spriteRenderer.transform;
		Vector3 initialPosition = tr.localPosition;
		Quaternion initialRotation = tr.rotation;
		Quaternion finalRotation = tr.rotation * rotation;
		Material mat = spriteRenderer.material;
		mat.SetColor("_Color", color);
		int id = Shader.PropertyToID("_Flash");

		float t = 0;
		while (t < 1)
		{
			t += Time.deltaTime / upTime;
			tr.localPosition = initialPosition + offset * t;
			tr.localRotation = Quaternion.Lerp(finalRotation, finalRotation, t);
			mat.SetFloat(id, t);
			yield return null;
		}
		mat.SetFloat(id, 1);

		CameraShaker.Presets.ShortShake2D();
		yield return new WaitForSeconds(plato);

		t = 0;
		while (t < 1)
		{
			t += Time.deltaTime / downTime;
			tr.localPosition = initialPosition + offset * (1 - t);
			tr.localRotation = Quaternion.Lerp(finalRotation, finalRotation, 1 - t);
			mat.SetFloat(id, 1 - t);
			yield return null;
		}

		tr.localPosition = initialPosition;
		tr.localRotation = initialRotation;
		mat.SetFloat(id, 0);
		mat.SetColor("_Color", Color.white);
	}

	
}
