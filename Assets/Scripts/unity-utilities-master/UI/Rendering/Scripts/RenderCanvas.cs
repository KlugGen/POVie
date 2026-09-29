using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DKK
{

	public class RenderCanvas : MonoBehaviour
	{
		private Texture2DMethod Callback;
		public Transform canvasTransform;
		public Canvas canvas;
		public Camera cam;

		private int w, h;
		private RenderTexture rend;
		private bool autoDestroy;
		private RectTransformMemory rtm = null;

		static private uint counter = 0;

		public enum Content
		{
			Duplicate = 0,
			Destroy = 1,
			Return = 2
		}

		public void Render (int width, int height, Texture2DMethod Callback, RectTransform rt = null, bool autoDestroy = false, Content content = Content.Destroy, params RenderCanvasOption[] options)
		{
			if (rt != null) {
				
				switch (content) {
				case Content.Destroy:
					rt.transform.SetParent (canvasTransform);
					rt.transform.localPosition = Vector3.zero;
//					if (options.Length > 0) {
					RectTransformChildrenOptions (rt, options);
//					}
					break;
				case Content.Duplicate:
					rt = CopyRectTransform (rt, canvasTransform);
//					if (options.Length > 0) {
					RectTransformChildrenOptions (rt, options);
//					}
					break;
				case Content.Return:
					rtm = new RectTransformMemory (rt);
					PrepareRectTransform (rt, canvasTransform);
					break;
				}

				Vector2 sizeDelta = new Vector2 (Mathf.Abs (rt.sizeDelta.x), Mathf.Abs (rt.sizeDelta.y));

				Vector2 size = UICalc.MatchAspectUpTo (sizeDelta.x, sizeDelta.y, width, height);
				Vector3 scale = new Vector3 (sizeDelta.x != 0 ? size.x / sizeDelta.x : 1, sizeDelta.y != 0 ? size.y / sizeDelta.y : 1, 1);
				rt.transform.localScale = scale;
			}

			Rect rect = new Rect (0, 0, width, height);
			w = width;
			h = height;

			this.autoDestroy = autoDestroy;
			this.Callback = Callback;
			StartCoroutine (RenderCoroutine (rect));
		}

		public void Render (Dimension dimension, int pixels, Texture2DMethod Callback, RectTransform rt, bool autoDestroy = false, Content content = Content.Destroy, params RenderCanvasOption[] options)
		{
			switch (content) {
			case Content.Destroy:
				rt.transform.SetParent (canvasTransform);
				rt.transform.localPosition = Vector3.zero;
//				if (options.Length > 0) {
				RectTransformChildrenOptions (rt, options);
//				}
				break;
			case Content.Duplicate:
				rt = CopyRectTransform (rt, canvasTransform);
//				if (options.Length > 0) {
				RectTransformChildrenOptions (rt, options);
//				}
				break;
			case Content.Return:
				rtm = new RectTransformMemory (rt);
				PrepareRectTransform (rt, canvasTransform);
				break;
			}

			Rect rect;
			Vector2 size;
			Vector2 sizeDelta = new Vector2 (Mathf.Abs (rt.sizeDelta.x), Mathf.Abs (rt.sizeDelta.y));
			switch (dimension) {
			case Dimension.Height:
				size = UICalc.MatchAspectUpToHeight (sizeDelta.x, sizeDelta.y, (float)pixels);
				size = SafeSize (size, 2048);
				float newWidth = size.x;
				rect = new Rect ((newWidth - size.x) / 2, (pixels - size.y) / 2, size.x, size.y);
				w = (int)newWidth;
				h = pixels;
				break;

			case Dimension.Width:
				size = UICalc.MatchAspectUpToWidth (sizeDelta.x, sizeDelta.y, (float)pixels);
				size = SafeSize (size, 2048);
				float newHeight = size.y;
				rect = new Rect ((pixels - size.x) / 2, (newHeight - size.y) / 2, size.x, size.y);
				w = pixels;
				h = (int)newHeight;
				break;

			default:
				rect = new Rect ();
				size = new Vector2 ();
				Debug.LogError ("Rendering dimension problem!");
				break;
			}

//			Debug.Log (sizeDelta);
			Vector3 scale = new Vector3 (sizeDelta.x != 0 ? size.x / sizeDelta.x : 1, sizeDelta.y != 0 ? size.y / sizeDelta.y : 1, 1);
			rt.transform.localScale = scale;

			this.autoDestroy = autoDestroy;
			this.Callback = Callback;
			StartCoroutine (RenderCoroutine (rect));
		}

		public void Render (Dimension dimension, int pixels, Texture2DMethod Callback, Rect rect, bool autoDestroy = false)
		{
			switch (dimension) {
			case Dimension.Height:
				h = pixels;
				w = (int)((float)Screen.width / (float)Screen.height * (float)pixels);
				break;

			case Dimension.Width:
				w = pixels;
				h = (int)(1 / (float)Screen.width / (float)Screen.height * (float)pixels);
				break;
			}

			this.autoDestroy = autoDestroy;
			this.Callback = Callback;
			StartCoroutine (RenderCoroutine (rect));
		}

		IEnumerator RenderCoroutine (Rect rect)
		{
			counter++;
			transform.position = new Vector3 (-1000 * counter, -1000, 0);
			if (w == 0 || h == 0) {
				Debug.LogWarning (string.Format ("Objects dimensions are {0}x{1}. And it cannot be rendered.", w, h));
				rend = RenderTexture.GetTemporary (2, 2, 24, RenderTextureFormat.ARGB32);
			} else {
				rend = RenderTexture.GetTemporary (w, h, 24, RenderTextureFormat.ARGB32);
			}
			cam.targetTexture = rend;

			yield return new WaitForEndOfFrame ();
			RenderTexture.active = rend;

			int rw = Mathf.CeilToInt (rect.width);
			if (rw > w) {
				rw = w;
				rect.x = 0;
			} else if (rw + ((int)rect.x * 2) > w) {
				rect.x = (w - rw) / 2;
			}
			rect.width = rw;

			int rh = Mathf.CeilToInt (rect.height);
			if (rh > h) {
				rh = h;
				rect.y = 0;
			} else if (rh + ((int)rect.y) * 2 > h) {
				rect.y = (h - rh) / 2;
			}
			rect.height = rh;

			Texture2D tex = new Texture2D (rw, rh, TextureFormat.RGBA32, false);

			tex.ReadPixels (rect, 0, 0);
			tex.Apply ();
			cam.targetTexture = null;
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary (rend);

			Callback (tex);

			if (rtm != null) {
				rtm.Return ();
				rtm = null;
			}

			counter--;

			if (autoDestroy) {
				Destroy (gameObject);
			}
		}

		public Rect MatchRectScale (Dimension dimension, int pixels, Rect rect)
		{
			float scale;
			switch (dimension) {
			case Dimension.Height:
				scale = pixels / (float)Screen.height;
				rect = new Rect (rect.x * scale, rect.y * scale, rect.width * scale, rect.height * scale);
				break;

			case Dimension.Width:
				scale = pixels / (float)Screen.width;
				rect = new Rect (rect.x * scale, rect.y * scale, rect.width * scale, rect.height * scale);
				break;

			default:
				rect = new Rect ();
				break;
			}

			return rect;
		}

		public RectTransform CopyRectTransform (RectTransform original, Transform parent = null, Vector3 parentOffset = default(Vector3))
		{
			GameObject copy = Instantiate (original.gameObject);
			RectTransform copyRT = copy.GetComponent<RectTransform> ();
			copyRT.gameObject.SetActive (true);

			return CopyRectTransform (original, copyRT, parent, parentOffset);
		}

		public RectTransform CopyRectTransformNoChildren (RectTransform original, Transform parent = null, Vector3 parentOffset = default(Vector3))
		{
			GameObject copy = new GameObject (original.name + " rt copy");
			RectTransform copyRT = copy.AddComponent<RectTransform> ();

			return CopyRectTransform (original, copyRT, parent, parentOffset);
		}

		public void PrepareRectTransform (RectTransform rt, Transform parent = null, Vector3 parentOffset = default(Vector3))
		{
			float width = rt.rect.width;
			float height = rt.rect.height;
			rt.anchorMin = new Vector2 (0.5f, 0.5f);
			rt.anchorMax = new Vector2 (0.5f, 0.5f);
			rt.pivot = new Vector2 (0.5f, 0.5f);
			rt.sizeDelta = new Vector2 (width, height);

			Vector3 originalScale = UICalc.FindMyCanvas (rt.transform).transform.localScale;
			if (parent != null) {
				rt.transform.SetParent (parent);
				parentOffset.x /= parent.localScale.x;
				parentOffset.y /= parent.localScale.y;
				parentOffset.z /= parent.localScale.z;
				rt.transform.localPosition = parentOffset;
				rt.localScale = Vector3.one;
			} else
				rt.localScale = originalScale;
		}

		private RectTransform CopyRectTransform (RectTransform oryginal, RectTransform copyRT, Transform parent = null, Vector3 parentOffset = default(Vector3))
		{
			copyRT.anchorMin = new Vector2 (0.5f, 0.5f);
			copyRT.anchorMax = new Vector2 (0.5f, 0.5f);
			copyRT.pivot = new Vector2 (0.5f, 0.5f);
			copyRT.sizeDelta = new Vector2 (oryginal.rect.width, oryginal.rect.height);

			Vector3 originalScale = UICalc.FindMyCanvas (oryginal.transform).transform.localScale;
			if (parent != null) {
				copyRT.transform.SetParent (parent, false);
				if (parent.localScale.x != 0)
					parentOffset.x /= parent.localScale.x;
				if (parent.localScale.y != 0)
					parentOffset.y /= parent.localScale.y;
				if (parent.localScale.z != 0)
					parentOffset.z /= parent.localScale.z;
				copyRT.transform.localPosition = parentOffset;
				copyRT.localScale = Vector3.one;
			} else
				copyRT.localScale = originalScale;

			return copyRT;
		}

		public void RectTransformChildrenOptions (RectTransform rt, params RenderCanvasOption[] options)
		{
			Transform[] ts = rt.transform.GetComponentsInChildren<Transform> ();

			for (int o = 0; o < options.Length; o++) {
				
				int optionInt = (int)options [o].option;

				for (int i = 0; i < ts.Length; i++) {
					if (ts [i] != null && ts [i].name == options [o].name) {
						switch (optionInt) {
						case 1:
							Image[] images = ts [i].GetComponentsInChildren<Image> ();
							for (int j = 0; j < images.Length; j++) {
								images [j].color = Color.white;
							}
							break;
						case 2:
							DestroyImmediate (ts [i].gameObject);
							break;
						}
					}
				}
			}

			RendererTag[] rTags = rt.transform.GetComponentsInChildren<RendererTag> ();
			foreach (RendererTag rTag in rTags) {
				switch (rTag.option) {
				case RendererTag.TagOption.None:
					break;
				case RendererTag.TagOption.RenderIgnore:
					rTag.gameObject.SetActive (false);
					break;
				case RendererTag.TagOption.AddOutline:
					rTag.gameObject.AddComponent<Outline> ();
					break;
				case RendererTag.TagOption.AddShadow:
					rTag.gameObject.AddComponent<Shadow> ();
					break;
				}

				if (rTag.componentsToDestroy.Count > 0) {
					foreach (string str in rTag.componentsToDestroy) {
						Component c = rTag.GetComponent (str);
						if (c != null)
							DestroyImmediate (c);
					}
				}
			}

			Canvas.ForceUpdateCanvases ();
		}

		private Vector2 SafeSize (Vector2 size, float safeSize)
		{
			if (size.x > safeSize) {
				float scale = safeSize / size.x;
				size.x = safeSize;
				size.y *= scale;
			}
			if (size.y > safeSize) {
				float scale = safeSize / size.y;
				size.y = safeSize;
				size.x *= scale;
			}
			return size;
		}

		public enum Dimension
		{
			Width,
			Height
		}

		public class RectTransformMemory
		{
			private Vector2 anchorMin, anchorMax, offsetMin, offsetMax, pivot, sizeDelta;
			private Vector3 localPosition, localScale;
			private Transform parent;
			private int sibID;
			private RectTransform rt;

			public RectTransformMemory (RectTransform rt)
			{
				this.rt = rt;
				anchorMin = rt.anchorMin;
				anchorMax = rt.anchorMax;
				offsetMin = rt.offsetMin;
				offsetMax = rt.offsetMax;
				pivot = rt.pivot;
				sizeDelta = rt.sizeDelta;
				parent = rt.transform.parent;
				sibID = rt.transform.GetSiblingIndex ();
				localPosition = rt.transform.localPosition;
				localScale = rt.localScale;
			}

			public void Return ()
			{
				rt.transform.SetParent (parent);
				rt.transform.SetSiblingIndex (sibID);
				rt.transform.localPosition = localPosition;
				rt.anchorMin = anchorMin;
				rt.anchorMax = anchorMax;
				rt.pivot = pivot;
				rt.sizeDelta = sizeDelta;
				rt.localScale = localScale;
				rt.offsetMin = offsetMin;
				rt.offsetMax = offsetMax;
			}
		}
	}

	public class RenderCanvasOption
	{
		public string name;
		public Option option;

		public RenderCanvasOption (string name, Option option)
		{
			this.name = name;
			this.option = option;
		}

		public enum Option
		{
			white = 1,
			destroyGameObject = 2
		}
	}
}
