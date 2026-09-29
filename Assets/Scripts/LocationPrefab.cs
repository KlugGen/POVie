using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elements;

public class LocationPrefab : MonoBehaviour, DKK.IClear
{
    private Cities _city;

    public Text preText, postText;

    public Text cityText;
    public Text hashTagText;
    public Text lastScore;
    public Text hashtagText;
    public Text starText;

    public Image shopIcon;
    public Image starIcon;
    public Image starLowerIcon;

    public Image balanceIcon;

    public GameObject removeBlockade_gameObject;
    public Text removeBlockade_text;

    public GameObject starParent;

    public List<Image> stars = new List<Image>();

    public GameObject availablePanel;

    // TODO: change to 4
    static int cityForStarsGroup = 4;

    int maxUnlocked;

    public CountryGroup _group;

    public bool godMode = false;
    private Coroutine actualizactionCoroutine = null;

    public void Initialize(Cities city)
    {

        _city = city;

        //_city.saves.Count.Log("CNT: ");
        //_city.saves.ForEach( o => Debug.Log(string.Format("{4}: {0} {1} {2} {3} ", o.language, o.score, o.date, o.stars, _city.name)));
        //_city.saves.ForEach(o => Debug.Log(o.stats.print()));
        maxUnlocked = UserController.Instance.maxUnlockdedGroup;
        cityText.text = city.name;

        hashTagText.text = "";

        removeBlockade_gameObject.DeActivate();
        preText.gameObject.SetActive(false);
        postText.gameObject.SetActive(false);
        shopIcon.gameObject.DeActivate();

        _group = _city.GetMyGroup();
        availablePanel.Activate();

        //_group.ticketPrice.Log("Ticket: ");

        starText.enabled = false;
        starIcon.enabled = false;
        // check if not blocked
        if (_city.GetMyGroup().cooloff && !_city.GetMyGroup().serializedFields.coolOffDone)
        {
            postText.gameObject.Activate();
            starLowerIcon.color = Color.gray;
            postText.text = TextTranslationModule.GetWord("Group is now locked");
            return;
        }

        bool starsAvailable = _city.CheckIfStarsAvailable();

        if (_group.id >= cityForStarsGroup)
        {
            if (!city.GetCurrentLanguageSet().wasBought)
            {
                //"not bought".Log();
                starText.enabled = true;
                starIcon.enabled = true;
                starIcon.color = SpritesDatabase.Instance.starGreen;
                postText.gameObject.Activate();
                preText.gameObject.DeActivate();
                starLowerIcon.gameObject.Activate();
                starLowerIcon.color = SpritesDatabase.Instance.starGreen;
                postText.text = TextTranslationModule.GetWord("Ticket price");
                
                starText.text = _group.ticketPrice.ToString();
                balanceIcon.gameObject.DeActivate();
                shopIcon.gameObject.Activate();
            }
            else
            {
                if (starsAvailable)
                {
                    postText.gameObject.Activate();
                    postText.text = TextTranslationModule.GetWord("Available");
                    //postText.text = string.Format(TextTranslationModule.GetWord("Spend {0} stars"), UserController.Instance.balance.cityStartCost).FirstUpper();
                }
                else
                {
                    postText.gameObject.Activate();
                    starLowerIcon.color = Color.gray;
                    postText.text = TextTranslationModule.GetWord("No more stars available");
                }
            }
        }
        else
        {
            if (maxUnlocked >= cityForStarsGroup && _group.id > 1)
            {
                if (starsAvailable)
                {
                    postText.gameObject.Activate();
                    postText.text = TextTranslationModule.GetWord("Available");
                }
                else
                {
                    postText.gameObject.Activate();
                    starLowerIcon.color = Color.gray;
                    postText.text = TextTranslationModule.GetWord("No more stars available");
                }
            }
            else
            {
                if (starsAvailable)
                {
                    postText.gameObject.SetActive(true);
                    postText.text = TextTranslationModule.GetWord("Available");
                }
                else
                {
                    postText.gameObject.Activate();
                    starLowerIcon.color = Color.gray;
                    postText.text = TextTranslationModule.GetWord("No more stars available");
                }
            }
        }

        if (_city.type == CitiesGameplayType.tutorial)
        {
            postText.gameObject.Activate();
            starLowerIcon.gameObject.DeActivate();
            postText.text = TextTranslationModule.GetWord("Play tutorial");
            postText.color = Color.gray;
        }

        CheckButtonStatus();


        //if (group.id > 1 && maxUnlocked >= cityForStarsGroup) {

        //    if (UserController.Instance.GetStars() < UserController.Instance.balance.cityStartCost && _city.WasUnsuccesfullyPlayed())
        //    {
        //        cityText.color = Color.gray;
        //    }
        //}
        //else
        //{
        //    int runs = city.GetCompleteRunsCount(UserController.Instance.GetLanguage());

        //    //switch (runs)
        //    //{
        //    //    case 0:
        //    //        hashTagText.text += " [new]";
        //    //        break;
        //    //    case 1:
        //    //        hashTagText.text += " [dis 1]";
        //    //        break;
        //    //    case 2:
        //    //        hashTagText.text += " [dis 2]";
        //    //        break;
        //    //    case 3:
        //    //        hashTagText.text += " [dis 3]";
        //    //        break;     
        //    //}

        //    if (runs > 3 && runs <7)
        //    {              
        //        //float scoreToBeat = city.GetScoreTarget(UserController.Instance.GetLanguage());  
        //        //hashTagText.text += string.Format("[target {0}]", (int)scoreToBeat);
        //    }                    
        //}

        string language = UserController.Instance.GetLanguage();

        hashTagText.text = "";

        int bestScore = city.GetBestPoints(language);
        //city.GetAllStars(UserController.Instance.GetLanguage()).LogDev("All stars: ");
        int score = city.GetCurrentLanguageSet().allStars;

        //score.LogDev("Score for " + city.name + ": ");

        if (score > 0)
        {
            starText.enabled = true;
            starIcon.enabled = true;
            starText.text = score.ToString();
            //if(city.GetCurrentLanguageSet().wasBought)
            balanceIcon.gameObject.Activate();
        }

        if (_group.id > 0)
        {
            if (bestScore > 0)
            {
                //lastScore.text = bestScore.ToString();

                // check by language city.GetTextOverlayController()
                //TextOverlayController toc = city.GetTextOverlayController();

                //if (toc != null)
                //{
                //    foreach (TextControllerSet tcs in toc.sets)
                //    {
                //        if (tcs.language.ToLower() == language)
                //        {
                //            //hashTagText.text += string.Format("#{0}", tcs.hashTagWord);
                //        }
                //    }
                //}
            }
            else
            {
                lastScore.text = hashtagText.text = "";
            }
        }
        else
        {
            lastScore.text = TextTranslationModule.GetWord("tutorial");
            hashtagText.text = "";
        }
    }

    private void CheckButtonStatus(bool firstPass = true)
    {
        if (_city.type != CitiesGameplayType.tutorial && _city.CheckIfStarsAvailable() && _city.WasPlayedToday())
        {
            postText.gameObject.Activate();
            starLowerIcon.gameObject.DeActivate();
            postText.text = string.Format(TextTranslationModule.GetWord("Playable again in {0}"), _city.GetReplayTimeoutInString());

            removeBlockade_gameObject.Activate();
            removeBlockade_text.text = Rules.GameBalance.replayCost.ToString();
        }

        if (_city.type != CitiesGameplayType.tutorial && _city.CheckIfStarsAvailable() && _city.WasRestartLimitReached())
        {
            postText.gameObject.Activate();
            starLowerIcon.gameObject.DeActivate();
            postText.text = string.Format(TextTranslationModule.GetWord("Playable again in {0}"), _city.GetRestartTimeoutInString());

            removeBlockade_gameObject.Activate();
            removeBlockade_text.text = Rules.GameBalance.restartCost.ToString();
        }

        if (firstPass)
        {
            if (actualizactionCoroutine != null)
                StopCoroutine(actualizactionCoroutine);

            actualizactionCoroutine = StartCoroutine(ACtualizationCoroutine());
        }
    }

    private IEnumerator ACtualizationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            CheckButtonStatus(false);
        }
    }

    public void RemoveBlockadeButton()
    {
        if (_city.type != CitiesGameplayType.tutorial && _city.CheckIfStarsAvailable() && _city.WasPlayedToday())
        {
            if (UserController.Instance.GetStars() < Rules.GameBalance.replayCost)
            {
                OutOfStarsScreen.Instance.Enable(LocationsView.Instance);
                return;
            }
            _city.GetCurrentLanguageSet().lastSuccessfullPlayDate = "";
            _city.GetCurrentLanguageSet().restartingCount = 0;
            UserController.Instance.RemoveStars(Rules.GameBalance.replayCost, "replayBlockadeRemove", "", true);
            UserController.Instance.SaveGame();
        }

        if (_city.type != CitiesGameplayType.tutorial && _city.CheckIfStarsAvailable() && _city.WasRestartLimitReached())
        {
            if (UserController.Instance.GetStars() < Rules.GameBalance.restartCost)
            {
                OutOfStarsScreen.Instance.Enable(LocationsView.Instance);
                return;
            }
            _city.GetCurrentLanguageSet().restartingCount = 0;
            UserController.Instance.RemoveStars(Rules.GameBalance.restartCost, "restartBlockadeRemove", "", true);
            UserController.Instance.SaveGame();
        }

        LocationsView.Instance.Disable();
        LocationsView.Instance.Enable(_city.country);
    }

    public void OnClick()
    {
        // open gameplay with give city
        if (_city == null || _city.gameplay == null)
            return;

        if (godMode)
        {
#if UNITY_EDITOR
            ProceedToPlay();
            return;
#endif
        }

        if (_city.GetMyGroup().cooloff && !_city.GetMyGroup().serializedFields.coolOffDone)
        {
            CoolOffScreen.Instance.Enable(_city.GetMyGroup());
            LocationsView.Instance.AddViewToClose(CoolOffScreen.Instance);
            return;
        }

        //if (UserController.Instance.GetStars() <1 && _city.WasUnsuccesfullyPlayed() && _city.GetMyGroup().id>1) 
        //{
        //    Debug.Log("Not allowing to proceed");
        //    return;
        //}

        if (_city.GetMyGroup().id >= cityForStarsGroup)
        {
            if (!_city.GetCurrentLanguageSet().wasBought)
            {
                if (UserController.Instance.GetStars() < _group.ticketPrice)
                {
                    // no stars
                    TutorialAdditionalsController.Instance.FourthTutorial();
                    //OutOfStarsScreen.Instance.Enable();
                    return;
                }
                else
                {
                    BuyConfirm();
                    return;
                    //LocationsView.Instance.AddViewToClose(YesNoPopupController.Instance);

                    //YesNoPopupController.Instance.Enable(
                    //    BuyConfirm,
                    //    string.Format(TextTranslationModule.GetWord("Buy city entrance ({0}\u2605)"), _group.ticketPrice),
                    //    true,
                    //    null,
                    //    true);

                    //return;
                }
            }
        }

        if (_city.type != CitiesGameplayType.tutorial && _city.CheckIfStarsAvailable() && _city.WasPlayedToday())
        {
            // TODO: check if enable gameplay
            ReplayImpossibleScreen.Instance.Enable();
            return;
        }

        if (_city.type != CitiesGameplayType.tutorial && _city.CheckIfStarsAvailable() && _city.WasRestartLimitReached())
        {
            // TODO: check if enable gameplay
            ReplayImpossibleScreen.Instance.EnableRestartScreen();
            return;
        }


        ProceedToPlay();
    }

    public void BuyConfirm()
    {
        UserController.Instance.RemoveStars(_group.ticketPrice, "ticket_buy", _city.name, true);
        _city.GetCurrentLanguageSet().wasBought = true;
        UserController.Instance.SaveGame();
        ProceedToPlay();
    }

    public void ProceedToPlay()
    {
        LocationsView.Instance.Disable();
        GamePlayController.Instance.Enable(_city);
    }

    public void Clear()
    {
        "clear interface call".Log();

        if(actualizactionCoroutine != null)
        {
            StopCoroutine(actualizactionCoroutine);
        }
    }
}
