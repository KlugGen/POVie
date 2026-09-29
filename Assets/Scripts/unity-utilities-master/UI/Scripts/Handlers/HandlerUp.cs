using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class HandlerUp : MonoBehaviour, IPointerUpHandler
{
	public UnityEvent action;

	public void OnPointerUp (PointerEventData eventData)
	{
		action.Invoke ();
	}
}
