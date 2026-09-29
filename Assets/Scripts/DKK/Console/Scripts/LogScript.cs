using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DKK
{
	public class LogScript : MonoBehaviour
	{
		public void Click ()
		{
			string log = GetComponent<Text> ().text;
			string value = "";
			for (int i = 0; i < log.Length; i++) {
				if (log [i] == ' ') {
					for (int j = i + 1; j < log.Length; j++) {
						value += log [j];
					}
					break;
				}
			}
			ConsoleManager.InsertTextToConsole (value);
			GUIUtility.systemCopyBuffer = value;
		}
	}
}