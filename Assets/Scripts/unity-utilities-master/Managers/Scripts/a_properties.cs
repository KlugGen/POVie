// Copyright for DKK Development (c) 2015


#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Configuration Custom Editor Window. Initializing settings values.
/// </summary>
public class a_properties : EditorWindow
{
	/// <summary>
	/// Initializing file with settings
	/// </summary>
	static a_properties ()
	{
		settings = new Settings (); 
		if (!File.Exists ("addonsSettings.dat")) {
			SaveSettings ();
		} else {
			LoadSettings ();
		} 
	}

	// Class witch start settings values
	[System.Serializable]
	public class Settings
	{
		public List<string> conditions = new List<string> (){ "All", "TODO", "HACK", "UNDONE", "FIXME", "_IMPORTANT" };
		public float myMargins = 0.1f;
	}

	/// <summary>
	/// Settings variable
	/// </summary>
	public static Settings settings;

	/// <summary>
	/// The name of the bookmark which appears in Unity.
	/// After changing - restart Unity and proper new name will appear.
	/// </summary>
	public const string bookmarkName = "DKK";

	/// <summary>
	/// Current selected category.
	/// </summary>
	private int selectedCategory = 1;
	private string newCondition = "";

	/// <summary>
	/// Scroll state of window.
	/// </summary>
	private static Vector2 scroll;

	/// <summary>
	/// Bool value for resizing window.
	/// </summary>
	private static bool fStart = true;

	/// <summary>
	/// The empty image.
	/// </summary>
	public static Sprite emptyImage;

	/// <summary>
	/// Enables configuration window.
	/// </summary>
	[MenuItem (bookmarkName + "/Configuration")]
	private static void ShowWindow ()
	{
		fStart = true;
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow (typeof(a_properties), false, "Configuration");
	}

	/// <summary>
	/// Raises the GU event.
	/// </summary>
	void OnGUI ()
	{	
		emptyImage = (Sprite)EditorGUILayout.ObjectField ("Empty Image", emptyImage, typeof(Sprite), false); 

		scroll = EditorGUILayout.BeginScrollView (scroll);
		EditorGUILayout.HelpBox ("Screenshots are placed in main project catalog", MessageType.Info);

		if (settings.myMargins > 1)
			settings.myMargins = 1;
		else if (settings.myMargins < 0)
			settings.myMargins = 0;

		settings.myMargins = EditorGUILayout.FloatField ("Layout margins: ", settings.myMargins);
		selectedCategory = EditorGUILayout.Popup ("Category: ", selectedCategory, settings.conditions.ToArray ());

		if (selectedCategory <= 5)
			EditorGUILayout.HelpBox (settings.conditions [selectedCategory].ToUpper () + " category is irremovable", MessageType.Warning);

		newCondition = EditorGUILayout.TextField ("New category", newCondition);

		EditorGUILayout.BeginHorizontal ();

		if (GUILayout.Button ("Add new")) {
			if (newCondition.Length > 0) {
				settings.conditions.Add (newCondition);
				newCondition = "";
				selectedCategory = settings.conditions.Count - 1;
			}
		}

		if (GUILayout.Button ("Delete current")) {
			if (selectedCategory > 5) {
				settings.conditions.RemoveAt (selectedCategory);
				selectedCategory = 0;
			}
		}

		EditorGUILayout.EndHorizontal ();

		GUILayout.Space (5);

		if (GUILayout.Button ("Save")) {
			SaveSettings ();
			Close ();
		}

		EditorGUILayout.EndScrollView ();

		if (fStart) {
			Rect tTrans = position;
			tTrans.height = 190;
			tTrans.width = 300;
			position = tTrans;			
			fStart = false;
		}
	}

	/// <summary>
	/// Saving file with current settings. 
	/// </summary>
	public static void SaveSettings ()
	{
		System.Environment.SetEnvironmentVariable ("MONO_REFLECTION_SERIALIZER", "yes");
			
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create ("addonsSettings.dat");
		try {
			bf.Serialize (file, settings);
		} catch (System.Exception) {
		}

		file.Close ();
	}

	/// <summary>
	/// Load settings from file
	/// </summary>
	public static void LoadSettings ()
	{
		System.Environment.SetEnvironmentVariable ("MONO_REFLECTION_SERIALIZER", "yes");
			
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open ("addonsSettings.dat", FileMode.Open);
		try {
			settings = (Settings)bf.Deserialize (file);
		} catch (System.Exception) {
		}
			
		file.Close ();		
	}
}
#endif