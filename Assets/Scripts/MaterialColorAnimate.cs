using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MaterialColorAnimate : MonoBehaviour
{
	[ColorUsage(true, true)]
	public Color color;
	public Material material;

	private void Update()
	{
		material.SetColor("_Color", color);
	}
}
