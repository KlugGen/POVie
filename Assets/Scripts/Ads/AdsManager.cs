using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//#if  UNITY_ANDROID
//using GoogleMobileAds.Api;
//#endif
using UnityEngine.UI;
using System;
using NaughtyAttributes;
using UnityEngine.Advertisements;

public class AdsManager : Singleton<AdsManager>, IUnityAdsInitializationListener, IUnityAdsShowListener
{
    private bool adInFront = false;

    public bool testMode = true;
    private string rewarded_name = "rewarded_android";

    private string coroutine_name = "";

    private bool audioBeforeAdEnabled = false;
    public bool AdsInFront()
    {
        return adInFront;
    }
     

    public void Start()
    {      
        string gameID = "4268167";

#if UNITY_IOS
gameID = "4268166";
rewarded_name = "rewarded_ios";
#endif

        Advertisement.Initialize(gameID, testMode, true, this);    

    }

    public void OnInitializationComplete()
    {
        Advertisement.Load(rewarded_name);
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        "UNITY ADS INIT FAILED".Log();
        adInFront = false;
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        "UNITY ADS SHOW FAILURE".Log();
        adInFront = false;
        AudioController.Instance.SetAudioState(audioBeforeAdEnabled);
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        "unity ads show start".Log();
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        "unity ads show click".Log();
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Advertisement.Load(rewarded_name);
        StartCoroutine(coroutine_name);
        AudioController.Instance.SetAudioState(audioBeforeAdEnabled);
        adInFront = false;       
    }

    public void OpenUnityAd()
    {

    }

    public void ScoreMultiplierAd()
    {
        ShowRewardedAd("AddMultiplyRewardCo");
    }

    public void LoadAds()
    {        
        LoadRewardedAd();
    }

    //[Button("LoadRewardedAd")]
    public void LoadRewardedAd()
    {
        return;
    }
      
    public void ShowRewardedAd(string cName)
    {
        if (Advertisement.IsReady(rewarded_name))
        {
            audioBeforeAdEnabled = AudioController.Instance.IsVoiceEnabled();
            AudioController.Instance.Mute();
            Advertisement.Show(rewarded_name, this);
            coroutine_name = cName;
            adInFront = true;
        }
      
        return;       
    }

    IEnumerator AddMultiplyRewardCo()
    {
        yield return new WaitForEndOfFrame();

        yield return new WaitForEndOfFrame();

        //"RewardDelegate".Log();
        SuccessScreenController.Instance.MultiplyReward();

    }

    public void ShowCooloffAd()
    {
        ShowRewardedAd("CooloffAdsCo"); 
    }

    IEnumerator CooloffAdsCo()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        CoolOffScreen.Instance.AdCallback();

    }

    public void ShowRewardedAdCapital()
    {
        ShowRewardedAd("BalanceAdsCo");       
    }

    IEnumerator BalanceAdsCo()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        CapitalScreen.Instance.AdsRewardCallback();   

    } 

    //[Button("IsRewardedAdLoaded")]
    public bool IsRewardedAdLoaded()
    {
        return Advertisement.IsReady(rewarded_name);     
    }

}
