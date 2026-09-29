using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
# if !UNITY_ANDROID  && !UNITY_EDITOR
using GoogleMobileAds.Placement;
using GoogleMobileAds.Api;
#endif

*/

public class RewardedAdController : MonoBehaviour
{
    public Text adButtonText;

    /*
#if !UNITY_ANDROID && !UNITY_EDITOR
    public RewardedAdGameObject rewardedAd;
#endif

    private string initialButtonText;
    public Button button;

    public void LoadAndShowRewardedAd()
    {
#if !UNITY_ANDROID && !UNITY_EDITOR
        button.interactable = false;
        rewardedAd.LoadAd();
        initialButtonText = adButtonText.text;
        adButtonText.text = "Loading ad..";
        Debug.Log(adButtonText.text);
#endif
    }


#if !UNITY_ANDROID && !UNITY_EDITOR
    public void OnUserEarnedReward(Reward reward)
    {
        Debug.Log("OnUserEarnedReward: reward=" +
            reward.Type + ", amount=" + reward.Amount);
    }
#endif


    public void OnAdLoaded()
    {
        //if (showAfterLoading)
        //{
            # if UNITY_ANDROID  && !UNITY_EDITOR
            ShowAd();
            adButtonText.text = initialButtonText;
            button.interactable = true;
            #endif
        //}
    }

    public void ShowAd() 
    {
#if !UNITY_ANDROID && !UNITY_EDITOR
        rewardedAd.ShowIfLoaded();
#endif
    }

    */
}
