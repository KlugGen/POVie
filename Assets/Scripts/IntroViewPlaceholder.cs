using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DKK;
using UnityEngine.UI;
using DG.Tweening;

public class IntroViewPlaceholder : View
{
    static private IntroViewPlaceholder instance;

    static public IntroViewPlaceholder Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(IntroViewPlaceholder))[0] as IntroViewPlaceholder;

            return instance;
        }
    }

    public Loader l1;
    public Text loadingStatus_text;
    public View mainView;

    public bool showOnboarding = false;

    UnityEngine.Events.UnityAction actions;

    public override void Enable()
    {
        base.Enable();
        CheckInternetAndProceed();
    }

    private void CheckInternetAndProceed()
    {
        if (!InternetController.IsInternetReachable() && !InternetController.Instance.allowOffline)
        {
            YesNoPopupController.Instance.Enable(Enable, "Please connect to the Internet. Retry now?", false, Application.Quit);
            return;
        }

        if (AssetBundleDownloader.Instance.HasAnythingToDownload())
        {
            loadingStatus_text.gameObject.SetActive(true);

            float assetsToDownloadSize = Mathf.Max(1f, AssetBundleDownloader.Instance.GetSizeOfAllBundles());

            string downloadInfo = TextTranslationModule.GetWord("New data to download found.\n\nDownload now?") + string.Format(" [{0} MB]", assetsToDownloadSize.ToString("F0"));

            if (InternetController.IsReachableByCarrier())
            {
                downloadInfo += string.Format("\n \n {0}!", TextTranslationModule.GetWord("Your device has no Wi-Fi connection (recommended)"));
            }

            YesNoPopupController.Instance.Enable(OnPopUpYesAction, downloadInfo, false, Application.Quit, true);
        }
        else
        {
            //"Nothing to download".LogDev();
            AssetBundleDownloader.Instance.AssetsCountToDownload().LogDev();
            loadingStatus_text.gameObject.SetActive(false);
            ResumeAssetBudles();
        }
    }

    private void OnPopUpYesAction()
    {
        float allElementsToDownloadCounter = AssetBundleDownloader.Instance.AssetsCountToDownload();

        ResumeAssetBudles();

        AssetBundleDownloader.Instance.ClearAllEvents();

        AssetBundleDownloader.Instance.RegisterOnDownloadCompletedEvent(delegate () {
            loadingStatus_text.gameObject.SetActive(false);
            ResumeAssetBudles();
        });

        AssetBundleDownloader.Instance.RegisterOnDownloadEvent(delegate () {
            float p = (allElementsToDownloadCounter - (float)AssetBundleDownloader.Instance.AssetsCountToDownload()) * 100f / allElementsToDownloadCounter;
          
            l1.SetPercentage((int)p);
            loadingStatus_text.text = string.Format("{0}: {1}%", TextTranslationModule.GetWord("Downloading content"), p.ToString("F0"));
        });

        AssetBundleDownloader.Instance.RegisterOnRetriesFailEvent(Application.Quit);

        // Download process starts
        AssetBundleDownloader.Instance.StartDownloading();                  
    }

    private void ResumeAssetBudles()
    {
        if (!AssetBundleDownloader.Instance.IsBusy())
        {
            InternetController.Instance.active = true;
            Disable();
        }

        if (!OnboardingController.Instance.wasPlayedOnce && !OnboardingController.Instance.IsPlaying())
        {
#if UNITY_EDITOR
            if (showOnboarding)
                Onboarding();
            else
            {
                if (!AssetBundleDownloader.Instance.IsBusy())
                {                   
                    PostLoadAction();
                }
            }
#else
            Onboarding();
#endif
        }
        else
        {
            if (!OnboardingController.Instance.IsPlaying())
            {              
                PostLoadAction();
            }
        }
    }

    public void Onboarding()
    {
        if (PlayerPrefs.HasKey("onboarding" + UserController.Instance.GetLanguage()) == false)
        {
            PlayerPrefs.SetInt("onboarding" + UserController.Instance.GetLanguage(), 1);
            OnboardingController.Instance.gameObject.SetActive(true);
        }
        else
        {
            if (!AssetBundleDownloader.Instance.IsBusy())
            {
                  PostLoadAction();
            }
        }
    }

    public void PostLoadAction() => SignInController.Instance.Enable();      
    
}
