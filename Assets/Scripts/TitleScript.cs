using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScript : MonoBehaviour
{
	public HideScreenImageEffect hideScreen;

	private void Awake()
	{
		hideScreen.SetValue(0);
	}

	public void Play()
	{
		hideScreen.SetValue(0);
		StartCoroutine(BeginSequence());
	}

	IEnumerator BeginSequence()
	{
		yield return new WaitForSeconds(hideScreen.Show());
		Game.Instance.NewGame();
	}
}
