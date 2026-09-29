using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStepI : Toturial
{
    static private TutorialStepI instance;

    static public TutorialStepI Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(TutorialStepI))[0] as TutorialStepI;

            return instance;
        }
    }

    Elements.Cities lastCity;

    private bool alreadyPlayed = false;

    public void Enable(Elements.Cities city)
    {
        lastCity = city;

        if (alreadyPlayed)
            GamePlayController.Instance.Enable(lastCity);
        else
        {
            base.Enable();
            alreadyPlayed = true;
        }
    }

    public void EnableCity() {
        GamePlayController.Instance.Enable(lastCity);
    }
}
