using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Elements;

public class SuccessScreenController : View
{
    static private SuccessScreenController instance;

    static public SuccessScreenController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(SuccessScreenController))[0] as SuccessScreenController;

            return instance;
        }
    }

    public ScoreScreenFields landscapeFields, portraitFields;

    public Sprite startFull, starEmpty;
    private bool avoidSecondScreen = false;

    private int lastStarsReward = 0;

    public void Enable(int points, int stars = 1, string hashTagWord = "", string hashTagSentence = "", ScoreStatistics statistics = null, Cities city = null)
    {
        "enabling success".LogDev();
        lastStarsReward = stars;

        Reset();
        Enable();

        if (Rules.AdsRules.MultiplyReward(stars))
        {
            GetAttributes().advertiseButton.SetActive(true);
        }
        else
        {
            GetAttributes().advertiseButton.SetActive(false);
        }

        if (statistics != null)
        {    
            if (statistics.engagingScore > -1)
            {
                GetAttributes().engagingProgress.progressParent.SetActive(true);
                GetAttributes().engagingProgress.progress.ActualizeValue(statistics.engagingScore, statistics.engagingMax);
            }

            if (statistics.entertainingScore > -1)
            {
                GetAttributes().entertainingProgress.progressParent.SetActive(true);
                GetAttributes().entertainingProgress.progress.ActualizeValue(statistics.entertainingScore, statistics.entertainingMax);
            }

            if (statistics.relaxingMax > -1)
            {
                GetAttributes().relaxingProgress.progressParent.SetActive(true);
                GetAttributes().relaxingProgress.progress.ActualizeValue(statistics.relaxingScore, statistics.relaxingMax);
            }
        }

        GetAttributes().starCountSection.SetActive(stars>0);

        GetAttributes().starReward_text.text = "+0";
        GetAttributes().capitalStars_text.text = TextTranslationModule.GetWord("Balance") + ": " + UserController.Instance.GetStars().ToString();
                    

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetAttributes().starRewardLineII.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetAttributes().starRewardLine.GetComponent<RectTransform>());

        if (city.GetController().CurrentSet() != null)
        {
            GetAttributes().personalTouch.text = city.GetController().CurrentSet().personalTouch;
            //GetAttributes().personalTouch.
            GetAttributes().secondScreenText.text = city.GetController().GetCurrentSentenceWithWord();
        }

        GetAttributes().avatarImage.sprite = city.avatar;
        string author_name = string.IsNullOrEmpty(city.author) ? "" : string.Format("@{0}", city.author);
        GetAttributes().authorName_text.text = author_name;
        GetAttributes().hashtagWord.text = hashTagWord.ToUpper();

        avoidSecondScreen = string.IsNullOrEmpty(hashTagSentence);
        if (avoidSecondScreen)
        {
            GetAttributes().firstScreen.gameObject.SetActive(true);
            GetAttributes().secondScreen.gameObject.SetActive(false);
        }
        else
        {
            GetAttributes().firstScreen.gameObject.SetActive(false);
            GetAttributes().secondScreen.gameObject.SetActive(true);
        }    

    }

    //private int starsMultiplier = 2;

    public void MultiplyReward()
    {
        try
        {
            int additionalStars = (lastStarsReward * Rules.GameBalance.adsScoreMultiplier) - lastStarsReward;
            UserController.Instance.AddStars(additionalStars, "ads_multiplier");

            //GetAttributes().starReward_text.text = "+" + (lastStarsReward * Rules.GameBalance.adsScoreMultiplier).ToString();
            GetAttributes().capitalStars_text.text = TextTranslationModule.GetWord("Balance") + ": " + UserController.Instance.GetStars().ToString();
            GetAttributes().advertiseButton.SetActive(false);

            //GetAttributes().starCountSection.transform.localScale = Vector3.one;
            //GetAttributes().starCountSection.transform.DOScale(Vector3.one * 1.5f, .1f).SetLoops(additionalStars * 2, LoopType.Yoyo);

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }

            animationCoroutine = StartCoroutine(PulseAnimationCoroutine(additionalStars, lastStarsReward, lastStarsReward + additionalStars));


            LayoutRebuilder.ForceRebuildLayoutImmediate(GetAttributes().starRewardLineII.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetAttributes().starRewardLine.GetComponent<RectTransform>());
        }catch(System.Exception e)
        {
            e.ToString().Log();
        }
    }

    [NaughtyAttributes.Button("P2")]
    public void Pulse2()
    {
        //Pulse(2);
        animationCoroutine = StartCoroutine(PulseAnimationCoroutine(2, 1, 2));
    }

    [NaughtyAttributes.Button("P4")]
    public void Pulse4()
    {
        //Pulse(4);
        animationCoroutine = StartCoroutine(PulseAnimationCoroutine(4, 1, 4));
    }

    private Coroutine animationCoroutine = null;

    IEnumerator PulseAnimationCoroutine(int counter, int startValue, int endValue)
    {
        yield return new WaitForSeconds(1f);
        int tempValue = startValue;
        float animTime = .2f;
        bool anim = true;
        float prcent10 = (animTime * 10f) / 100f;

        while (anim)
        {
            GetAttributes().starCountSection.transform.localScale = Vector3.one;
            GetAttributes().starCountSection.transform.DOScale(Vector3.one * 1.5f, animTime).SetLoops(1, LoopType.Yoyo);
            GetAttributes().starReward_text.text = "+" + tempValue.ToString();

            if (tempValue == endValue)
                anim = false;
            else
            {
                tempValue++;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetAttributes().starRewardLine.GetComponent<RectTransform>());

            yield return new WaitForSeconds(animTime+ prcent10);
        }
    }

    public void Pulse(int amount)
    {
        GetAttributes().starCountSection.transform.localScale = Vector3.one;
        GetAttributes().starCountSection.transform.DOScale(Vector3.one * 1.5f, .1f).SetLoops(amount * 2, LoopType.Yoyo);
    }

    public void ContinueClick()
    {
        //if (avoidSecondScreen)
        //{
        //    ContinueClickSecond();
        //    return;
        //}

        GetAttributes().firstScreen.gameObject.SetActive(true);
        GetAttributes().secondScreen.gameObject.SetActive(false);

        if (lastStarsReward > 0)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            animationCoroutine = StartCoroutine(PulseAnimationCoroutine(lastStarsReward, 1, lastStarsReward));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetAttributes().starRewardLineII.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetAttributes().starRewardLine.GetComponent<RectTransform>());
    }

    public void Reset()
    {
        GetAttributes().secondScreen.gameObject.SetActive(false);
        GetAttributes().firstScreen.gameObject.SetActive(false);
        GetAttributes().basicProgress.progressParent.SetActive(false);
        GetAttributes().engagingProgress.progressParent.SetActive(false);
        GetAttributes().entertainingProgress.progressParent.SetActive(false);
        GetAttributes().relaxingProgress.progressParent.SetActive(false);
    }

    public void ContinueClickSecond()
    {
        GamePlayController.Instance.Disable();
        Disable();
        MapViewController.Instance.Enable();
    }

    public ScoreScreenFields GetAttributes()
    {
        if (ViewsController.Instance.GetCurrentOrientation() == ScreenOrientation.Portrait)
            return portraitFields;

        return landscapeFields;
    }

    public override void Disable()
    {      
        base.Disable();
        if(animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    [System.Serializable]
    public class ScoreStatistics
    {
        //public int basicScore = -1;
        //public int basicMax = -1;
        public int engagingScore = -1;
        public int engagingMax = -1;
        public int entertainingScore = -1;
        public int entertainingMax = -1;
        public int relaxingScore = -1;
        public int relaxingMax = -1;
        public int gameMode;

        public ScoreStatistics()
        {
            if (FirebaseDatabaseController.Instance.IsStatisticLoaded())
                gameMode = UserController.Instance.GetGameMode().id;
        }

        public ScoreStatistics(int basicScore, int basicMax, int engagingScore, int engagingMax, int entertainingScore, int entertainingMax, int relaxingScore, int relaxingMax)
        {
            //this.basicScore = basicScore;
            //this.basicMax = basicMax;
            this.engagingScore = engagingScore;
            this.engagingMax = engagingMax;
            this.entertainingScore = entertainingScore;
            this.entertainingMax = entertainingMax;
            this.relaxingScore = relaxingScore;
            this.relaxingMax = relaxingMax;
        }

        public string print()
        {
            string p = "ENG: " + engagingScore;
            p += "ENT: " + entertainingScore;
            p += "REL: " + relaxingScore;

            string  f= "\n MAX: ENG: " + engagingMax;
           f += "ENT: " + entertainingMax;
            f += "REL: " + relaxingMax;

            return p + f;
        }
    }
}

[System.Serializable]
public class ScoreScreenFields
{
    //public Text score;
    public Text hashtagWord;
    public Text personalTouch;
    public Text authorName_text;
    public Image avatarImage;

    public GameObject firstScreen;
    public GameObject starCountSection;
    public GameObject advertiseButton;

    public GameObject secondScreen;
    public Text secondScreenText;

    public List<Image> stars = new List<Image>();

    public StarRewardLine starRewardLine;
    public GameObject starRewardLineII;

    public Text starReward_text, capitalStars_text;
    public ProgressSet basicProgress, relaxingProgress, engagingProgress, entertainingProgress ;
    

    public void SetStars(int level)
    {
        //stars.ForEach(o => o.sprite = SuccessScreenController.Instance.starEmpty);
        //for (int i = 0; i < level; i++)
        //    stars[i].sprite = SuccessScreenController.Instance.startFull;
    }

    [System.Serializable]
    public class ProgressSet
    {
        public GameObject progressParent;
        public string progress_name;
        public ProgressBarPercentage progress;
        public Text title;
    }


}
