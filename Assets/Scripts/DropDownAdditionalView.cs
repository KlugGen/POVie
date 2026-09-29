using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DropDownAdditionalView : View
{
    public DropDownAdditionalViewFields portraitFields, landscapeFields;
    private View parentView = null;

    public View shopView;

    static private DropDownAdditionalView instance;

    static public DropDownAdditionalView Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(DropDownAdditionalView))[0] as DropDownAdditionalView;

            return instance;
        }
    }

    public void EnableView(View v)
    {
        parentView = v;
    }

    public void EnableHelp()
    {
        Enable();
        GetAttributes().helpScreen_gameObject.SetActive(true);
    }

    public void EnableSound()
    {
        AudioController.Instance.ChangeState();       
    }

    public void EnableCapital()
    {
        Enable();
        //GetAttributes().capitalScreen_gameObject.SetActive(true);
        GetAttributes().capitalScreenController.Enable();
        //GetAttributes().capital_text.text = UserController.Instance.GetStars().ToString();

    }

   public override void Enable()
    {
        base.Enable();
        GetAttributes().DisableAll();
    }      

    public DropDownAdditionalViewFields GetAttributes()
    {
        if (ViewsController.Instance.GetCurrentOrientation() == ScreenOrientation.Portrait)
            return portraitFields;

        return landscapeFields;
    }

    public void OnboardingButton()
    {
        UnityAction action = delegate ()
        {
            "Enabling onboarding".LogDev();
            GamePlayController.Instance.Disable();
            MapViewController.Instance.Disable();
            OnboardingController.Instance.gameObject.SetActive(true);
            //OnboardingController.Instance.TestMethod();
            Disable();
        };

        if (parentView != null && parentView.confirmBackWithPopup)
        {
            YesNoPopupController.Instance.Enable(action, parentView.confirmationMessage);
            return;
        }

        action.Invoke();
    }

    public void TutorialsButton()
    {
        GetAttributes().tutorials.Enable();
        GetAttributes().t2_gameObject.SetActive(false);
        GetAttributes().t3_gameObject.SetActive(false);
        
        if (UserController.Instance.GetStats().HasDictionary("tut2" + UserController.Instance.GetLanguage()))
        {
            GetAttributes().t2_gameObject.SetActive(true);
        }

        if (UserController.Instance.GetStats().HasDictionary("tut3" + UserController.Instance.GetLanguage()))
        {
            GetAttributes().t3_gameObject.SetActive(true);
        }
    }

    public void TutorialIButton()
    {
        UnityAction action = delegate ()
        {
            GamePlayController.Instance.Disable();
            MapViewController.Instance.Disable();
            GamePlayController.Instance.Enable(Elements.ElementsDatabase.Instance.GetCityByName("Mdina"));
            Disable();
        };

        if (parentView != null && parentView.confirmBackWithPopup)
        {
            YesNoPopupController.Instance.Enable(action, parentView.confirmationMessage);
            return;
        }

        action.Invoke();
    }

    public void TutorialIIButton()
    {
        UnityAction action = delegate ()
        {
            GamePlayController.Instance.Disable();
            MapViewController.Instance.Disable();
            MapViewController.Instance.Enable();
            TutorialAdditionalsController.Instance.SecondTutorial();
            Disable();
        };

        if (parentView != null && parentView.confirmBackWithPopup)
        {
            YesNoPopupController.Instance.Enable(action, parentView.confirmationMessage);
            return;
        }

        action.Invoke();
    }

    public void TutorialIIIButton()
    {       
        UnityAction action = delegate ()
        {
            GamePlayController.Instance.Disable();
            MapViewController.Instance.Disable();
            MapViewController.Instance.Enable();
            TutorialAdditionalsController.Instance.ThirdTutorial();
            Disable();
        };

        if (parentView != null && parentView.confirmBackWithPopup)
        {
            YesNoPopupController.Instance.Enable(action, parentView.confirmationMessage);
            return;
        }

        action.Invoke();
    }

 
    public void EnableShop()
    {
        // check if user is loggrd in on solid account
        var user = FirebaseController.Instance.GetAuthenticatedUser().GetUser();
        
        string title = "";
        string message = TextTranslationModule.GetWord("Please login to access shop");
       
        if (user == null)
        {
            UniversalInfoPopUp.Instance.Enable(title, message);
            MapViewController.Instance.AddViewToClose(UniversalInfoPopUp.Instance);
            return;
        }

        //if (user.IsAnonymous)
        //{
        //    UniversalInfoPopUp.Instance.Enable(title, message);
        //    MapViewController.Instance.AddViewToClose(UniversalInfoPopUp.Instance);
        //    return;
        //} 
            
        MapViewController.Instance.DropDownButton();
        shopView.Enable();
        MapViewController.Instance.AddViewToClose(shopView);
    }

    public override void Disable()
    {
        base.Disable();
        parentView = null;
    }
}

[System.Serializable]
public class DropDownAdditionalViewFields
{
    public GameObject helpScreen_gameObject, soundScreen_gameObject, capitalScreen_gameObject;
    public FAQController faqController;
    public View tutorials;
    public GameObject t1_gameObject, t2_gameObject, t3_gameObject;


    public CapitalScreen capitalScreenController;

    public Text capital_text;

    public void DisableAll()
    {
        if (helpScreen_gameObject != null)
            helpScreen_gameObject.SetActive(false);

        if (soundScreen_gameObject != null)
            soundScreen_gameObject.SetActive(false);

        if (capitalScreen_gameObject != null)
            capitalScreen_gameObject.SetActive(false);

        faqController.Disable();
    }
}
