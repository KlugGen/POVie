using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DKK
{
	public class ConsoleResize : MonoBehaviour
	{
		public RectTransform rt;
		public RectTransform trigger;

		private float diffPercY, diffPercX;

		public void OnDrag ()
		{
			Vector2 v = GetPercentages ();

			Vector2 rtanchorMin = new Vector2 (rt.anchorMin.x, v.y + diffPercY);
			rtanchorMin.y = Mathf.Clamp (rtanchorMin.y, 0, 1);
			rt.anchorMin = rtanchorMin;

			Vector2 triggeranchors = new Vector2 (v.x + diffPercX, trigger.anchorMin.y);
			triggeranchors.x = Mathf.Clamp (triggeranchors.x, 0, 1);
			trigger.anchorMin = triggeranchors;
			trigger.anchorMax = triggeranchors;
		}

		public void OnDragStart ()
		{
			Vector2 v = GetPercentages ();
			diffPercY = rt.anchorMin.y - v.y;
			diffPercX = trigger.anchorMin.x - v.x;
		}

		private Vector2 GetPercentages ()
		{
			return new Vector2 (Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
		}
	}
}