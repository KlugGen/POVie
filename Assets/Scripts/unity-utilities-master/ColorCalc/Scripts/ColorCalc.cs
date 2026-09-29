using UnityEngine;
using System.Collections;

/// <summary>
/// Calculations on colors.
/// </summary>
public class ColorCalc : MonoBehaviour
{

	/// <summary>
	/// Get 0-255 value from 0-1 value.
	/// </summary>
	/// <param name="v">Value 0-1.</param>
	static public int ToDec (float v)
	{
		return (int)(v * 255);
	}

	/// <summary>
	/// Get 0-1 normalized value from 0-255 value.
	/// </summary>
	/// <param name="v">Value 0-255.</param>
	static public float ToNorm (int v)
	{
		return ((float)v) / 255;
	}

	/// <summary>
	/// Get new brightness using color in middle brightness.
	/// </summary>
	/// <param name="c">Color in middle brightness.</param>
	/// <param name="v">New brightness in 0-2 range.</param>
	static public Color GetColorWithNewBrightness (Color c, float v)
	{
		Color color;
		float scale, val = v * 2;

		if (val > 1) {

			scale = 2 - val;

			float r = 1 - ((1 - c.r) * scale);
			float g = 1 - ((1 - c.g) * scale);
			float b = 1 - ((1 - c.b) * scale);

			color = new Color (r, g, b);

		} else {

			color = c;

			color.r *= val;
			color.g *= val;
			color.b *= val;
		}

		return color;
	}

	/// <summary>
	/// Get color brightness (0-1 range).
	/// </summary>
	/// <param name="color">Color.</param>
	static public float GetBrightness (Color color)
	{
		float val;

		Vector2 mm = GetMaxMin (color);
		val = (mm.x + mm.y) / 2;

		return val;
	}

	/// <summary>
	/// Get HSV color values.
	/// </summary>
	/// <param name="color">Color.</param>
	static public Vector3 GetHSV (Color color)
	{
		int state;
		float d, h = 0, s = 0, v;
		Vector2 mm = GetMaxMin (color, out state);

		d = mm.x - mm.y;

		v = (mm.x + mm.y) / 2;

		if (d != 0) {
			switch (state) {
			case 0:
				h = ((color.g - color.b) / d) % 6;
				if (h <= 0)
					h += 6;
				break;

			case 1:
				h = (color.b - color.r) / d + 2;
				break;

			case 2:
				h = (color.r - color.g) / d + 4;
				break;
			}

			s = d / (1 - Mathf.Abs (2 * v - 1));
		}

		return new Vector3 (h, s, v);
	}

	/// <summary>
	/// Get minimium and maximum values of color. Vector2 (x = max, y = min).
	/// </summary>
	/// <param name="color">Color.</param>
	static public Vector2 GetMaxMin (Color color)
	{
		float max;
		float min;

		if (color.r > color.g) {
			if (color.r > color.b) {
				max = color.r;
			} else {
				max = color.b;
			}

			if (color.g < color.b)
				min = color.g;
			else
				min = color.b;
		} else if (color.b > color.g) {

			max = color.b;

			if (color.g < color.r)
				min = color.g;
			else
				min = color.r;

		} else {

			max = color.g;

			if (color.b < color.r)
				min = color.b;
			else
				min = color.r;
		}

		return new Vector2 (max, min);
	}

	/// <summary>
	/// Get minimium and maximum values of color. Vector2 (x = max, y = min).
	/// </summary>
	/// <param name="color">Color.</param>
	/// <param name="which">Tells which component is the biggest. 0-red, 1-green, 2-blue.</param>
	static public Vector2 GetMaxMin (Color color, out int which)
	{
		float max;
		float min;

		if (color.r > color.g) {
			if (color.r > color.b) {
				which = 0;
				max = color.r;
			} else {
				which = 2;
				max = color.b;
			}

			if (color.g < color.b)
				min = color.g;
			else
				min = color.b;
		} else if (color.b > color.g) {

			which = 2;
			max = color.b;

			if (color.g < color.r)
				min = color.g;
			else
				min = color.r;

		} else {

			which = 1;
			max = color.g;

			if (color.b < color.r)
				min = color.b;
			else
				min = color.r;
		}

		return new Vector2 (max, min);
	}
}
