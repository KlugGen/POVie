using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class NewCityWindow : EditorWindow
{
    [MenuItem("Window/New city")]
    static public void Init()
    {
        serializedObjectFound = false;
        GetWindow(typeof(NewCityWindow));  
    }

    public static string choosenCountry = "", choosenCity = "",newCityName  = "";
    Sprite sprite;
    
    //[SerializeField]
    public List<int> degrees = new List<int>();
    public static bool serializedObjectFound = false;

    void OnGUI()
    {
        GUILayout.BeginVertical("box");
        //ResetGUIColor();
        NewCityDialog();
        GUILayout.EndVertical();
    }

    SerializedObject serializedObject;

    private void NewCityDialog()
    {
        if (choosenCountry != null && choosenCountry != "")
        {
            GUILayout.Label("Add city to " + choosenCountry);
            newCityName = GUILayout.TextField(newCityName);

            sprite = (Sprite)EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), allowSceneObjects: true);

            if (!serializedObjectFound)
            {
                serializedObject = new SerializedObject(this);
                serializedObjectFound = true;
            }

            var property = serializedObject.FindProperty("degrees");
            serializedObject.Update();
            EditorGUILayout.PropertyField(property, true);
            serializedObject.ApplyModifiedProperties();

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("degrees"));
            //serializedObject.ApplyModifiedProperties();

            //degrees = (List<int>)EditorGUILayout.li.ObjectField(degrees, typeof(List<int>));

            if (GUILayout.Button("+ Add city"))
            {
                if (EditorUtility.DisplayDialog("Add new city?", "Add new city?", "Ok"))
                {
                    GamePlayCreator.Instance.AddNewCity(choosenCountry, newCityName, sprite, "English", degrees);
                    Debug.Log("New city");
                    newCityName = "";
                }
            }

            EditorGUILayout.Separator();
        }


    }

}


#endif