using UnityEngine;
using TMPro;
using System.Collections;
using CameraShake;

public class PlayerView : MonoBehaviour
{
	[SerializeField] private Animator animator;
	[SerializeField] public SpriteRenderer spriteRenderer;
	[SerializeField] private HealthView healthView;
	[SerializeField] private TextMeshProUGUI energyText;

	private enum PlayerViewState { None, Normal, Dead };

	private PlayerViewState currentState = PlayerViewState.None;
	private PlayerViewState nextState = PlayerViewState.Normal;

	int idleHash = Animator.StringToHash("KnightIdle");
	int deathHash = Animator.StringToHash("KnightDeath");
	int deathIdleHash = Animator.StringToHash("KnightDeathIdle");

	private void Start()
	{
		StartCoroutine(Tick());
	}

	IEnumerator Tick()
	{
		while (true)
		{
			Player player = Game.Player;

			healthView.Display(player.health);
			energyText.text = $"{player.energy.current}/{player.energy.max}";

			if (player.health.IsDead && currentState != PlayerViewState.Dead)
			{
				nextState = PlayerViewState.Dead;
			}

			if (!player.health.IsDead && currentState != PlayerViewState.Normal)
			{
				nextState = PlayerViewState.Normal;
			}

			// states
			if (currentState == PlayerViewState.Normal)
			{
				animator.CrossFade(idleHash, 0);
			}
			if (currentState == PlayerViewState.Dead)
			{
				animator.CrossFade(deathIdleHash, 0);
			}

			// transitions
			if (nextState == PlayerViewState.Dead)
			{
				animator.CrossFade(deathHash, 0);
				currentState = PlayerViewState.Dead;
				nextState = PlayerViewState.None;
				yield return new WaitForSeconds(0.5f);
			}

			yield return null;
		}
	}

	public void TakeHit()
	{
		StartCoroutine(MoveAndFlash(Color.white, 0.05f, 0.05f, 0.05f, Vector3.left * 0.2f, Quaternion.identity));
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
