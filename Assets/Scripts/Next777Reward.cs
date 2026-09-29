using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Next777Reward : View
{
    static private Next777Reward instance;

    static public Next777Reward Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(Next777Reward))[0] as Next777Reward;

            return instance;
        }
    }

    public Text reward_text;
    public Text total_text;
    
    public void Enable(int phase, int stars)
    {
        Enable();
        reward_text.text = stars.ToString();
        total_text.text = TextTranslationModule.GetWord("Total: ") +  UserController.Instance.GetStars().ToString();
    }
    
}
