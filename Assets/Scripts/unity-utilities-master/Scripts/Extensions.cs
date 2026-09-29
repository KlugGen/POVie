using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Linq;
using DKK;

public static class Extensions
{

	public static bool displayLogs = true;

	/// <summary>
	/// Determines if is text null or empty the specified input.
	/// </summary>
	/// <returns><c>true</c> if is text null or empty the specified input; otherwise, <c>false</c>.</returns>
	/// <param name="input">Input.</param>
	public static bool IsTextNullOrEmpty (this InputField input)
	{
		return string.IsNullOrEmpty (input.text);
	}

	/// <summary>
	/// Log the specified obj and premessage.
	/// </summary>
	/// <param name="obj">Object.</param>
	/// <param name="premessage">Premessage.</param>
	public static void Log(this object obj, string premessage = "", bool forceDisplaying = false)
	{
        if (!displayLogs && !forceDisplaying)
        {
			return;
        }

		Debug.Log (premessage + obj.ToString ());
	}

    public static void LogDev(this object obj, string premessage = "", GameObject selection = null)
    {
#if UNITY_EDITOR
        if (selection != null)
            Debug.Log(premessage + obj.ToString(), selection);
        else
            Debug.Log(premessage + obj.ToString());
#endif
    }
    

    /// <summary>
    /// Clear the specified list.
    /// </summary>
    /// <param name="list">List.</param>
    public static void CompleteClear (this List<GameObject> list, bool withClear = false)
	{		
		foreach (GameObject g in list) {
			if (withClear) {
				IClear ic = g.GetComponent<IClear> ();
				if (ic != null) {
					ic.Clear ();
				}
			}

			MonoBehaviour.Destroy (g);
		}
		list.Clear ();
	}

    public static string RepairSeparators(this string path)
    {
        string ret = path;
        switch (Path.DirectorySeparatorChar)
        {
            case '\\':
                ret = path.Replace('/', Path.DirectorySeparatorChar);
                break;

            case '/':
                ret = path.Replace('\\', Path.DirectorySeparatorChar);
                break;
        }

        return ret;
    }


    public  static void Activate (this List<GameObject> gos)
	{
		foreach (GameObject g in gos) {
			g.SetActive (true);
		}
	}

	public  static void DeActivate (this List<GameObject> gos)
	{
		foreach (GameObject g in gos) {
			g.SetActive (false);
		}
	}

	public  static void Activate (this GameObject g)
	{
		g.SetActive (true);
	}

	public  static void DeActivate (this GameObject g)
	{
		g.SetActive (false);
	}

	public static void ResetPositionsFit (this RectTransform g)
	{
		g.anchorMin = Vector2.zero;
		g.anchorMax = Vector2.one;
   
		g.offsetMax = Vector2.zero;
		g.offsetMin = Vector2.zero;
		g.anchoredPosition = Vector2.zero;
		g.anchoredPosition3D = Vector3.zero;
		g.localPosition = Vector3.zero;
		g.position = Vector3.zero;
		g.anchoredPosition3D = Vector3.zero;
	}


	public  static void ResetPositions (this RectTransform g)
	{
		g.anchoredPosition = Vector2.zero;
		g.anchoredPosition3D = Vector3.zero;
		g.localPosition = Vector3.zero;
		g.position = Vector3.zero;
		g.offsetMax = Vector2.zero;
		g.offsetMin = Vector2.zero;
		g.anchoredPosition3D = Vector3.zero;
	}

	public static float GetSizeOf (this string s)
	{
		return (float)System.Text.ASCIIEncoding.ASCII.GetByteCount (s);
	}

	public static string GetMemoryFormat (this float f)
	{
		string type = "B";
		float tempValue = f;
		if (tempValue >= 1024) {
			tempValue /= 1024;
			type = "KB";

			if (tempValue >= 1024) {
				tempValue /= 1024;
				type = "MB";
			}

			return tempValue.ToString ("F2") + type;
		}

		return tempValue.ToString ("F0") + type;
	}

	public static string ToJSON <T> (this List<T> list)
	{
		string newList = "";

		foreach (T g in list) {
			JsonUtility.ToJson (g).Log ("String: ");
			newList += JsonUtility.ToJson (g);
		}
			
		return newList;
	}

	public static void SetParents (this List<GameObject> list, Transform parent)
	{
		foreach (GameObject g in list) {
			g.transform.SetParent (parent, false);
		}
	}

	//public static List<object> ToObjectsArray (this List<int> list)
	//{
	//	list.Count.Log ("List:");
	//	List<object> newList = new List<object> ();
	
	//	foreach (int g in list) {	
	//		if (g != 0)
	//			newList.Add ((Data.pictograms [g] as IPictogramElement) as object);
	//	}

	//	newList.Count.Log ("Count: ");
	
	//	return newList;
	//}

	//	public static List<object> ToObjectsArray (this List<int> list)
	//	{
	//		List<object> newList = new List<object> ();
	//
	//		foreach (int g in list) {
	//			newList.Add (g as object);
	//		}
	//
	//		return newList;
	//	}

	public static string GetContent (this List<int> list)
	{
		string answer = "";
		foreach (int s in list) {
			answer += s.ToString ();
		}

		return answer;
	}


	public static void ForceRebuildLayoutImmediate (this RectTransform rt)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate (rt);
	}

	static IEnumerator ForceRebuildLayoutImmediateCoroutine (RectTransform rt)
	{
		yield return new WaitForSeconds (1f);
		LayoutRebuilder.ForceRebuildLayoutImmediate (rt);
	}


	public static void SetCellSize (this GridLayoutGroup group, int columns = -1, int rows = 1)
	{

		if (group == null)
			return;
		
		if (columns > 0) {
			group.constraintCount = columns;
		}

		float space = group.padding.left + group.padding.right + (group.spacing.x * (group.constraintCount - 1));
		RectTransform rt = group.GetComponent<RectTransform> ();
		float nw = (rt.rect.width - space) / group.constraintCount;
		group.cellSize = new Vector2 (nw, nw);
	}

	public static void MatchWidthToHeight (this RectTransform rectTransform, RectTransform parent = null)
	{
		if (parent == null) {
			if (rectTransform.parent.GetComponent<RectTransform> () == null && rectTransform.parent == null)
				return;
			
			parent = rectTransform.parent.GetComponent<RectTransform> ();
		} else if (parent.GetComponent<RectTransform> () == null)
			return;
			
		
		if (rectTransform.GetComponent<LayoutElement> () == null)
			rectTransform.gameObject.AddComponent <LayoutElement> ();

		rectTransform.GetComponent<LayoutElement> ().preferredWidth = parent.rect.height;
	}

	public static void MatchHeightToWidth (this RectTransform rectTransform, RectTransform parent = null)
	{
		if (parent == null) {
			if (rectTransform.parent.GetComponent<RectTransform> () == null && rectTransform.parent == null)
				return;

			parent = rectTransform.parent.GetComponent<RectTransform> ();
		} else if (parent.GetComponent<RectTransform> () == null)
			return;
				
		if (rectTransform.GetComponent<LayoutElement> () == null)
			rectTransform.gameObject.AddComponent <LayoutElement> ();


		rectTransform.GetComponent<LayoutElement> ().preferredHeight = parent.rect.width;
	}


	public static Rect GetScreenRect (this RectTransform rectTransform, Canvas canvas, float margin = 0f)
	{
		Vector3[] corners = new Vector3[4];
		Vector3[] screenCorners = new Vector3[2];

		rectTransform.GetWorldCorners (corners);

		if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace) {
			screenCorners [0] = RectTransformUtility.WorldToScreenPoint (canvas.worldCamera, corners [1]);
			screenCorners [1] = RectTransformUtility.WorldToScreenPoint (canvas.worldCamera, corners [3]);
		} else {
			screenCorners [0] = RectTransformUtility.WorldToScreenPoint (null, corners [1]);
			screenCorners [1] = RectTransformUtility.WorldToScreenPoint (null, corners [3]);
		}

		screenCorners [0].y = Screen.height - screenCorners [0].y;
		screenCorners [1].y = Screen.height - screenCorners [1].y;

		// Jacek: odwrócenie po wysokości
		Rect result = new Rect (screenCorners [0], screenCorners [1] - screenCorners [0]);
		result.y = Screen.height - (result.y + result.height);

		result.width -= margin;
		result.height -= margin;
		result.x += margin;
		result.y += margin;

		return result;
	}

	public static float GetScreenDiagonalCM ()
	{
		return GetScreenDiagonalINCH () * 2.5f;
	}

	public static float GetScreenDiagonalINCH ()
	{
		float width = Screen.width * Screen.width;
		float height = Screen.height * Screen.height;
		float ypotinousa = width + height;
		ypotinousa = Mathf.Sqrt (ypotinousa);
		float diagonalInches = ypotinousa / Screen.dpi;
		return diagonalInches;
	}

	public static T Convert <T> (this object o)
	{		
		return (T)o;
	}

	public static byte[] ToBytesSerialize (this object obj)
	{
		if (obj == null)
			return null;
		BinaryFormatter bf = new BinaryFormatter ();
		using (MemoryStream ms = new MemoryStream ()) {
			bf.Serialize (ms, obj);
			return ms.ToArray ();
		}
	}

	public static object ToObjectDeserialize (this byte[] bytes)
	{
		if (bytes.Length == 0)
			return null;
		BinaryFormatter bf = new BinaryFormatter ();
		using (MemoryStream ms = new MemoryStream (bytes)) {
			return bf.Deserialize (ms);
		}
	}

	public static void Replace<T> (this IList<T> list, T obj1, T obj2)
	{
		int id1 = list.IndexOf (obj1);
		int id2 = list.IndexOf (obj2);
		T o = obj1;
		list [id1] = obj2;
		list [id2] = o;
	}

	public static void InsertOnPlace <T> (this IList<T> list, T obj1, int index)
	{
		T obj = obj1;
		list.Remove (obj1);
		list.Insert (index, obj);
	}

	public static void ClearAndDestroy (this IList list)
	{
		for (int i = 0; i < list.Count; i++) {
			MonoBehaviour.Destroy (list [i] as UnityEngine.Object);
		}
		list.Clear ();
	}

	

	public static void DestroySprite (this Image img)
	{
		if (img.sprite != null) {
			MonoBehaviour.Destroy (img.sprite.texture);
			MonoBehaviour.Destroy (img.sprite);
		}
	}

	public static void CreateSprite (this Image image, Texture2D t)
	{
		image.sprite = Sprite.Create (t, new Rect (0, 0, t.width, t.height), new Vector2 (0.5f, 0.5f));
	}

	static public Color32[] Merge (this Texture2D tex, Texture2D topTexture)
	{
		if (tex.width != topTexture.width)
			Debug.LogWarning ("Textures widths are different!");
		if (tex.height != topTexture.height)
			Debug.LogWarning ("Textures heights are different!");

		Color32[] texColors = tex.GetPixels32 ();
		Color32[] topTextureColors = topTexture.GetPixels32 ();

		for (int i = 0; i < texColors.Length; i++) {
			if (topTextureColors [i].a != 0) {
				float alpha = (float)topTextureColors [i].a / 255;
				texColors [i].r = (byte)((int)topTextureColors [i].r * alpha + (int)texColors [i].r * (1 - alpha));
				texColors [i].g = (byte)((int)topTextureColors [i].g * alpha + (int)texColors [i].g * (1 - alpha));
				texColors [i].b = (byte)((int)topTextureColors [i].b * alpha + (int)texColors [i].b * (1 - alpha));
			}
		}

		tex.SetPixels32 (texColors);
		tex.Apply ();

		return texColors;
	}

	static public float GetNormalizedPosY (this ScrollRect sr)
	{
		if (sr.content.anchoredPosition.y == 0)
			return 1;
		else
			return sr.normalizedPosition.y;
	}

	static public float GetNormalizedPosX (this ScrollRect sr)
	{
		if (sr.content.anchoredPosition.x == 0)
			return 0;
		else
			return sr.normalizedPosition.x;
	}

	static public void ScrollVertically (this ScrollRect scroll, float v)
	{
		float value = scroll.content.rect.height - scroll.GetComponent<RectTransform> ().rect.height;
		if (value > 0) {
			value = v / value;
		}
		scroll.verticalNormalizedPosition += value * Time.deltaTime;
	}

	static public void ScrollHorizontally (this ScrollRect scroll, float v)
	{
		float value = scroll.content.rect.width - scroll.GetComponent<RectTransform> ().rect.width;
		if (value > 0) {
			value = v / value;
		}
		scroll.horizontalNormalizedPosition += value * Time.deltaTime;
	}

	static public bool IsNullOrEmpty (this string str)
	{
		return string.IsNullOrEmpty (str);
	}

	static public void ClearSelf (this Image img)
	{
		if (img.sprite != null) {
			if (img.sprite.texture != null) {
				MonoBehaviour.Destroy (img.sprite.texture);
			}
			MonoBehaviour.Destroy (img.sprite);
		}
	}

	static public bool Remove<T> (this LinkedList<T> list, Predicate<T> match)
	{
		if (list == null) {
			throw new System.Exception ("list is null");
		}
		if (match == null) {
			throw new System.Exception ("match is null");
		}
		var node = list.First;
		while (node != null) {
			var next = node.Next;
			if (match (node.Value)) {
				list.Remove (node);
				return true;
			}
			node = next;
		}
		return false;
	}

	static public void AlphaToColor (this Texture2D tex, Color32 color)
	{
		Color32[] colors = tex.GetPixels32 ();
		for (int i = 0; i < colors.Length; i++) {
			float alpha = (float)colors [i].a / 255f;
			colors [i].r = (byte)(alpha * (float)colors [i].r + (color.r * (1 - alpha)));
			colors [i].g = (byte)(alpha * (float)colors [i].g + (color.g * (1 - alpha)));
			colors [i].b = (byte)(alpha * (float)colors [i].b + (color.b * (1 - alpha)));
			colors [i].a = 255;
		}
		tex.SetPixels32 (colors);
		tex.Apply ();
	}

	static public string FirstUpper (this string str)
	{
        if (str.Length > 0)
            return str.First().ToString().ToUpper() + str.Substring(1);
        else
            return str;
    }


	/// <summary>
	/// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
	/// </summary>
	/// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
	/// <param name="rectTransform">Rect transform.</param>
	/// <param name="camera">Camera.</param>
	private static int CountCornersVisibleFrom (this RectTransform rectTransform, Camera camera)
	{
		Rect screenBounds = new Rect (0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
		Vector3[] objectCorners = new Vector3[4];
		rectTransform.GetWorldCorners (objectCorners);

		int visibleCorners = 0;
		Vector3 tempScreenSpaceCorner; // Cached
		for (var i = 0; i < objectCorners.Length; i++) { // For each corner in rectTransform
			tempScreenSpaceCorner = camera.WorldToScreenPoint (objectCorners [i]); // Transform world space position of corner to screen space
			if (screenBounds.Contains (tempScreenSpaceCorner)) { // If the corner is inside the screen
				visibleCorners++;
			}
		}
		return visibleCorners;
	}

	/// <summary>
	/// Determines if this RectTransform is fully visible from the specified camera.
	/// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
	/// </summary>
	/// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
	/// <param name="rectTransform">Rect transform.</param>
	/// <param name="camera">Camera.</param>
	public static bool IsFullyVisibleFrom (this RectTransform rectTransform, Camera camera)
	{
		return CountCornersVisibleFrom (rectTransform, camera) == 4; // True if all 4 corners are visible
	}

	/// <summary>
	/// Determines if this RectTransform is at least partially visible from the specified camera.
	/// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
	/// </summary>
	/// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
	/// <param name="rectTransform">Rect transform.</param>
	/// <param name="camera">Camera.</param>
	public static bool IsVisibleFrom (this RectTransform rectTransform, Camera camera)
	{
		return CountCornersVisibleFrom (rectTransform, camera) > 0; // True if any corners are visible
	}

	public static Vector2 GetPositionOnCircle (this Vector2 center, float radius, float radians)
	{
		float x = radius * Mathf.Cos (radians);
		float y = radius * Mathf.Sin (radians);

		x += center.x;
		y += center.y;

		return new Vector2 (x, y);
	}

    //	public static List<T> FromJSON <T> (this List<JSONObject> list)
    //	{
    //		List<T> myList = new List<T> ();
    //		foreach (JSONObject t in list) {
    //			T t1 = new T ();
    //			JSerializable js = t1 as JSerializable;
    //			js.SetJson (t);
    //			myList.Add (t1);
    //		}
    //
    //		return myList;
    //	}


    #region DateTime


    public static string FormatFull(this DateTime date)
    {
        return date.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string FormatSimple(this DateTime date)
    {
        return date.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    }


    #endregion
}
