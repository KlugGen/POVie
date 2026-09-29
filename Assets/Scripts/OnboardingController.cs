using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class OnboardingController : Singleton<OnboardingController>
{
    public MenuController menu;

    public bool wasPlayedOnce = false;
    public List<Outline> outlines = new List<Outline>();
    
    public bool IsPlaying()
    {
        return gameObject.activeInHierarchy;
    }

    public UnityEngine.Playables.PlayableDirector director;

    private void OnEnable()
    {
#if !UNITY_EDITOR
        AnalyticsEvent.FirstInteraction();
#endif
        director.Play();

        "ENABLING ONBOARDING".LogDev();
    }


    public void TestMethod()
    {
        "TEST METHOD IN ONBOARDING".LogDev();
    }

    public void PauseTrack()
    {
        "Pause Track".LogDev();
        //director.playableGraph.GetRootPlayable(0).
        director.Pause();
    }

    public void PlayTrack()
    {
        "Play Track".LogDev();
        director.Play();
       //director.Resume();
    }

    public void EnableOutlines()
    {
        outlines.FindAll(o => o != null).ForEach(o=>o.enabled = true);
    }


    public void DisableOutlines()
    {
        outlines.FindAll(o => o != null).ForEach(o => o.enabled = false);
    }

    public void Resume()
    {
        "Resume".LogDev();
        StartCoroutine("ResetAndDisable");
        AnalyticsEvent.TutorialComplete();
    }    

    IEnumerator ResetAndDisable()
    {
        if (!AssetBundleDownloader.Instance.IsBusy())
        {
            if (FirebaseController.Instance.GetAuthenticatedUser().HasUser())
            {
                menu.Enable();
                DailyRewardModule.Instance.Check();
            }
            else
            {
                SignInController.Instance.Enable();
            }
        }

        director.time = 0f;
        director.Play();
        wasPlayedOnce = true;
        yield return new WaitForEndOfFrame();     
        gameObject.SetActive(false);
    }
}
