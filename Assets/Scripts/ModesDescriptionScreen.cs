using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModesDescriptionScreen : View
{
    static private ModesDescriptionScreen instance;

    static public ModesDescriptionScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(ModesDescriptionScreen))[0] as ModesDescriptionScreen;

            return instance;
        }
    }
    
    public Text message;

    [NaughtyAttributes.Button("E1")]
    public void Engaging()
    {       
        Enable();

        message.text = TextTranslationModule.GetWord("Focus on Strategy!<br><br>Your reward is doubled for completing the level in as few moves as possible");

        //message.text = TextTranslationModule.GetWord("Prepare for the challenge! <br><br>Plan your next move carefully. In this mode you will be rewarded for the flawless approach. <br><br>But beware! To many mistakes or usage of a hint can rule out your stars bonus for the levels.");
    }

    [NaughtyAttributes.Button("E2")]
    public void Relaxing()
    {
        // Focus on Visualisations!<br><br>Your reward is multiplied for completing letters out of angles
        // Focus on Visualisations!<br><br>Your reward is multiplied for completing magenta coloured sequence in one go

        Enable();

        message.text = TextTranslationModule.GetWord("Focus on Visualisations!<br><br>Your reward is multiplied for completing letters out of angles");

        //message.text = TextTranslationModule.GetWord("Sit down and relax. <br><br>You will be rewarded for your visualisation skills in the first place. Double up the reward by creating the magenta coloured sequence in one go.<br><br>Have fun!");
    }

    [NaughtyAttributes.Button("E3")]
    public void Entertaining()
    {
              Enable();
        message.text = TextTranslationModule.GetWord("Focus on Speed!<br><br>Your reward is doubled for being fast and precise");

        //message.text = TextTranslationModule.GetWord("Want to have some chill out? <br><br>Don’t worry about the stars as in this mode your reward will be doubled. <br><br>Enjoy!");
    }
}