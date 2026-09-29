using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplayImpossibleScreen : View
{
    static private ReplayImpossibleScreen instance;

    static public ReplayImpossibleScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(ReplayImpossibleScreen))[0] as ReplayImpossibleScreen;

            return instance;
        }
    }

    public GameObject replayScreen, restartScreen;
    
    public override void Enable()
    {
        base.Enable();
        replayScreen.SetActive(true);
    }
    public Text restartText;

    public void EnableRestartScreen()
    {
        base.Enable();
        restartScreen.SetActive(true);
        restartText.text = string.Format(TextTranslationModule.GetWord("Come back shortly with a new gameplan or use {0} to get immediate access for the level again"), "\u2605");
    }

    public override void Disable()
    {
        base.Disable();
        replayScreen.SetActive(false);
        restartScreen.SetActive(false);
    }

    public void GetTextForRestart(UnityEngine.Events.UnityAction<string> action)
    {
        action(string.Format(TextTranslationModule.GetWord("Come back shortly with a new gameplan or use {0} to get immediate access for the level again"), "\u2605"));
    }
}
