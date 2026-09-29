using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquareLoaderUnit : MonoBehaviour
{
    public Image childImage;

    /// <summary>
    /// Change state of SquareUnit
    /// </summary>
    /// <param name="fullfilled"></param>
    public void SetState(bool fullfilled) {
        childImage.enabled = fullfilled;
    }
}
