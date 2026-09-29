using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UniRx;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class FirebaseController : Singleton<FirebaseController>
{    
    private FirebaseAuthenticatedUser authUser = new FirebaseAuthenticatedUser(); 

    private bool ready = false;
    public bool IsReady() => ready;
    public FirebaseAuthenticatedUser GetAuthenticatedUser() => authUser;
    public FirebaseAuth GetFirebaseAuth() => GetAuthenticatedUser().GetAuth();

    public void InitializeFirebaseAuthentication()
    {       
        FirebaseApp.LogLevel = Firebase.LogLevel.Info;
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        authUser.Initialize(auth);       

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;          

            if (dependencyStatus == DependencyStatus.Available)
            {            
                ready = true;
            }
            else
            {
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }  

    public void LogInFaulted()
    {
        if (authUser.HasUser())
        {
            SignInController.Instance.Disable();
            MenuController.Instance.Enable();
        }
        else
        {
            SignInController.Instance.Enable();
            SignInController.Instance.NotLoggedIn();
        }
    }

    public void FB_Login(string token)
    {
        Credential credential = FacebookAuthProvider.GetCredential(token);
        authUser.LoginWithService(credential);
    }

    public void GoogleLogin(string token)
    {
        Credential credential = GoogleAuthProvider.GetCredential(token, null);
        authUser.LoginWithService(credential);        
    }     

    public void AnonymousLogIn()
    {
        authUser.AnonymousLogIn();
    }

    public void SignOut()
    {
        authUser.SignOut();
    }      

}

public class FirebaseAuthenticatedUser
{
    private FirebaseAuth _auth;

    public FirebaseAuth GetAuth() => _auth;
    public void Initialize(FirebaseAuth auth)
    {
        _auth = auth;
        _auth.StateChanged += OnAuthenticationStateChanged;
    }

    private void OnAuthenticationStateChanged(object sender, System.EventArgs e)
    {       
        if (_auth.CurrentUser != null)
        {
            _auth.CurrentUser.UserId.Log("Current user id: ");
            UserController.Instance.generalInfo.name = GetUserName();
            UserController.Instance.generalInfo.accountType = GetProvider();
        }

        SignInController.Instance.AfterFirebaseAuthenticationCallback();
    }

    public bool IsFB()
    {
        foreach (IUserInfo userInfo in _auth.CurrentUser.ProviderData)
        {
            if (userInfo.ProviderId.Equals("facebook.com"))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsGoogle()
    {
        foreach (IUserInfo userInfo in _auth.CurrentUser.ProviderData)
        {
            if (userInfo.ProviderId.Equals("google.com"))
            {
                return true;
            }
        }

        return false;
    }

    public string GetProvider()
    {
        if (IsGoogle())
            return "google";

        if (IsFB())
            return "FB";

        return "anonymous";
    }

    public bool HasUser() => _auth != null && _auth.CurrentUser != null;

    public FirebaseUser GetUser() => _auth.CurrentUser;

    public string GetUserName()
    {
        if (_auth.CurrentUser.IsAnonymous)
        {
            return TextTranslationModule.GetWord("Anonymous");
        }

        foreach (IUserInfo userInfo in _auth.CurrentUser.ProviderData)
        {
            if (userInfo.DisplayName != null && !string.IsNullOrEmpty(userInfo.DisplayName))
            {
                return userInfo.DisplayName;
            }
        }


        return "No name";
    }

    public string GetUserPhoto()
    {
        foreach (IUserInfo userInfo in _auth.CurrentUser.ProviderData)
        {
            if (userInfo.PhotoUrl != null && !string.IsNullOrEmpty(userInfo.PhotoUrl.AbsoluteUri))
            {
                return userInfo.PhotoUrl.AbsoluteUri;
            }
        }

        return null;
    }

    public void SignOut()
    {
        if (IsFB())
        {
            FbScript.Instance.LogOut();
        }

        if (IsGoogle())
        {
            GoogleSignInDemo.Instance.SignOutFromGoogle();
        }

        _auth.SignOut();
        UserController.Instance.LogOut();
        FirebaseDatabaseController.Instance.LoggedOut();
    }

    public void LoginWithService(Credential credential)
    {
        if (_auth.CurrentUser != null && _auth.CurrentUser.IsAnonymous)
        {
            _auth.CurrentUser.LinkAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("ACCOUNT LINKING CANCELED: " + task.Result.ToString());
                    return;
                }

                if (task.IsFaulted)
                {
                    try
                    {
                        throw task.Exception;
                    }
                    catch (FirebaseAccountLinkException)
                    {
                        AccountLinkingProblemCallback();
                    }
                    catch (System.AggregateException)
                    {
                        AccountLinkingProblemCallback();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Another exception during accounts linking:" + e.ToString());
                    }

                    return;
                }

                if (task.IsCompleted)
                {
                    SignInController.Instance.Disable();
                    MenuController.Instance.Enable();
                }

                task.Result.Log("End: ");
            });
            return;
        }

        _auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });
    }

    private void AccountLinkingProblemCallback()
    {
        SignInController.Instance.Disable();
        MenuController.Instance.Enable();
        string title = TextTranslationModule.GetWord("Accounts linking problem");
        string message = TextTranslationModule.GetWord("The account you are currently logged in is already in our database. Please choose another account.");
        UniversalInfoPopUp.Instance.Enable(title, message);
        MenuController.Instance.AddViewToClose(UniversalInfoPopUp.Instance);
    }

    public void AnonymousLogIn()
    {
        if (_auth.CurrentUser != null)
        {
            _auth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(task =>
            {
                task.Result.LogDev("T res: ");
            });
        }

        _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

        });
    }
}
