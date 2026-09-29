using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace DKK
{
	public delegate void BoolMethod (bool b);
	public delegate void StringMethod (string s);
	public delegate void FloatMethod (float f);
	public delegate void IntMethod (int i);
	public delegate void BytesMethod (byte[] bytes);
	public delegate void ColorsMethod (Color[] colors);
	public delegate void Colors32Method (Color32[] colors);
	public delegate void Texture2DMethod (Texture2D t);
	public delegate void Texture2DObjMethod (Texture2D t, object o);
	public delegate void BoolTexture2DMethod (bool b, Texture2D t);
	public delegate void BytesObjMethod (byte[] bytes, object o);
	public delegate void ResponseMethod (bool success, object data);
	public delegate void BoolBytesMethod (bool b, byte[] bytes);
	public delegate void ObjectMethod (object o);
	public delegate void JsonMethod (JSONObject json);
	public delegate void VoidMethod ();

	public delegate void ParamsMethod (params object[] objects);
	public delegate IEnumerator Coroutine ();
	public delegate bool ReturnBoolMethod ();
	public delegate Color32[] ReturnColors32Method ();
	public delegate Texture2D ReturnTexture2DMethod ();

	public interface IPopup
	{
		void Open (string title, string textAccept, string textCancel, UnityAction callbackYes = null, UnityAction callbackNo = null);
	}

	public interface IClear
	{
		void Clear ();
	}

	public interface IInit
	{
		void Init (object o);
	}

	//public interface IGridElement
	//{
	//	void Init (object o, GridContainer container, ObjectMethod onClick);

	//	void Clear ();

	//	int GetID ();
	//}


	public interface IInitData
	{
		List<object> GetChildrenInits (bool onlyVisible);

		int GetID ();

		string GetName ();
	}

   
    public static class MyExtensions
	{
		/// <summary>
		/// Convert List<objects> to HashSet<T>.
		/// </summary>
		/// <returns>The hash set.</returns>
		/// <param name="objects">Objects.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static HashSet<T> ToHashSet <T> (this List<object> objects)
		{

			//UnityAction<bool, int, int> action;


			HashSet<T> hashset = new HashSet<T> ();
			foreach (object o in objects)
				hashset.Add ((T)o);
			return hashset;
		}



        public static string Cut (this string str, int maxLenght)
		{
			if (str.Length > maxLenght)
				return str.Substring (0, 10) + "...";
			else
				return str;
		}

		//		public static T CopyComponent<T> (this GameObject gameobject, GameObject destination) where T : Component
		//		{
		//			T original = gameobject.GetComponent<T> ();
		//
		//			if (original == null) {
		//				Debug.LogError ("There is no component type attached to " + gameobject.name);
		//				return null;
		//			}
		//
		////			System.Type type = original.GetType ();
		////			Component copy = destination.AddComponent (type);
		////			System.Reflection.FieldInfo[] fields = type.GetFields ();
		////			foreach (System.Reflection.FieldInfo field in fields) {
		////				field.SetValue (copy, field.GetValue (original));
		////			}
		////			return copy as T;
		//
		//			System.Type type = original.GetType ();
		//			var dst = destination.GetComponent (type) as T;
		//			if (!dst)
		//				dst = destination.AddComponent (type) as T;
		//			var fields = type.GetFields ();
		//			foreach (var field in fields) {
		//				if (field.IsStatic)
		//					continue;
		//				field.SetValue (dst, field.GetValue (original));
		//			}
		//			var props = type.GetProperties ();
		//			foreach (var prop in props) {
		//				if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name" || prop.PropertyType.Equals (typeof(Material)) || prop.PropertyType.Equals (typeof(Material[])))
		//					continue;
		//			}
		//			return dst as T;
		//
		//		}
	}

	public class Vector2int
	{
		public int x, y;

		public Vector2int (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2int (Vector2 vector)
		{
			this.x = (int)vector.x;
			this.y = (int)vector.y;
		}

		static private Vector2int zero = new Vector2int (0, 0);

		static public Vector2int Zero {
			get {
				return zero;
			}
		}
	}

	public class Pair
	{
		public object object1, object2;

		public Pair (object object1, object object2)
		{
			this.object1 = object1;
			this.object2 = object2;
		}
	}
}

