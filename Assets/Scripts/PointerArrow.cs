using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerArrow : MonoBehaviour
{
	[SerializeField] private Camera cam;
	[SerializeField] private SpriteRenderer head;
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
		to.z = 10;

		for (int i = 0; i < PointsCount; i++)
		{
			float t = i / (PointsCount - 1f);
			points[i] = Vector3.Lerp(from, to, t);
			points[i].y += Mathf.Lerp(0, 3, 1 - (2 * t - 1) * (2 * t - 1));
		}

		Vector3 dir = (points[PointsCount - 1] - points[PointsCount - 2]).normalized;
		head.transform.position = points[PointsCount - 1];
		head.transform.localRotation = Quaternion.AngleAxis(
			-Vector3.SignedAngle(dir, Vector3.up, Vector3.forward), Vector3.forward);

		lineRenderer.positionCount = PointsCount;
		lineRenderer.SetPositions(points);
	}
}
