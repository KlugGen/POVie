using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Extensions;

public class GoogleSignInDemo : Singleton<GoogleSignInDemo>
{

    
    public string webClientId = "<your client id here>";

    //private FirebaseAuth auth;
//#if  UNITY_ANDROID
    private GoogleSignInConfiguration configuration;
//#endif

    private void Awake()
    {
//#if  UNITY_ANDROID
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
//#endif
    }
       
    public bool linking = false;

    public void SignInWithGoogle() { OnSignIn(); }
    public void SignOutFromGoogle() { OnSignOut(); }

    private void OnSignIn()
    {
//#if  UNITY_ANDROID

        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        if (linking)
            GoogleSignIn.Configuration.ForceTokenRefresh = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnAuthenticationFinished);
//#endif
    }

    private void OnSignOut()
    {
//#if  UNITY_ANDROID
        AddToInformation("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
//#endif
    }

    public void OnDisconnect()
    {
//#if  UNITY_ANDROID
        "Calling Disconnect".Log();
        AddToInformation("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
//#endif
    }

//#if  UNITY_ANDROID
    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {

        if (task.IsFaulted)
        {          
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {              
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddToInformation("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                }
            }

            FirebaseController.Instance.LogInFaulted();
        }
        else if (task.IsCanceled)
        {
            AddToInformation("Canceled");
            FirebaseController.Instance.LogInFaulted();
        }
        else
        {
            AddToInformation("Welcome: " + task.Result.DisplayName + "!");
            AddToInformation("Email = " + task.Result.Email);
            AddToInformation("Google ID Token = " + task.Result.IdToken);
            AddToInformation("Email = " + task.Result.Email);
            FirebaseController.Instance.GoogleLogin(task.Result.IdToken);
            //SignInWithGoogleOnFirebase(task.Result.IdToken);
        }
    }
//#endif

    public void OnSignInSilently()
    {
//#if  UNITY_ANDROID
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWithOnMainThread(OnAuthenticationFinished);
//#endif
    }

    public void OnGamesSignIn()
    {
//#if  UNITY_ANDROID
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        AddToInformation("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnAuthenticationFinished);
//#endif
    }

    private void AddToInformation(string str)
    {
        str.Log();
        //infoText.text += "\n" + str;
    }
}