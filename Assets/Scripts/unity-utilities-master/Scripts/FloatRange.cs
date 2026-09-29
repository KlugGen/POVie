using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatRanges
{
	private List<FloatRange> ranges = new List<FloatRange> ();

	public void AddRange (FloatRange fr)
	{
		FloatRange newFr = null;
		List<int> mergedIds = new List<int> ();
		int inserID = 0;
		for (int i = 0; i < ranges.Count; i++) {

			if (newFr != null) {

				FloatRange nfr = Merge (ranges [i], newFr);
				if (nfr == null)
					break;
				else {
					mergedIds.Add (i);
					newFr = nfr;
				}

			} else {
				int r = ranges [i].Compare (fr);
				switch (r) {
				case -1:
					inserID = i;
					break;
				case 0:
					inserID = i + 1;
					break;
				default:
					newFr = ForceMerge (ranges [i], fr);
					inserID = i;
					mergedIds.Add (i);
					fr = newFr;
					break;
				}
			}
		}

		if (newFr != null) {
			for (int i = mergedIds.Count - 1; i >= 0; i--) {
				ranges.RemoveAt (mergedIds [i]);
			}
			ranges.Insert (inserID, newFr);
		} else {
			ranges.Insert (inserID, fr);
		}
	}

	static public FloatRange Merge (FloatRange fr0, FloatRange fr1)
	{
		if (fr0.Compare (fr1) > 0) {
			return ForceMerge (fr0, fr1);
		}
		return null;
	}

	static public FloatRange ForceMerge (FloatRange fr0, FloatRange fr1)
	{
		float start;
		bool startIncluded;
		if (fr0.start < fr1.start) {
			start = fr0.start;
			startIncluded = fr0.startIncluded;
		} else {
			start = fr1.start;
			startIncluded = fr1.startIncluded;
		}

		float end;
		bool endIncluded;
		if (fr0.end > fr1.end) {
			end = fr0.end;
			endIncluded = fr0.endIncluded;
		} else {
			end = fr1.end;
			endIncluded = fr1.endIncluded;
		}

		FloatRange fr = new FloatRange (start, startIncluded, end, endIncluded);
		return fr;
	}
}

public class FloatRange
{
	public float start, end;
	public bool startIncluded, endIncluded;

	public FloatRange (float start, bool startIncluded, float end, bool endIncluded)
	{
		this.start = start;
		this.startIncluded = startIncluded;
		this.end = end;
		this.endIncluded = endIncluded;
	}

	/// <summary>
	/// Check the collision.
	/// </summary>
	/// <returns>Returns:
	/// 1 - are the same
	/// 2 - fr is in range
	/// 3 - fr starts in range
	/// 4 - fr ends in range 
	/// 0 - fr is higher than range
	/// -1 - fr is lower than range</returns>
	/// <param name="fr">Fr.</param>
	public int Compare (FloatRange fr)
	{
		if (start == fr.start && startIncluded == fr.startIncluded && end == fr.end && endIncluded == fr.endIncluded)
			return 1;

		bool startIsIn = (fr.start > start && fr.start < end) || (fr.startIncluded && ((startIncluded && fr.start == start) || (endIncluded && fr.start == end)));

		bool endIsIn = (fr.end > start && fr.end < end) || (fr.endIncluded && ((startIncluded && fr.end == start) || (endIncluded && fr.end == end)));

		if (startIsIn && endIsIn)
			return 2;
		if (startIsIn)
			return 3;
		if (endIsIn)
			return 4;
				
		if (fr.end < start)
			return -1;
		else
			return 0;
	}

	public bool IsInRange (float value)
	{
		return (value > start && value < end) || (startIncluded && value == start) || (endIncluded && value == end);
	}

	public bool IsHigher (float value)
	{
		return (!endIncluded && value >= end) || (value > end);
	}

	public bool IsLower (float value)
	{
		return (!startIncluded && value <= start) || (value < start);
	}
}
