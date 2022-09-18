using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CableColorPicker : MonoBehaviour, IPointerDownHandler
{

	public GridControl master;
	public int index;

	public void OnPointerDown(PointerEventData pointerEventData)
	{
		master.UpdateCableState(index);
	}
}
