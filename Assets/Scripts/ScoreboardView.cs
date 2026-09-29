using Elements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardView : View
{

    static private ScoreboardView instance;

    static public ScoreboardView Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(ScoreboardView))[0] as ScoreboardView;

            return instance;
        }
    }

    public GameObject prefab;

    //public Transform containerPortrait, containerLandscape;

    //public Text descriptionPortrait, descriptionLandscape;

    public ScoreboardAttributes portraitFields, landscapeFields;

    JSONObject lastLoadedScoreboard = new JSONObject();
    //bool jsonLoaded = false;

    private int placesNumber = 0;
    private int myPlace = 0;
    private int myPoints = 0;
    //public ScoreboardEntry entEntry, engEntry, visEntry;
    public Color disabledDigit, enabledDigit;

    public Button editorButton;
    public Image padlockImage;
    public Sprite pLocked, pUnlocked;

    //public TextAsset sampleJson;

    public List<Text> digits = new List<Text>();

    public List<Text> digits_partI = new List<Text>();
    public List<Text> digits_partII = new List<Text>();

    public Image f7, s7;
    public int levelEditorTreshold = 10;
    public int rewardsAfterLevelEditorTreshold = 7;

    public ScrollRect scrollRect;


    public int CNT = -1; 

    public override void Enable()
    {
        base.Enable();

        scrollRect.horizontalNormalizedPosition = 0f;
        UserController.UsersGameplayStats stats = UserController.Instance.GetCurrentStatisticsForScoreboard();

        int counter = UserController.Instance.GetTopScores();

        if (CNT > -1)
            counter = CNT;

        counter.LogDev("Counter: ");

        digits_partII.ForEach(o => o.color = disabledDigit);
        digits_partI.ForEach(o => o.color = disabledDigit);
        
        digits_partI.ForEach(o => o.transform.parent.gameObject.SetActive(true));
        List<Text> availableDigitsPartI = new List<Text>();

        if (counter >= levelEditorTreshold)
        {
            editorButton.interactable = true;
            padlockImage.sprite = pUnlocked;
            portraitFields.get_top_scores_to_unlock.text = string.Format(TextTranslationModule.GetWord("Get {0} top scores to unlock your reward"),7);

            if (counter >= levelEditorTreshold + rewardsAfterLevelEditorTreshold)
            {
                for(int i = rewardsAfterLevelEditorTreshold; i < digits_partI.Count; i++)
                {
                    digits_partI[i].transform.parent.gameObject.SetActive(false);
                }
                availableDigitsPartI = digits_partI.GetRange(0, rewardsAfterLevelEditorTreshold);
            }
            else
            {
                availableDigitsPartI = digits_partI;
            }
        }
        else
        {
            availableDigitsPartI = digits_partI;
            portraitFields.get_top_scores_to_unlock.text = string.Format(TextTranslationModule.GetWord("Get {0} top scores to unlock your reward"), 10);
            editorButton.interactable = false;
            padlockImage.sprite = pLocked;
        }

        List<Text> allAvailableDigits = new List<Text>();
        allAvailableDigits.AddRange(availableDigitsPartI);
        allAvailableDigits.AddRange(digits_partII);

        if (counter >= 10)
        {
            if (counter >= levelEditorTreshold + rewardsAfterLevelEditorTreshold)
            {
                int couterWithoutFirstReward = counter - levelEditorTreshold;
                int modulo = couterWithoutFirstReward % 7;
                int phases = couterWithoutFirstReward / 7;

                int modulo14 = couterWithoutFirstReward % 14;              

                f7.sprite = s7.sprite = pLocked;
                f7.color = s7.color = disabledDigit;


                allAvailableDigits.ForEach(o => o.color = disabledDigit);

                allAvailableDigits.GetRange(0, modulo14).ForEach(o => o.color = enabledDigit);

                if (modulo14 >= 7)
                {
                    f7.sprite = pUnlocked;
                    f7.color = enabledDigit;
                }
            }
            else
            {

                s7.sprite =pLocked;
                s7.color =  disabledDigit;
                f7.sprite = pUnlocked;
                f7.color = enabledDigit;

                allAvailableDigits.GetRange(0, counter-1).ForEach(o => o.color = enabledDigit);
            }
         }
        else
        {
            //(counter % levelEditorTreshold).LogDev("Counter modulo: ");
            allAvailableDigits.GetRange(0, counter % levelEditorTreshold).ForEach( o => o.color = enabledDigit);
            f7.sprite = s7.sprite = pLocked;
            f7.color = s7.color = disabledDigit;
        }

        //return;

      

        //engEntry.digits.ForEach( o => o.color = disabledDigit);
        //entEntry.digits.ForEach(o => o.color = disabledDigit);
        //visEntry.digits.ForEach(o => o.color = disabledDigit);

        //int clampedEng = stats.engaging > 7 ? 7 : stats.engaging;
        //int clampedEnt = stats.entertaining > 7 ? 7 : stats.entertaining;
        //int clampedRel = stats.relaxing > 7 ? 7 : stats.relaxing;

        ////clampedEng = 7;

        //if (clampedEng>0)
        //    engEntry.digits.GetRange(0, clampedEng).ForEach(o=>o.color = enabledDigit);

        //if (clampedEnt > 0)
        //    entEntry.digits.GetRange(0, clampedEnt).ForEach(o => o.color = enabledDigit);

        //if (clampedRel > 0)
        //    visEntry.digits.GetRange(0, clampedRel).ForEach(o => o.color = enabledDigit);


        //int progressTarget = 7;

        //stats.engaging.LogDev("For eng: ");
        // stats.entertaining.LogDev("For ent: ");
        // stats.relaxing.LogDev("For rel: ");
        //string msg = string.Format("Engaging: {0}/{1} \nEntertaining: {2}/{1} \nRelaxing: {3}/{1}", stats.engaging, progressTarget, stats.entertaining, stats.relaxing);

        //msg = string.Format("Speed: <b>{0}</b>", (stats.engaging > 7 ? 7 : stats.engaging));
        //msg += string.Format("\nStrategy: <b>{0}</b>", (stats.entertaining > 7 ? 7 : stats.entertaining));
        //msg += string.Format("\nVisualisation: <b>{0}</b>", (stats.relaxing > 7 ? 7 : stats.relaxing));

        //msg.LogDev("Msg: ");

        //GetAttributes().tempStats.text = msg;


        //GetAttributes().allLevelsPlayed.text = string.Format(TextTranslationModule.GetWord("You completed {0} levels with top score in {1} of them"), "<b>"+ stats.citiesCount + "</b>", "<b>" + stats.counter + "</b>");

        GetAttributes().allLevelsPlayed.text = stats.citiesCount.ToString();
        GetAttributes().topLevelsPlayed.text = counter.ToString();

        //if(stats.counter == 1)
        //    GetAttributes().topLevelsPlayed.text = string.Format(TextTranslationModule.GetWord("You have top scores in {0} level"), "<b>" + stats.counter + "</b>");
        //else
        //    GetAttributes().topLevelsPlayed.text = string.Format(TextTranslationModule.GetWord("You have top scores in {0} levels"), "<b>" + stats.counter + "</b>");

        stats.counter = 0;
        //no_results_to_display
        //foreach (Cities city in cities)
        //{
        //    GameObject g = (GameObject)Instantiate(prefab, GetAttributes().container);
        //    g.GetComponent<CityStatisticPrefab>().Initialize(city);
        //    g.GetComponent<CityStatisticPrefab>().SetColor(counter % 2 == 0 ? normalColor : evenColor);
        //    counter++;
        //    trash.Add(g);    
            
        //}

        //lastLoadedScoreboard = new JSONObject();
        //InitializeJsonData(sampleJson.text);
        return;
        // DKK.ServerManager.Instance.Send("unity/index.html")
        //     .AsGet(delegate (string s, bool b)
        //     {
        //         if (b)
        //         {
        //             Debug.Log(s);
        //             InitializeJsonData(s);
        //         }
        //     });

        //another api version
        //DKK.ServerManager.Instance.Send("http://localhost:5000/api/perspectivist")
        //    .AsGet(delegate (string s, bool b)
        //    {
        //        lastLoadedScoreboard = new JSONObject(s);

        //        myPlace = (int)lastLoadedScoreboard["myPlace"].i;
        //        placesNumber = (int)lastLoadedScoreboard["placesNumber"].i;
        //        myPoints = (int)lastLoadedScoreboard["points"].i;

        //        jsonLoaded = true;
        //        if (ViewsController.Instance.GetCurrentOrientation() == ScreenOrientation.Landscape)
        //        {
        //            EnableLandscape();
        //        }
        //        else
        //        {
        //            EnablePortrait();
        //        }
        //    });
    }

    public override void EnableLandscape()
    {
        //base.EnableLandscape();
        //if (jsonLoaded)
        //{
        //    descriptionLandscape.text = "You are " + myPlace + " of " + placesNumber + " with " + myPoints + " points";
        //    InstantiatePrefabs(containerLandscape, lastLoadedScoreboard);
        //}
    }

    public override void EnablePortrait()
    {
        base.EnablePortrait();
        //if (jsonLoaded)
        //{
        //    descriptionPortrait.text = "You are " + myPlace + " of " + placesNumber + " with " + myPoints + " points";
        //    InstantiatePrefabs(containerPortrait, lastLoadedScoreboard);
        //}
    }


    List<GameObject> trash = new List<GameObject>();
    public Color normalColor, evenColor;

    private void InstantiatePrefabs(Transform container, JSONObject j)
    {
        ClearList();
        int evenCounter = 0;
        if (j.HasField("places") && j["places"].IsArray)
        {
            foreach (JSONObject jo in j["places"].list)
            {
                GameObject g = (GameObject)Instantiate(prefab, container);
                g.GetComponent<ScorePrefabUnit>().Initialize(jo, evenCounter % 2 == 0 ? normalColor : evenColor);
                trash.Add(g);
                evenCounter++;
            }
        }
    }

    private void ClearList()
    {
        trash.ForEach(o => Destroy(o));
        trash.Clear();
    }

    public override void Disable()
    {
        base.Disable();
        ClearList();
    }
    

    public ScoreboardAttributes GetAttributes()
    {
        if (ViewsController.Instance.GetCurrentOrientation() == ScreenOrientation.Portrait)
            return portraitFields;

        return landscapeFields;
    }
}

[System.Serializable]
public class ScoreboardAttributes
{
    public RectTransform container;
    public Text description;
    public Text no_results_to_display;
    public Text get_top_scores_to_unlock;

    public Text allLevelsPlayed, topLevelsPlayed, tempStats;
}

[System.Serializable]
public class ScoreboardEntry
{
    public Text title;
    public List<Text> digits = new List<Text>();
}
