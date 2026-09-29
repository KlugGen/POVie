using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniversalInfoPopUp : View
{
    static private UniversalInfoPopUp instance;

    static public UniversalInfoPopUp Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(UniversalInfoPopUp))[0] as UniversalInfoPopUp;

            return instance;
        }
    }

    public Text textMessage, titleMessage;

    public void Enable(string title, string message)
    {
        titleMessage.text = title;
        textMessage.text = message;
        Enable();
    }
}
