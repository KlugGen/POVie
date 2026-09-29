using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : View
{

    static private SettingsView instance;

    static public SettingsView Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(SettingsView))[0] as SettingsView;

            return instance;
        }
    }

    public Text languageNamePortrait, languageNameLandscape;
    public SettingsViewProperites portraitFields, landscapeFields;

    public GameObject fb_button, google_button;

    public override void Enable()
    {
        base.Enable();

        string lang = UserController.Instance.GetLanguage();

        //bool relaxModeEnabled = PlayerPrefs.GetInt("mode_relax_" + lang, 0) == 1;
        //bool engagModeEnabled = PlayerPrefs.GetInt("mode_engaging_" + lang, 0) == 1;
        //bool enterModeEnabled = PlayerPrefs.GetInt("mode_entertaining_" + lang, 0) == 1;

        //portraitFields.toggle[1].transform.parent.gameObject.SetActive(relaxModeEnabled);
        //portraitFields.toggle[2].transform.parent.gameObject.SetActive(engagModeEnabled);
        //portraitFields.toggle[3].transform.parent.gameObject.SetActive(enterModeEnabled);        


        //portraitFields.logIn_title.SetActive(true);
       // portraitFields.logIn_button.SetActive(true);
        //portraitFields.nickname_text.text = "";
       // portraitFields.logOut_button.SetActive(false);

        //if (SignIn.Instance.user == null)
        //{
        //    portraitFields.logIn_title.SetActive(true);
        //    portraitFields.logIn_button.SetActive(true);
        //    portraitFields.nickname_text.text = "";
        //    portraitFields.logOut_button.SetActive(false);
            
        //}
        //else
        //{
        //    portraitFields.logOut_button.SetActive(true);
        //    portraitFields.logIn_title.SetActive(false);
        //    portraitFields.logIn_button.SetActive(false);
        //    portraitFields.nickname_text.text = SignIn.Instance.user.DisplayName;
        //}

        //languageNameLandscape.text = UserController.Instance.GetStats().GetString("lastLang", "English");

        languageNamePortrait.text = UserController.Instance.GetLanguage().FirstUpper();

        skipSetingMode = true;

        int mode = UserController.Instance.GetGameModeID();

        portraitFields.toggle.ForEach(o => o.isOn = false);
        portraitFields.toggle[mode].isOn = true;

        landscapeFields.toggle.ForEach(o => o.isOn = false);
        landscapeFields.toggle[mode].isOn = true;

        portraitFields.nickname_text.text = FirebaseController.Instance.GetAuthenticatedUser().GetUserName();

        skipSetingMode = false;

        fb_button.SetActive(false);
        google_button.SetActive(false);

        if (FirebaseController.Instance.GetAuthenticatedUser().GetUser() != null && FirebaseController.Instance.GetAuthenticatedUser().GetUser().IsAnonymous)
        {
            fb_button.SetActive(true);
            google_button.SetActive(true);
        }

#if UNITY_IOS
   google_button.SetActive(false);
#endif

        //portraitFields.signOut_button.SetActive(!FirebaseController.Instance.GetUser().IsAnonymous);
    }

    private bool skipSetingMode = false;

    public void SetModeExternaly(int mode, View context = null)
    {
        if (context == null)
            context = this;

        string lang = UserController.Instance.GetLanguage();

        SetMode(mode);

        bool modeAlreadyUsed = UserController.Instance.GetStats().GetInt("mode_used_" + mode + "_" + lang, 0) == 1;

        if (!modeAlreadyUsed && mode != 0)
        {
            context.AddViewToClose(ModesDescriptionScreen.Instance);
        }
    }

    public void SetMode(int mode)
    {     
        if (skipSetingMode)
            return;

        string lang = UserController.Instance.GetLanguage();

        //if (externalContext == null)
        //    externalContext = this;

        //bool relaxModeEnabled = PlayerPrefs.GetInt("mode_relax_" + lang, 0) == 1;
        //bool engagModeEnabled = PlayerPrefs.GetInt("mode_engaging_" + lang, 0) == 1;
        //bool enterModeEnabled = PlayerPrefs.GetInt("mode_entertaining_" + lang, 0) == 1;
        //bool modesAvailable = (mode == 1 && !engagModeEnabled) || (mode == 2 && !enterModeEnabled) || (mode == 3 && !relaxModeEnabled);

        List<Elements.Cities> cities = Elements.ElementsDatabase.Instance.GetAllDoneCities();
       
        if (mode>0 && cities.Count < 7)
        {         
            portraitFields.toggle.ForEach(o => o.SetIsOnWithoutNotify(false));
            portraitFields.toggle[0].SetIsOnWithoutNotify(true);
            //AddViewToClose(ModesUnavailable.Instance);
            ModesUnavailable.Instance.Enable(this);
            return;
        }        

        int previousMode = UserController.Instance.GetGameModeID();
        UserController.Instance.SetGameMode(mode);
        //mode.LogDev("Mode after update: ");

        if (mode != previousMode)
        {
          
            AudioController.Instance.ChangeAmbientSoundtrack();
            //if (mode > 0)
            //{
            //    FloatingTextPanel.Instance.Show(
            //        new FloatingTextParameters()
            //        {
            //            text = Elements.ElementsDatabase.Instance.GetGameModeById(mode).GetModeIsOnMessage()
            //        }
            //        );
            //}
            //else
            //{
            //    FloatingTextPanel.Instance.Hide();
            //}

            Elements.GameMode.DisplayExplentation(mode);
        }
         
        UserController.Instance.gameMode = mode;

        //bool modeAlreadyUsed = PlayerPrefs.GetInt("mode_used_" + mode + "_" + lang, 0) == 1;
        //if (!modeAlreadyUsed && mode != 0)

            
    }

    public void LogOut()
    {
        // check if you are anonymous and inform about consequences

        // sign out

        FirebaseController.Instance.SignOut();
        Disable();
        SignInController.Instance.Enable();
    }

    public void LogIn()
    {

    }

    public void FBLogIn()
    {
        Disable();
        SignInController.Instance.Enable();
        SignInController.Instance.FB_button();
    }

    public void GoogleLogIn()
    {
        Disable();
        SignInController.Instance.Enable();
        GoogleSignInDemo.Instance.linking = true;
        SignInController.Instance.Google_button();
    }
}

[System.Serializable]
public class SettingsViewProperites
{
    public GameObject logIn_title;
    public GameObject logIn_button;
    public GameObject logOut_button;
    public Text nickname_text;
    public List<Toggle> toggle = new List<Toggle>();

    public GameObject signOut_button;
}