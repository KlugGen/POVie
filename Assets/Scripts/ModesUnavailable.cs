using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModesUnavailable : View
{
    static private ModesUnavailable instance;

    static public ModesUnavailable Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(ModesUnavailable))[0] as ModesUnavailable;

            return instance;
        }
    }

    public override void Enable()
    {
        base.Enable();
        //"Enabling view".LogDev();
    }
}
