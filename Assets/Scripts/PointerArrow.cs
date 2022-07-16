using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerArrow : MonoBehaviour
{
	[SerializeField] private Camera cam;
	private LineRenderer lineRenderer;

	private const int PointsCount = 25;
	private Vector3[] points = new Vector3[PointsCount];

	private void OnEnable()
	{
		lineRenderer = GetComponent<LineRenderer>();
	}

	public void Display(Vector3 from)
	{
		Vector3 to = cam.ScreenToWorldPoint(Input.mousePosition);

		for (int i = 0; i < PointsCount; i++)
		{
			float t = i / (PointsCount - 1f);
			points[i] = Vector3.Lerp(from, to, t);
			points[i].y += Mathf.Lerp(0, 3, 1 - (2 * t - 1) * (2 * t - 1));
		}

		lineRenderer.positionCount = PointsCount;
		lineRenderer.SetPositions(points);
	}
}
