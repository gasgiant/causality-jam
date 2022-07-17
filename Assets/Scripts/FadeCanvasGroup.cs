using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeCanvasGroup : MonoBehaviour
{
	public CanvasGroup canvasGroup;
	public float duration = 0.5f;

	public float Show()
	{
		gameObject.SetActive(true);
		StartCoroutine(LerpValue(0, 1, duration));
		return duration;
	}

	public float Hide()
	{
		StartCoroutine(LerpValue(1, 0, duration));
		return duration;
	}

	IEnumerator LerpValue(float from, float to, float duration)
	{
		float t = 0;

		while (t < 1)
		{
			t += Time.deltaTime / duration;
			canvasGroup.alpha = Mathf.Lerp(from, to, t);
			yield return null;
		}
		canvasGroup.alpha = to;

		gameObject.SetActive(to > 0);
	}
}
