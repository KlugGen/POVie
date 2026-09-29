#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class GameViewUtils
{
    static object gameViewSizesInstance;
    static MethodInfo getGroup;

    static GameViewUtils()
    {
        // gameViewSizesInstance  = ScriptableSingleton<GameViewSizes>.instance;
        var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instanceProp = singleType.GetProperty("instance");
        getGroup = sizesType.GetMethod("GetGroup");
        gameViewSizesInstance = instanceProp.GetValue(null, null);
    }

    public enum GameViewSizeType
    {
        AspectRatio, FixedResolution
    }

    [MenuItem("Test/AddSize")]
    public static void AddTestSize()
    {
        AddCustomSize(GameViewSizeType.AspectRatio, GameViewSizeGroupType.Standalone, 123, 456, "Test size");
    }

    [MenuItem("Test/SizeTextQuery")]
    public static void SizeTextQueryTest()
    {
        Debug.Log(SizeExists(GameViewSizeGroupType.Standalone, "Test size"));
    }

    [MenuItem("Test/Query16:9Test")]
    public static void WidescreenQueryTest()
    {
        Debug.Log(SizeExists(GameViewSizeGroupType.Standalone, "16:9"));
    }

    [MenuItem("Test/Set16:9")]
    public static void SetWidescreenTest()
    {
        SetSize(FindSize(GameViewSizeGroupType.Standalone, "16:9"));
    }

    public static void SetSizeExternal(string s)
    {
        SetSize(FindSize(GameViewSizeGroupType.Standalone, s));
    }

    public static void SetSizeExternal(GameViewSizeGroupType gt, int width, int height)
    {
        SetSize(FindSize(gt, width, height));
    }

    [MenuItem("Test/SetTestSize")]
    public static void SetTestSize()
    {
        int idx = FindSize(GameViewSizeGroupType.Standalone, 123, 456);
        if (idx != -1)
            SetSize(idx);
    }

    public static void SetSize(int index)
    {
        var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var gvWnd = EditorWindow.GetWindow(gvWndType);
        selectedSizeIndexProp.SetValue(gvWnd, index, null);
    }

    [MenuItem("Test/SizeDimensionsQuery")]
    public static void SizeDimensionsQueryTest()
    {
        Debug.Log(SizeExists(GameViewSizeGroupType.Standalone, 123, 456));
    }

    public static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
    {
        // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupTyge);
        // group.AddCustomSize(new GameViewSize(viewSizeType, width, height, text);

        var group = GetGroup(sizeGroupType);
        var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize"); // or group.GetType().
        var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        var ctor = gvsType.GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(string) });
        var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
        addCustomSize.Invoke(group, new object[] { newSize });
    }

    public static bool SizeExists(GameViewSizeGroupType sizeGroupType, string text)
    {
        return FindSize(sizeGroupType, text) != -1;
    }

    public static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
    {
        // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
        // string[] texts = group.GetDisplayTexts();
        // for loop...

        var group = GetGroup(sizeGroupType);
        var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
        var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
        for (int i = 0; i < displayTexts.Length; i++)
        {
            string display = displayTexts[i];
            // the text we get is "Name (W:H)" if the size has a name, or just "W:H" e.g. 16:9
            // so if we're querying a custom size text we substring to only get the name
            // You could see the outputs by just logging
            // Debug.Log(display);
            int pren = display.IndexOf('(');
            if (pren != -1)
                display = display.Substring(0, pren - 1); // -1 to remove the space that's before the prens. This is very implementation-depdenent
            if (display == text)
                return i;
        }
        return -1;
    }

    public static bool SizeExists(GameViewSizeGroupType sizeGroupType, int width, int height)
    {
        return FindSize(sizeGroupType, width, height) != -1;
    }

    public static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height)
    {
        // goal:
        // GameViewSizes group = gameViewSizesInstance.GetGroup(sizeGroupType);
        // int sizesCount = group.GetBuiltinCount() + group.GetCustomCount();
        // iterate through the sizes via group.GetGameViewSize(int index)

        var group = GetGroup(sizeGroupType);
        var groupType = group.GetType();
        var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
        var getCustomCount = groupType.GetMethod("GetCustomCount");
        int sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
        var getGameViewSize = groupType.GetMethod("GetGameViewSize");
        var gvsType = getGameViewSize.ReturnType;
        var widthProp = gvsType.GetProperty("width");
        var heightProp = gvsType.GetProperty("height");
        var indexValue = new object[1];
        for (int i = 0; i < sizesCount; i++)
        {
            indexValue[0] = i;
            var size = getGameViewSize.Invoke(group, indexValue);
            int sizeWidth = (int)widthProp.GetValue(size, null);
            int sizeHeight = (int)heightProp.GetValue(size, null);
            if (sizeWidth == width && sizeHeight == height)
                return i;
        }
        return -1;
    }

    static object GetGroup(GameViewSizeGroupType type)
    {
        return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
    }

    //[MenuItem("Test/Camera/Take Screenshot #&c", false, 1)]
    public static void TakeScreenshot(string path = "", string forcedName = "")
    {
        string sName = Screen.width + "_" + Screen.height + "_";
        string extenstion = ".jpg";

        string date = System.DateTime.Now.ToString("h:mm:ss tt").Replace(":", "-");
        string endName = forcedName + extenstion;
        if (forcedName == "")
            endName = sName + date + extenstion;

        Debug.Log("Image resolution: " + Screen.width + "x" + Screen.height);
        Debug.Log("Name: " + endName);

        ScreenCapture.CaptureScreenshot(path + endName);
    }

    [MenuItem("Test/Camera/Take Super Screenshot", false, 1)]
    private static void TakeSuperScreenshot()
    {
        string sName = Screen.width + "_" + Screen.height + "_";
        string extenstion = ".jpg";

        string date = System.DateTime.Now.ToString("h:mm:ss tt").Replace(":", "");
        string endName = sName + date + extenstion;

        Debug.Log("Image resolution: " + Screen.width + "x" + Screen.height);
        Debug.Log("Name: " + endName);

        ScreenCapture.CaptureScreenshot("super" + endName, 2);
    }

    [MenuItem("Test/Complex Screenshot", false, 1)]
    private static void TestComplexScreenshots()
    {        
        TakeScreenshot();
        SetSize(FindSize(GameViewSizeGroupType.Standalone, "16:9"));
        TakeScreenshot();
        SetSize(FindSize(GameViewSizeGroupType.Standalone, "16:10"));
        TakeScreenshot();
    }

    static public List<Vector2> GetAllSizes(GameViewSizeGroupType type) {

        List<Vector2> sizes = new List<Vector2>();

        var group = GetGroup(type);
        var groupType = group.GetType();
        var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
        var getCustomCount = groupType.GetMethod("GetCustomCount");
        int sizesCount = (int)getBuiltinCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
        var getGameViewSize = groupType.GetMethod("GetGameViewSize");
        var gvsType = getGameViewSize.ReturnType;
        var widthProp = gvsType.GetProperty("width");
        var heightProp = gvsType.GetProperty("height");
        var indexValue = new object[1];
        for (int i = 0; i < sizesCount; i++)
        {
            indexValue[0] = i;
            var size = getGameViewSize.Invoke(group, indexValue);
            int sizeWidth = (int)widthProp.GetValue(size, null);
            int sizeHeight = (int)heightProp.GetValue(size, null);

            sizes.Add(new Vector2(sizeWidth, sizeHeight));
        }

        return sizes;
    }
    
    [MenuItem("Test/Rotate 45 selected #&q", false, 1)]
    private static void Rotate45More()
    {
        if (Selection.gameObjects.Length == 0)
            return;

        for(int i = 0; i< Selection.gameObjects.Length; i++)
        {
            RectTransform rt = Selection.gameObjects[i].GetComponent<RectTransform>();
            rt.Rotate(new Vector3(0, 0, 1), 45f);
            EditorUtility.SetDirty(rt);
        }

     

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

}
//[MenuItem("Test/LogCurrentGroupType")]
//public static void LogCurrentGroupType()
//{
//    Debug.Log(GetCurrentGroupType());
//}
//public static GameViewSizeGroupType GetCurrentGroupType()
//{
//    var getCurrentGroupTypeProp = gameViewSizesInstance.GetType().GetProperty("currentGroupType");
//    return (GameViewSizeGroupType)(int)getCurrentGroupTypeProp.GetValue(gameViewSizesInstance, null);
//}

#endif