#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class a_UI : EditorWindow
{
	public const string bookmarkName = "DKK";
	int layoutSetMode = 0, topBottomMode = 0, leftRightMode = 0;
	float customMargin = 0.1f;

	[MenuItem (bookmarkName + "/UI Builder")]
	private static void ShowWindow ()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow (typeof(a_UI), false, "Configuration");
	}

	void OnGUI ()
	{
		int btnHeight = 40;
		GUILayout.Label ("Layout themes");
		EditorGUILayout.BeginHorizontal ();	
		EditorGUILayout.BeginVertical ();	
		GUILayout.Label ("Axis");
		layoutSetMode = EditorGUILayout.Popup (layoutSetMode, new string[] { "Vertical", "Horizontal" });
		EditorGUILayout.EndVertical ();	
		EditorGUILayout.BeginVertical ();	
		GUILayout.Label ("Start pivot");
		if (layoutSetMode == 0) {
			topBottomMode = EditorGUILayout.Popup (topBottomMode, new string[] { "Top", "Bottom" });
		} else {
			leftRightMode = EditorGUILayout.Popup (leftRightMode, new string[] { "Left", "Right" });
		}
		EditorGUILayout.EndVertical ();	
		EditorGUILayout.EndHorizontal ();	

		EditorGUILayout.BeginHorizontal ();	

		if (GUILayout.Button ("1/6", GUILayout.Height (btnHeight))) {
			SetSelectionAsLayout (1f / 6f);
		}

		if (GUILayout.Button ("2/6", GUILayout.Height (btnHeight))) {
			SetSelectionAsLayout (2f / 6f);
		}

		if (GUILayout.Button ("3/6", GUILayout.Height (btnHeight))) {
			SetSelectionAsLayout (3f / 6f);
		}
		if (GUILayout.Button ("4/6", GUILayout.Height (btnHeight))) {
			SetSelectionAsLayout (4f / 6f);
		}
		if (GUILayout.Button ("5/6", GUILayout.Height (btnHeight))) {
			SetSelectionAsLayout (5f / 6f);
		}
		if (GUILayout.Button ("6/6", GUILayout.Height (btnHeight))) {
			SetSelectionAsLayout (6f / 6f);
		}	

		EditorGUILayout.EndHorizontal ();
		GUILayout.Space (10f);
		GUILayout.Label ("Layout sets");
		EditorGUILayout.BeginHorizontal ();

		if (GUILayout.Button ("Horizontal")) {
			if (Selection.gameObjects.Length > 0)
				for (int i = 0; i < Selection.gameObjects.Length; i++) {
					if (Selection.gameObjects [i].GetComponent<HorizontalLayoutGroup> () == null)
						Selection.gameObjects [i].AddComponent<HorizontalLayoutGroup> ();
				}
		}

		if (GUILayout.Button ("Vertical")) {
			if (Selection.gameObjects.Length > 0)
				for (int i = 0; i < Selection.gameObjects.Length; i++) {
					if (Selection.gameObjects [i].GetComponent<VerticalLayoutGroup> () == null)
						Selection.gameObjects [i].AddComponent<VerticalLayoutGroup> ();
				}
		}

		if (GUILayout.Button ("Grid Layout")) {
			if (Selection.gameObjects.Length > 0)
				for (int i = 0; i < Selection.gameObjects.Length; i++) {
					if (Selection.gameObjects [i].GetComponent<GridLayoutGroup> () == null)
						Selection.gameObjects [i].AddComponent<GridLayoutGroup> ();
				}
		}
			

		EditorGUILayout.EndHorizontal ();

		GUILayout.Space (10f);
		GUILayout.Label ("Margins sets");

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Margin 0.1", GUILayout.Height (btnHeight))) {
			if (Selection.gameObjects.Length > 0) {
				for (int i = 0; i < Selection.gameObjects.Length; i++) {
					Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin = new Vector2 (0.1f, 0.1f);
					Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax = new Vector2 (0.9f, 0.9f);
					Selection.gameObjects [i].GetComponent<RectTransform> ().offsetMax = new Vector2 (0f, 0f);
					Selection.gameObjects [i].GetComponent<RectTransform> ().offsetMin = new Vector2 (0f, 0f);
				}
			} else
				Debug.LogWarning ("Select al least one transform!");
		}

		if (GUILayout.Button ("Margin 0.2", GUILayout.Height (btnHeight))) {
			if (Selection.gameObjects.Length > 0) {
				for (int i = 0; i < Selection.gameObjects.Length; i++) {
					Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin = new Vector2 (0.2f, 0.2f);
					Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax = new Vector2 (0.8f, 0.8f);
					Selection.gameObjects [i].GetComponent<RectTransform> ().offsetMax = new Vector2 (0f, 0f);
					Selection.gameObjects [i].GetComponent<RectTransform> ().offsetMin = new Vector2 (0f, 0f);
				}
			} else
				Debug.LogWarning ("Select al least one transform!");
		}
	
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
//		GUILayout.Label ();
		customMargin = EditorGUILayout.FloatField ("Custom margin: ", customMargin);
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
	
		if (GUILayout.Button (string.Format ("Make custom margin ({0})", customMargin.ToString ()), GUILayout.Height (btnHeight))) {
			if (Selection.gameObjects.Length > 0) {
				for (int i = 0; i < Selection.gameObjects.Length; i++) {
					Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin = new Vector2 (customMargin, customMargin);
					Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax = new Vector2 (1f - customMargin, 1f - customMargin);
					Selection.gameObjects [i].GetComponent<RectTransform> ().offsetMax = new Vector2 (0f, 0f);
					Selection.gameObjects [i].GetComponent<RectTransform> ().offsetMin = new Vector2 (0f, 0f);
				}
			} else
				Debug.LogWarning ("Select al least one transform!");
		}

		EditorGUILayout.EndHorizontal ();
	}

	void SetSelectionAsLayout (float val)
	{
		if (Selection.gameObjects.Length > 0) {
			for (int i = 0; i < Selection.gameObjects.Length; i++) {
				if (Selection.gameObjects [i].GetComponent<RectTransform> () != null) {
//					float startValue = 0;
					if (layoutSetMode == 1) {
						if (leftRightMode == 1) {
							Vector2 vMin = Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin;
							Vector2 vMax = Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax;
							Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin = new Vector2 (1f - val, vMin.y);
							Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax = new Vector2 (1f, vMax.y);
						} else {
							Vector2 vMin = Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin;
							Vector2 vMax = Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax;
							Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin = new Vector2 (0f, vMin.y);
							Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax = new Vector2 (0f + val, vMax.y);
						}
					} else {
						if (topBottomMode == 1) {
							Vector2 vMin = Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin;
							Vector2 vMax = Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax;
							Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin = new Vector2 (vMin.x, 0);
							Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax = new Vector2 (vMax.x, 0f + val);
						} else {
							Vector2 vMin = Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin;
							Vector2 vMax = Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax;
							Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMin = new Vector2 (vMin.x, 1f - val);
							Selection.gameObjects [i].GetComponent<RectTransform> ().anchorMax = new Vector2 (vMax.x, 1f);
						}
					}
				}
			}
		}
	}
}
#endif