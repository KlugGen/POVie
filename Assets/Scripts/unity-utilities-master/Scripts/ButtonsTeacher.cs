//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.IO;
//using System;
//using UnityEditor;
//using DKK;
//
//namespace DKK
//{
//
//	[Serializable] public class BTCollectionDictionary : Dictionary<string, ButtonsTeacher.Collection>
//	{
//
//	}
//
//	[CustomPropertyDrawer (typeof(BTCollectionDictionary))]
//	public class BTCollectionDictionaryDrawer : DictionaryDrawer<string, ButtonsTeacher.Collection>
//	{
//
//	}
//
//	[Serializable] public class StringDictionary : Dictionary<string, string>
//	{
//
//	}
//
//	[CustomPropertyDrawer (typeof(StringDictionary))]
//	public class StringDictionaryDrawer : DictionaryDrawer<string, string>
//	{
//
//	}
//
//	[Serializable] public class GameObjectDictonary : Dictionary<string, GameObject>
//	{
//
//	}
//
//	[CustomPropertyDrawer (typeof(GameObjectDictonary))]
//	public class GameObjectDictonaryDrawer : DictionaryDrawer<string, GameObject>
//	{
//
//	}
//
//	public class ButtonsTeacher : MonoBehaviourDKK
//	{
//		public string confPath = "buttonsTeacher";
//		static private ButtonsTeacher self;
//
//		public BTCollectionDictionary<string, Collection> elements = new BTCollectionDictionary<string, Collection> ();
//		public StringDictionary<string, string> descriptions = new StringDictionary<string, string> ();
//
//		public override void OnAppStart ()
//		{
//			self = this;
//
//			string path = Path.Combine (Application.persistentDataPath, "Config");
//			if (!Directory.Exists (path))
//				Directory.CreateDirectory (path);
//			confPath = Path.Combine (path, confPath);
//
//			if (File.Exists (confPath)) {
//
//			}
//		}
//
//		[Serializable]
//		public class Collection
//		{
//			GameObjectDictonary<string, GameObject> objects = new GameObjectDictonary<string, GameObject> ();
//		}
//	}
//}