using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TextTranslationModule : Singleton<TextTranslationModule>
{

    public TextAsset textTranslation;

    static Dictionary<int, Dictionary<string, string>> translations = new Dictionary<int, Dictionary<string, string>>();
    static Dictionary<string, TranslationComplete> translationComplete = new Dictionary<string, TranslationComplete>();

    // Start is called before the first frame update

    public void EnableForPreaload()
    {
        LoadDataFromJson();
        BuildDictionaries();
    }

    public void Initialize()
    {
        LoadDataFromJson();
        BuildDictionaries();


        string firstLanguage = "";
        //PlayerPrefs.DeleteAll();

        //Application.systemLanguage.Log();

        if (PlayerPrefs.HasKey("lastLang") == false)
        {
           // "No language saved.".Log();

            firstLanguage = FindMotherLanguage();
            if (firstLanguage == "")
                firstLanguage = "English";
        }
        else
        {
            firstLanguage = PlayerPrefs.GetString("lastLang", "English");
        }

        ChangeLanguage(firstLanguage);
        //currentDictionary = GetDictionaryByLanguage("German");
    }

    [NaughtyAttributes.Button("Mother language")]
    public string FindMotherLanguage()
    {
        string lang = "";
        switch (Application.systemLanguage) {
            case SystemLanguage.English:
                lang = "english";
                break;
            case SystemLanguage.German:
                lang = "deutsch";
                break;
            case SystemLanguage.Chinese:
                lang = "中文(香港)";
                break;
            case SystemLanguage.ChineseSimplified:
                lang = "中文(香港)";
                break;
            case SystemLanguage.ChineseTraditional:
                lang = "中文(香港)";
                break;
        }

        lang.Log("Language fit: ");
        return lang;
    }

    public void LoadDataFromJson()
    {
        translationComplete.Clear();

        JSONObject jo = new JSONObject(textTranslation.text);

        if (jo != null && jo.list != null)
        {
            foreach (var j in jo.list)
            {

                string word = "";
                string english = "", germany = "", chinese = "";

                if (j.IsArray)
                {
                    word = j[0].str;
                    english = j[0].str;
                    germany = j[1].str;
                    chinese = j[2].str;


                    if (!translationComplete.ContainsKey(word.ToLower()))
                    {
                        TranslationComplete tc = new TranslationComplete();
                        tc.word = word.ToLower();
                        tc.translations.Add("english", english);
                        tc.translations.Add("deutsch", germany);
                        tc.translations.Add("中文(香港)", chinese);

                        translationComplete.Add(word.ToLower(), tc);
                    }

                }

            }
        }
    }

    public void BuildDictionaries()
    {
        translations.Clear();

        Dictionary<string, string> c_ger = new Dictionary<string, string>();
        Dictionary<string, string> c_eng = new Dictionary<string, string>();
        Dictionary<string, string> c_chi = new Dictionary<string, string>();

        foreach (TranslationComplete t in translationComplete.Values)
        {
            c_ger.Add(t.word, t.translations["deutsch"]);
            c_eng.Add(t.word, t.translations["english"]);
            c_chi.Add(t.word, t.translations["中文(香港)"]);

        }

        translations.Add(0, c_eng);
        translations.Add(1, c_ger);
        translations.Add(2, c_chi);

    }

    public void InitializeSceneTexts()
    {
        Text[] texts = Resources.FindObjectsOfTypeAll(typeof(Text)) as Text[];
        List<Text> textsList = texts.ToList<Text>().FindAll(o => o.text.Length > 1);

        foreach (Text t in textsList)
        {
            LanguageTextUnit ltu = t.GetComponent<LanguageTextUnit>();
            if (ltu != null)
                ltu.RemoteInitialize();
        }
    }

    public Dictionary<string, TranslationComplete> pictogramsTranslations = new Dictionary<string, TranslationComplete>();
       
    static Dictionary<string, string> currentDictionary = new Dictionary<string, string>();
    public TextAsset inputJsonPictograms;
    static string currentLanguageString_temp = "";

    public void ChangeLanguageFromButton(string language)
    {
        TextTranslationModule.ChangeLanguage(language);
    }

    static public void ChangeLanguage()
    {
        string currentLanguageString = "English";

        //currentLanguageString = DKK.Lang.GetCurrentLanguage();
        currentLanguageString_temp = currentLanguageString;

        ChangeLanguage(currentLanguageString);
    }

    static public void ChangeLanguage(string language)
    {
        //language.Log("setting langauge to");
       
        PlayerPrefs.SetString("lastLang", language);

        //Debug.Log("Change language to: " + language);
        currentDictionary = GetDictionaryByLanguage(language);

        LanguageTextUnit[] allUnits = Resources.FindObjectsOfTypeAll(typeof(LanguageTextUnit)) as LanguageTextUnit[];
        List<LanguageTextUnit> allNotEmptyUnits = allUnits.ToList<LanguageTextUnit>().FindAll(o => o.field != null && o.field.text.Length > 0);

        //Debug.Log("Texts found: " + allNotEmptyUnits.Count);

        foreach (LanguageTextUnit t in allNotEmptyUnits)
        {
            string word = t.startText.ToLower();
            t.field.text = GetWord(word);
            continue;

            //if (currentDictionary.ContainsKey(word))
            //{
            //    //Debug.Log("Found word for:  " + word + " which is : " + currentDictionary[word]);

            //    t.field.text = currentDictionary[word];

            //    continue;
            //}

            //if (currentDictionary.ContainsValue(word))
            //{
            //    //Debug.LogWarning("Contains but value");
            //    t.field.text = word;

            //    continue;
            //}

        }
    }

    static private Dictionary<string, string> GetDictionaryByLanguage(string language)
    {
        switch (language.ToLower())
        {
            case "中文(香港)":
                return translations[2];

            case "english":
                return translations[0];

            case "deutsch":
                return translations[1];
            default:
                return translations[0];
        }
    }

    static public bool HasWord(string word)
    {
        // Debug.Log("Searching:  " + currentDictionary.Count);
        // foreach(string s in currentDictionary.Values){
        //     Debug.Log(s);
        // }

        // Debug.Log("Has word: " + word);
        if (currentDictionary.ContainsKey(word.ToLower()) || currentDictionary.ContainsValue(word.ToLower()))
        {
            return true;
        }

        return false;

    }

    private static string ProcessWord(string word) {
        
        word = word.Replace("<br>", "\n");
        return word;
    }

    static public string GetWord(string word)
    {
      
        if (currentDictionary.ContainsKey(word.ToLower()) && (word.Length>0 && currentDictionary[word.ToLower()].Length>0))
        {
            return ProcessWord(currentDictionary[word.ToLower()]);
        }

        if (currentDictionary.ContainsValue(word.ToLower()))
        {
            return ProcessWord(word);
        }

#if UNITY_EDITOR
        //word.LogDev("Missing: ");
        ReportMissing(word);
        return "_" + ProcessWord(word);
#else
        return ProcessWord(word);
#endif

    }
    static public void ReportMissing(string word)
    {
        string missings = PlayerPrefs.GetString("missingWords");
        JSONObject jo = new JSONObject(missings);

        JSONObject obj = new JSONObject(word);

        //word.Log("Missing: ");

        if(obj == null)
        {
            "Obj null".Log();
            return;
        }

        if(jo == null || !jo.IsArray)
        {
            "jo null".Log();
            jo.Add(word);
        }
        else
        {
            bool found = false;

            foreach(JSONObject j in jo.list)
            {
                if(j.str.ToLower() == word.ToLower())
                {
                    found = true;
                    //"Already has word".Log();
                    break;
                }               
            }

            if (!found)
            {
                jo.Add(word);
            }
          
        }

        //jo.Print(true).Log();

        DKK.IO.SaveToFile(Application.persistentDataPath + "\\missings", jo.ToString());

        PlayerPrefs.SetString("missingWords",jo.ToString());
    }


    static public string GetWordInLanguage(string word, string language)
    {
        Dictionary<string, string> tempDictionary = GetDictionaryByLanguage(language);

        if (tempDictionary.ContainsKey(word.ToLower()))
        {
            return tempDictionary[word.ToLower()];

        }

        if (tempDictionary.ContainsValue(word.ToLower()))
        {
            return word;
        }


        return word;
    }
}

public class TranslationComplete
{
    public string word = "", english = "", german = "", french = "", polish = "", czech = "", macedonian = "", portugal = "", chinese = "";
    public Dictionary<string, string> translations = new Dictionary<string, string>();
}

class TranslationPair
{
    public string word = "", translation = "", language;
}