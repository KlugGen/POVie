//using Facebook.Unity;
using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FbScript : MonoBehaviour
{

    static private FbScript instance;

    static public FbScript Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(FbScript))[0] as FbScript;

            return instance;
        }
    }

    //void Awake()
    //{
    //    if (!FB.IsInitialized)
    //    {
    //        // Initialize the Facebook SDK
    //        FB.Init(InitCallback, OnHideUnity);
    //    }
    //    else
    //    {
    //        // Already initialized, signal an app activation App Event
    //        FB.ActivateApp();
    //    }
    //}

    public void InitializeFB()
    {
        if (!FB.IsInitialized)
        {
            //Initialize the Facebook SDK            
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            //Already initialized, signal an app activation App Event           
            FB.ActivateApp();
            OnLogInButtonClicked();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            OnLogInButtonClicked();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            SignInController.Instance.NotLoggedIn();
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            //Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            //Time.timeScale = 1;
        }
    }

    public void OnLogInButtonClicked()
    {
        var perms = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(perms, AuthCallback);        
    }

    private void AuthCallback(ILoginResult result)
    {        
        if (FB.IsLoggedIn)
        {            
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            
            if(aToken != null)
            {
                "FB login".Log();

                if (aToken.UserId != null)
                    Debug.Log(aToken.UserId);
                if (aToken.TokenString != null)
                    Debug.Log(aToken.TokenString);
            }
         
            // Print current access token's granted permissions
            //foreach (string perm in aToken.Permissions)
            //{
            //    Debug.Log(perm);
            //}

            FirebaseController.Instance.FB_Login(aToken.TokenString);
        }
        else
        {
            FirebaseController.Instance.LogInFaulted();
            //result.Error.Log("Result: ");
            //Debug.Log("User cancelled login");
        }
    }

    public void LogOut()
    {
        if (FB.IsLoggedIn)
        {
            "Log out".Log();
            FB.LogOut();
        }
    }
}
