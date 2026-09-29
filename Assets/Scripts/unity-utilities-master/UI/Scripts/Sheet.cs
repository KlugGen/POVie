using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DKK
{
	public class Sheet : MonoBehaviourDKK
	{
		public bool loadWhiteBackground = true;
		public RawImage img;
		public GameObject sheet;
		protected Texture2D tex;

		public Texture2D Texture {
			get {
				return tex;
			}
		}

		protected float scaleFactor;

		public float ScaleFactor {
			get {
				return scaleFactor;
			}
		}

		protected void OpenSelf (Texture2D texture, bool createNewTexture = true)
		{
			if (tex != null)
				Destroy (tex);

			if (createNewTexture) {
				tex = new Texture2D (texture.width, texture.height);
				tex.wrapMode = TextureWrapMode.Clamp;
				Color32[] colors = texture.GetPixels32 ();

				if (loadWhiteBackground)
					for (int i = 0; i < colors.Length; i++) {
						float alpha = (float)colors [i].a / 255f;
						float background = 255f * (1 - alpha);
						colors [i].r = (byte)(alpha * (float)colors [i].r + background);
						colors [i].g = (byte)(alpha * (float)colors [i].g + background);
						colors [i].b = (byte)(alpha * (float)colors [i].b + background);
						colors [i].a = 255;
					}

				tex.SetPixels32 (colors);
				tex.Apply ();
			} else
				tex = texture;

			Run ();
		}

		protected void OpenSelf (int width, int height)
		{
			if (tex != null)
				Destroy (tex);
			tex = new Texture2D (width, height);
			tex.wrapMode = TextureWrapMode.Clamp;
			Color32[] whiteColors = new Color32[width * height];
			Color32 white = new Color32 (255, 255, 255, 255);
			for (int i = 0; i < whiteColors.Length; i++)
				whiteColors [i] = white;
			tex.SetPixels32 (whiteColors);
			tex.Apply ();
			Run ();
		}

		protected void Run ()
		{
			gameObject.SetActive (true);

			RectTransform sheetRT = sheet.GetComponent<RectTransform> ();
			RectTransform sheetParentRT = sheet.transform.parent.GetComponent<RectTransform> ();
			Adjust (sheetRT, sheetParentRT);
			img.texture = tex;

			OnRun ();
		}

		public virtual void OnRun ()
		{

		}

		public void Adjust (RectTransform sheet, RectTransform sheetParent)
		{
			Vector2 sd = UICalc.MatchAspectUpTo (tex.width, tex.height, sheetParent.rect.size.x, sheetParent.rect.size.y);
			sheet.sizeDelta = sd;
			scaleFactor = sd.x / (float)tex.width;
		}
	}
}