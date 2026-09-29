using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextOverlayController : Singleton<TextOverlayController>
{
    public string country;
    public string city;

    [HideInInspector]
    public CityController myCity;

    private List<LetterController> letters = new List<LetterController>();
    public List<TextControllerSet> sets = new List<TextControllerSet>();

    public void Initialize()
    {
        specialUnlocked = false;

        string language = PlayerPrefs.GetString("lastLang");
        //Debug.Log(language);
        int lanaguageID = 0;

        foreach (TextControllerSet tcs in sets)
        {
            if (tcs.language.ToLower() == language.ToLower())
                lanaguageID = sets.IndexOf(tcs);
        }
       
        sets.ForEach(o => o.parentGameObject.SetActive(false));
        sets[lanaguageID].parentGameObject.SetActive(true);
        letters = sets[lanaguageID].letters;

        foreach (LetterController l in letters)
        {
            l.textOverlay = gameObject;
            l.textController = this;
        }            
    }


    public void EnableWithSentence(string sentence)
    {
        //sentence.Log();
        gameObject.SetActive(true);
        TextDeconstructionController.Instance.sentenceText.gameObject.SetActive(true);
    }  

    public string GetHashTagSentence()
    {
        string language = PlayerPrefs.GetString("lastLang");

        foreach (TextControllerSet tcs in sets)
        {
            if (tcs.language.ToLower() == language.ToLower())
            {
                if(string.IsNullOrEmpty(tcs.htSentence_full))
                    return tcs.hashTagSentence;
                else
                {
                    return tcs.htSentence_full;
                }
            }
               
        }

        return "";        
    }

    public string GetHashTagSentenceMin()
    {
        string language = PlayerPrefs.GetString("lastLang");

        foreach (TextControllerSet tcs in sets)
        {
            if (tcs.language.ToLower() == language.ToLower())
                return tcs.GetMinWithDots();
        }

        return "";
    }

    public string GetHashtagWord()
    {
        string language = PlayerPrefs.GetString("lastLang");

        foreach (TextControllerSet tcs in sets)
        {
            if (tcs.language.ToLower() == language.ToLower())
                return tcs.hashTagWord;
        }

        return "";
    }

    public bool HasSome()
    {
        foreach (LetterController l in letters)
        {
            //if (l.HasSome() == true && l.specialLetter == true)
            if (l.HasSome() == true)
                return true;
        }

        return false;
    }

    public bool specialUnlocked = false;

    public bool IsCompleted() {
        //Debug.Log("Is completed started");
        foreach (LetterController l in letters)
        {
            if (!l.HasSome())
            {
                //Debug.Log("Not ready: " + l.helpMessage);
                return false;
            }
        }

       //Debug.Log("Completed!");
        return true;
    }

    public int SpecialLetterElementsCounter(string lang = "")
    {
        string language = PlayerPrefs.GetString("lastLang");

        int lanaguageID = 0;

        foreach (TextControllerSet tcs in sets)
        {
            if (tcs.language.ToLower() == language.ToLower())
                lanaguageID = sets.IndexOf(tcs);
        }
        
        List<LetterController> letters = sets[lanaguageID].letters;

        if (letters == null)
        {
            "Letters null".Log();
            return 1;
        }

        LetterController lc = letters.Find(o =>o.specialLetter);
        if (lc == null)
            return 1;

        return lc.pairs.Count;
    }

    public int ElementsCounter()
    {
        string language = PlayerPrefs.GetString("lastLang");

        int lanaguageID = 0;

        foreach (TextControllerSet tcs in sets)
        {
            if (tcs.language.ToLower() == language.ToLower())
                lanaguageID = sets.IndexOf(tcs);
        }

        List<LetterController> letters = sets[lanaguageID].letters;

        if (letters == null)
        {
            "Letters null".Log();
            return 1;
        }
        int counter = 0;

        letters.ForEach(o => counter += o.pairs.Count);
      
        return counter;
    }

    public bool AreSpecialCompleted()
    {
        foreach(LetterController lc in letters.FindAll(o => o.specialLetter))
        {
            if (!lc.HasSome())
            {
                lc.correctDropsExpected.Log("Not completed. Drops expected: ");
                return false;
            }
        }
        return true;
    }

    public void ShowAllReadyLetters() {
        foreach (LetterController l in letters)
        {
            if (l.HasSome())
                l.ShowLetter();
        }
    }

    public LetterController FindMyLetter(SpotController controller)
    {
        //Debug.Log("Looking for letter");
        foreach (LetterController l in letters)
        {
            foreach (ElementsPair ep in l.pairs)
            {
                if (ep.spotController == controller)
                    return l;
            }
        }

        return null;
    }

    public Sprite FindMySprite(SpotController controller)
    {
        //Debug.Log("Looking for letter");
        foreach (LetterController l in letters)
        {
            foreach (ElementsPair ep in l.pairs)
            {
                if (ep.spotController == controller)
                    return ep.element.GetComponent<UnityEngine.UI.Image>().sprite;
            }
        }

        return null;
    }


    public List<UnityEngine.Events.UnityAction> onClick = new List<UnityEngine.Events.UnityAction>();

    private float onClickTimer = 1f;
    private bool canClick = false;

    private void OnEnable()
    {
        onClickTimer = 1f;
        canClick = false;
    }

    private void Update()
    {
        onClickTimer -= Time.deltaTime;

        if (onClickTimer < 0f)
        {
            canClick = true;
        }
    }

    public bool CanExit()
    {
        return canClick;
    }

    public void OnClickDelayed()
    {
        if (canClick)
            OnClick();
    }

    public void OnClick() {

        if (myCity != null)
            myCity.ShowTextOverlay();

        onClick.ForEach(o=>o.Invoke());
    }

    

    //[NaughtyAttributes.Button("Show Hide elements")]
    public void ShowHideImages()
    {
        foreach(TextControllerSet tcs in sets)
        {
            tcs.letters.ForEach( o => o.pairs.ForEach(i => i.element.GetComponent<UnityEngine.UI.Image>().enabled = !i.element.GetComponent<UnityEngine.UI.Image>().enabled));
        }
    }

    public void HideLettersImages()
    {
        foreach (TextControllerSet tcs in sets)
        {
            tcs.letters.ForEach(o => o.pairs.ForEach(i => i.element.GetComponent<UnityEngine.UI.Image>().enabled = false));
        }
    }

    [NaughtyAttributes.Button("English hashtag P1")]
    public void ShowEnglishHashtag()
    {
        //if (!Application.isPlaying)
        //    return;

        TextControllerSet tcs = sets.Find(o=>o.language.ToLower() == "english");
        
        HashtahSentenceController.Instance.Enable();
        HashtahSentenceController.Instance.GetAttributes().sentence.gameObject.SetActive(false);
        HashtahSentenceController.Instance.GetAttributes().sentence_partI.gameObject.Activate();
        HashtahSentenceController.Instance.GetAttributes().sentence_partII.gameObject.DeActivate();
        HashtahSentenceController.Instance.GetAttributes().personal_touch.gameObject.DeActivate();
        HashtahSentenceController.Instance.GetAttributes().sentence_partI.text = string.Format(tcs.htSentence_min, "...");
        HashtahSentenceController.Instance.GetAttributes().sentence_partII.text = string.Format(tcs.htSentence_full,tcs.hashTagWord);
    }

    [NaughtyAttributes.Button("English hashtag P2")]
    public void ShowEnglishHashtagP2()
    {
        //if (!Application.isPlaying)
        //    return;

        TextControllerSet tcs = sets.Find(o => o.language.ToLower() == "english");

        HashtahSentenceController.Instance.Enable();
        HashtahSentenceController.Instance.GetAttributes().sentence.gameObject.SetActive(false);
        HashtahSentenceController.Instance.GetAttributes().sentence_partI.gameObject.DeActivate();
        HashtahSentenceController.Instance.GetAttributes().sentence_partII.gameObject.Activate();
        HashtahSentenceController.Instance.GetAttributes().personal_touch.gameObject.Activate();
        HashtahSentenceController.Instance.GetAttributes().sentence_partI.text = string.Format(tcs.htSentence_min, "...");
        HashtahSentenceController.Instance.GetAttributes().sentence_partII.text = string.Format(tcs.htSentence_full, "<b>#"+ tcs.hashTagWord + "</b>");
        HashtahSentenceController.Instance.GetAttributes().personal_touch.text = tcs.personalTouch;
    }
}

[System.Serializable]
public class TextControllerSet
{
    //public Elements.Language lang;
    public string language;
    public string hashTagWord="";
    [TextArea(1,3)]
    public string hashTagSentence="";

    [TextArea(1, 3)]
    public string htSentence_min = "";

    [TextArea(1, 3)]
    public string htSentence_full = "";

    [TextArea(1, 5)]
    public string personalTouch = "";

    public bool minAddDots = true;

    public string GetMinWithDots()
    {
        if (minAddDots)
            return htSentence_min + string.Format( " ... {0}", TextTranslationModule.GetWord("more").ToLower());

        else return htSentence_min;
    }

    public GameObject parentGameObject;
    public List<LetterController> letters = new List<LetterController>();
}
