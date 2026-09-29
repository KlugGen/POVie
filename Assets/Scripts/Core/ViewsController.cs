using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class ViewsController : Singleton<ViewsController>
{
    public SharedViews sharedViews;

    public List<View> openViews = new List<View>();

    [DisVar(1f)]
    [TimeVar(3f)]
    public float someVar = 2f;

    public View startScreen;

    public Canvas mainCanvas;
    public CanvasScaler canvasScaler;

    public void AddToCurrentContextToClose(View v)
    {
        if (openViews.Count > 0)
        {
            View view = openViews[openViews.Count - 1];
            if(view != null)
            {
                view.AddViewToClose(v);
            }
        }
    }
    
    public void EnabledView(View view)
    {        
        if(!openViews.Contains(view))
            openViews.Add(view);
    }

    public void DisabledView(View view)
    {
        //view.gameObject.name.LogDev("Removing from queue: ");        
        openViews.Remove(view);
    }

    void Start()
    {
        TextTranslationModule.Instance.Initialize();
        StartCoroutine(CheckForChange());       
        startScreen.Enable();                    
                               
        //FirebaseController.Instance.LogEvent("APP STARTED ");
        //AttributeCheck();
    }

    private void AttributeCheck()
    {
        // someVar.Log();

        MonoBehaviour[] sceneActive =  FindObjectsOfType<MonoBehaviour>();
        //List<MonoBehaviour> sceneActive = new List<MonoBehaviour>() { this};
        sceneActive.Length.LogDev("Monos: ");
        foreach (MonoBehaviour mono in sceneActive)
        {
            FieldInfo[] objectFields = mono.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < objectFields.Length; i++)
            {
                TimeVar attribute = System.Attribute.GetCustomAttribute(objectFields[i], typeof(TimeVar)) as TimeVar;
                if (attribute != null)
                {
                    Debug.Log(objectFields[i].Name); // The name of the flagged variable.
                    Debug.Log(objectFields[i].FieldType);
                    Debug.Log(objectFields[i].GetValue(this));
                    Debug.Log("TV: " + attribute.value);
                    objectFields[i].SetValue(this, attribute.value);
                    // The name of the flagged variable.
                }

                DisVar a = System.Attribute.GetCustomAttribute(objectFields[i], typeof(DisVar)) as DisVar;
                if (a != null)
                {
                    Debug.Log(objectFields[i].Name); // The name of the flagged variable.
                    Debug.Log(objectFields[i].FieldType);
                    Debug.Log(objectFields[i].GetValue(this));
                    Debug.Log("DV: " + a.value);
                    objectFields[i].SetValue(this, a.value);
                    // The name of the flagged variable.
                }
            }
        }


        //someVar.Log();
    }

    static Vector2 resolution;                    // Current Resolution
    static ScreenOrientation orientation;        // Current Device Orientation
    static bool isAlive = true;                    // Keep this script running?

    public static float CheckDelay = 1f;        // How long to wait until we check again.

    private float lastEditorRatio = 1f;
    IEnumerator CheckForChange()
    {
        resolution = new Vector2(Screen.width, Screen.height);
        lastEditorRatio = resolution.x / resolution.y;


        orientation = Screen.orientation;

        while (isAlive)
        {
#if UNITY_EDITOR
            if (resolution.x != Screen.width || resolution.y != Screen.height)
            {
                resolution = new Vector2(Screen.width, Screen.height);
                //GyroscopeController.Instance.ActualizeGyroAttitude();
                GyroForParallax.Instance.Reset();
                float actualRatio = resolution.x / resolution.y;
                if ((lastEditorRatio > 1f && actualRatio < 1f) || (lastEditorRatio < 1f && actualRatio > 1f))
                    ChangeOrientation();

                lastEditorRatio = actualRatio;
            }
#else
                   if (orientation != Screen.orientation)
                   {
                       orientation = Screen.orientation;
                       //GyroscopeController.Instance.ActualizeGyroAttitude();
                        GyroForParallax.Instance.Reset();
                       ChangeMobileOrientation();
                   }
#endif

            yield return new WaitForSeconds(CheckDelay);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !AdsManager.Instance.AdsInFront())
        {
            GoToPreviousScreen();
        }
    }

    [NaughtyAttributes.Button("Back")]
    private void GoToPreviousScreen() {

        //"previous screen".Log();
        if (openViews.Count > 0 && openViews[0] != null)
        {
            //openViews[0].name.LogDev("Back. Enabling previous for: ");
            openViews[0].EnablePrevious();
        }
    }


    private void ChangeResolution()
    {
        openViews.ForEach(o => o.ChangeResolution());
    }

    private void ChangeMobileOrientation()
    {
        openViews.ForEach(o => o.ChangeOrientation(orientation));
    }

    private void ChangeOrientation()
    {

        ScreenOrientation so = ScreenOrientation.Landscape;

        if (Screen.width < Screen.height)
            so = ScreenOrientation.Portrait;

        openViews.ForEach(o => o.ChangeOrientation(so));
    }

    public ScreenOrientation GetCurrentOrientation()
    {
#if UNITY_EDITOR
        ScreenOrientation so = ScreenOrientation.Landscape;

        Vector2 gameViewSize = GetMainGameViewSize();

        if (gameViewSize.x < gameViewSize.y)
            so = ScreenOrientation.Portrait;

        return so;
#else
    return Screen.orientation;

#endif
    }

    public static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }

    [NaughtyAttributes.Button("Complex screen")]
    public void CaptureComplexScreen()
    {
        //GameViewUtils.GetAllSizes(UnityEditor.GameViewSizeGroupType.Android).ForEach(delegate (Vector2 v)
        //{
        //    Debug.Log("ID: " + GameViewUtils.FindSize(UnityEditor.GameViewSizeGroupType.Android, (int)v.x, (int)v.y) + " with dimension " + v);
        //});
        StartCoroutine("MultipleScreenshot");
    }

#if UNITY_EDITOR
    IEnumerator MultipleScreenshot()
    {
        foreach (Vector2 v in GameViewUtils.GetAllSizes(UnityEditor.GameViewSizeGroupType.Android))
        {
            Debug.Log("Creating for: " + v);
            if (v.x < 200 || v.y < 200)
            {
                Debug.Log("Passing");
                continue;
            }

            yield return new WaitForSeconds(.1f);
            GameViewUtils.SetSizeExternal(UnityEditor.GameViewSizeGroupType.Android, (int)v.x, (int)v.y);
            yield return new WaitForSeconds(.5f);
            GameViewUtils.TakeScreenshot();

        }

        // GameViewUtils.TakeScreenshot();

        //yield return new WaitForSeconds(1f);
        //GameViewUtils.SetSizeExternal("16:9");
        //yield return new WaitForSeconds(1f);
        //GameViewUtils.TakeScreenshot();

        //yield return new WaitForSeconds(1f);
        //GameViewUtils.SetSizeExternal("16:10");
        //yield return new WaitForSeconds(1f);
        //GameViewUtils.TakeScreenshot();

        //yield return new WaitForSeconds(1f);
        //GameViewUtils.SetSizeExternal("5:4");
        //yield return new WaitForSeconds(1f);
        //GameViewUtils.TakeScreenshot();
    }
#endif
}

[System.AttributeUsage(System.AttributeTargets.Field)]
public class TimeVar : System.Attribute {
    public object value;
    public TimeVar(object o)
    {
        value = o;
    }
}

[System.AttributeUsage(System.AttributeTargets.Field)]
public class DisVar : System.Attribute
{
    public object value;
    public DisVar(object o)
    {
        value = o;
    }
}

[System.Serializable]
public class SharedViews
{
    public View allLevelsCompleted;
}