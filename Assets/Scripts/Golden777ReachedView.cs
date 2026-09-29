using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golden777ReachedView : View
{

    static private Golden777ReachedView instance;

    static public Golden777ReachedView Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(Golden777ReachedView))[0] as Golden777ReachedView;

            return instance;
        }
    }

    public string wwwUrl = "";
    public string mailUrl = "";
    
    public void ButtonWWW()
    {
        OpenWWW();
        Disable();
    }

    public void ButtonMail()
    {
        OpenMail();
        Disable();
    }

    public void OpenWWW()
    {
        Application.OpenURL(wwwUrl);
    }

    public void OpenMail()
    {
        Application.OpenURL("mailto:" + MyEscapeURL(mailUrl));
    }

    string MyEscapeURL(string url)
    {
        return UnityEngine.Networking.UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }

    public void GoToScoreboard()
    {
        Disable();
        MapViewController.Instance.Disable();
        ScoreboardView.Instance.Enable();
    }
}
