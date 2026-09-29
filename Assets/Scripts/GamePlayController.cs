using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elements;
using UnityEngine.Analytics;
using System;
using DG.Tweening;

public class GamePlayController : View
{
    static private GamePlayController instance;

    static public GamePlayController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(GamePlayController))[0] as GamePlayController;

            return instance;
        }
    }

    public GameplayBonuses bonuses = new GameplayBonuses();

    public GameObject dragablePrefab;
    public GameObject gamePlayWindow;

    public View blinkingAnimationExplenation;

    public GameObject newLayoutTopBar;

    private List<GameObject> trash = new List<GameObject>();

    private List<GameObject> currentDragableList = new List<GameObject>();

    public GamePlayAttributes landscapeFields, portraitFields;     
    
    private Cities lastCity = null;

    public GameplayBalance gamePlayBalance;

    public GameObject pauseOverlay;

    public ScrollRect scrollRect;

    public Button wordDeconstructionScreenButton;
       
    public AnimationCurve progressHintVisibility;

    public List<Color> pointsColors = new List<Color>();

    public View portalExplenation_view;
    public View norwayHelp_view;

    public GameplayStatistics statistics = new GameplayStatistics();

    public GameObject magentaTutorial_gameObject;
    public GameObject boostersTutorial_gameObject;

    public Canvas topCenter_canvas;

    public Image highlightExplenationAngle;


    /// <summary>
    /// Offset for display during element dragging
    /// </summary>
    private float draggingOffset = 250f;

    
    private float points = 0f;
    private float extraPoints = 0f;
    private int extraSeconds = 0;

    private bool firstElementPicked = false;

    private bool gamePaused = true;
    private bool mistakesHelpDisplayed = false;

    private bool scrollingRect = false;
    //public float scrollingSpeed = 10f;

    private int movesCounter = 0;
    private int wrongCounter = 0;

    private bool assetLoaded = false;
    private bool endedSuccesfully = false;

    public DropDownButtonsGameplayOverlay dropDown_script;
    public ViewAttributes<GamePlayAttributes> attributes = new ViewAttributes<GamePlayAttributes>();

    private float lastPoints = 0f;

    private void Update()
    {        
        if (gamePaused)
            return;

        attributes.Get().timer.ActualizeTimer();

        ComputeScorePoints();
        VerifyPoints();

        if (points < 0f)
            points = 0f;

        if (lastPoints != points)
        {
            lastPoints = points;
            ActualizePointsText();
        }

        if (!distractorsEnabled)
        {
            int i = DistractorsController.Instance.ShouldTimerLimitedBeTriggered();
            if (i > 0)
            {
                DistractorsController.Instance.RunDistratctor(i);
            }
        }
        else
        {
            DistractorsController.Instance.CheckHowLongEnabled();
        }

        if (helpTimerEnabled)
        {
            helpTimer -= Time.deltaTime;

            if (helpTimer <= 0)
            {
                helpTimerEnabled = false;
                // enable help logic
                FloatingTextPanel.Instance.Show(new FloatingTextParameters() { text = TextTranslationModule.GetWord("Help: look out for logic to<br>solve the level fast"), generalClickAction = delegate() {
                    FloatingTextPanel.Instance.Hide();
                } });
            }
        }
    }

  

    public override void Enable()
    {
        gameEnded = false;
        attributes.Initialize(portraitFields, landscapeFields);

        base.Enable();

        if (lastCity == null)
            return;

        lastCity.ReportUnsuccesfullPlay();

        GameplayScenario scenario = GetCurrentCity().GetScenario();
       
        foreach (int degree in GetCurrentCity().elementsDegrees)
        {
            GameObject g1 = (GameObject)Instantiate(dragablePrefab, attributes.Get().container.transform);
         
            DragableElement dr = g1.GetComponent<DragableElement>();
            dr.Initialize(ElementsDatabase.Instance.GetElementByDegree(degree), scrollRect);
            dr.baseContainer = attributes.Get().container;
            dr.SetDraggingTime(scenario.draggingTime);
            
            ElementReplacement er = g1.AddComponent<ElementReplacement>();
            er.Initialize(attributes.Get().container.gameObject, g1, 2, true, true);
            er.SetDragOffset(new Vector2(0f, draggingOffset));
            
            attributes.Get().dragableStartList.Add(g1);
            trash.Add(g1);
        }

        blinkingLimitationDisplayed = false;
        blinkingLimitationReadyToDisplay = false;

        if (Rules.GameplayRules.BlinkingAnimation())
        {         
            blinkingLimitation = true;
            blinkinkLimit = GetCurrentCity().elementsDegrees.Count + 2;
        }
        else
        {          
            blinkingLimitation = false;
        }

        startElementsCount = attributes.Get().dragableStartList.Count;

        RefreshBottomBars();
          
        attributes.Get().InitializeStarsTexts(true);
        ActualizeText();
        gameStartTime = Time.realtimeSinceStartup;
        availableHighlights.Clear();
        availableHighlights = GetCurrentCity().GetController().SpotsList();
        enabled = true;
    }

    private bool DisplayStarsCondition()
    {      
        int maxUnlocked = UserController.Instance.maxUnlockdedGroup;
        if (maxUnlocked >= 3 && GetCurrentCity().type != CitiesGameplayType.tutorial)
            return true;

        return Rules.BoostersRules.FirstBoosterAvailable() && GamePlayController.Instance.GetCurrentCity().type != Elements.CitiesGameplayType.tutorial;        
    }

    public void Enable(Cities city)
    {
        lastCity = city;
        gameMode = UserController.Instance.GetGameModeID();

        // change ambient sound
        ElementsDatabase.AmbientSound ambientSound = ElementsDatabase.AmbientSound.UNDEFINED;

        if (city.ambientSound != ElementsDatabase.AmbientSound.UNDEFINED)
        {
            ambientSound = city.ambientSound;
        }
        else
        {
            ambientSound = Elements.ElementsDatabase.Instance.GetCountryByCity(city).ambientSound;
        }

        AudioController.Instance.ChangeAmbient(ambientSound, city.audioOffset);

        wordDeconstructionScreenButton.interactable = false;       
        GetCurrentCity().GetController().Initialize();

        string hashTagSentence = GetCurrentCity().GetController().GetCurrecntSentenceDotsMin();

        if (string.IsNullOrEmpty(hashTagSentence))
        {          
            hashTagSentence = GetCurrentCity().GetController().GetCurrecntSentenceDots();
        }

        canProceedAfterAssetLoad = false;

        if (!string.IsNullOrEmpty(hashTagSentence))
        {
            if (lastCity.bundle.requireBundle)
                GetImageFromAssetBundle();

            HashtahSentenceController.Instance.RegisterOnClickCallback(delegate ()
            {
                canProceedAfterAssetLoad = true;
                WaitForBundleLoad();
            });
            HashtahSentenceController.Instance.Enable(hashTagSentence, GetCurrentCity());
        }
        else
        {
            GetImageFromAssetBundle();
        }               
    }

    public void GetImageFromAssetBundle()
    {
        if (lastCity.bundle.requireBundle)
        {
            if(lastCity.GetController().image.sprite != null)
            {
                assetLoaded = true;
                WaitForBundleLoad();
                return;
            }
                      
            DKK.AssetsBundleManager.Instance.Load(lastCity.bundle.GetBundleName(), lastCity.bundle.GetImageName(), (obj) =>
            {              
                Texture2D t2d = obj as Texture2D;
                Sprite s = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(.5f, .5f));
                lastCity.GetController().image.sprite = s;
               
                assetLoaded = true;
                WaitForBundleLoad();           
            }, null, null);
        }
        else
        {           
            EnableGeneralGameplay();
        }
    }

    private void WaitForBundleLoad()
    {
        if (!lastCity.bundle.requireBundle)
        {
            MainLoader.Instance.Disable();
            EnableGeneralGameplay();
            return;
        }

        if (assetLoaded && canProceedAfterAssetLoad)
        {
            MainLoader.Instance.Disable();
            EnableGeneralGameplay();
        }
        else if (canProceedAfterAssetLoad && !assetLoaded)
        {
            MainLoader.Instance.Enable();
        }
    }

    private IEnumerator EnableGameplayGeneralCo()
    {
        gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        EnableGeneralGameplay();
    }

    public bool generalGameplayEnabled = false;

    public void EnableGeneralGameplay()
    {
        Enable();
        statistics.Reset();
        bonuses.Reset();
        generalGameplayEnabled = true;

        UserController.Instance.MaxUnlockedGroup();
       
        FloatingPointPanelController.Instance.Hide();

        wordDeconstructionScreenButton.interactable = false;
        GyroForParallax.Instance.Reset();

        if (lastCity.GetController().zoomed)
        {
            attributes.Get().zoom_button.SetAlpha(1f);
        }

        magentaTutorial_gameObject.SetActive(false);
        HideBoostersTutorial();

        helpTimerEnabled = false;
        helpTimer = 0f;
        draggingLocked = false;
        statistics.halfElementsCompletedTrigger = false;
        topCenter_canvas.overrideSorting = false;
        gamePaused = false;
        firstElementPicked = false;
        prePreparingStars = 0;
        prePreparingCounter = 0;
        magentaCompleted = false;
        hintBuyCounter = 0;       
        mistakesHelpDisplayed = false;
        scrollRect.horizontalNormalizedPosition = 0f;

        lastProccessedLetter = null;

        GetCurrentCity().GetController().StartLevel();

        ScrollRect sRect = lastCity.GetController().GetComponent<ScrollRect>();
        if (sRect != null)
        {
            bool zoomMode = lastCity.type == CitiesGameplayType.zooming;
            sRect.vertical = zoomMode;
            attributes.Get().zoom_button.SetActive(zoomMode);
            if (zoomMode)
            {
                attributes.Get().zoom_button.Interactable();
                attributes.Get().zoom_button.SetSprite(AssetsDatabase.AssetsDatabase.Instance.plusButton_icon);
            }
        }

        GetCurrentCity().GetController().DisableFlashesOnAll();
        attributes.Get().EnableBottonHorizontalGroup();

        OutOfStarsScreen.Instance.Disable();
        pauseOverlay.SetActive(false);
        distractorsEnabled = false;
        distractorSuspended = false;
        distractorSuspendedByAnimation = false;

        dropDown_script.Disable();
        enabled = true;
        Time.timeScale = 1f;
        points = extraPoints = 0f;
        movesCounter = 0;
        wrongCounter = 0;
        extraSeconds = 0;

        attributes.Get().timer.Reset();
        attributes.Get().timer.Disable();

        attributes.Get().timerNormalState.SetActive(true);
        attributes.Get().timerPausedState.SetActive(false);

        ActualizePointsText();
        DisableDistractors(); 
       
        attributes.Get().starsPanel_topCenter.SetActive(true);

        if(topCenter_canvas.GetComponent<Button>() != null)
        {
            topCenter_canvas.GetComponent<Button>().enabled = true;
            topCenter_canvas.GetComponent<Button>().interactable = DisplayStarsCondition();
        }

        if (topCenter_canvas.GetComponent<GraphicRaycaster>() != null)
        {
            topCenter_canvas.GetComponent<GraphicRaycaster>().enabled = true;
        }
                
        LayoutRebuilder.ForceRebuildLayoutImmediate(attributes.Get().starsPanel_topCenter.GetComponent<RectTransform>());
#if !UNITY_EDITOR
                       
        AnalyticsEvent.LevelStart(GetCurrentCity().name);
#endif


        AnalyticsController.Instance.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelStart, new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevelName, GetCurrentCity().name));

        if (mySequence != null)
            mySequence.Kill();

        attributes.Get().starCapitalGroup.transform.DOKill();
        attributes.Get().starCapitalGroup.transform.localScale = Vector3.one;


        if (lastCity.helpLogic)
        {
            helpTimerEnabled = true;
            helpTimer = 35f; // logic help
        }
    }

    private void ComputeScorePoints()
    {
        float time = attributes.Get().timer.GetTime();
        float timeWithExtra = time + (float)extraSeconds;
        float distractorMultiplier = 1f;     

        if (gameMode > 0)
        {
            distractorMultiplier = 1.25f;
        }
        
        float pointsForDrops = correctDrops * gamePlayBalance.pointsForEveryDragable;

        points = pointsForDrops - (timeWithExtra * distractorMultiplier * gamePlayBalance.minusPointsForEverySecond) + extraPoints;
    }

    public void AllLettersCollected()
    {
        FloatingTextPanel.Instance.Hide();
        FloatingPointPanelController.Instance.Hide();
    }

    private void ActualizePointsText()
    {
        attributes.Get().pointsText.text = GetScore().ToString("F0");
    }

    private void VerifyPoints()
    {
        if (pointsColors.Count < 3)
            return;
    }

    public void AddSeconds(int seconds)
    {
        extraSeconds += seconds;
    }
    public void AddPoints(int points)
    {
        this.extraPoints -= points;
        if (this.points < 0)
            this.points = 0;
    }

    public void ElementPicked()
    {
        if (!firstElementPicked)
        {
            firstElementPicked = true;
            GetCurrentCity().GetController().ReportFirstPick();
            statistics.Reset();
        }

        attributes.Get().timer.Enable();
    }

    private bool draggingLocked = false;

    public void LockDragging(GameObject g)
    {
        draggingLocked = true;
        List<GameObject> tempList = new List<GameObject>(trash);
        tempList.Remove(g);
        tempList.ForEach(o => o.GetComponent<DragableElement>().LockDragging());

        if (lastCity.GetController().zoomed)
        {
            attributes.Get().zoom_button.NotInteractable();
            attributes.Get().zoom_button.SetAlpha(.2f);
        }
    }
   
    public void UnlockDragging(GameObject g)
    {       
        draggingLocked = false;
        trash.ForEach(o => o.GetComponent<DragableElement>().UnlockDragging());
        if (lastCity.GetController().zoomed)
        {
            attributes.Get().zoom_button.Interactable();
            attributes.Get().zoom_button.SetAlpha(1f);
        }
    }

    int gameMode = 0; 
    private bool canProceedAfterAssetLoad = false;   

    public void CityAnimationEnd()
    {
        if (GetCurrentCity().GetMyGroup().id >= 1)
        {
            int completedRuns = GetCurrentCity().GetCompleteRunsCount(UserController.Instance.GetLanguage());

            if (completedRuns > 0)
            {
                string key = "portalExplain" + UserController.Instance.GetLanguage();

                if (UserController.Instance.GetStats().HasDictionary(key) == false)
                {
                    UserController.Instance.GetStats().AddInt(key, 1);
                    portalExplenation_view.Enable();
                    portalExplenation_view.RegisterOnDisableAction(CityAnimationEnd);
                    return;
                }
            }
        }

        int maxUnlocked = UserController.Instance.maxUnlockdedGroup;
  
        if (GetCurrentCity().GetMyGroup().id > -2 && GetCurrentCity().type != Elements.CitiesGameplayType.tutorial)
        {
            if (Rules.BoostersRules.FirstBoosterTutorial())
            {
                string key = "bonusesIntroduced_3" + UserController.Instance.GetLanguage();

                if (UserController.Instance.GetStats().HasDictionary(key) == false)
                {                  
                    ShowBoostersTutorial();
                    UserController.Instance.GetStats().AddInt(key, 1);
                    UserController.Instance.GetStats().AddFlag(key);
                    return;
                }
            }

            if (Rules.BoostersRules.SecondBoosterTutorial())
            {
                string key = "bonusesIntroduced_4" + UserController.Instance.GetLanguage();

                if (UserController.Instance.GetStats().HasDictionary(key) == false)
                {
                    SetTimerState(true);
                    BonusesScreen.Instance.Enable(2);
                    BonusesScreen.Instance.RegisterOnDisableAction(CityAnimationEnd);
                    UserController.Instance.GetStats().AddInt(key, 1);
                    UserController.Instance.GetStats().AddFlag(key);
                    return;
                }
            }

            if (Rules.BoostersRules.ThirdBoosterTutorial())
            {
                string key = "bonusesIntroduced_5" + UserController.Instance.GetLanguage();

                if (UserController.Instance.GetStats().HasDictionary(key) == false)
                {
                    SetTimerState(true);
                    BonusesScreen.Instance.Enable(3);
                    BonusesScreen.Instance.RegisterOnDisableAction(CityAnimationEnd);
                    UserController.Instance.GetStats().AddInt(key, 1);
                    UserController.Instance.GetStats().AddFlag(key);
                    return;
                }
            }

            if(Rules.BoostersRules.AllBoostersTutorial())
            {
                string key = "bonusesIntroduced_6" + UserController.Instance.GetLanguage();

                if (UserController.Instance.GetStats().HasDictionary(key) == false)
                {
                    SetTimerState(true);
                    BonusesScreen.Instance.Enable(4);
                    BonusesScreen.Instance.RegisterOnDisableAction(CityAnimationEnd);
                    UserController.Instance.GetStats().AddInt(key, 1);
                    UserController.Instance.GetStats().AddFlag(key);
                    return;
                }
            }
        }

        if (lastCity.type == CitiesGameplayType.zooming)
        {
            string key = "zoomIntroduced" + UserController.Instance.GetLanguage();

            if (UserController.Instance.GetStats().HasDictionary(key) == false)
            {
                ZoomTutorial();
            }
        }
    }

    private void ZoomTutorial()
    {
        Canvas c = attributes.Get().zoom_button.gameObject.GetComponent<Canvas>();

        if (c == null)
        {
            c = attributes.Get().zoom_button.gameObject.AddComponent<Canvas>();
        }

        c.overrideSorting = true;
        c.sortingLayerID = 2;
        attributes.Get().zoom_button.overlayInfo.SetActive(true);
        attributes.Get().zoom_button.screenOverlay.SetActive(true);
    }

    public void ZoomTutorialExit()
    {
        Canvas c = attributes.Get().zoom_button.gameObject.GetComponent<Canvas>();

        if (c != null)
        {
            Destroy(c);
        }

        UnityEngine.EventSystems.EventSystem es = attributes.Get().zoom_button.gameObject.GetComponent<UnityEngine.EventSystems.EventSystem>();

        if (es != null)
        {
            Destroy(es);
        }

        attributes.Get().zoom_button.overlayInfo.SetActive(false);
        attributes.Get().zoom_button.screenOverlay.SetActive(false);
        string key = "zoomIntroduced" + UserController.Instance.GetLanguage();
        UserController.Instance.GetStats().AddInt(key, 1);
    }

    public void ZoomImage()
    {
        if (!lastCity.GetController().enterAnimationEnded)
            return;

        if (lastCity.GetController().zoomed)
        {
            if (draggingLocked)
                return;

            lastCity.GetController().ZoomOut();
            attributes.Get().zoom_button.SetSprite(AssetsDatabase.AssetsDatabase.Instance.plusButton_icon);
        }
        else
        {
            lastCity.GetController().ZoomIn();
            attributes.Get().zoom_button.SetSprite(AssetsDatabase.AssetsDatabase.Instance.minusButton_icon);
        }
    }

    public void PauseGame()
    {
        pauseOverlay.SetActive(!pauseOverlay.activeSelf);

        if (pauseOverlay.activeSelf)
        {
            SuspendDistractorWithAnimation();
        }
        else
        {
            ChangeSuspendedByAnimation();
        }

        SetTimerState(pauseOverlay.activeSelf);
    }

    public void SetTimerState(bool isPaused)
    {
        gamePaused = isPaused;

        if (gamePaused)
        {
            attributes.Get().timer.Disable();
        }
        else
        {
            if (firstElementPicked)
                attributes.Get().timer.Enable();
        }

        attributes.Get().timerNormalState.SetActive(!gamePaused);
        attributes.Get().timerPausedState.SetActive(gamePaused);
    }

    public void ShowTextOverlay()
    {
        GetCurrentCity().GetController().ShowTextOverlay();
    }

    public Cities GetCurrentCity()
    {
        return lastCity;
    }
        
    private void RefreshBottomBars()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(portraitFields.container.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(landscapeFields.container.GetComponent<RectTransform>());
    }

    private float gameStartTime;

    public override void EnableLandscape()
    {
        base.EnableLandscape();
    }

    public override void EnablePortrait()
    {
        base.EnablePortrait();
    }

    public override void ChangeOrientation(ScreenOrientation orientation)
    {
        GetCurrentCity().GetController().ScaleImage();
    }

    public void IncorrectDrop(GameObject g)
    {

        CheckBlinkingLimitationDisplayStatus();
        statistics.ReportMove();
        statistics.ResetIdleTimer();
        wrongCounter++;
        movesCounter++;
        GetCurrentCity().GetController().IncorrectDrop();
        ActualizeText();

        if (GetCurrentCity().GetController().country.ToLower() == "norway" && wrongCounter == 3)
        {
            string key = "norwayHelp_" + UserController.Instance.GetLanguage();
            if (UserController.Instance.GetStats().HasDictionary(key) == false)
            {
                norwayHelp_view.Enable();
                UserController.Instance.GetStats().AddInt(key, 1);
            }
        }
        
        // todo: 
        if (GetCurrentCity().type != CitiesGameplayType.tutorial && UserController.Instance.maxUnlockdedGroup >= 3 && !mistakesHelpDisplayed)
        {
            if (wrongCounter > startElementsCount + 7)
            {
                SetTimerState(true);
                ExtendGameplayController.Instance.Enable();
                mistakesHelpDisplayed = true;
                return;
            }
        }

        int i = DistractorsController.Instance.ShouldBeTriggered();
        if (i > 0)
        {
            DistractorsController.Instance.RunDistratctor(i);
        }
    }

    public void ContinueGameplay()
    {
        wrongCounter = 0;
        SetTimerState(false);
    }

    private int correctDrops = 0;
    private int startElementsCount = 0;

    private int specialDrops = 0;

    private bool helpTimerEnabled = false;
    private float helpTimer = 0f;
    //private int regularDrop = 0;

    public void DeleteElement(GameObject g, Spot spot)
    {
        movesCounter++;
        correctDrops++;
        statistics.ReportMove();
        statistics.ResetIdleTimer();    

        wordDeconstructionScreenButton.interactable = GetCurrentCity().GetController().textOverlayController.HasSome();

        trash.Remove(g);
        Destroy(g);
        ActualizeText();

        ComputeScorePoints();
        VerifyPoints();
        ActualizePointsText();

        if (IsReady())
        {          
            if (GetCurrentCity().type == CitiesGameplayType.zooming)
            {
                if (GetCurrentCity().GetController().zoomed)
                {
                    GetCurrentCity().GetController().ZoomOut();
                }
            }

            DisableDistractors();
            attributes.Get().timer.Disable();
            enabled = false;
        }

        if (GetCurrentCity().GetController().textOverlayController != null)
        {
            if (GetCurrentCity().GetController().textOverlayController.gameObject == null)
            {               
                EndGameIfReady();
                return;
            }
        }
        else
        {
            {
                EndGameIfReady();
                return;
            }
        }

        int i = DistractorsController.Instance.ShouldBeTriggered();
        if (i > 0)
        {
            DistractorsController.Instance.RunDistratctor(i);
        }
    }
    

    [HideInInspector]
    public bool distractorsEnabled = false;
    [HideInInspector]
    public bool distractorSuspended = false;

    [HideInInspector]
    public bool distractorSuspendedByAnimation = false;
   
    public bool IsReady()
    {
        return correctDrops == startElementsCount;
    }

    public void EndGameIfReady()
    {
        if (IsReady())
        {
            EndGame();
        }
    }

    public List<ElementReplacement> GetElementReplacementList()
    {
        List<ElementReplacement> list = new List<ElementReplacement>();

        foreach (GameObject g in trash)
        {
            ElementReplacement er = g.GetComponent<ElementReplacement>();
            if (er != null)
            {
                list.Add(er);
            }
        }

        return list;
    }
    
    private int GetMaxCityPoints()
    {
        return GetCurrentCity().elementsDegrees.Count * gamePlayBalance.pointsForEveryDragable;
    }  

    private int GetScore()
    {
        if (points < 0f)
            points = 0f;

        return (int)points;
    }

    private int GetStars(ref SuccessScreenController.ScoreStatistics statistics)
    {
        int starsGiven = 0;
        int extraStars = 0;
     
        int score = GetScore();
        int maxPoints = GetMaxCityPoints();
        int gameMode = UserController.Instance.GetGameModeID();

        if (score > maxPoints * 0.66f)
        {
            starsGiven = 2;
        }

        if (score < maxPoints * 0.66f && score > maxPoints * 0.33f)
            starsGiven = 1;
        
        if (gameMode == 2)
        {          
            statistics.entertainingMax = 4;
            statistics.entertainingScore = starsGiven*2;
            return starsGiven*2;
        }

        if (hintBuyCounter == 0)
        {
            int allElements = GetCurrentCity().elementsDegrees.Count;

            int extraStarsDifference = allElements - movesCounter;

            // no mistakes 
            if (gameMode == 1)
            {
                //maxExtraStars = 4;

                if (extraStarsDifference > -3)
                {
                    extraStars = 4;
                }
                else if (extraStarsDifference > -4)
                {
                    extraStars = 2;
                }
                else if (extraStarsDifference > -6)
                {
                    extraStars = 1;
                }
                else
                {
                    extraStars = 0;
                }
            }
            else
            {
                if (extraStarsDifference > -3)
                {
                    extraStars = 2;
                }
                else if (extraStarsDifference > -4)
                {
                    extraStars = 1;
                }
                else
                {
                    extraStars = 0;
                }  
            }

            if (extraStars < 0)
                extraStars = 0;
          
            // engaging
            if (gameMode == 1)
            {
                statistics.engagingMax = 4;
                statistics.engagingScore = extraStars;
                return extraStars;
            }
        }

         // BOOKMARK 

        switch (gameMode)
        {
            case 0:
                // if no special mode was choosen
                int gID = UserController.Instance.maxUnlockdedGroup;
                //int scenarioID = ElementsDatabase.Instance.groups.Find(o => o.id == gID).scenarioID;

                if (gID >= 2)
                {
                    int s = starsGiven + extraStars;

                    statistics.engagingMax = 2;
                    statistics.engagingScore = extraStars;

                    if (lastCity.GetController().SpecialAnglesCounter() > 1)
                    {
                        s += prePreparingStars;
                        statistics.relaxingMax = 4;
                        int maxPreprepare = (lastCity.GetController().SpecialAnglesCounter() - 1)*2;
                        //if (maxPreprepare < 2)
                        //    maxPreprepare = 2;

                        statistics.relaxingMax = maxPreprepare;
                        statistics.relaxingScore = prePreparingStars;
                    }

                    statistics.entertainingMax = 2;
                    statistics.entertainingScore = starsGiven;
                    //"sc 1".LogDev();
                    return s;
                }
                else
                {
                    statistics.entertainingMax = 2;
                    statistics.entertainingScore = starsGiven;
                    return starsGiven;
                }
            case 1:
                // if engaging
                statistics.engagingMax = 4;
                statistics.engagingScore = extraStars;
                return extraStars;
            case 2:
             
                statistics.entertainingMax = 4;
                statistics.entertainingScore = starsGiven*2;

               
                return starsGiven * 2;
            case 3:
                // if relaxing

                "relaxing".LogDev();

                if (lastCity.GetController().SpecialAnglesCounter() > 1)
                {
                    //"angles greate than 1".LogDev();
                    int maxPreprepare = (lastCity.GetController().SpecialAnglesCounter() - 1) * 3;
                    statistics.relaxingMax = maxPreprepare;
                    statistics.relaxingScore = prePreparingStars;
                    //maxPreprepare.LogDev("Max prepare: ");
                    //prePreparingStars.LogDev("stars: ");


                    return prePreparingStars;
                }
                
                return 0;
        }

        return starsGiven + extraStars + prePreparingStars;
    }

    private bool gameEnded = false;

    public void EndGame(bool success = true)
    {
        "End game".Log();
        gameEnded = true;
        enabled = false;
        attributes.Get().timer.Disable();        

        GetCurrentCity().GetCurrentLanguageSet().allRuns++;

        if (success)
        {
            FloatingTextPanel.Instance.Hide();
            FloatingPointPanelController.Instance.Hide();

            endedSuccesfully = true;
            AudioController.Instance.PlayVictory();
            GetCurrentCity().GetCurrentLanguageSet().isNew = false;
            GetCurrentCity().MarkAsPlayedToday();
            GetCurrentCity().GetCurrentLanguageSet().restartingCount = 0;

            int result = GetScore();
            SuccessScreenController.ScoreStatistics stats = new SuccessScreenController.ScoreStatistics();
            int starsCount = GetStars(ref stats);    
            

            string hashtag = GetCurrentCity().GetController().GetCurrentWordWithHash();
            string hashTagSentence = GetCurrentCity().GetController().GetCurrentSentenceWithWord();

            if (!GetCurrentCity().CheckIfStarsAvailable())
            {
                stats = new SuccessScreenController.ScoreStatistics();
                starsCount = 0;
            }

            if(GetCurrentCity().GetCurrentLanguageSet().succesfullRuns == 0)
            {
                UserController.Instance.GetStats().AddFlag("showNewFinishedCity");
            }

            UserController.Instance.AddStars(starsCount, "stars_complete_level");
            UserController.Instance.AddEntry(GetCurrentCity().GetController(), result, starsCount, stats);
            SuccessScreenController.Instance.Enable(result, starsCount, hashtag, hashTagSentence, stats, GetCurrentCity());
            DisableDistractors();


#if !UNITY_EDITOR
                       
            AnalyticsEvent.LevelComplete(GetCurrentCity().name, GetGamePlayStats());           
#endif

            int stars = GetStars(ref stats);

            if (!GetCurrentCity().CheckIfStarsAvailable())
            {
                stars = 0;
            }

            //Firebase.Analytics.Parameter[] parameters = new Firebase.Analytics.Parameter[] {
            //    new Firebase.Analytics.Parameter("name", GetCurrentCity().name),
            //    new Firebase.Analytics.Parameter("score", (long)GetScore()),
            //      new Firebase.Analytics.Parameter("time", (long)attributes.Get().timer.GetTime()),
            //         new Firebase.Analytics.Parameter("stars", stars),
            //            new Firebase.Analytics.Parameter("wrongMoves", wrongCounter),
            //               new Firebase.Analytics.Parameter("movesCounter", movesCounter)
            //};

            JSONObject jo = new JSONObject();
            jo.AddField("name", GetCurrentCity().name);
            jo.AddField("score", (long)GetScore());
            jo.AddField("time", (long)attributes.Get().timer.GetTime());
            jo.AddField("stars", stars);
            jo.AddField("wrongMoves", wrongCounter);
            jo.AddField("movesCounter", movesCounter);

            Firebase.Analytics.Parameter[] parameters = new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevelName, GetCurrentCity().name),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterSuccess, jo.ToString())
            };

            AnalyticsController.Instance.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelEnd, parameters);                       

            UserController.Instance.SaveGame();

            //        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            //  Firebase.Analytics.FirebaseAnalytics.EventLogin,
            //  new Firebase.Analytics.Parameter[] {
            //new Firebase.Analytics.Parameter(
            //  Firebase.Analytics.FirebaseAnalytics.ParameterMethod, "AfterLogInAction"),
            //  }
            //);
        }
        else
        {
#if !UNITY_EDITOR
                       
            AnalyticsEvent.LevelFail(GetCurrentCity().name, GetGamePlayStats());
#endif

            //Firebase.Analytics.Parameter[] parameters = new Firebase.Analytics.Parameter[] {
            //    new Firebase.Analytics.Parameter("name", GetCurrentCity().name),
            //    new Firebase.Analytics.Parameter("score", 0),
            //      new Firebase.Analytics.Parameter("time", (long)attributes.Get().timer.GetTime()),
            //         new Firebase.Analytics.Parameter("stars", 0),
            //            new Firebase.Analytics.Parameter("wrongMoves", wrongCounter),
            //               new Firebase.Analytics.Parameter("movesCounter", movesCounter)
            //};



            //Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelEnd, parameters);

            JSONObject jo = new JSONObject();
            jo.AddField("name", GetCurrentCity().name);
            jo.AddField("score",0);
            jo.AddField("time", (long)attributes.Get().timer.GetTime());
            jo.AddField("stars", 0);
            jo.AddField("wrongMoves", wrongCounter);
            jo.AddField("movesCounter", movesCounter);

            Firebase.Analytics.Parameter[] parameters = new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevelName, GetCurrentCity().name),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterSuccess, jo.ToString())
            };

            AnalyticsController.Instance.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelEnd, parameters);

            AudioController.Instance.PlayLoose();
            GetCurrentCity().AddUnsuccesfullPlay();
            UserController.Instance.SaveGame();
            Disable();
            MapViewController.Instance.Enable();
        }

        correctDrops = 0;
        specialDrops = 0;

        Analytics.FlushEvents();
    }

    private Dictionary<string, object> GetGamePlayStats()
    {
        var stats = new SuccessScreenController.ScoreStatistics();
        int stars = GetStars(ref stats);

        if (!GetCurrentCity().CheckIfStarsAvailable())
        {
            stars = 0;
        }

        Dictionary<string, object> customParams = new Dictionary<string, object>();
        customParams.Add("score", GetScore());
        customParams.Add("time", attributes.Get().timer.GetTime());
        customParams.Add("stars", stars);
        customParams.Add("wrongMoves", wrongCounter);
        customParams.Add("correctMoves", correctDrops);
        customParams.Add("movesCounter", movesCounter);
       // customParams.Add("user_id", SystemInfo.deviceUniqueIdentifier);
        customParams.Add("user_id","");
        return customParams;
    }

    public override bool ReadyToSkipConfirm()
    {
        return gameEnded;
    }

    private void AddPoints()
    {

    }

    public void EndGameWithoutPopUps()
    {
        enabled = false;
        correctDrops = 0;
        GetCurrentCity().GetCurrentLanguageSet().isNew = false;
        GetCurrentCity().GetCurrentLanguageSet().allRuns++;
        UserController.Instance.AddEntry(GetCurrentCity().GetController(), 0, 0);
        Disable();
        MapViewController.Instance.Enable();
    }

    public void ActualizeText()
    {
        attributes.Get().starCapitalText.text = UserController.Instance.GetStars().ToString();
        if (attributes.Get().starCapitalGroup != null)
        {
            attributes.Get().starCapitalGroup.GetComponent<RectTransform>().ForceRebuildLayoutImmediate();
        }
    }

    public override void Disable()
    {
        base.Disable();

        if (lastCity != null)
        {
            if (lastCity.type == CitiesGameplayType.tutorial)
                InteractiveTutorial.Instance.Disable();

            GetCurrentCity().GetController().Disable();
        }

        OutOfStarsScreen.Instance.Disable();
        correctDrops = 0;
        trash.ForEach(o => Destroy(o));
        trash.Clear();
        portraitFields.dragableStartList.Clear();
        landscapeFields.dragableStartList.Clear();
        assetLoaded = false;
        enabled = false;
        endedSuccesfully = false;
        generalGameplayEnabled = false;

        DisableDistractors();

        FloatingTextPanel.Instance.Hide();
        FloatingPointPanelController.Instance.Hide();

        InfoScreen.Instance.onDisable.Clear();
        InfoScreen.Instance.Disable();
        AudioController.Instance.ChangeAmbient(ElementsDatabase.AmbientSound.UNDEFINED);
    }

    private bool shouldRestoreDistractors = false;

    //[NaughtyAttributes.Button("SuspendDistractorWithAnimation")]
    public void SuspendDistractorWithAnimation()
    {
        shouldRestoreDistractors = distractorsEnabled;
        distractorSuspendedByAnimation = true;
        DisableDistractors();
    }

    //[NaughtyAttributes.Button("UnSuspendDistractorWithAnimation")]
    public void ChangeSuspendedByAnimation()
    {
        distractorSuspendedByAnimation = false;
        statistics.Reset();

        if (shouldRestoreDistractors)
        {
            DistractorsController.Instance.RunLastDestractor();
        }
    }

    public void DisableDistractors()
    {
        // disable drone distractor if enabled

        distractorsEnabled = false;
        distractorSuspended = false;
        statistics.Reset();

        try
        {
            DronesDistractorController.Instance.Disable();
            SandDistractorController.Instance.Disable();
            SquaresDistractorController.Instance.Disable();
            ShakeDetector.Instance.enabled = false;
            ShakeDetector.Instance.gameObject.SetActive(false);
            SquaresDistractorController.Instance.Disable();
        }
        catch (System.Exception)
        {
            Debug.LogWarning("Distractor error");
        }
    }

 
    public void ScrollRight()
    {
        ActualizeSrollButtons();
    }

    public void ScrollLeft()
    {
        ActualizeSrollButtons();
    }

    private void ActualizeSrollButtons()
    {
        portraitFields.ActualizeSrollButtons();
    }

    public void RebuildDragableScroll()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine("RebuildDragableScrollCoroutine");
    }

    IEnumerator RebuildDragableScrollCoroutine()
    {
        yield return new WaitForSeconds(1f);

        if (landscapeFields.scrollRect != null)
        {
            landscapeFields.scrollRect.content.ForceRebuildLayoutImmediate();
        }

        if (portraitFields.scrollRect != null)
        {
            portraitFields.scrollRect.content.ForceRebuildLayoutImmediate();
        }
    }

    private List<Spot> availableHighlights = new List<Spot>();
    private int hintBuyCounter = 0;

    // spending stars
    public void TopCenterElement_OnClick()
    {
        if (dropDown_script.IsEnabled())
            return;              

        if (!Rules.BoostersRules.BoosterButtonActive(GetCurrentCity().GetMyGroup().id) || GetCurrentCity().type == Elements.CitiesGameplayType.tutorial)
        {
            "Button not active".LogDev();
            return;
        }

        SetTimerState(true);
        BonusesScreen.Instance.Enable();
    }

    public void BuyHighlight()
    {
        List<Spot> spotsLeft = availableHighlights.FindAll(o => o.parentSpotController.closeArea.flashEnabled == false && o.parentSpotController.spot.IsAvailable());
        
        if (spotsLeft.Count == 0)
            return;

        if (UserController.Instance.GetStats().freeHighlight)
        {
            UserController.Instance.GetStats().freeHighlight = false;
            UserController.Instance.SaveStatistics();
            FloatingTextPanel.Instance.Show(new FloatingTextParameters() { text = TextTranslationModule.GetWord("Free Booster On") });
        }
        else
        {
            if (UserController.Instance.GetStars() < Rules.GameBalance.highlightBonus)
            {
                OutOfStarsScreen.Instance.Enable();
                return;
            }

            UserController.Instance.RemoveStars(Rules.GameBalance.highlightBonus, "buy_highlight", "", true);
        }

        hintBuyCounter++;

        EnableHintHighlight();
    }

    public void EnableHintHighlight()
    {
        List<Spot> spotsLeft = availableHighlights.FindAll(o => o.parentSpotController.closeArea.flashEnabled == false && o.parentSpotController.spot.IsAvailable());

        if (spotsLeft.Count == 0)

            return;
        // choose one wich was not choosen before 
        int rand = UnityEngine.Random.Range(0, spotsLeft.Count - 1);
        Spot spot = spotsLeft[rand];

        // enable      
        spot.parentSpotController.closeArea.FlashEnable();

        CheckIfSpotIsVisible(spot);
        spotsLeft.RemoveAt(rand);

        ActualizeText();
    }

    private void CheckIfSpotIsVisible(Spot spot)
    {
        Vector3 spotPosition = spot.parentSpotController.transform.localPosition;
        Vector3 newPosition = GetCurrentCity().GetController().image.transform.localPosition;
        newPosition.x = spotPosition.x * GetCurrentCity().GetController().image.transform.localScale.x * -1f;
        newPosition.y = spotPosition.y * GetCurrentCity().GetController().image.transform.localScale.y * -1f;
        GetCurrentCity().GetController().HintAtnimationStart(newPosition.x, newPosition.y);
        return;
       }

    public void HighlightedElementsInfo()
    {
        InfoScreen.Instance.Enable("Fit angles first within highlighted areas");
        InfoScreen.Instance.onDisable.Add(delegate ()
        {
            attributes.Get().timer.Enable();
        });

        attributes.Get().timer.Disable();
    }


    public void DropDownButton()
    {
        if (!dropDown_script.gameObject.activeInHierarchy)
        {
            dropDown_script.Enable();
            SetTimerState(true);
        }
        else
        {
            dropDown_script.Disable();
            SetTimerState(false);
        }
    }

    public void DropDown_Disable()
    {
        SetTimerState(false);
    }

    public void StarSpendingButton()
    {

    }

    // last completed letter
    // completion counter

    private LetterController lastProccessedLetter = null;
    private int prePreparingCounter = 0;
    private int prePreparingStars = 0;

    private bool magentaCompleted = false;

    public void PrePreparingCheck(Spot spot, bool fullyCompleted)
    {
        CheckMagentaCompletness();

        LetterController currentLetter = spot.parentSpotController.letter;

        if (currentLetter == null)
        {
            return;
        }

        if (!spot.parentSpotController.letter.specialLetter)
        {
            lastProccessedLetter = currentLetter;
            distractorSuspended = false;          
            if (!magentaCompleted)
            {
                prePreparingCounter = 0;
                prePreparingStars = 0;
                ClearMagentaMarkers();
            }
            return;
        }

        lastProccessedLetter = currentLetter;

        prePreparingCounter++;
        distractorSuspended = true;

        if (prePreparingCounter > 1)
        {
            prePreparingStars ++;
        }

        SetMagentaMarker();

        if (fullyCompleted)
        {
            if (Rules.GameplayRules.VisualisationExplenation() && prePreparingStars>0)
            {
                FloatingTextPanel.Instance.Show(new FloatingTextParameters()
                {
                    text = TextTranslationModule.GetWord("Special angles completed") + "!"
                });
            }

            magentaCompleted = true;
            int gameMode = UserController.Instance.GetGameModeID();
            if (gameMode == 3)
            {
                prePreparingStars *= 3;
            }
            else
            {
                prePreparingStars *= 2;
            }

            "full completed".LogDev();
            distractorSuspended = false;
            prePreparingCounter = 0;
            ClearMagentaMarkers();
            return;
        }
    }

    public void CheckMagentaCompletness()
    {
        if (!Rules.GameplayRules.VisualisationExplenation())
        {
            return;
        }
        
        TextControllerSet set = GetCurrentCity().GetController().CurrentSet();

        if(set == null)
        {
            "set null".LogDev();
            return;
        }

        foreach (LetterController lc  in set.letters)
        {
            if (lc.specialLetter)
            {              
                LetterController.CompletnessLevel completness = lc.CompletenessLevel();
                //string.Format("Completness. All: {0}, Done: {1}", completness.allElements, completness.correctDrops).LogDev();
            }
        }
    }

    public float pulsePointsReplay = 3f;
    private Sequence mySequence = null;

    [NaughtyAttributes.Button("Pulse")]
    public void PulsePointsAndStars()
    {
        attributes.Get().starCapitalGroup.transform.DOKill();
        attributes.Get().starCapitalGroup.transform.localScale = Vector3.one;
        
        mySequence = DOTween.Sequence();
        mySequence.Append(attributes.Get().starCapitalGroup.transform.DOScale(1.5f, .5f).SetLoops(4, LoopType.Yoyo)).AppendInterval(pulsePointsReplay).SetLoops(-1,LoopType.Yoyo);
    }

    private bool magentaTutorialShown = false;

    private void SetMagentaMarker()
    {
        if (!Rules.GameplayRules.VisualisationExplenation())
        {
            return;
        }

        TextControllerSet set = GetCurrentCity().GetController().CurrentSet();

        if (set == null)
        {
            "set null".LogDev();
            return;
        }

        LetterController specialLetter = set.letters.Find(o => o.specialLetter);
        if (specialLetter.pairs.Count < 2)
            return;

        List<ElementsPair> pairsLeft = specialLetter.pairs.FindAll(o => o.done == false);
        //pairsLeft.Count.LogDev("Pairs fit: ");
        if (pairsLeft.Count > 0)
        {
            int degrees = pairsLeft[0].spotController.spot.degrees;
            foreach(GameObject g in trash)
            {
                if (g != null)
                {
                    DragableElement de = g.GetComponent<DragableElement>();
                    if (de != null && de.magentaAvailable && de.GetDegrees() == degrees)
                    {
                        //Debug.Log("choosen", g);
                        scrollRect.horizontalNormalizedPosition = 0f;
                        de.SetAsMagenta();
                        g.transform.SetAsFirstSibling();

                        // check if display tutorial

                        string key = "magentaTutorial" + UserController.Instance.GetLanguage();
                        int value =  UserController.Instance.GetStats().GetInt(key,0);

                        if (value<2 || !magentaTutorialShown)
                        {
                            if (value < 2)
                            {
                                "value updated".LogDev();
                                value++;
                                UserController.Instance.GetStats().AddInt(key, value);
                            }

                            magentaTutorialShown = true;
                            ShowMagentaTutorial(de);
                        }
                        return;
                    }
                }
            }
        }
    }

    private void ClearMagentaMarkers()
    {
        if (!Rules.GameplayRules.VisualisationExplenation())
        {
            return;
        }

        "clear magenta".LogDev();
        trash.ForEach(delegate (GameObject g)
        {
            if (g != null)
            {
                DragableElement de = g.GetComponent<DragableElement>();
                if (de != null)
                {
                    de.ClearMagentaFeatures();
                }
            }
        });
    }

    public int testBonuses = 1;

    [NaughtyAttributes.Button("Test bonuses")]
    public void TestBonues1()
    {
        BonusesScreen.Instance.Enable(testBonuses);
    }

    private DragableElement magentaDragableElement;

    public void ShowMagentaTutorial(DragableElement de)
    {
        magentaDragableElement = de;
        magentaTutorial_gameObject.SetActive(true);

        highlightExplenationAngle.sprite = de.GetElementSprite();

        Canvas c = magentaDragableElement.GetComponent<Canvas>();
        if(c == null)
        {
           c = magentaDragableElement.gameObject.AddComponent<Canvas>();
        }

        ElementReplacement er = magentaDragableElement.GetComponent<ElementReplacement>();
        if(er != null)
        {
            er.motionAllowed = false;
        }        

        c.overrideSorting = true;
        c.sortingOrder = 20;
    }

    public void ClearMagentaTutporial()
    {
        magentaTutorial_gameObject.SetActive(false);
        Canvas c = magentaDragableElement.GetComponent<Canvas>();

        if(c != null)
        {
            Destroy(c);
        }

        ElementReplacement er = magentaDragableElement.GetComponent<ElementReplacement>();
        if (er != null)
        {
            er.motionAllowed = true;
        }
    }

    public void ShowBoostersTutorial()
    {
        boostersTutorial_gameObject.SetActive(true);
        topCenter_canvas.overrideSorting = true;
        if (topCenter_canvas.GetComponent<GraphicRaycaster>() != null)
        {
            topCenter_canvas.GetComponent<GraphicRaycaster>().enabled = false;
        }
    }

    public void HideBoostersTutorial()
    {
        if (boostersTutorial_gameObject.activeInHierarchy)
            topCenter_canvas.overrideSorting = false;

        boostersTutorial_gameObject.SetActive(false);

        if (topCenter_canvas.GetComponent<GraphicRaycaster>() != null)
        {
            topCenter_canvas.GetComponent<GraphicRaycaster>().enabled = true;
        }
    }

    public void GoToBonusesScreen()
    {
        SetTimerState(true);
        BonusesScreen.Instance.Enable(1);
        BonusesScreen.Instance.RegisterOnDisableAction(CityAnimationEnd);
    }
    
    public void BuyBonus(string type)
    {
        UserController uc = UserController.Instance;
        int capital = uc.GetStars();
        switch (type)
        {
            case "auto-rotation":
                if (bonuses.autoRotationActive)
                {                   
                    return;
                }

                if (UserController.Instance.GetStats().freeAutorotation)
                {
                    UserController.Instance.GetStats().freeAutorotation = false;
                    UserController.Instance.SaveStatistics();
                    bonuses.autoRotationActive = true;
                    FloatingTextPanel.Instance.Show(new FloatingTextParameters() { text = TextTranslationModule.GetWord("Free Booster On") });
                    //ActualizeText();
                    break;
                }

                if (UserController.Instance.GetStats().autoRotation_balance > 0)
                {
                    UserController.Instance.GetStats().autoRotation_balance--;
                    UserController.Instance.SaveStatistics();
                    bonuses.autoRotationActive = true;
                    ActualizeText();
                    break;
                }
                
                if (!CheckCapitalOrRunOut(capital, Rules.GameBalance.autoRotationBonus))
                    return;

                uc.RemoveStars(Rules.GameBalance.autoRotationBonus, "buy_auto-rotation", "",true);
                bonuses.autoRotationActive = true;
                ActualizeText();
                break;
            case "no-distractor":
                if (bonuses.noDistractorActive)
                {
                    return;
                }

                if (UserController.Instance.GetStats().freeNoDistractor)
                {
                    UserController.Instance.GetStats().freeNoDistractor = false;
                    UserController.Instance.SaveStatistics();
                    bonuses.noDistractorActive = true;
                    FloatingTextPanel.Instance.Show(new FloatingTextParameters() { text = TextTranslationModule.GetWord("Free Booster On") });
                    //ActualizeText();
                    break;
                }

                if (UserController.Instance.GetStats().noDistractor_balance > 0)
                {
                    UserController.Instance.GetStats().noDistractor_balance--;
                    UserController.Instance.SaveStatistics();
                    DisableDistractors();
                    bonuses.noDistractorActive = true;
                    ActualizeText();
                    break;
                }

                if (!CheckCapitalOrRunOut(capital, Rules.GameBalance.noDistractorBonus))
                    return;

                DisableDistractors();
                uc.RemoveStars(Rules.GameBalance.noDistractorBonus, "buy_no-distractors", "", true);
                bonuses.noDistractorActive = true;
                ActualizeText();
                break;
            case "endless-timer":
                if (bonuses.endlessTimerActive)
                {
                    //"Already has".Log();
                    return;
                }


                if (UserController.Instance.GetStats().freeEndlessTimer)
                {
                    UserController.Instance.GetStats().freeEndlessTimer = false;
                    UserController.Instance.SaveStatistics();
                    bonuses.endlessTimerActive = true;
                    FloatingTextPanel.Instance.Show(new FloatingTextParameters() { text = TextTranslationModule.GetWord("Free Booster On") });
                    //ActualizeText();
                    break;
                }

                if (UserController.Instance.GetStats().endlessTimer_balance > 0)
                {
                    UserController.Instance.GetStats().endlessTimer_balance--;
                    UserController.Instance.SaveStatistics();
                    bonuses.endlessTimerActive = true;
                    ActualizeText();
                    break;
                }

                if (!CheckCapitalOrRunOut(capital, Rules.GameBalance.endlessTimerBonus))
                    return;

                uc.RemoveStars(Rules.GameBalance.endlessTimerBonus, "buy_endless-timer", "", true);
                bonuses.endlessTimerActive = true;
                ActualizeText();
                break;
        }

        //dropDown_script.ActualizeBonusesButtons();
        //dropDown_script.Disable();
        //DropDownButton();
    }

    public bool CheckCapitalOrRunOut(int capital, int price)
    {
        if (capital < price)
        {
#if !UNITY_EDITOR
                       
            Dictionary<string, object> customParams = new Dictionary<string, object>();
            customParams.Add("user_id", SystemInfo.deviceUniqueIdentifier);
          
            AnalyticsEvent.Custom("run_out_of_stars", customParams);
#endif
            OutOfStarsScreen.Instance.Enable();
            return false;
        }

        return true;
    }

    public int GetCorrectDrops()
    {
        return correctDrops;
    }

    public int GetDropsTarget()
    {
        return startElementsCount;
    }

    #region Blinking limitation


    private bool blinkingLimitation = false;
    private int blinkinkLimit = 0;
    private bool blinkingLimitationDisplayed = false;
    private bool blinkingLimitationReadyToDisplay = false;

    public void ReportUniqueBlinking()
    {
        //"reporting blinnking".LogDev();
        if (blinkingLimitation)
        {
            //"reporting blinnking limititaion on".LogDev();
            if (blinkinkLimit > 0)
            {
                blinkinkLimit--;
                blinkinkLimit.Log("Blinking left");
                //blinkinkLimit.LogDev("reporting blinnking - reported");

                if (blinkinkLimit == 0)
                {
                    blinkingLimitationReadyToDisplay = true;
                }                  
            }            
        }
    }

    public void CheckBlinkingLimitationDisplayStatus()
    {
        if (blinkingLimitation && blinkingLimitationReadyToDisplay && !blinkingLimitationDisplayed)
        {
            blinkingLimitationDisplayed = true;
            NoMoreAnimationDisplay();
        }
    }

    private void NoMoreAnimationDisplay()
    {
        if (IsReady())
        {
            "No more animation skipped!!".LogDev();
            return;
        }

        string key = "noMoreAnimationDisplayed";

        bool wasDisplayed = UserController.Instance.GetStats().HasFlag(key);
        float time = 3f;

        if (!wasDisplayed)
        {
            UserController.Instance.GetStats().AddFlag(key);
            time = 5f;
        }

        UnityEngine.Events.UnityAction clickAction = delegate ()
        {
            blinkingAnimationExplenation.Enable();
            SuspendDistractorWithAnimation();
            blinkingAnimationExplenation.RegisterOnDisableAction(delegate() {
                PulsePointsAndStars();
                ChangeSuspendedByAnimation();

            });
            FloatingTextPanel.Instance.Hide();
        };
        

        FloatingTextParameters parameters = new FloatingTextParameters() {
            text = TextTranslationModule.GetWord("No more blinking animations!"),
            time = time,
            generalClickAction = clickAction,
            rightIconClickAction = clickAction,
            type = FloatingTextParameters.Type.INTERACTABLE
        };

        FloatingTextPanel.Instance.Show(parameters);
        PulsePointsAndStars();
    }

    public bool CanBlink()
    {
        if (!blinkingLimitation)
            return true;

        if (blinkinkLimit > 0)
            return true;

        return false;
    }


    #endregion

    #region Buttons for editor and console commands

    //[NaughtyAttributes.Button("Drones")]
    //public void RunDrones()
    //{
    //    DistractorsController.Instance.RunDistratctor(3);
    //}

    //[NaughtyAttributes.Button("Sand")]
    //public void RunSand()
    //{
    //    DistractorsController.Instance.RunDistratctor(1);
    //}

    //[NaughtyAttributes.Button("Squares")]
    //public void RunSquares()
    //{
    //    DistractorsController.Instance.RunDistratctor(4);
    //}

    //[NaughtyAttributes.Button("Trash")]
    //public void RunTrash()
    //{
    //    DistractorsController.Instance.RunDistratctor(2);
    //}

    //[NaughtyAttributes.Button("Adjust colors")]
    //public void ColorsAdjust()
    //{
    //    colors.AdjustColors(UnityEngine.Random.Range(0,4));
    //}
       
    [NaughtyAttributes.Button("Help: look out for logic")]
    public void DEV_HelpLookOut()
    {
        FloatingTextPanel.Instance.Show(new FloatingTextParameters()
        {
            text = TextTranslationModule.GetWord("Help: look out for logic to<br>solve the level fast"),
            generalClickAction = delegate () {
                FloatingTextPanel.Instance.Hide();
            }
        });
    }

    public void Console_FinishLevel()
    {
        correctDrops = startElementsCount;
        ComputeScorePoints();
        EndGameIfReady();
    }

    #endregion
}

public class GameplayBonuses
{
    public bool autoRotationActive = false;
    public bool noDistractorActive = false;
    public bool endlessTimerActive = false;

    public void Reset()
    {
        autoRotationActive = false;
        noDistractorActive = false;
        endlessTimerActive = false;
    }
}

[System.Serializable]
public class GameplayBalance
{
    public int minusPointsForEverySecond = 2;
    public int pointsForEveryDragable = 100;
    public int secondsForAngle = 10;
}

#region ---Attributes

[System.Serializable]
public class GamePlayAttributes
{
    public RectTransform container;

    public Text pointsText;
    public Text starAttemptsText, starCapitalText;

    public LayoutGroup starCapitalGroup;

    public GameObject timerNormalState, timerPausedState, starsPanel_topCenter;       

    public List<GameObject> separators;

    public ScrollRect scrollRect;
    public GameObject scrollButtonRight, scrollButtonLeft;
    public List<GameObject> dragableStartList = new List<GameObject>();

    public ZoomButton zoom_button;

    //public GameObject zoomButton;
    //public GameObject zoomInfo;
    //public CanvasGroup zoomButtonCanvasGroup;

    public TimerGamePlayController timer;

    public Text generalTimer;

    public void ActualizeSrollButtons()
    {
        if (scrollRect.horizontalNormalizedPosition < .1f)
        {
            scrollButtonLeft.SetActive(false);
            scrollRect.horizontalNormalizedPosition = 0f;
        }
        else
            scrollButtonLeft.SetActive(true);

        if (scrollRect.horizontalNormalizedPosition > .9f)
        {
            scrollButtonRight.SetActive(false);
            scrollRect.horizontalNormalizedPosition = 1f;
        }
        else
            scrollButtonRight.SetActive(true);
    }

    public void EnableStarsTexts()
    {
        InitializeStarsTexts(true);
    }

    public void DisableStarsTexts()
    {
        InitializeStarsTexts(false);
    }

    public void InitializeStarsTexts(bool state)
    {
        starCapitalText.gameObject.SetActive(state);
        separators.ForEach(o => o.SetActive(state));
    }

    public void DisableBottonHorizontalGroup()
    {
        if (container == null)
            return;

        LayoutGroup lg = container.GetComponent<LayoutGroup>();
        lg.enabled = false;
    }

    public void EnableBottonHorizontalGroup()
    {
        LayoutGroup lg = container.GetComponent<LayoutGroup>();
        lg.enabled = true;
    }

    [System.Serializable]
    public class ZoomButton
    {
        public GameObject gameObject;
        public Image image;
        public GameObject overlayInfo;
        public CanvasGroup canvasGroup;
        public GameObject screenOverlay;
        private bool isZoomed = false;

        public void SetAlpha(float a)
        {
            canvasGroup.alpha = a;
        }

        public bool IsZoomed()
        {
            return isZoomed;
        }

        public void Reset()
        {
            isZoomed = false;
            canvasGroup.alpha = 1f;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetSprite(Sprite s)
        {
            image.sprite = s;
        }

        public void NotInteractable()
        {
            image.raycastTarget = false;
        }

        public void Interactable()
        {
            image.raycastTarget = true;
        }
    }
}

#endregion

#region Stats

public class GameplayStatistics
{
    public bool showLogs = false;
    public float idleTimer = 0f;
    public float lastPortalDistractor = 0f;
    public float entertainingTimer = 0f;
    public int gameMode = -1;
    public float madnessModeTimer = 0f;
    
    public List<float> movesTimers = new List<float>();
    GamePlayController controller;


    public float idleTime = 18f;
    public bool halfElementsCompletedTrigger = false;

    public void ReportMove()
    {
        movesTimers.Add(Time.realtimeSinceStartup);

        if (movesTimers.Count > 5)
        {
            movesTimers.RemoveAt(0);
        }
    }
  
    public void ResetIdleTimer()
    {
        idleTimer = Time.realtimeSinceStartup;
    }

    public void Reset()
    {
        controller = GamePlayController.Instance;
        idleTimer = lastPortalDistractor = entertainingTimer = madnessModeTimer = Time.realtimeSinceStartup;
        gameMode = UserController.Instance.GetGameModeID();
        movesTimers.Clear();
    }    
}

#endregion


