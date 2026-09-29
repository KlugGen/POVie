using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;

public class Addons : MonoBehaviour
{
    private const string catName = "DKK";

    [MenuItem(catName + "/Editor/ChangeResoulutions", false, 1)]
    private static void Resolutions()
    {
        Debug.Log(Handles.GetMainGameViewSize());

        //return;
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
        if(GetMainGameView == null)
        {
            Debug.Log("Game View null");
            return;
        }
        System.Object Res = GetMainGameView.Invoke(null, null);

        EditorWindow v2 = (EditorWindow)Res;
        Rect R = v2.position;
        R.width = 100;
        R.height = 500;
        v2.position = R;
    }

    [MenuItem(catName + "/Camera/Take Screenshot #&c", false, 1)]
    private static void TakeScreenshot()
    {
        string sName = Screen.width + "_" + Screen.height + "_";
        string extenstion = ".jpg";

        string date = System.DateTime.Now.ToString("h:mm:ss tt").Replace(":", "");
        string endName = sName + date + extenstion;

        Debug.Log("Image resolution: " + Screen.width + "x" + Screen.height);
        Debug.Log("Name: " + endName);

        ScreenCapture.CaptureScreenshot(endName);

    }

    [MenuItem(catName + "/Open persistent data path", false, 1)]
    private static void OpenPDP()
    {
        Application.OpenURL(Application.persistentDataPath);
    }

    [MenuItem(catName + "/Open project path", false, 1)]
    private static void OpenProjectPath()
    {
        Application.OpenURL(Application.dataPath);
    }

}
#endif
