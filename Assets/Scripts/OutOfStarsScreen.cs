using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfStarsScreen : View
{
    static private OutOfStarsScreen instance;

    static public OutOfStarsScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(OutOfStarsScreen))[0] as OutOfStarsScreen;

            return instance;
        }
    }
}
