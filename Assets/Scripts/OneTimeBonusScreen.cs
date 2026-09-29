using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OneTimeBonusScreen : View
{
    static private OneTimeBonusScreen instance;

    static public OneTimeBonusScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(OneTimeBonusScreen))[0] as OneTimeBonusScreen;

            return instance;
        }
    }

    public Text message_text;
    public Text numer_text;

    public void Enable(int bonus)
    {
        Enable();
        message_text.text = string.Format(TextTranslationModule.GetWord("Your {0} bonus"), "\u2605");
        numer_text.text = bonus.ToString();
    }
}
