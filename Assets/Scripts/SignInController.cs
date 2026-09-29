using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignInController : View
{

    static private SignInController instance;

    static public SignInController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(SignInController))[0] as SignInController;

            return instance;
        }
    }

    public GameObject fb_btn, anonymous_btn, google_btn;

    public GameObject logInParent, helloParent;

    public GameObject avatarParent;
    public Image avatarImage;
    public Text nicknameText;

    public GameObject loggingInText;

    public List<GameObject> buttons = new List<GameObject>();
        
    public override void Enable()
    {
        base.Enable();
        loggingInText.SetActive(false);
        helloParent.SetActive(false);
        logInParent.SetActive(true);
        avatarParent.SetActive(false);
        fb_btn.Activate();
        anonymous_btn.Activate();

        if (!FirebaseController.Instance.IsReady())
        {
            FirebaseController.Instance.InitializeFirebaseAuthentication();
            buttons.ForEach( o => o.DeActivate());
        }
    }

    public void NotLoggedIn()
    {
        google_btn.Activate();
        fb_btn.Activate();
        anonymous_btn.Activate();


#if UNITY_IOS
   google_btn.SetActive(false);
#endif
    }

    public void Google_button()
    {
        loggingInText.SetActive(true);
        GoogleSignInDemo.Instance.SignInWithGoogle();
        buttons.ForEach(o => o.DeActivate());
    }

    public void FB_button()
    {
        loggingInText.SetActive(true);
        FbScript.Instance.InitializeFB();
        buttons.ForEach(o => o.DeActivate());
    }

    public void Anonymous_button()
    {
        loggingInText.SetActive(true);
        FirebaseController.Instance.AnonymousLogIn();
        buttons.ForEach(o => o.DeActivate());
    }

    public void AfterFirebaseAuthenticationCallback()
    {
        if (FirebaseController.Instance.GetAuthenticatedUser().HasUser() && this.IsEnabled())
        {
            AfterLogInAction();
            return;
        }

        buttons.ForEach(o => o.Activate());

#if UNITY_IOS
   google_btn.SetActive(false);
#endif
    }

    public void AfterLogInAction()
    {
        if (FirebaseDatabaseController.Instance.IsInitialized())
            FirebaseDatabaseController.Instance.ReinitializeAfterBeingRelogged();
        else
            FirebaseDatabaseController.Instance.Initialize();

        Firebase.Analytics.FirebaseAnalytics.SetUserId(FirebaseController.Instance.GetAuthenticatedUser().GetUser().UserId);

        AnalyticsController.Instance.LogEvent( Firebase.Analytics.FirebaseAnalytics.EventLogin,  new Firebase.Analytics.Parameter[] {
    new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterMethod, "AfterLogInAction"),});

        FirebaseMessaging.Instance.Initialize();

        InitializeHelloUISection();

        DKK.CommonManager.Coroutine(delegate ()
        {
            FirebaseDatabaseController.Instance.LoadSaves();
        }, 1f);
    }

    private void InitializeHelloUISection()
    {
        logInParent.SetActive(false);
        helloParent.SetActive(true);

        FirebaseAuthenticatedUser authenticatedUser = FirebaseController.Instance.GetAuthenticatedUser();

        nicknameText.text = authenticatedUser.GetUserName();
        string photo = authenticatedUser.GetUserPhoto();

        if (photo == null)
        {
            avatarParent.SetActive(true);
            avatarImage.sprite = AssetsDatabase.AssetsDatabase.Instance.sprites.anonymousSprite;
            return;
        }

        DKK.ServerManager.GetTextureFromServer(photo, delegate (bool success, Texture2D t)
            {
                if (!success)                
                    return;
                
                avatarParent.SetActive(true);
                avatarImage.sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.one * .5f);
            });
    }

    public void AfterLoadingSavesAction()
    {
        if (ServerTimeController.Instance.IsReady() && FirebaseDatabaseController.Instance.IsAllDataReady())
        {         
            Disable();
            MenuController.Instance.Enable();
            AdditionalScreens();
            AudioController.Instance.ChangeAmbientSoundtrack();
        }
    }

    private void AdditionalScreens()
    {
        if (DailyRewardModule.Instance.ShouldDisplay())
        {
            DailyRewardModule.Instance.Check();
        }
        else
        {
            Elements.GameMode.DisplayPopUpExplenation(UserController.Instance.gameMode);
        }
    }
}
