using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MenuController : View
{
    public GameObject tweenTest;
    public Text userName_text;
    public Image userImage;

    static private MenuController instance;

    static public MenuController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(MenuController))[0] as MenuController;

            return instance;
        }
    }  

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {            
#if !UNITY_EDITOR && UNITY_ANDROID
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call<bool>("moveTaskToBack", true);

#endif
        }
    }

    public override void Enable()
    {
        base.Enable();

        var user = FirebaseController.Instance.GetAuthenticatedUser();

        if (user.HasUser())
        {
            userName_text.text = user.GetUserName();

            if (UserController.Instance.logInReward)
            {
                if (user.IsFB() || user.IsGoogle())
                {
                    UserController.Instance.AddStars(Rules.GameBalance.logInReward, "log_in_reward");
                    UserController.Instance.logInReward = false;
                    SignInReward.Instance.Enable();
                }
            }

            string photo = user.GetUserPhoto();

            if (photo == null)
            {
                userImage.sprite = AssetsDatabase.AssetsDatabase.Instance.sprites.anonymousSprite;
                return;
            }

            DKK.ServerManager.GetTextureFromServer(photo, delegate (bool b, Texture2D t)
            {
                if (b)
                {
                    userImage.sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.one * .5f);
                }
            });            
        }               
    }

    #region Debug Options
    public void PrintAccountDetails()
    {
        var user = FirebaseController.Instance.GetAuthenticatedUser().GetUser();
        bool b = user.DisplayName != null;
        if (b)
        {
            "display name not null".Log();
            user.DisplayName.Log("DP: ");
        }

        b = user.PhotoUrl != null;
        if (b)
        {
            "photo not null".Log();
            user.PhotoUrl.Log("Photo: ");
        }

        FirebaseController.Instance.GetAuthenticatedUser().GetProvider().Log("Provider: ");

        "PROIVIDERS".Log();

        foreach (Firebase.Auth.IUserInfo info in user.ProviderData)
        {
            info.ProviderId.Log("Provider: ");

            if (info.PhotoUrl != null)
                info.PhotoUrl.Log("Picture: ");
            else
            {
                "null picture".Log();
            }

            if (info.DisplayName != null)
                info.DisplayName.Log("Name: ");
            else
            {
                "null name".Log();
            }
        }
    }

    public void PrintEmail_Console()
    {
        if (FirebaseController.Instance.GetAuthenticatedUser().HasUser())
        {
            FirebaseController.Instance.GetAuthenticatedUser().GetUser().Email.Log("Email: ");
        }

    }
    #endregion
}
