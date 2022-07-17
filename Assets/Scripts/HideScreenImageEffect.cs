using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]

public class HideScreenImageEffect : MonoBehaviour
{
	public AnimationCurve curve;
	public float duration = 0.7f;
	public Material material;

	void Start()
	{
		if (null == material || null == material.shader ||
		   !material.shader.isSupported)
		{
			enabled = false;
			return;
		}
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, material);
	}

	int propID = Shader.PropertyToID("_Value");

	public void SetValue(float v)
	{
		material.SetFloat(propID, v);
	}

	public float Show()
	{
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
			material.SetFloat(propID, Mathf.Lerp(from, to, curve.Evaluate(t)));
			yield return null;
		}

		material.SetFloat(propID, to);
	}
}