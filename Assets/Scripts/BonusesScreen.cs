using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusesScreen : View
{
    static private BonusesScreen instance;

    static public BonusesScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(BonusesScreen))[0] as BonusesScreen;

            return instance;
        }
    }

    public Button highlightedBonus_button;
    public Button autoRotationBonus_button;
    public Button noDistractor_button;
    public Button endlessTimer_button;


    public Text highlightedBonus_text;
    public Text autoRotationBonus_text;
    public Text noDistractor_text;
    public Text endlessTimer_text;

   // public Text endlessTimer_text;

    public List<BoosterListUnit> boosters = new List<BoosterListUnit>();

    private int autoBuy = 0;

    public void Enable(int autoBuy)
    {
        this.autoBuy = autoBuy;
        Enable();
    }

    public override void Enable()
    {
        base.Enable();
        int maxUnlcocked = UserController.Instance.MaxUnlockedGroup();
        //maxUnlcocked.LogDev("Munlocked: ");

        // check if display highlighted buttons
        //boosters.Find(o => o.boosterName == "area highlight").title.text = TextTranslationModule.GetWord("Area highlight");

        boosters.ForEach(delegate(BoosterListUnit b) {
            b.parent.SetActive(false);
            b.starCost.gameObject.SetActive(true);
            b.star.gameObject.SetActive(true);
            b.counter.gameObject.SetActive(false);
            b.description.gameObject.SetActive(true);
            b.activatedText.SetActive(false);
            b.freeToUseText.SetActive(false);
        });

        boosters.Find(o => o.boosterName == "highlighted area").starCost.text = Rules.GameBalance.highlightBonus.ToString();
        boosters.Find(o => o.boosterName == "auto rotation").starCost.text = Rules.GameBalance.autoRotationBonus.ToString();
        boosters.Find(o => o.boosterName == "endless timer").starCost.text = Rules.GameBalance.endlessTimerBonus.ToString();
        boosters.Find(o => o.boosterName == "no distractor").starCost.text = Rules.GameBalance.noDistractorBonus.ToString();

        //    string.Format("{0} <b>{1}{2}</b>", TextTranslationModule.GetWord("Area highlight"), UserController.GameBalance.highlightBonus, "\u2605").Replace(" ", "\n");
        //autoRotationBonus_text.text = string.Format("{0} <b>{1}{2}</b>", TextTranslationModule.GetWord("Auto rotation"), UserController.GameBalance.autoRotationBonus, "\u2605").Replace(" ", "\n");
        //noDistractor_text.text = string.Format("{0} <b>{1}{2}</b>", TextTranslationModule.GetWord("No distractor"), UserController.GameBalance.noDistractorBonus, "\u2605").Replace(" ", "\n");
        //endlessTimer_text.text = string.Format("{0} <b>{1}{2}</b>", TextTranslationModule.GetWord("Endless timer"), UserController.GameBalance.endlessTimerBonus, "\u2605").Replace(" ", "\n");



        //if (!GamePlayController.Instance.bonuses.autoRotationActive)
        //{
        //    boosters.Find(o => o.boosterName == "auto rotation").canvasGroup.interactable = false;
        //    boosters.Find(o => o.boosterName == "auto rotation").canvasGroup.alpha = .3f;
        //}

        //if (!GamePlayController.Instance.bonuses.noDistractorActive)
        //{
        //    boosters.Find(o => o.boosterName == "no distractor").canvasGroup.interactable = false;
        //    boosters.Find(o => o.boosterName == "no distractor").canvasGroup.alpha = .3f;
        //}

        //if (!GamePlayController.Instance.bonuses.endlessTimerActive)
        //{
        //    boosters.Find(o => o.boosterName == "endless timer").canvasGroup.interactable = false;
        //    boosters.Find(o => o.boosterName == "endless timer").canvasGroup.alpha = .3f;
        //}

        //autoRotationBonus_button.interactable = !GamePlayController.Instance.bonuses.autoRotationActive;
        //noDistractor_button.interactable = !GamePlayController.Instance.bonuses.noDistractorActive;
        //endlessTimer_button.interactable = !GamePlayController.Instance.bonuses.endlessTimerActive;

        if (autoBuy > 0)
        {
            switch (autoBuy)
            {
                case 1:
                    //GamePlayController.Instance.bonuses.autoRotationActive = true;
                    UserController.Instance.GetStats().freeAutorotation = true;
                    UserController.Instance.SaveStatistics();
                    break;
                case 2:
                    UserController.Instance.GetStats().freeHighlight = true;
                    UserController.Instance.SaveStatistics();
                    //GamePlayController.Instance.bonuses.h = true;
                    break;
                case 3:
                    //GamePlayController.Instance.bonuses.endlessTimerActive = true;
                    UserController.Instance.GetStats().freeEndlessTimer = true;
                    UserController.Instance.SaveStatistics();
                    break;
                case 4:
                    //GamePlayController.Instance.bonuses.noDistractorActive = true;
                    UserController.Instance.GetStats().freeNoDistractor = true;
                    UserController.Instance.SaveStatistics();
                    break;
            }
        }


        if (Rules.BoostersRules.FirstBoosterAvailable())
        {
            BoosterListUnit ar = boosters.Find(o => o.boosterName == "auto rotation");
            ar.parent.SetActive(true);
            ar.SetState(GamePlayController.Instance.bonuses.autoRotationActive,
                UserController.Instance.GetStats().autoRotation_balance,
                Rules.GameBalance.autoRotationBonus, UserController.Instance.GetStats().freeAutorotation);
        }

        if (Rules.BoostersRules.SecondBoosterAvailable())
        {
            BoosterListUnit highlighted = boosters.Find(o => o.boosterName == "highlighted area");
            highlighted.parent.SetActive(true);
            highlighted.SetState(false, UserController.Instance.GetStats().highlights_balance, Rules.GameBalance.highlightBonus, UserController.Instance.GetStats().freeHighlight);
        }

        if (Rules.BoostersRules.ThirdBoosterAvailable())
        {
            //"all boosters available".LogDev();
            BoosterListUnit et = boosters.Find(o => o.boosterName == "endless timer");
            et.parent.SetActive(true);
            et.SetState(GamePlayController.Instance.bonuses.endlessTimerActive,
                UserController.Instance.GetStats().endlessTimer_balance,
                Rules.GameBalance.endlessTimerBonus, UserController.Instance.GetStats().freeEndlessTimer);

            et.description.text = string.Format(TextTranslationModule.GetWord("Drag-and-drop timer is extended to {0} sec"), Rules.GameBalance.extendedTimer.ToString());

        }

        if (Rules.BoostersRules.AllBoostersAvailable())
        {         
            BoosterListUnit nd = boosters.Find(o => o.boosterName == "no distractor");
            nd.parent.SetActive(true);
            nd.SetState(GamePlayController.Instance.bonuses.noDistractorActive,
                UserController.Instance.GetStats().noDistractor_balance,
                Rules.GameBalance.noDistractorBonus, UserController.Instance.GetStats().freeNoDistractor);
        }
    }

    public void HighlightedBonus()
    {
        GamePlayController.Instance.BuyHighlight();
        Disable();
    }

    public void AutoRotationBonus()
    {
        GamePlayController.Instance.BuyBonus("auto-rotation");
        Disable();
    }

    public void EndlessTimerBonus()
    {
        GamePlayController.Instance.BuyBonus("endless-timer");
        Disable();
    }

    public void NoDistractorBonus()
    {
        GamePlayController.Instance.BuyBonus("no-distractor");
        Disable();
    }

    public override void Disable()
    {
        base.Disable();
        //if (autoBuy > 0)
        //{
        //    if(autoBuy == 2)
        //    {
        //        GamePlayController.Instance.EnableHintHighlight();
        //    }
           
        //    FloatingTextPanel.Instance.Show(new FloatingTextParameters() { text = TextTranslationModule.GetWord("Free Booster On") });
        //}
                

        autoBuy = 0;
        if (GamePlayController.Instance.IsEnabled())
            GamePlayController.Instance.SetTimerState(false);
    }
}

[System.Serializable]
public class BoosterListUnit
{
    public string boosterName;
    public GameObject parent;
    public CanvasGroup canvasGroup;
    public Text title, description;
    public Text counter;
    public Text starCost;
    public GameObject activatedText;
    public GameObject freeToUseText;
    public Image star;
     
    public void SetState(bool alreadyActive, int balance, int cost, bool freeToUse)
    {
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1f;
        
        if (alreadyActive)
        {
            starCost.gameObject.SetActive(false);
            star.gameObject.SetActive(false);
            counter.gameObject.SetActive(false);         

            canvasGroup.interactable = false;
            canvasGroup.alpha = .8f;
            description.gameObject.SetActive(true);
            activatedText.SetActive(true);
            return;
        }

        if (freeToUse)
        {
            starCost.gameObject.SetActive(false);
            star.gameObject.SetActive(false);
            counter.gameObject.SetActive(false);

            //canvasGroup.interactable = false;
           // canvasGroup.alpha = .8f;
            description.gameObject.SetActive(true);
            activatedText.SetActive(false);
            freeToUseText.SetActive(true);            
            return;
        }

        if (balance == 0)
        {
            starCost.gameObject.SetActive(true);
            star.gameObject.SetActive(true);
            counter.gameObject.SetActive(false);

            starCost.text = cost.ToString();
        }
        else
        {
            starCost.gameObject.SetActive(false);
            star.gameObject.SetActive(false);
            counter.gameObject.SetActive(true);

            counter.text = "x" + balance.ToString();
        }
    }
}