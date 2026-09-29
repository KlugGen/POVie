using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneController : MonoBehaviour
{
    void Start() {
        StartCoroutine(LoadScene());
    }

    AsyncOperation asyncOperation = null;


    IEnumerator LoadScene()
    {
        yield return null;

        //Begin to load the Scene you specify
        asyncOperation = SceneManager.LoadSceneAsync("MainScene");
        //Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;

        Debug.Log(IntroViewController.Instance);
        Debug.Log(IntroViewController.Instance.GetActiveLoader());

        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone)
        {

            //m_Text.text = "Loading progress: " + (asyncOperation.progress * 100) + "%";

           // Debug.Log(asyncOperation.progress);
            IntroViewController.Instance.GetActiveLoader().SetPercentage((int)(asyncOperation.progress * 100f));

            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f)
            {
                //Change the Text to show the Scene is ready
                //m_Text.text = "Press the space bar to continue";
                //Wait to you press the space key to activate the Scene

                sceneReady = true;
            }

            yield return null;
        }

       
    }

    bool sceneReady = false;

    public void ClickScene() {
        if(asyncOperation!=null && sceneReady)
            asyncOperation.allowSceneActivation = true;
    }
}
