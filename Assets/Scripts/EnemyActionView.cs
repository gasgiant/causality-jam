using UnityEngine;
using TMPro;

public class EnemyActionView : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI descriptionText;

	public void Display(string description)
	{
		descriptionText.text = description;
	}
}
