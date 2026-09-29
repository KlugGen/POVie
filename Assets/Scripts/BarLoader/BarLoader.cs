using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarLoader : Loader
{
    public RectTransform loaderBar;

    public override void SetPercentage(int percentage)
    {         
        float parentWidth = loaderBar.parent.GetComponent<RectTransform>().rect.width;
        loaderBar.sizeDelta = new Vector2(parentWidth * ((float)percentage / 100f), loaderBar.sizeDelta.y); 
    }

    //[UnityEditor.Callbacks.DidReloadScripts]
    //private static void OnScriptsReloaded()
    //{
    //    // do something
    //    Debug.Log("bar");   

    //} 
}