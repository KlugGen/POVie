using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarRewardScreen :  View
{
    static private StarRewardScreen instance;

    static public StarRewardScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(StarRewardScreen))[0] as StarRewardScreen;

            return instance;
        }
    }

    public Text points_text;
    public Text information_text;
    public Text balance_text;

    public void Enable(string information, int points_int)
    {
        base.Enable();
        points_text.text = points_int.ToString();
        information_text.text = information;
        balance_text.text = string.Format("{0}: {1}",TextTranslationModule.GetWord("Total"), UserController.Instance.GetStars().ToString());
    }
}
