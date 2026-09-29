using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace DKK
{
	public class UICalc : MonoBehaviour
	{
		/// <summary>
		/// Convert screen position to UI position.
		/// </summary>
		/// <param name="position">Screen position.</param>
		/// <param name="myCanvas">UI canvas.</param>
		static public Vector2 ScreenToUIposition (Vector2 position, Canvas myCanvas)
		{
			Vector2 pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle (myCanvas.transform as RectTransform, position, null /* myCanvas.worldCamera*/, out pos);
			return myCanvas.transform.TransformPoint (pos);
		}

		/// <summary>
		/// Convert UI position to screen position.
		/// </summary>
		/// <param name="rectTransform">RectTransform which position will be translated to screen position.</param>
		static public Vector2 UItoScreenPosition (RectTransform rectTransform, Camera cam = null)
		{
			return RectTransformUtility.WorldToScreenPoint (cam, rectTransform.position);
		}

		/// <summary>
		/// Convert UI position to screen position.
		/// </summary>
		/// <param name="position">Position on RectTransform which will be translated to screen position.</param>
		/// <param name="parentRectTransform">RectTransform on which the position is indicated.</param>
		static public Vector2 UItoScreenPosition (Vector2 position, RectTransform parentRectTransform)
		{
			Vector2 pos = UItoScreenPosition (parentRectTransform);
			pos.x -= parentRectTransform.rect.width / 2;
			pos.y -= parentRectTransform.rect.height / 2;
			pos += position;
			return pos;
		}

		/// <summary>
		/// Convert UI position to screen position.
		/// </summary>
		/// <param name="x">Position x on RectTransform which will be translated to screen position.</param>
		/// <param name="x">Position y on RectTransform which will be translated to screen position.</param>
		/// <param name="parentRectTransform">RectTransform on which the position is indicated.</param>
		static public Vector2 UItoScreenPosition (float x, float y, RectTransform parentRectTransform)
		{
			Vector2 pos = UItoScreenPosition (parentRectTransform);
			pos.x -= parentRectTransform.rect.width / 2;
			pos.y -= parentRectTransform.rect.height / 2;
			pos.x += x;
			pos.y += y;
			return pos;
		}

		/// <summary>
		/// Finding Canvas on which UI element is placed.
		/// </summary>
		/// <param name="ob">UI element.</param>
		static public Canvas FindMyCanvas (Transform trans)
		{
			do {
				Canvas cnvs = trans.GetComponent<Canvas> ();
				if (cnvs != null)
					return cnvs;
				trans = trans.parent;
			} while(trans != null);
			
//			if (ob.transform.parent != null)
//				return FindMyCanvas (ob.transform.parent.gameObject);
			
			return null;
		}

		//		static public Bounds CalculateBoundsWithChildren (RectTransform transform, float uiScaleFactor)
		//		{
		//			Bounds bounds = new Bounds (transform.position, new Vector3 (transform.rect.width, transform.rect.height, 0.0f) * uiScaleFactor);
		//
		//			if (transform.childCount > 0) {
		//				foreach (RectTransform child in transform) {
		//					Bounds childBounds = new Bounds (child.position, new Vector3 (child.rect.width, child.rect.height, 0.0f) * uiScaleFactor);
		//					bounds.Encapsulate (childBounds);
		//				}
		//			}
		//
		//			return bounds;
		//		}
		//
		//		static public Bounds CalculateBounds (RectTransform transform, float uiScaleFactor)
		//		{
		//			Bounds bounds = new Bounds (transform.position, new Vector3 (transform.rect.width, transform.rect.height, 0.0f) * uiScaleFactor);
		//			return bounds;
		//		}
		//
		/// <summary>
		/// Returns texture position on screen.
		/// </summary>
		/// <returns>The position on screen.</returns>
		/// <param name="image">Image.</param>
		static public Rect ImagePositionOnScreen (Image image)
		{
			if (image != null && image.sprite != null && image.sprite.texture != null) {
				Rect rect = RectTransformToScreenSpace (image.GetComponent<RectTransform> ());
				if (image.preserveAspect) {
					Vector2 size = MatchAspectUpTo (image.sprite.texture.width, image.sprite.texture.height, rect.width, rect.height);
					float cutW = rect.width - size.x;
					float cutH = rect.height - size.y;
					float marginX = (rect.width - size.x) / 2;
					float marginY = (rect.height - size.y) / 2;

					rect.width -= cutW;
					rect.height -= cutH;
					rect.x += marginX;
					rect.y += marginY;
				}

				return rect;
			} else {
				Debug.LogError ("Image is incorrect. Check if texture is not missing.");
			}

			return new Rect ();
		}

		static public Rect ImagePositionOnScreen (RawImage image)
		{
			if (image != null && image.texture != null) {
				Rect rect = RectTransformToScreenSpace (image.GetComponent<RectTransform> ());
				Vector2 size = MatchAspectUpTo (image.texture.width, image.texture.height, rect.width, rect.height);
				float cutW = rect.width - size.x;
				float cutH = rect.height - size.y;
				float marginX = (rect.width - size.x) / 2;
				float marginY = (rect.height - size.y) / 2;

				rect.width -= cutW;
				rect.height -= cutH;
				rect.x += marginX;
				rect.y += marginY;

				return rect;
			} else {
				Debug.LogError ("Image is incorrect. Check if texture is not missing.");
			}

			return new Rect ();
		}

		static public Bounds GetUIBounds (RectTransform uiElement, Canvas canvas)
		{
			Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds (canvas.transform, uiElement.transform);
			Vector3 p = UICalc.UItoScreenPosition (canvas.GetComponent<RectTransform> ());
			bounds.SetMinMax (p + bounds.min * canvas.scaleFactor, p + bounds.max * canvas.scaleFactor);
			return bounds;
		}

		static public Vector2 ImagePixel (Vector2 position, Image image)
		{
			return ImagePixel (position, image, ImagePositionOnScreen (image));
		}

		static public Vector2 ImagePixel (Vector2 position, RawImage image)
		{
			return ImagePixel (position, image, ImagePositionOnScreen (image));
		}

		static public Vector2 ImagePixel (Vector2 position, Image image, Rect imageRectOnScreen)
		{
			position.x -= imageRectOnScreen.x;
			position.y -= imageRectOnScreen.y;

			position.x /= imageRectOnScreen.width;
			position.y /= imageRectOnScreen.height;

			position.x *= image.sprite.texture.width;
			position.y *= image.sprite.texture.height;

			position.x = Mathf.Clamp (position.x, 1, image.sprite.texture.width);
			position.y = Mathf.Clamp (position.y, 1, image.sprite.texture.height);

			return position;
		}

		static public Vector2 ImagePixel (Vector2 position, RawImage image, Rect imageRectOnScreen)
		{
			position.x -= imageRectOnScreen.x;
			position.y -= imageRectOnScreen.y;

			position.x /= imageRectOnScreen.width;
			position.y /= imageRectOnScreen.height;

			position.x *= image.texture.width;
			position.y *= image.texture.height;

			position.x = Mathf.Clamp (position.x, 1, image.texture.width);
			position.y = Mathf.Clamp (position.y, 1, image.texture.height);

			return position;
		}

		public static Rect RectTransformToScreenSpace (RectTransform transform)
		{
			Vector2 size = Vector2.Scale (transform.rect.size, transform.lossyScale);
			return new Rect ((Vector2)transform.position - (size * 0.5f), size);
		}

		static public Vector2 MatchAspectUpTo (float w, float h, float wMax, float hMAx)
		{
			w = Mathf.Abs (w);
			h = Mathf.Abs (h);

			if (w / wMax > h / hMAx) {
				float s = wMax / w;
				w = wMax;
				h *= s;
			} else {
				float s = hMAx / h;
				h = hMAx;
				w *= s;
			}

			return new Vector2 (w, h);
		}

		static public Vector2 MatchAspectUpToWidth (float w, float h, float wMax)
		{
			w = Mathf.Abs (w);
			h = Mathf.Abs (h);

			float s = wMax / w;
			w = wMax;
			h *= s;

			return new Vector2 (w, h);
		}

		static public Vector2 MatchAspectUpToHeight (float w, float h, float hMAx)
		{
			w = Mathf.Abs (w);
			h = Mathf.Abs (h);

			float s = hMAx / h;
			h = hMAx;
			w *= s;

			return new Vector2 (w, h);
		}

		static public void TransLocalPosToPivot(RectTransform rt)
        {
			Vector2 shift = new Vector2(rt.localPosition.x / rt.localScale.x, rt.localPosition.y / rt.localScale.y);

			float w = rt.rect.width;
			float h = rt.rect.height;

			Vector2 currentPivot = rt.pivot;
			float currenX = currentPivot.x * w;
			float currentY = currentPivot.y * h;

			float newX = currenX - shift.x;
			float newY = currentY - shift.y;

			Vector2 newPivot = new Vector2(newX / w, newY / h);


			rt.pivot = newPivot;
			rt.localPosition = Vector3.zero;

			//Debug.Log(newPivot);
			//rt.pivot = pivotShift;
        }
	}
}