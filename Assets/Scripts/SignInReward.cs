using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignInReward : View
{
    static private SignInReward instance;

    static public SignInReward Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(SignInReward))[0] as SignInReward;

            return instance;
        }
    }

    public Text points;

    public override void Enable()
    {
        base.Enable();
        points.text = Rules.GameBalance.logInReward.ToString();
    }
}
