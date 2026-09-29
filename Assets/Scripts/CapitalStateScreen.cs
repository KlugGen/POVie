using Elements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CapitalStateScreen : MonoBehaviour
{
    public Text starText;
    public Text starsText;
    public Text getMoreText;
    public Text extraText;

    public Animator starButton_animator;

    public VerticalLayoutGroup vGroup;

    const string defaultBottomMessage = "Go to {0} marked countries";
    const string bottomMessage_v2 = "Congratulations! \nYou unlocked new levels:{0} \n\nAs before, fit angles first within highlighted areas \nYet now you have {1} attempts for each highlighted area \nIf you use your {2} attempts, you can continue with angles fitting with {3} previously earned (1 attempt = 1 {3})";
    const string bottomMessage_v3 = "After you complete each level, you will receive an overall score and possibly \u2605 , the latter you will use later as you progress in the game";

    // \u2605

    public void SetStarButtonState()
    {
        if (UserController.Instance.GetStats().HasDictionary("starButtonUnchecked" + UserController.Instance.GetLanguage()))
        {
            // enable animation for star button
            starButton_animator.Play("Flashing");

        }
        else
        {
            starButton_animator.Play("Idle");
        }
    }

    public void Initialize(int stars)
    {
        gameObject.SetActive(true);
        starsText.text = stars.ToString();     
        getMoreText.gameObject.SetActive(stars<1);

        if (UserController.Instance.GetStats().HasDictionary("starButtonUnchecked" + UserController.Instance.GetLanguage()))
        {
            //"delete key".Log();
            UserController.Instance.GetStats().RemoveFromDictionary("starButtonUnchecked" + UserController.Instance.GetLanguage());
            SetStarButtonState();
        }

        if (UserController.Instance.GetStats().GetInt("displayUnlocked" + UserController.Instance.GetLanguage(),0) >0)
        {
            int unlockedId = UserController.Instance.GetStats().GetInt("maxUnlockedId_saved" + UserController.Instance.GetLanguage(),0);

            if(unlockedId == 1)
            {
                extraText.text = bottomMessage_v3;
            }
            else
            {
                //Elements.ElementsDatabase.Instance.
                List<Countries> countries = ElementsDatabase.Instance.GetCountriesByGroupId(unlockedId);
                CountryGroup cg = ElementsDatabase.Instance.groups.Find(o => o.id == unlockedId);

                if (countries != null && cg!= null && countries.Count > 0)
                {
                    string countriesList = "\n";
                    countries.ForEach( o => countriesList += "\n" + o.name);
                    extraText.text =  string.Format(TextTranslationModule.GetWord(bottomMessage_v2), countriesList, -1,-1, "\u2605");
                }
            }           
        }

      
        LayoutRebuilder.ForceRebuildLayoutImmediate(vGroup.GetComponent<RectTransform>());
    }

    public void Start()
    {
        starText.text = string.Format(TextTranslationModule.GetWord(defaultBottomMessage), "\u2605");
    }

    public void Disable()
    {
       gameObject.SetActive(false);
    }

    public void DisplayNewMessage()
    {

    }
}
