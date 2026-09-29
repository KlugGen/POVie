using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class HandlerDown : MonoBehaviour, IPointerDownHandler
{
	public UnityEvent action;

	public void OnPointerDown (PointerEventData eventData)
	{
		action.Invoke ();
	}
}
