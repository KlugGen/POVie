using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DKK;

[RequireComponent(typeof(Text))]
public class LanguageTextUnit : MonoBehaviour
{
    public string startText = "";

    public string format = "{0}";
    public Text field;
    public List<LanguageTextUnitParam> parameters = new List<LanguageTextUnitParam>();

    public bool debugMode = false;
    public bool getStringFromMethod = false;

    public MyStringEvent stringEvent;

    public enum style
    {
        NORMAL = 1,
        FUPPER = 2,
        UPPER = 3
    };

    public style textStyle = style.NORMAL;

    //private bool isInitialized = false;

    public void RemoteInitialize()
    {
        field = GetComponent<Text>();

        if (getStringFromMethod)
        {          
            stringEvent.Invoke(ExternalTextMethod);
        }

        startText = field.text;
        //isInitialized = true;
    }

    IEnumerator Start()
    {
        if (debugMode)
        {
            gameObject.name.Log("field initializa START");
        }

        if (InitializeField() == false)
        {
            if (debugMode)
            {
                "field initializa failed".LogDev();
            }
            yield break;
        }

        yield return null;

        if (!string.IsNullOrEmpty(startText))
        {
            if (debugMode)
            {
                startText.LogDev("Start Text: ");
                TextTranslationModule.GetWord(startText).LogDev("Translation: ");
            }
          
            field.text = formatText(TextTranslationModule.GetWord(startText));
        }
        else
        {
            if (debugMode)
            {
                "startText empty!".LogDev();
            }
        }
    }

    private bool InitializeField()
    {
        field = GetComponent<Text>();

        if (getStringFromMethod)
        {
            stringEvent.Invoke(ExternalTextMethod);
        }

        if (field == null)
        {
            return false;
        }
        else
            startText = field.text;

        return true;
    }

    private string formatText(string text)
    {
        switch (textStyle)
        {
            case style.NORMAL:
                return text;
            case style.UPPER:
                return text.ToUpper();     
            case style.FUPPER:
                return text.FirstUpper();
        }

        return text;
    }

    public void ActualizeStartText()
    {
        if (field == null)
            InitializeField();
        
        startText = field.text;
    }

    public void Reload()
    {
        if (!string.IsNullOrEmpty(startText))
        {
            string temp = formatText(TextTranslationModule.GetWord(startText));
            field.text = temp;
            return;
        }
    }

    private void ExternalTextMethod(string s)
    {
        field.text = s;
        startText = field.text;
    }

    //bool forcedTranslation = false;

    public void ForceTranslation()
    {
        //forcedTranslation = true;
    }

}

[System.Serializable]
public class LanguageTextUnitParam
{
    public string key;
    // public Lang.CaseType caseType = Lang.CaseType.original;

    // public LanguageTextUnitParam (string key, Lang.CaseType caseType = Lang.CaseType.original)
    // {
    // 	this.key = key;
    // 	this.caseType = caseType;
    // }
}

[System.Serializable]
public class MyStringEvent : UnityEngine.Events.UnityEvent<UnityEngine.Events.UnityAction<string>>
{
}