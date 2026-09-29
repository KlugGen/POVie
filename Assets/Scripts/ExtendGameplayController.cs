using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtendGameplayController : View
{
    static private ExtendGameplayController instance;

    static public ExtendGameplayController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(ExtendGameplayController))[0] as ExtendGameplayController;

            return instance;
        }
    }

    public ExtendGameplayControllerAttributes landscapeFields, portraitFields;

    public void Enable(string title)
    {
        GetAttributes().title_text.text = TextTranslationModule.GetWord(title);
        Enable();
    }

    public override void Enable()
    {
        base.Enable();
        if (UserController.Instance.GetStars() < Rules.GameBalance.extendGameplayCost)
        {
            GetAttributes().starButton_image.enabled = false;
            GetAttributes().buy_button.interactable = false;
            GetAttributes().starsAmount_text.text = TextTranslationModule.GetWord("Get more stars");
        }
        else
        {
            GetAttributes().starButton_image.enabled = true;
            GetAttributes().buy_button.interactable = true;
            string s = string.Format(TextTranslationModule.GetWord("Spend {0} stars"), Rules.GameBalance.extendGameplayCost.ToString());
            GetAttributes().starsAmount_text.text = s;
        }
    }

    public void Buy_Click()
    {
        if (UserController.Instance.GetStars() > Rules.GameBalance.extendGameplayCost)
        {
            UserController.Instance.RemoveStars(Rules.GameBalance.extendGameplayCost);
            GamePlayController.Instance.ContinueGameplay();
            Disable();
        }
    }

    public void Ads_Click()
    {
       
    }

    public void GoToGameplay()
    {
        if (GamePlayController.Instance.IsEnabled())
            GamePlayController.Instance.SetTimerState(false);

        Disable();
    }

    public void Cancel_Click()
    {
        Disable();
        GamePlayController.Instance.Disable();
        MapViewController.Instance.Enable();
    }

    public ExtendGameplayControllerAttributes GetAttributes()
    {
        if (ViewsController.Instance.GetCurrentOrientation() == ScreenOrientation.Portrait)
            return portraitFields;

        return landscapeFields;
    }

    [System.Serializable]
    public class ExtendGameplayControllerAttributes
    {
        public Image starButton_image;
        public Button buy_button;
        public Text title_text;
        public Text starsAmount_text;

    }
}
