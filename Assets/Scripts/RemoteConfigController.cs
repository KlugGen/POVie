using System.Collections;
using System.Collections.Generic;
using Unity.RemoteConfig;
using UnityEngine;

public class RemoteConfigController : MonoBehaviour
{
    
    public struct userAttributes { }
    public struct appAttributes { }

    void Start()
    {

    }

    // Retrieve and apply the current key-value pairs from the service on Awake:
    void Awake()
    {
#if UNITY_EDITOR
        if(enabled)
        FetchConfig();
#endif
    }

    public void FetchConfig()
    {
        // Add a listener to apply settings when successfully retrieved:        

        ConfigManager.FetchCompleted += ApplyRemoteSettings;

        // Set the user’s unique ID:
        //ConfigManager.SetCustomUserID("some-user-id");

        //// Set the environment ID:
        //ConfigManager.SetEnvironmentID("an-env-id");
        // Fetch configuration setting from the remote service:
        
        ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
    }

    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        //configResponse.status.ToString().LogDev("Status: ");
        // Conditionally update settings, depending on the response's origin:
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("No settings loaded this session; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                //Debug.Log("New settings loaded this session; update values accordingly.");
                //ConfigManager.appConfig.ToString().LogDev("Remote config: ");
                string bundlesUrl = ConfigManager.appConfig.GetString("assetBundlesUrl", DKK.AssetsBundleManager.Instance.serverUrl);

                Rules.GameBalance.adsBalanceReward = ConfigManager.appConfig.GetInt("adsBalanceReward", Rules.GameBalance.adsBalanceReward);

                Rules.GameBalance.autoRotationBonus = ConfigManager.appConfig.GetInt("autoRotationBonus", Rules.GameBalance.autoRotationBonus);
                Rules.GameBalance.highlightBonus = ConfigManager.appConfig.GetInt("highlightBonus", Rules.GameBalance.highlightBonus);
                Rules.GameBalance.noDistractorBonus = ConfigManager.appConfig.GetInt("noDistractorBonus", Rules.GameBalance.noDistractorBonus);
                Rules.GameBalance.endlessTimerBonus = ConfigManager.appConfig.GetInt("endlessTimerBonus", Rules.GameBalance.endlessTimerBonus);

                Rules.GameBalance.extendedTimer = ConfigManager.appConfig.GetFloat("extendedTimer", Rules.GameBalance.extendedTimer);

                Rules.GameBalance.replayCost = ConfigManager.appConfig.GetInt("replayCost", Rules.GameBalance.replayCost);
                Rules.GameBalance.restartCost = ConfigManager.appConfig.GetInt("restartCost", Rules.GameBalance.restartCost);

                Rules.GameBalance.adsScoreMultiplier = ConfigManager.appConfig.GetInt("adsScoreMultiplier", Rules.GameBalance.adsScoreMultiplier);
                Rules.GameBalance.adsBalanceReward = ConfigManager.appConfig.GetInt("adsBalanceReward", Rules.GameBalance.adsBalanceReward);

                Rules.GameBalance.rotationAccuracyCheck = ConfigManager.appConfig.GetInt("rotationAccuracyCheck", Rules.GameBalance.rotationAccuracyCheck);

                Rules.GameBalance.angleRotationStep = ConfigManager.appConfig.GetInt("angleRotationStep", Rules.GameBalance.angleRotationStep);

                Rules.GameBalance.adsBalanceAvailableTreshold = ConfigManager.appConfig.GetInt("adsBalanceAvailableTreshold", Rules.GameBalance.adsBalanceAvailableTreshold);

                //Rules.GameBalance.adsBalanceAvailableTreshold.Log("Adstres: ");
                //bundlesUrl.LogDev("Bundles: ");
                break;
        }

        //"deleting delegate".LogDev();
        ConfigManager.FetchCompleted -= ApplyRemoteSettings;
    }
}
