using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroLoader : MonoBehaviour
{
    public Loader l1, l2;

    void Awake()
    {
        StartCoroutine(LoadYourAsyncScene());        
    }

    IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            int i = (int)asyncLoad.progress * 100;
            Debug.Log(i.ToString());
            l1.SetPercentage(i);
            l2.SetPercentage(i);
           
            yield return null;
        }

        Debug.Log("Loaded");
    }
}
