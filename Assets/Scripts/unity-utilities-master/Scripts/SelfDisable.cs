using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDisable : MonoBehaviour
{
	public void Disable ()
	{
		gameObject.SetActive (false);
	}
}
