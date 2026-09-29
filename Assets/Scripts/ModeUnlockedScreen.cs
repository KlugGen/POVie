using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeUnlockedScreen : View
{

    static private ModeUnlockedScreen instance;

    static public ModeUnlockedScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(ModeUnlockedScreen))[0] as ModeUnlockedScreen;

            return instance;
        }
    }

    public Text message;
    private int unlockedMode = 0;


    [NaughtyAttributes.Button("E1")]
    public void EngagingUnlocked()
    {
        unlockedMode = 1;
        Enable();
        //message.text = TextTranslationModule.GetWord("Nice!<br><br>Do you know that <b><size=32>Engaging</size></b> mode is available to you.<br><br>Do you want to try it now?");

        message.text = string.Format("{0}\n\n{1}", TextTranslationModule.GetWord("Brilliant!<br><br>Engaging Mode is available to you"), TextTranslationModule.GetWord("Do you want to try it now?"));
    }

    [NaughtyAttributes.Button("E2")]
    public void RelaxingUnlocked()
    {
        unlockedMode = 3;
        Enable();
        //message.text = TextTranslationModule.GetWord("Great!<br><br>Do you know that <b><size=32>Relaxing</size></b> mode is available to you.<br><br>Do you want to try it now?");

        message.text = string.Format("{0}\n\n{1}", TextTranslationModule.GetWord("Nice!<br><br>Relaxing Mode is available to you"), TextTranslationModule.GetWord("Do you want to try it now?"));
    }

    [NaughtyAttributes.Button("E3")]
    public void EntertainingUnlocked()
    {
        unlockedMode = 2;
        Enable();

        message.text = string.Format("{0}\n\n{1}", TextTranslationModule.GetWord("Great!<br><br>Entertaining Mode is available to you"), TextTranslationModule.GetWord("Do you want to try it now?"));        
    }

    public void Yes()
    {      
        SettingsView.Instance.SetModeExternaly(unlockedMode);
        Disable();
    }

    public void No()
    {
        Disable();
    }
}
