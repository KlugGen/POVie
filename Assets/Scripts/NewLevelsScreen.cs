using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewLevelsScreen : View
{
    static private NewLevelsScreen instance;

    static public NewLevelsScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(NewLevelsScreen))[0] as NewLevelsScreen;

            return instance;
        }
    }

    public override void Disable()
    {
        base.Disable();

        Elements.ElementsDatabase.Instance.allCities.ForEach(o => o.wasInSaves = true);
        UserController.Instance.SaveGame();
        UserController.Instance.hasNewCities = false;
    }
}
