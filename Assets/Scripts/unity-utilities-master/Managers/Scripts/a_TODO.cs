// Copyright for DKK Development (c) 2015

#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Makes Editor Custom Window for listing all ToDO's code fragments from all scriptable files in project.
/// </summary>
[InitializeOnLoad]
public class a_TODO : EditorWindow
{
	// Class for one unit in ToDo's list
	public class ToDoAsset
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="a_TODO+ToDoAsset"/> class.
		/// </summary>
		/// <param name="nm">Name of file containing todo</param>
		/// <param name="ct">Content of todo</param>
		/// <param name="fp">Full path to file containing unit</param>
		/// <param name="ln">Line of todo</param>
		public ToDoAsset (string nm, string ct, string fp, int ln)
		{
			fullPath = fp;
			sName = nm;
			content = ct;
			line = ln;
		}

		public string sName;
		public string content;
		public string fullPath;
		public int line;
	}

	/// <summary>
	/// List of all todo-like units in project
	/// </summary>
	private static List <ToDoAsset> todoAssets = new List<ToDoAsset> ();

	/// <summary>
	/// Position of scrollable area
	/// </summary>
	private Vector2 scrollPosition = Vector2.zero;

	/// <summary>
	/// Texture set for GUI styles
	/// </summary>
	private static Texture2D[] texture = new Texture2D[4];

	/// <summary>
	/// Type of mark (ToDo, ToFIX etc.) to find
	/// </summary>
	private static int selection = 0, lastSelection = 0;

	/// <summary>
	/// All gui styles
	/// </summary>
	private static GUIStyle s_line, s_inner, s_cell, s_btn, s_text_1, s_text_2, s_important;

	/// <summary>
	/// String for search bar
	/// </summary>
	private static string searchString = "";


	/// <summary>
	/// Slider to adjust height of the unit on the list
	/// </summary>
	private static float heightSlider = 20f, lastHeightValue = -1;

	/*[MenuItem(a_properties.bookmarkName+ "/ToDo/Find in game-object's", false)]
	private static void FindToDos ()
	{
		if (Selection.gameObjects.Length > 0) {
			for (int i=0; i<Selection.gameObjects.Length; i++) {
				MonoBehaviour [] monos = Selection.gameObjects [i].GetComponents<MonoBehaviour> ();
				if (monos.Length > 0) {
					for (int j=0; j<monos.Length; j++) {

						if (UnityEditor.MonoScript.FromMonoBehaviour (monos [j]).text.Contains ("TODO")) {

							string [] lines = UnityEditor.MonoScript.FromMonoBehaviour (monos [j]).text.Split ('\n');

							for (int k = 0; k<lines.Length; k++) {
								if (lines [k].Contains ("TODO")) {
									Debug.Log ("File:<b> " + UnityEditor.MonoScript.FromMonoBehaviour (monos [j]).name + "</b>. [" + k + "]: " + lines [k]);
								}
							}
						}
					}
				} else {
					Debug.LogWarning ("No scripts attached!");
				}
			}
		} else
			Debug.LogWarning ("Select at least one game object!");
	}*/

	/// <summary>
	/// Shows the editor window.
	/// </summary>
	[MenuItem (a_properties.bookmarkName + "/Task list", false, -50)]
	private static void ShowWindow ()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow (typeof(a_TODO), false, "Tasks");
	}

	/// <summary>
	/// Prepares the list of ToDo's
	/// </summary>
	/// <param name="condition">Type of wanted statement.</param>
	private static void PrepareList (string condition)
	{
		// clear the list
		todoAssets.Clear ();

		// find all paths to files inside the project that fullfills condition t: Script (are scriptable)
		string[] allAssets = AssetDatabase.FindAssets ("t:Script");

		// remove /Assets at the end of path to the project
		string dataPath = Application.dataPath.Remove (Application.dataPath.Length - 6);

		// for all founded paths
		for (int i = 0; i < allAssets.Length; i++) {
			// take current to string
			string filePath = UnityEditor.AssetDatabase.GUIDToAssetPath (allAssets [i]);

			// cobine path in the project with path OF the project to get path to the file on hard-disk
			string fullPath = dataPath + filePath;

			// open file from the path
			System.IO.StreamReader sReader = System.IO.File.OpenText (fullPath);

			// read all file
			string content = sReader.ReadToEnd ();

			// if the file contains condition
			if (content.Contains (condition)) {

				// split string to seperate lines
				string[] lines = content.Split ('\n');

				// create id integer variable
				int id = -1;

				// find the last slash from the end...
				for (int z = filePath.Length - 1; z > 0; z--) {
					if (filePath [z] == '/') {
						id = z;
						break;
					}
				}

				// ... end get name of asset
				filePath = filePath.Substring (id + 1);

				// for all lines in current file
				for (int j = 0; j < lines.Length; j++) {
					// if current line in current file contains condition string
					if (lines [j].Contains (condition)) {
						// and if there is not addons file
						if (filePath != "a_TODO.cs" && filePath != "a_properties.cs")
							// add matching file to list by creating unit with file name, line, full path
							todoAssets.Add (new ToDoAsset ("<i>" + filePath + " </i>", lines [j].Replace ("\t", ""), fullPath, j + 1));
					}
				}
			}
		}
	}

	/// <summary>
	/// Prepares the list matching with any condition from all conditions array
	/// </summary>
	private static void PrepareList ()
	{
		// clear the list
		todoAssets.Clear ();
		
		// find all paths to files inside the project that fullfills condition t: Script (are scriptable)
		string[] allAssets = AssetDatabase.FindAssets ("t:Script");
		
		// remove /Assets at the end of path to the project
		string dataPath = Application.dataPath.Remove (Application.dataPath.Length - 6);
		
		// for all founded paths
		for (int i = 0; i < allAssets.Length; i++) {
			// take current to string
			string filePath = UnityEditor.AssetDatabase.GUIDToAssetPath (allAssets [i]);
			
			// cobine path in the project with path OF the project to get path to the file on hard-disk
			string fullPath = dataPath + filePath;
			
			// open file from the path
			System.IO.StreamReader sReader = System.IO.File.OpenText (fullPath);
			
			// read all file
			string content = sReader.ReadToEnd ();
			// for all string in conditions array
			for (int g = 1; g < a_properties.settings.conditions.Count; g++) {
				// if file contains current condition
				if (content.Contains (a_properties.settings.conditions [g])) {

					// split string to seperate lines
					string[] lines = content.Split ('\n');
					
					// create id integer variable
					int id = -1;
					
					// find the last slash from the end...
					for (int z = filePath.Length - 1; z > 0; z--) {
						if (filePath [z] == '/') {
							id = z;
							break;
						}
					}

					// ... end get name of asset
					filePath = filePath.Substring (id + 1);

					// for all lines in current file
					for (int j = 0; j < lines.Length; j++) {
						// and for all conditions
						for (int c = 1; c < a_properties.settings.conditions.Count; c++) {
							// if current lin in current file contains cuurent condition
							if (lines [j].Contains (a_properties.settings.conditions [c])) {
								// except THIS file
								if (filePath != "a_TODO.cs" && filePath != "a_properties.cs") {
									// add this file to list by creating unit with file name, line, full path
									todoAssets.Add (new ToDoAsset ("<i>" + filePath + " </i>", lines [j].Replace ("\t", ""), fullPath, j));

								}
							}				
						}
					}
					break;
				}
			}
		}	
	}

	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable ()
	{  		
		// Prepare list
		PrepareList ();

		// Initialize textures
		MakeTexture ();

		// Initialize variable
		lastSelection = -1;
	}

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	void OnDisable ()
	{
		// destroy all textures
		for (int i = 0; i < texture.Length; i++)
			DestroyImmediate (texture [i]);
	}

	/// <summary>
	/// Create textures for gui styles
	/// </summary>
	private static void MakeTexture ()
	{
		// color array
		Color[] colors = new Color[4];
		colors [0] = new Color (0.52f, 0.52f, 0.52f);
		colors [1] = new Color (0.6f, 0.6f, 0.6f);
		colors [2] = new Color (0f, 0f, 0f);
		colors [3] = new Color (0.6f, 0f, 0f);

		// for all textures
		for (int i = 0; i < texture.Length; i++) {
			// if current texture is uninitialized
			if (texture [i] == null) {
				// create new
				texture [i] = new Texture2D (900, 900);

				// set every pixel of current texture with color
				for (int k = 0; k < texture [i].width; k++)
					for (int j = 0; j < texture [i].height; j++) {
						texture [i].SetPixel (k, j, colors [i]);						
					}

				// apply texture
				texture [i].Apply ();
			}
		}
	}

	/// <summary>
	/// Prepares the styles for GUI.
	/// </summary>
	/// <param name="isSize">If set to <c>true</c> is size.</param>
	private void PrepareStyles ()
	{
		//_IMPORTANT: 
		s_btn = new GUIStyle ();
		s_btn.fixedHeight = heightSlider;
		
		s_text_1 = new GUIStyle (s_btn);
		s_text_1.richText = true;
		s_text_1.alignment = TextAnchor.MiddleLeft;
		s_text_1.padding.left = 10;
		
		s_text_2 = new GUIStyle (s_text_1);
		s_text_2.alignment = TextAnchor.LowerRight;
		s_text_2.padding.bottom = 5;
		
		s_cell = new GUIStyle (s_btn);	
		s_cell.normal.background = texture [0];
		
		s_inner = new GUIStyle (s_btn);
		s_inner.fixedWidth = position.width / 2 - 15;

		s_important = new GUIStyle (s_btn);
		s_important.normal.background = texture [3];

		s_line = new GUIStyle ();
		s_line.fixedWidth = position.width;
		s_line.fixedHeight = 1;
		s_line.normal.background = texture [2];
	}

	/// <summary>
	/// Raises the GUI event.
	/// </summary>
	void OnGUI ()
	{
		ToDoWindowFuction ();
	}

	/// <summary>
	/// GUI function (in case when it'll be update, under switch in OnGUI() will appear next functions).
	/// </summary>
	void ToDoWindowFuction ()
	{
		// Prepare styles without resizing
		PrepareStyles ();

		// initialize id integer
		int id = 0;

		// draw scrollable area and get scroll value
		scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition);

		// begin horizontal view
		EditorGUILayout.BeginHorizontal (GUILayout.Width (position.width / 2));

		// draw label category
		EditorGUILayout.LabelField ("Category", GUILayout.Width (100));	
		// draw popup menu with conditions
		selection = EditorGUILayout.Popup (selection, a_properties.settings.conditions.ToArray ());

		// id condition was changed
		if (lastSelection != selection) {
			// if it is first run
			if (lastSelection == -1) {
				// prepare styles
				PrepareList ();
			} else {
				if (selection == 0)
					PrepareList ();
				else
					PrepareList (a_properties.settings.conditions [selection]);
			}

			// overwrite selection
			lastSelection = selection;

			PrepareStyles ();
		}

		// if unit height was changed 
		if (lastHeightValue != heightSlider) {
			// overwrite last value
			lastHeightValue = heightSlider;
			// redraw styles
			PrepareStyles ();
		}

		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.BeginHorizontal (GUILayout.Width (position.width / 2));

		// draw label for item size
		EditorGUILayout.LabelField ("Item size:", GUILayout.Width (100));		
		// draw slider for item height and get value
		heightSlider = GUILayout.HorizontalSlider (heightSlider, 20f, 50f);
				
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.BeginHorizontal (GUILayout.Width (position.width / 2));

		EditorGUILayout.LabelField ("Filter:", GUILayout.Width (100));	
		searchString = EditorGUILayout.TextField (searchString);

		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginVertical ();

		// draw horizontal line
		GUILayout.Box ("", s_line);

		// for all units in list
		foreach (ToDoAsset t in todoAssets) {

			// if unitu contains search string
			if (t.content.Contains (searchString) || t.sName.Contains (searchString)) {
				// draw withc different background in turns
				if (id % 2 == 1)
					s_cell.normal.background = texture [0];
				else
					s_cell.normal.background = texture [1];

				// if it is IMPORTANT condition use special background
				if (t.content.Contains ("_IMPORTANT"))
					s_cell.normal.background = texture [3];


				EditorGUILayout.BeginHorizontal (s_cell); 
				EditorGUILayout.BeginVertical (s_inner);

				// draw content in unit
				EditorGUILayout.LabelField ("<b>" + t.content + "</b>", s_text_1);
				
				EditorGUILayout.EndVertical ();		
				EditorGUILayout.BeginVertical (s_inner);
				EditorGUILayout.BeginHorizontal ();

				// draw file name in unit
				EditorGUILayout.LabelField (t.sName + "[l. " + t.line + "]", s_text_2);	
				
				GUILayout.Space (10);

				// draw button to edit file
				if (GUILayout.Button ("Edit", GUILayout.Width (40), GUILayout.Height (15 * (heightSlider / 20))))
					// opens file at specified line
					UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal (t.fullPath, t.line + 1);
				
				EditorGUILayout.EndHorizontal ();			
				EditorGUILayout.EndVertical ();
				EditorGUILayout.EndHorizontal ();						

				// draw line
				GUILayout.Box ("", s_line);

				// increment unit counter
				id++;
			}
		}
		
		EditorGUILayout.EndVertical ();
		EditorGUILayout.EndScrollView ();
	}
}
#endif