using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionCatcher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public bool IsSelected { get; private set; }

	public void OnPointerEnter(PointerEventData eventData)
	{
		IsSelected = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsSelected = false;
	}
}
