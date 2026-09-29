//using Facebook.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignIn : Singleton<SignIn>
{   
    ////Firebase.Auth.FirebaseAuth auth;
    //public Firebase.Auth.FirebaseUser user;

    //public string email, password;

    //public string loggedName, loggedEmail, loggedPhoto;

    //public TextAsset asset;

    ////public GameObject RegLogGo;

    ////Start is called before the first frame update
    
    //public void Init()
    //{
    //    // add log event
    //   // auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    //    auth.StateChanged += AuthStateChanged;
    //    AuthStateChanged(this, null);

    //    // init FB
    //    FB.Init(() =>
    //    {
    //        Debug.Log("Firebase and Facebook initialized!");
    //        //logs.text += "Initialized! ";
    //    });
    //}

    //public void SetEmailToLogin(string emailValue)
    //{
    //    email = emailValue;
    //}

    //public void SetPasswordToLogin(string passwordValue)
    //{
    //    password = passwordValue;
    //}

    //private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    //{
    //    if (auth.CurrentUser != user)
    //    {
    //        bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
    //        if (!signedIn && user != null)
    //        {
    //            Debug.Log("Signed out " + user.UserId);
    //        }
    //        user = auth.CurrentUser;
    //        if (signedIn)
    //        {
    //            Debug.Log("Signed in " + user.UserId);
    //            loggedName = user.DisplayName ?? "";
    //            loggedEmail = user.Email ?? "";
    //            loggedPhoto = user.PhotoUrl.ToString() ?? "";
    //        }
    //    }
    //}

    //public void Reg()
    //{
    //    auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
    //        if (task.IsCanceled)
    //        {
    //            Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
    //            return;
    //        }
    //        if (task.IsFaulted)
    //        {
    //            Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
    //            return;
    //        }

    //        // Firebase user has been created.
    //        Firebase.Auth.FirebaseUser newUser = task.Result;
    //        Debug.LogFormat("Firebase user created successfully: {0} ({1})",
    //            newUser.DisplayName, newUser.UserId);
    //    });
    //}

    //public void Log()
    //{
    //    auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
    //        if (task.IsCanceled)
    //        {
    //            Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
    //            return;
    //        }
    //        if (task.IsFaulted)
    //        {
    //            Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
    //            return;
    //        }

    //        Firebase.Auth.FirebaseUser newUser = task.Result;
    //        Debug.LogFormat("User signed in successfully: {0} ({1})",
    //            newUser.DisplayName, newUser.UserId);
    //    });
    //}

    //public void LogFB()
    //{
    //    FB.LogInWithPublishPermissions(
    //        new List<string>() { "publish_to_groups" },
    //        (result) =>
    //        {
    //            Firebase.Auth.Credential credential =
    //                Firebase.Auth.FacebookAuthProvider.GetCredential(result.AccessToken.TokenString);
    //            auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
    //                if (task.IsCanceled)
    //                {
    //                    Debug.LogError("SignInWithCredentialAsync was canceled.");
    //                    return;
    //                }
    //                if (task.IsFaulted)
    //                {
    //                    Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
    //                    return;
    //                }

    //                Firebase.Auth.FirebaseUser newUser = task.Result;
    //                Debug.LogFormat("User signed in successfully: {0} ({1})",
    //                    newUser.DisplayName, newUser.UserId);

    //                user = newUser;
    //            });
    //        }
    //    );
    //}

    //public void PostOnFB()
    //{
    //    if (FB.IsLoggedIn)
    //    {
    //        FB.FeedShare(
    //            link: new System.Uri("https://dkk-development.cloud"),
    //            linkName: "DKK",
    //            callback: FeedCallback
    //        );
    //    }
    //    else
    //    {
    //        Debug.Log("NO USER");
    //    }
    //}

    //private void FeedCallback(IShareResult result)
    //{
    //    if (result.Cancelled)
    //    {
    //        Debug.Log("CANCELD");
    //    }
    //    else
    //    {
    //        Debug.Log("DONE");
    //    }
    //}

    //public void PrintUserData()
    //{
    //    if (auth != null)
    //    {
    //        Debug.Log(user.PhotoUrl);
    //        Debug.Log(user.DisplayName);
    //    }
    //    else
    //    {
    //        Debug.Log("NO USER");
    //    }
    //}

    //public void Logout()
    //{
    //    if (auth != null)
    //    {
    //        auth.SignOut();
    //    }
    //    FB.LogOut();
    //}
}
