using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
public class TranslationsEditor : EditorWindow
{
    static Dictionary<string, TranslationComplete> translationComplete = new Dictionary<string, TranslationComplete>();

    private static List<TranslationComplete> tranlsationsList = new List<TranslationComplete>();

    static GUIStyle entriesWithBR;

    static string search = "";
    [MenuItem("Window/Translations")]
    static void Init()
    {
        entriesWithBR = new GUIStyle(EditorStyles.textArea);
        entriesWithBR.normal.textColor = Color.green;     

        search = "";
        newTranslation = new TranslationComplete();
        GetWindow(typeof(TranslationsEditor), true, "Translations Editor");
        translationComplete.Clear();

        JSONObject jo = new JSONObject(TextTranslationModule.Instance.textTranslation.text);

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
                        tc.english = english;
                        tc.chinese = chinese;
                        tc.german = germany;
                        tc.translations.Add("english", english);
                        tc.translations.Add("deutsch", germany);
                        tc.translations.Add("中文(香港)", chinese);

                        translationComplete.Add(word.ToLower(), tc);
                    }
                    else
                    {
                        word.LogDev("Already has: ");
                    }
                }
            }
        }

        tranlsationsList = new List<TranslationComplete>(translationComplete.Values.ToList<TranslationComplete>()).OrderBy(o => o.word).ToList();
    }

    Vector2 scrollPos;
    static TranslationComplete newTranslation;

    void OnGUI()
    { 
        GUILayout.BeginHorizontal(GUILayout.Width(GetWidth(100f)));
        GUILayout.Label("Search:", GUILayout.Width(GetWidth(10f)));
        search = GUILayout.TextField(search);
        EH();
       

        BH();
        GUILayout.Label("English", GUILayout.Width(GetWidth(50f)));
        GUILayout.Label("Chinese", GUILayout.Width(GetWidth(50f)));
        EH();

        BH();
        newTranslation.english = GUILayout.TextArea(newTranslation.english, GUILayout.Width(GetWidth(50f)));
        newTranslation.chinese = GUILayout.TextArea(newTranslation.chinese, GUILayout.Width(GetWidth(50f)));
        EH();


        BH();
        if (GUILayout.Button("Add new"))
        {
            AddEntry();
        }
        EH();

       
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(GetWidth(100f)));
        GUILayout.BeginVertical();   
        foreach (TranslationComplete tc in tranlsationsList.FindAll(o => o.english.ToLower().Contains(search.ToLower())))
        {
            BH();

            if(tc.english.Contains("<br"))
                tc.english = GUILayout.TextArea(tc.english, entriesWithBR, GUILayout.Width(GetWidth(48f)));
            else
                tc.english = GUILayout.TextArea(tc.english, GUILayout.Width(GetWidth(48f)));

            tc.chinese = GUILayout.TextArea(tc.chinese, GUILayout.Width(GetWidth(48f)));

            EH();
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
      

        BH();
        if (GUILayout.Button("Save"))
        {
            SaveJson();
        }
        EH();

    }

    void BH()
    {
        GUILayout.BeginHorizontal();
    }

    void EH()
    {
        GUILayout.EndHorizontal();
    }

    private void AddEntry()
    {
        if (newTranslation.english == "")
            return;

        if (!translationComplete.ContainsKey(newTranslation.english))
        {
            newTranslation.word = newTranslation.english;

            TranslationComplete tc = new TranslationComplete() {
                word = newTranslation.word,
                english = newTranslation.english,
                german = newTranslation.german,
                chinese = newTranslation.chinese
            };

            translationComplete.Add(tc.word, tc);
            tranlsationsList = new List<TranslationComplete>(translationComplete.Values.ToList<TranslationComplete>()).OrderBy(o => o.word).ToList();
            newTranslation.word = newTranslation.english = newTranslation.chinese = "";
        }
        else
        {
            "already has key".LogDev();
        }
    }

    private void SaveJson()
    {
        JSONObject jo = new JSONObject();

        foreach (TranslationComplete tc in tranlsationsList)
        {
            JSONObject j = new JSONObject();
            j.Add(tc.english);
            j.Add(tc.german);
            j.Add(tc.chinese);
            jo.Add(j);
        }

        jo.ToString().LogDev();
        //string s = TextTranslationModule.Instance.textTranslation.text;
        //s = s.Replace("\n", "");
        //s.Length.LogDev("L1: ");
        //jo.ToString().Length.LogDev("L2: ");
        //TextTranslationModule.Instance.textTranslation.text.CompareTo(jo.ToString()).LogDev("Compare: ");

        //string p1 = Path.Combine(Application.persistentDataPath, "1.json");
        //string p2 = Path.Combine(Application.persistentDataPath, "2.json");

        File.WriteAllText(AssetDatabase.GetAssetPath(TextTranslationModule.Instance.textTranslation), jo.ToString());
        EditorUtility.SetDirty(TextTranslationModule.Instance.textTranslation);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(TextTranslationModule.Instance.gameObject.scene);

        //DKK.IO.SaveToFile(p1, jo.ToString());
        //DKK.IO.SaveToFile(p2, s);

    }

    private float GetWidth(float percnetage)
    {
        return (position.width * percnetage) / 100f;
    }

    public class TranslationComplete
    {
        public string word = "", english = "", german = "", french = "", polish = "", czech = "", macedonian = "", portugal = "", chinese = "";
        public Dictionary<string, string> translations = new Dictionary<string, string>();
    }
}
#endif