using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ResolutionBook : MonoBehaviour
{
    public List<Vector2> resolutions = new List<Vector2>();

    public string setName;
    public string viewName;

    private string GetCurrentPath()
    {
        return Application.persistentDataPath + "\\ScreenshotSets\\" + setName + "\\" + viewName + "\\"; 
    }


    [NaughtyAttributes.Button("CreateScreenshotsSet")]
    public void CreateScreenshotsSet()
    {       

        if (string.IsNullOrEmpty(setName))
        {
            Debug.LogWarning("Set name empty");
        }

        if (string.IsNullOrEmpty(viewName))
        {
            Debug.LogWarning("View name empty");
        }

        string directoryName = GetCurrentPath();
        if (Directory.Exists(directoryName))
        {
            Debug.LogWarning("Set folder with name already exists");
            return;
        }

        Directory.CreateDirectory(directoryName);

        StartCoroutine("MultipleScreenshot");

    }

#if UNITY_EDITOR



    IEnumerator MultipleScreenshot()
    {
       
        foreach (Vector2 v in resolutions)
        {
            string resolution = (int)v.x + "x" + (int)v.y;
            int id = GameViewUtils.FindSize(UnityEditor.GameViewSizeGroupType.Android, (int)v.x, (int)v.y);

            if (id == -1)
            {
                Debug.LogWarning("Resolution " + resolution + " is missing");
                continue;
            }

            yield return new WaitForSeconds(.2f);
            GameViewUtils.SetSizeExternal(UnityEditor.GameViewSizeGroupType.Android, (int)v.x, (int)v.y);
            yield return new WaitForSeconds(.2f);
            GameViewUtils.TakeScreenshot(GetCurrentPath());
        }
    }
#endif
}
