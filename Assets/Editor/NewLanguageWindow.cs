using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class NewLanguageWindow : EditorWindow
{
     static public void Init()
    {
        serializedObjectFound = false;
        GetWindow(typeof(NewLanguageWindow));
    }

    public static Elements.Cities choosenCityObject = null;
    public static bool serializedObjectFound = false;

    public static string choosenCountry = "", choosenCity = "", languageName = "";
    [SerializeField] List<LetterToMake> lettersToMake = new List<LetterToMake>();


    void OnGUI()
    {
        GUILayout.BeginVertical("box");
        NewLanguageDialog();
        GUILayout.EndVertical();
    }

    SerializedObject serializedObject;

    private void NewLanguageDialog()
    {
        if (choosenCity != null && choosenCity != "")
        {
            GUILayout.Label(string.Format("Add new language for {0} [{1}]", choosenCity,choosenCountry) );
            languageName = GUILayout.TextField(languageName);

            if(serializedObjectFound == false)
            {
                serializedObject = new SerializedObject(this);
                serializedObjectFound = true;
            }
          
            var property = serializedObject.FindProperty("lettersToMake");
            serializedObject.Update();
            EditorGUILayout.PropertyField(property, true);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("+ Add langauge"))
            {
                GamePlayCreator.Instance.AddLanguageToCity(languageName, choosenCityObject, lettersToMake);
                languageName = "";
            }

            EditorGUILayout.Separator();
        }


    }
}
#endif