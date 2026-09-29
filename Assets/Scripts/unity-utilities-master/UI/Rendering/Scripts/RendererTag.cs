using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DKK
{
	public class RendererTag : MonoBehaviour
	{
		public enum TagOption
		{
			None = -1,
			RenderIgnore = 0,
			AddOutline = 1,
			AddShadow = 2
		}

		public List<string> componentsToDestroy = new List<string> ();

		public TagOption option;
	}
}