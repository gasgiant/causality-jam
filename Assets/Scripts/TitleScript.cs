using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScript : MonoBehaviour
{
	public HideScreenImageEffect hideScreen;

	public FadeCanvasGroup mainMenu;
	public FadeCanvasGroup intro0;
	public FadeCanvasGroup intro1;
	public FadeCanvasGroup intro2;

	bool showingIntro;

	private void Awake()
	{
		hideScreen.SetValue(0);

		StartCoroutine(IntroSequence());
	}

	private void Update()
	{
		if (showingIntro && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)))
		{
			showingIntro = false;
			StopAllCoroutines();
			mainMenu.HardSet(1);
			intro0.HardSet(0);
			intro1.HardSet(0);
			intro2.HardSet(0);
		}
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

	IEnumerator IntroSequence()
	{
		showingIntro = true;
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(intro0.Show());
		yield return new WaitForSeconds(2f);
		yield return new WaitForSeconds(intro1.Show());
		yield return new WaitForSeconds(5f);
		yield return new WaitForSeconds(intro2.Show());
		yield return new WaitForSeconds(4f);
		intro0.Hide();
		intro1.Hide();
		intro2.Hide();
		yield return new WaitForSeconds(1.2f);
		yield return new WaitForSeconds(mainMenu.Show());
		showingIntro = false;
	}
}
