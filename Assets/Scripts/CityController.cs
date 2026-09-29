using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CityController : MonoBehaviour
{
    //public bool randomizedDropSpots = false;
    //public bool showShadows = false;

    public string country;
    public string cityName;

    public List<Spot> spots = new List<Spot>();

    public List<SpotController> manualSpotList;

    public List<SpotsGroup> groups;

    public GameObject spotsParent;

    public bool zoomed = false;

    public Image image;
    RectTransform _rectTransform;
    private RectTransform _imageRectTransform, parentsRectTransform;

    private ScrollRect _scrollRect;
    private Button animationtopButton;

    private bool externalAnmation = false;

    public bool autoCollectSpots = true;
    public bool enableTutorial = false;

    public int freeMagentaTries;
    public Elements.Cities cityObject = null;

    public float _initialScale;

    [NaughtyAttributes.Button("Enable gameplay with me")]
    public void EnableGameWithMe()
    {
        //cityName.Log("City name: ");

        Elements.Cities c = Elements.ElementsDatabase.Instance.GetCityByName(cityName);
        if (c != null)
            GamePlayController.Instance.Enable(c);
    }

    public string GetCurrentWord()
    {
        return GetTextOverlayController().GetHashtagWord();
    }

    public string[] GetCurrentWords()
    {
        string words = GetTextOverlayController().GetHashtagWord();
        if (words.Contains("<br>"))
        {
            string[] wordsArray = words.Split(new string[] { "<br>" }, System.StringSplitOptions.None);
            return wordsArray;

        }

        return new string[] { words };
    }

    public void OnScrollingEvent(Vector2 v)
    {

    }

    public int CountWordsForCurrent()
    {
        string hashTagWord = GetCurrentWord();

        if (hashTagWord.Contains("<br>"))
        {
            string[] words = hashTagWord.Split(new string[] { "<br>" }, System.StringSplitOptions.None);
            return words.Length;
        }
        else
        {
            return 1;
        }
    }

    public string GetCurrentWordWithHash()
    {
        string hashTagWord = GetCurrentWord();
        string output = "";

        if (hashTagWord.Contains("<br>"))
        {
            string[] words = hashTagWord.Split(new string[] { "<br>" }, System.StringSplitOptions.None);
            foreach (string s in words)
            {
                output += string.Format(" # {0}", s);
            }
        }
        else
        {
            output = string.Format("# {0}", hashTagWord);
        }

        return output;
    }

    public string GetCurrentSentence()
    {
        return GetTextOverlayController().GetHashTagSentence();
    }

    public string GetCurrecntSentenceDots()
    {
        string word = GetCurrentWord();
        int counter = CountWordsForCurrent();      

        string sentence = GetTextOverlayController().GetHashTagSentence();

        if (counter == 1)
        {
            return string.Format(sentence, "<b> . . . </b>");
        }
        else
        {
            return string.Format(sentence, "<b> . . . </b>", "<b> . . . </b>");
        }
    }

    public string GetCurrecntSentenceDotsMin()
    {
        string word = GetCurrentWord();
        int counter = CountWordsForCurrent();

        string sentence = GetTextOverlayController().GetHashTagSentenceMin();
        return sentence;
        //if (counter == 1)
        //{
        //    return string.Format(sentence, "<b> . . . </b>");
        //}
        //else
        //{
        //    return string.Format(sentence, "<b> . . . </b>", "<b> . . . </b>");
        //}
    }

    public string GetCurrentSentenceWithWord()
    {
        string[] words = GetCurrentWords();

        if (words.Length > 1)
        {
            return string.Format(GetTextOverlayController().GetHashTagSentence(), "<b><size=35>#" + words[0] + "</size></b>", "<b><size=35>#" + words[1] + "</size></b>");
        }
        else
        {
            return string.Format(GetTextOverlayController().GetHashTagSentence(), "<b><size=35>#" + words[0] + "</size></b>");
        }
    }   

    public TextOverlayController GetTextOverlayController()
    {
        textOverlayController = TextDeconstructionController.Instance.GetMyOverlay(this);

        if (textOverlayController != null && textOverlayController.gameObject != null)
        {
            textOverlayController.gameObject.SetActive(false);
            textOverlayController.HideLettersImages();
        }
        else
        {
            "null toc".LogDev();
        }        

        return textOverlayController;
    }

    public TextControllerSet CurrentSet()
    {
        return GetTextOverlayController().sets.Find(o => o.language.ToLower() == UserController.Instance.GetLanguage());
    }

    private EventTrigger eventTrigger;

    private Elements.Cities GetCity()
    {
        if(cityObject == null)
            cityObject = Elements.ElementsDatabase.Instance.GetCityByName(cityName);

        return cityObject;
    }

    public void Initialize()
    {
        cityObject = Elements.ElementsDatabase.Instance.GetCityByName(cityName);


        FirebaseDatabaseController.Instance.GetDataForCity(cityObject.country.name, cityObject.name);

        randomizedSpots = null;
        spots = new List<Spot>(SpotsList());
                

        textOverlayController = GetTextOverlayController();

        enterAnimationEnded = false;
        gameObject.SetActive(true);
        externalAnmation = false;
        zoomingAnimation = false;

        shadowsOpacity = 0f;
        progressiveShadowsHintEnabled = false;
        progressiceShadowsTimer = 0f;

        parentsRectTransform = GetComponent<RectTransform>();
        _scrollRect = GetComponent<ScrollRect>();
        animationtopButton = _scrollRect.content.GetComponent<Button>();

        eventTrigger = _scrollRect.content.GetComponent<EventTrigger>();

        if (eventTrigger == null)
        {
            eventTrigger = _scrollRect.content.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => { OnClick(); });
            eventTrigger.triggers.Add(entry);
        }

        if (textOverlayController != null)
            textOverlayController.Initialize();

        // reset position
        _imageRectTransform = image.GetComponent<RectTransform>();
        _imageRectTransform.anchoredPosition = Vector2.zero;

        // reset zoom - no zoom yet


        foreach (Spot s in spots)
        {
            if (s != null)
                s.parentSpotController.Initialize(this);
            else
            {
                "null in c".LogDev();
            }

            if (s != null && s.parentSpotController != null && s.parentSpotController.letter != null)
                s.parentSpotController.letter.Reset();
            else
            {
                "null in c".LogDev();
            }
        }

        spots.ForEach(o => o.Reset());
        spots.ForEach(delegate (Spot s)
        {
            Image i_c = s.parentSpotController.closeArea.GetComponent<Image>();
            Image i_d = s.GetComponent<Image>();

            if (i_c != null)
            {
                Color c = i_c.color;
                c.a = 0;
                i_c.color = c;
            }


            if (i_d != null)
            {
                Color c = i_d.color;
                c.a = 0;
                i_d.color = c;
            }
        });

        if (GetCity().HasRandomizedDropSpots() && GetCity().showMimics)
        {
            spots.ForEach(o => o.figureImage.color = o.parentSpotController.shadowColor * new Color(1f, 1f, 1f, GetCity().mimicsTransparency));
        }

        magentaDone = false;
        TextDeconstructionController.Instance.sentenceText.text = GetCurrecntSentenceDotsMin();
    }

    // animation for hints

    private bool hintAnimation = false;

    private Vector3 hintAnimationTarget;

    private float hintAnimationSpeed = 3f;
    private float hintAnimationLastShift = 0f;
    private int hintAnimationStepCounter = 0;

    private bool zoomingAnimation = false;
    private Vector3 zoomingAnimationTarget;
    private float zoomingAnimationSpeed = 3f;

    public void HintAtnimationStart(float target, float? targetY = null)
    {
        hintAnimation = true;
        hintAnimationTarget = image.transform.localPosition;
        hintAnimationTarget.x = target;
        if (targetY != null && zoomed)
        {
            hintAnimationTarget.y = (float)targetY;
        }
        hintAnimationLastShift = -1f;
        hintAnimationStepCounter = 0;
        animationtopButton.enabled = true;
        image.rectTransform.pivot = Vector2.one * .5f;
    }

    public void HintAnimationStop()
    {
        hintAnimation = false;
        hintAnimationStepCounter = 0;
        animationtopButton.enabled = false;
    }

    private void Update()
    {
        if (hintAnimation)
        {
            hintAnimationStepCounter++;

            image.transform.localPosition = Vector3.Lerp(image.transform.localPosition, hintAnimationTarget, Time.deltaTime * hintAnimationSpeed);

            float diffOffset = hintAnimationLastShift - _scrollRect.horizontalNormalizedPosition;
            //diffOffset.Log("D: ");


            if (Vector3.Distance(image.transform.localPosition, hintAnimationTarget) < 5f || (Mathf.Abs(diffOffset) < 0.001f && hintAnimationStepCounter > 5))
            {
                //"Done animation".Log();
                HintAnimationStop();
                image.transform.localPosition = hintAnimationTarget;
            }

            hintAnimationLastShift = _scrollRect.horizontalNormalizedPosition;
        }

        if (zoomingAnimation)
        {
            image.transform.localScale = Vector3.Lerp(image.transform.localScale, zoomingAnimationTarget, Time.deltaTime * zoomingAnimationSpeed);

            if (Vector3.Distance(image.transform.localScale, zoomingAnimationTarget) < .01f)
            {
                image.transform.localScale = zoomingAnimationTarget;
                zoomingAnimation = false;
            }
        }

        if (progressiveShadowsHintEnabled)
        {
            if (progressiceShadowsTimer > 0)
            {
                progressiceShadowsTimer -= Time.deltaTime;
            }
            else
            {
                opacityEvaluation += (opacityIncreaseTimerStep / 100f);
                //shadowsOpacity += opacityIncreaseStep;
                progressiceShadowsTimer = opacityIncreaseTimerStep;

                List<Spot> spots = SpotsList().FindAll(o => o.IsAvailable());

                float opacity = GamePlayController.Instance.progressHintVisibility.Evaluate(opacityEvaluation);

                spots.ForEach(delegate (Spot s)
                {
                    Color c = s.parentSpotController.shadowColor;
                    c.a = opacity;
                    s.figureImage.color = c;
                });

                opacity.LogDev("New opacity: ");

                if (spots.Count == 0 || opacity >= .8f)
                {
                    progressiveShadowsHintEnabled = false;
                }               
            }
            //wait 30 s

            // get all uncollected spots

            // set o.figureImage color to shadow with decreased opacity 

             // set timer or end
        }
    }

    public void ReportFirstPick()
    {
        // hide all flashes on magenta spots
        //DisableFlashes();
    }

    public void DisableFlashes()
    {
        SpotsList().FindAll(o => o.parentSpotController.letter.specialLetter).ForEach(o => o.parentSpotController.closeArea.FlashDisable());
    }

    public void StartLevel()
    {
        ScaleImage();

        if (enableTutorial)
        {
            InteractiveTutorial.Instance.PreEnable();
        }

        if (animationtopButton != null && _scrollRect != null)
            StartAnimation(false);
        else
            Debug.Log("Cannot start animation");

        if (cityObject.GetScenario().randomizationDragables)
            GamePlayController.Instance.attributes.Get().dragableStartList.ForEach(o => o.GetComponent<DragableElement>().RandomRotation());
    }

    public void StartExternalAnimation()
    {
        StartAnimation(true);
    }

    public void StartAnimation(bool external)
    {
        _scrollRect.horizontalNormalizedPosition = 0f;
        externalAnmation = external;
        StartCoroutine("AnimationForScroll");
    }

    public void IncorrectDrop()
    {
        //if (magentaFirstRequired && !magentaDone)
        //{
        //    freeMagentaTries--;
        //    if(freeMagentaTries == 0)
        //    {
        //        if (cityObject.GetMyGroup().id == 1)
        //        {
        //            GamePlayController.Instance.EndGame(false);
        //        }
        //        else
        //        {
        //            // TODO: take from stars capital
        //            if (UserController.Instance.GetStars() > 0)
        //            {
        //                UserController.Instance.RemoveStars(1);
        //                freeMagentaTries++;
        //            }
        //            else
        //            {
        //                GamePlayController.Instance.EndGame(false);
        //            }
        //        }
        //    }
        //}
    }

    private List<Spot> randomizedSpots = null;

    public List<Spot> SpotsList()
    {
        if (GetCity().HasRandomizedDropSpots())
        {
            if (randomizedSpots == null)
            {
                //"randomized null".LogDev();
                randomizedSpots = RandomizeSpots();
            }

            return randomizedSpots;
        }

        List<Spot> tempSpots = new List<Spot>();

        if (spotsParent != null && autoCollectSpots)
        {
            // clear all spots
            for (int i = 0; i < spotsParent.transform.childCount; i++)
            {
                Transform t = spotsParent.transform.GetChild(i);

                if (t != null && t.gameObject != null && t.gameObject.GetComponent<SpotController>() != null && t.gameObject.GetComponent<SpotController>().spot != null)
                {
                    tempSpots.Add(t.gameObject.GetComponent<SpotController>().spot);
                }
            }
        }
        else
        {
            manualSpotList.ForEach(o => tempSpots.Add(o.spot));
        }

        return tempSpots;
    }

    private bool magentaDone = false;

    public void ReportCorrectDrop(GameObject g)
    {
        //g.GetComponent<DragableElement>().Log();
        //if (magentaFirstRequired && textOverlayController.AreSpecialCompleted())
        //{
        //    SpotsList().ForEach(o => o.parentSpotController.gameObject.SetActive(true));
        //    magentaDone = true;
        //    GamePlayController.Instance.MagentasDone();
        //}
    }

    public bool IsMagentaDone()
    {
        return magentaDone;
    }


    public void EnableFlashes()
    {
        SpotsList().FindAll(o => o.parentSpotController.letter.specialLetter).ForEach(o => o.parentSpotController.closeArea.FlashEnable());
    }

    public void EnableFlashesOnAll()
    {
        SpotsList().ForEach(o => o.parentSpotController.closeArea.FlashEnable());
    }

    public void DisableFlashesOnAll()
    {
        SpotsList().ForEach(o => o.parentSpotController.closeArea.FlashDisable());
    }


    IEnumerator AnimationForScroll()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        animationtopButton.enabled = true;
       

        bool horizontalAnimation = true;

        float imageRatio = _imageRectTransform.rect.width / _imageRectTransform.rect.height;
        float screenWidthHeightRatio = parentsRectTransform.rect.width / parentsRectTransform.rect.height;

        if (screenWidthHeightRatio>imageRatio )
            horizontalAnimation = false;

        if (horizontalAnimation)
        {
            
            _scrollRect.horizontalNormalizedPosition = 0f;
        }
        else
        {
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        //horizontalAnimation.LogDev("Horizontal animation: ");
        //_scrollRect.vertical.LogDev("Vert: ");
        //screenWidthHeightRatio.LogDev("Screen ratio: ");
        //imageRatio.LogDev("Image ratio: ");

        yield return new WaitForSeconds(.4f);
        if (horizontalAnimation)
        {
            while (_scrollRect.horizontalNormalizedPosition < .95f)
            {
                _scrollRect.horizontalNormalizedPosition = Mathf.Lerp(_scrollRect.horizontalNormalizedPosition, 1f, Time.deltaTime * 1.2f);
                yield return null;
            }
        }
        else
        {
            while (_scrollRect.verticalNormalizedPosition < .95f)
            {
                _scrollRect.verticalNormalizedPosition = Mathf.Lerp(_scrollRect.verticalNormalizedPosition, 1f, Time.deltaTime * 1.2f);
                yield return null;
            }
        }

        _imageRectTransform.anchoredPosition = Vector2.zero;
        animationtopButton.enabled = false;
        OnAnimationEnd();

    }

    public void ExternalScroll(float speed, bool xaxis = true)
    {
        if (xaxis)
            _scrollRect.horizontalNormalizedPosition += speed * Time.deltaTime;
        else if (cityObject.type == Elements.CitiesGameplayType.zooming)
            _scrollRect.verticalNormalizedPosition += speed * Time.deltaTime;
    }

    private IEnumerator DisableFlashCoroutine()
    {
        yield return new WaitForSeconds(2f);
        //DisableFlashes();
    }

    public void OnClick()
    {
        //"click".Log();
        StopCoroutine("AnimationForScroll");
        animationtopButton.enabled = false;
        if (!hintAnimation)
            _imageRectTransform.anchoredPosition = Vector2.zero;
        OnAnimationEnd();
        HintAnimationStop();
    }

    public List<UnityEngine.Events.UnityAction> onAnimationEnd = new List<UnityEngine.Events.UnityAction>();
    public bool enterAnimationEnded = false;

    private void OnAnimationEnd()
    {
        //"Animation end".LogDev();
        enterAnimationEnded = true;
        onAnimationEnd.ForEach(o => o.Invoke());

        if (!externalAnmation)
        {
            if (enableTutorial)
            {
                InteractiveTutorial.Instance.Enable(this);
            }
        }

        Destroy(eventTrigger);
        GamePlayController.Instance.CityAnimationEnd();
        EnableProgressiveShadowsHint();
    }

    private float shadowsOpacity = 0f;
    private bool progressiveShadowsHintEnabled = false;
    private float progressiceShadowsTimer = 0f;
    private float opacityIncreaseTimerStep = 15f;
    private float opacityIncreaseStep = .2f;
    private float opacityEvaluation = 0f;

    private void EnableProgressiveShadowsHint()
    {
        if(cityObject.progressiveShadowsHint && !cityObject.HasRandomizedDropSpots())
        {
            opacityEvaluation = 0f;
            shadowsOpacity = 0f;
            progressiveShadowsHintEnabled = true;
            progressiceShadowsTimer = opacityIncreaseTimerStep;
        }
    }

    public void ZoomIn()
    {
        //if (!enterAnimationEnded)
        //    return;

        DKK.UICalc.TransLocalPosToPivot(image.rectTransform);

        zoomingAnimation = true;
        float mult = cityObject.zoomingMultiplier;       

        zoomingAnimationTarget = new Vector3(_initialScale * mult, _initialScale * mult, 1f);
        //_imageRectTransform.transform.localScale = new Vector3(_initialScale*2f, _initialScale * 2f, 1f);
        zoomed = true;

        //image.rectTransform.pivot = new Vector2(_scrollRect.horizontalNormalizedPosition, .5f);
    }

    public void ZoomOut()
    {
        DKK.UICalc.TransLocalPosToPivot(image.rectTransform);

        zoomingAnimationTarget = new Vector3(_initialScale, _initialScale, 1f);
        //_imageRectTransform.transform.localScale = new Vector3(_initialScale, _initialScale, 1f);
        zoomingAnimation = true;
        //image.rectTransform.pivot = new Vector2(.5f, .5f);
        zoomed = false;
    }

    [NaughtyAttributes.Button("Fit scale")]
    public void ScaleImage()
    {
        zoomed = false;

        if (_imageRectTransform == null || parentsRectTransform == null)
        {
            parentsRectTransform = GetComponent<RectTransform>();
            _imageRectTransform = image.GetComponent<RectTransform>();
        }

        float imageRatio = _imageRectTransform.rect.width / _imageRectTransform.rect.height;

        //parentsRectTransform.rect.height.LogDev("H: ");
        //parentsRectTransform.rect.width.LogDev("W: ");

        float screenWidthHeightRatio = parentsRectTransform.rect.width / parentsRectTransform.rect.height;
        //screenWidthHeightRatio.LogDev("Screen ratio: ");
        //imageRatio.LogDev("Image ratio: ");

        float newScale;

        if (imageRatio > screenWidthHeightRatio)
        {
            newScale = parentsRectTransform.rect.height / _imageRectTransform.rect.height;
        }
        else
        {
            newScale = parentsRectTransform.rect.width / _imageRectTransform.rect.width;
        }

        _initialScale = newScale;

        _imageRectTransform.transform.localScale = new Vector3(newScale, newScale, 1f);
    }

    public void ScaleImageInvert()
    {
        if (_imageRectTransform == null || parentsRectTransform == null)
        {
            parentsRectTransform = GetComponent<RectTransform>();
            _imageRectTransform = image.GetComponent<RectTransform>();
        }

        float mapWidthHeightRatio = _imageRectTransform.rect.height / _imageRectTransform.rect.width;
        float screenWidthHeightRatio = Screen.safeArea.height / Screen.safeArea.width;
        float newScale;

        if (mapWidthHeightRatio > screenWidthHeightRatio)
        {
            newScale = parentsRectTransform.rect.width / _imageRectTransform.rect.width;
        }
        else
        {
            newScale = parentsRectTransform.rect.height / _imageRectTransform.rect.height;
        }

        _imageRectTransform.transform.localScale = new Vector3(newScale, newScale, newScale);
    }


    [NaughtyAttributes.Button("Current position")]
    public void GetCurrentPosition()
    {
        Debug.Log(image.GetComponent<RectTransform>().anchoredPosition);
    }

    public void Disable()
    {
        //"Dsiabvle".Log();
        StopCoroutine("AnimationForScroll");
        StopCoroutine("DisableFlashCoroutine");

        gameObject.SetActive(false);
        hintAnimation = false;

        if (enableTutorial)
        {
            InteractiveTutorial.Instance.Disable();
        }

        DisableTextOverlay();
    }

    public TextOverlayController textOverlayController;

    public void CheckIfComplted()
    {
        if (textOverlayController != null)
        {
            if (textOverlayController.IsCompleted())
            {
                textOverlayController.gameObject.SetActive(true);
                AnimateAll();
            }
        }
    }

    public void AnimateAll()
    {
        spots.ForEach(o => o.ResetAnimation());
        spots.ForEach(o => o.parentSpotController.EnableAnimation());
    }

    public void ChangeStateOfTextOverlay()
    {
        //"change".LogDev();
        if (textOverlayController != null)
            textOverlayController.gameObject.SetActive(!textOverlayController.gameObject.activeSelf);

        if (!textOverlayController.gameObject.activeSelf)
        {
            FloatingTextPanel.Instance.Hide();           
        }

        TextDeconstructionController.Instance.sentenceText.gameObject.SetActive(textOverlayController.gameObject.activeSelf);
    }

    public void DisableTextOverlay()
    {
        //"disable".LogDev();

        if (textOverlayController != null)
        {
            if (textOverlayController.gameObject != null)
            {
                textOverlayController.gameObject.SetActive(false);
            }
        }
    }

    public void SimulateDragAndDropForAll()
    {
        // get all dragable elements left

        List<GameObject> dragables = GamePlayController.Instance.attributes.Get().dragableStartList;

        // get all spots left

        List<Spot> spots = SpotsList().FindAll(o => o.IsAvailable() == true);
        dragables.Count.LogDev("Dragables: ");
        dragables.FindAll(o => o!= null).Count.LogDev("Dragables not null: ");
        spots.Count.LogDev("Spots available: ");
        foreach (GameObject g in dragables)
        {
            if (g == null)
            {
                continue;
            }
            DragableElement dragable = g.GetComponent<DragableElement>();

            if (dragable == null)
            {
                //Debug.LogError("No dragable component");
                continue;
            }

            Spot spot = spots.Find(o => o.degrees == dragable.GetDegrees());
            if (spot == null)
            {
                //Debug.LogError("No spot for degrees " + dragable.GetDegrees());
                continue;
            }

            spots.Remove(spot);

            dragable.ExternalAnimation(spot.gameObject);
        }

        // start moving coroutine

    }

    public void GamePlaySimulation()
    {
        StartCoroutine("GamePlaySimulationCoroutine");
    }

    private IEnumerator GamePlaySimulationCoroutine()
    {
        SimulateDragAndDropForAll();

        yield return new WaitForSeconds(1f);

        //SpotsList().ForEach(o=> o.parentSpotController.letter.CorrectDrop(o.parentSpotController,false));
        // ForceOverlayDisplaying();
    }

    public void ForceOverlayDisplaying()
    {
        SpotsList().ForEach(o => o.parentSpotController.letter.CorrectDrop(o.parentSpotController, false));
        SpotsList().ForEach(o => o.parentSpotController.gameObject.SetActive(true));
        SpotsList().ForEach(o => o.parentSpotController.animationSpeed = 2f);

        if (textOverlayController != null)
        {
            if (textOverlayController.gameObject != null)
            {
                textOverlayController.gameObject.SetActive(true);
                AnimateAll();
            }
        }

    }

    public void ShowTextOverlay()
    {
        if (textOverlayController != null)
        {
            if (textOverlayController.gameObject != null && textOverlayController.HasSome() && textOverlayController.CanExit())
            {
                ChangeStateOfTextOverlay();

                if (textOverlayController.gameObject.activeSelf)
                {
                    GamePlayController.Instance.SuspendDistractorWithAnimation();
                    AnimateAll();
                }
                else
                {
                    GamePlayController.Instance.ChangeSuspendedByAnimation();
                }

                GamePlayController.Instance.EndGameIfReady();
            }
        }
    }

    public void EnableInEditor()
    {
        GamePlayController.Instance.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void DisableInEditor()
    {
        Debug.Log("Disabling: " + cityName);
        gameObject.SetActive(false);
        GamePlayController.Instance.gameObject.SetActive(false);
    }

    [NaughtyAttributes.Button("Show images")]
    public void ShowImages()
    {
        if (Elements.ElementsDatabase.Instance.GetCityByName(cityName).type == Elements.CitiesGameplayType.zooming)
            return;

        ShowDropAreas();
        ShowCloseAreas();
    }

    [NaughtyAttributes.Button("Hide images")]
    public void HideImages()
    {
        if (Elements.ElementsDatabase.Instance.GetCityByName(cityName).type == Elements.CitiesGameplayType.zooming)
            return;

        HideCloseAreas();
        HideDropAreas();

    }

    public void ShowCloseAreas()
    {
        Color color = Color.green;
        color.a = .5f;
        SpotsList().ForEach(o => o.parentSpotController.closeArea.GetComponent<Image>().color = color);
    }

    public void HideCloseAreas()
    {
        SpotsList().ForEach(o => o.parentSpotController.closeArea.GetComponent<Image>().color = new Color(0f, 1f, 1f, 0f));
    }

    public void ShowDropAreas()
    {
        Color color = Color.red;
        color.a = .5f;
        SpotsList().ForEach(o => o.GetComponent<Image>().color = color);
    }

    public void HideDropAreas()
    {
        SpotsList().ForEach(o => o.GetComponent<Image>().color = new Color(0f, 1f, 1f, 0f));
    }

    public int SpecialAnglesCounter()
    {
        //int counter = 0;
        // _city.textOverlayController.FindMyLetter(this);
        //TextDeconstructionController.Instance.GetMyOverlay(this);

        foreach (Spot s in SpotsList())
        {
            LetterController lc = TextDeconstructionController.Instance.GetMyOverlay(this).FindMyLetter(s.parentSpotController);
            if (lc == null)
                continue;

            //"not null".LogDev();

            if (lc.specialLetter)
                return lc.pairs.Count;
        }

        return -1;
    }


    //[Header("Reset")]
    [NaughtyAttributes.Button("-ResetAllGroups")]
    public void ResetAllGroups()
    {
        groups.ForEach(o => o.spots.ForEach(i => i.gameObject.SetActive(true)));
        groups.ForEach(o => o.spots.ForEach(i => i.spot.figureImage.color = Color.white));
    }

    public void HideALLSPOTS()
    {
        Transform t = image.transform.Find("Groups");
        if (t != null)
        {
            t.gameObject.SetActive(false);
            spotsParent.SetActive(false);
        }
    }

    [NaughtyAttributes.Button("RestoreGroups")]
    public void RestoreGroups()
    {
        Transform t = image.transform.Find("Groups");
        if (t != null)
        {
            "has groups".LogDev();
            groups.Clear();

            if (t.childCount > 0)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    SpotsGroup group = new SpotsGroup();
                    Transform child = t.GetChild(i);

                    if (child.tag == "dropsGroup" && child.gameObject.activeSelf)
                    {
                        group.groupID = i;
                        group.groupParent = child.gameObject;
                        if (child.childCount > 0)
                        {
                            for (int j = 0; j < child.childCount; j++)
                            {
                                Transform subChild = child.GetChild(j);
                                SpotController sc = subChild.GetComponent<SpotController>();
                                if (sc != null && subChild.gameObject.activeSelf)
                                {
                                    group.spots.Add(sc);
                                }
                            }
                        }
                    }

                    if (group.groupParent != null && group.spots.Count > 0)
                    {
                        groups.Add(group);
                    }
                }
            }
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }

    public bool CheckScales()
    {
        Transform t = image.transform.Find("Groups");
        if (t != null)
        {
            if (t.localScale != Vector3.one)
                return false;

            if (t.childCount > 0)
            {
                for (int i = 0; i < t.childCount; i++)
                {
                    if (t.GetChild(i).localScale != Vector3.one)
                        return false;
                }
            }
        }

        return true;
    }

    //[NaughtyAttributes.Button("RandomTest")]
    //public void RandomTest()
    //{
    //    List<Spot> spotsList = new List<Spot>();
    //    List<string> toCheck = new List<string>();

    //    for (int z = 0; z < 1; z++)
    //    {
    //        //bool shouldBreak = false;
    //        string tempToCheck = "";

    //        TextOverlayController toc = TextDeconstructionController.Instance.GetCity(country, cityName);
    //        List<LetterController> tempLetters = new List<LetterController>();

    //        List<SpotsGroup> tempGroups = new List<SpotsGroup>(groups);
    //        // reset groups targets
    //        groups.ForEach(o => o.spots.ForEach(i => i.tempLetterElement = null));
    //        tempGroups.ForEach(o => o.spots.ForEach(i => i.gameObject.SetActive(false)));


    //        if (toc != null)
    //        {

    //            foreach (TextControllerSet tcs in toc.sets)
    //            {
    //                if (tcs.language.ToLower() != "english")
    //                    continue;

    //                tempLetters = new List<LetterController>(tcs.letters);
    //                tempLetters = tempLetters.OrderBy(o => !o.specialLetter).ToList();

    //                //tcs.hashTagWord.LogDev("Word: ");
    //                foreach (LetterController lc in tempLetters)
    //                {
    //                    if (lc == null)
    //                        continue;

    //                    //lc.helpMessage.LogDev("Help: ");
    //                    //lc.pairs.Count.LogDev("Pairs: ");

    //                    foreach (ElementsPair ep in lc.pairs)
    //                    {
    //                        if (ep == null)
    //                            continue;

    //                        int fitCounter = 0;
    //                        float rotZ = ep.element.transform.rotation.eulerAngles.z;

    //                        //ep.spotController.spot.degrees.LogDev("Looking for degrees: ");
    //                        //rotZ.LogDev("", ep.element.gameObject);

    //                        //SpotsGroup choosenSpotGroup = null;
    //                        List<SpotsGroup> correctCandidates = new List<SpotsGroup>();
    //                        SpotController choosenSpot = null;
    //                        List<SpotController> spotsCandidates = new List<SpotController>();

    //                        foreach (SpotsGroup s in tempGroups)
    //                        {
    //                            s.spots.ForEach(delegate (SpotController sc)
    //                            {

    //                                float dist = Quaternion.Angle(sc.transform.rotation, ep.element.transform.rotation);
    //                                //string.Format("--Processing rotation {0} with degrees {1}", sc.transform.rotation.eulerAngles.z, sc.spot.degrees).LogDev();
    //                                float rotZT = sc.transform.rotation.eulerAngles.z;

    //                                double v1 = (double)rotZT;
    //                                double v2 = (double)rotZ;

    //                                bool c1 = dist == 0;
    //                                bool c2 = sc.spot.degrees == ep.spotController.spot.degrees;

    //                                // string.Format("-- [{0}] - [{1}], {2} - {3}, {4},{5}, A: {6}", v1, v2, sc.spot.degrees, ep.spotController.spot.degrees, c1, c2, dist).LogDev();                              

    //                                if ((lc.specialLetter && c1 && c2) || (!lc.specialLetter && c2))
    //                                //if ( c1 &&c2)
    //                                {
    //                                    //"!!--found".LogDev();
    //                                    if (!correctCandidates.Contains(s))
    //                                    {
    //                                        //s.groupID.LogDev("Adding cand g: ");
    //                                        correctCandidates.Add(s);
    //                                    }

    //                                    if (!spotsCandidates.Contains(sc))
    //                                    {
    //                                        sc.matchInLastIterationFound = true;
    //                                        spotsCandidates.Add(sc);
    //                                    }

    //                                    fitCounter++;
    //                                }
    //                            });
    //                        }



    //                        if (fitCounter == 0)
    //                        {
    //                            //RandomTest();
    //                            //shouldBreak = true;
    //                            //return;
    //                            "-- !! no spots".LogDev();
    //                        }
    //                        else
    //                        {
    //                            //choosenSpot = spotsCandidates[UnityEngine.Random.Range(0, spotsCandidates.Count)];
    //                            SpotsGroup choosenTemp = correctCandidates[UnityEngine.Random.Range(0, correctCandidates.Count)];
    //                            List<SpotController> availableSpots = choosenTemp.spots.FindAll(o => o.matchInLastIterationFound);
    //                            choosenSpot = availableSpots[UnityEngine.Random.Range(0, availableSpots.Count)];
    //                            tempToCheck += choosenTemp.groupID;
    //                            tempGroups.Remove(choosenTemp);
    //                            choosenSpot.gameObject.SetActive(true);
    //                            choosenSpot.spot.figureImage.color = lc.letterColor;
    //                            //Color c = choosenSpot.shadowColor;
    //                            //c.a = .5f;
    //                            //choosenSpot.spot.figureImage.color = c;
    //                            tempGroups.ForEach(o => o.spots.ForEach(i => i.matchInLastIterationFound = false));



    //                            spotsList.Add(choosenSpot.spot);
    //                        }


    //                        //ep.element.transform.rotation.eulerAngles.LogDev();

    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    public void ResetAllGroupSpots()                        
    {
        groups.ForEach(o => o.spots.ForEach(i => i.spot.Reset()));
    }

    [NaughtyAttributes.Button("RandomTest")]
    public void PrepareSetsForRandomized()
    {
        //ResetAllGroupSpots();

        if (Application.isPlaying)
        {
            //ResetAllGroupSpots();
            RandomTest(true);

            //"playing".LogDev();
        }
        else
        {
            //"not playing".LogDev();
        }
    }

    public struct RandomizedPair
    {
        public ElementsPair elementPair;
        public SpotController spotController;
        public LetterController letterController;
    }

    public List<RandomizedPair> RandomTest(bool assignDropSpots = false, int iterations = 0)
    {
        ResetAllGroupSpots();
        List<Spot> spotsList = new List<Spot>();

        List<RandomizedPair> randomizedPairs = new List<RandomizedPair>();
        

        TextOverlayController toc = TextDeconstructionController.Instance.GetCity(country, cityName);
        List<LetterController> tempLetters = new List<LetterController>();

        List<SpotsGroup> tempGroups = new List<SpotsGroup>(groups);
        tempGroups = tempGroups.OrderBy(x => Guid.NewGuid()).ToList();

        // reset groups targets
        groups.ForEach(o => o.spots.ForEach(i => i.tempLetterElement = null));
        tempGroups.ForEach(o => o.spots.ForEach(i => i.gameObject.SetActive(false)));
        tempGroups.ForEach(o => o.spots.ForEach(i => i.matchInLastIterationFound = false));

        if (toc != null)
        {
            foreach (TextControllerSet tcs in toc.sets)
            {
                int allElementsCounter = 0;

                if (tcs.language.ToLower() != "english")
                    continue;

                tempLetters = new List<LetterController>(tcs.letters);
                tempLetters = tempLetters.OrderBy(o => !o.specialLetter).ToList();

                foreach (LetterController lc in tempLetters)
                {
                    if (lc == null)
                        continue;

                    foreach (ElementsPair ep in lc.pairs)
                    {
                        if (ep == null)
                            continue;

                        allElementsCounter++;

                        int fitCounter = 0;
                        float rotZ = ep.element.transform.rotation.eulerAngles.z;

                        //SpotsGroup choosenSpotGroup = null;
                        List<SpotsGroup> correctCandidates = new List<SpotsGroup>();
                        SpotController choosenSpot = null;
                        List<SpotController> spotsCandidates = new List<SpotController>();

                        //tempGroups.Count.Log("Temps: ");

                        foreach (SpotsGroup s in tempGroups)
                        {
                            s.spots.ForEach(delegate (SpotController sc)
                            {
                                float dist = Quaternion.Angle(sc.transform.rotation, ep.element.transform.rotation);

                                float rotZT = sc.transform.rotation.eulerAngles.z;

                                double v1 = (double)rotZT;
                                double v2 = (double)rotZ;

                                bool c1 = dist == 0;
                                int degrees = 0;

                                if(ep.spotController != null)
                                {                                  
                                    degrees = ep.spotController.spot.degrees;
                                }
                                else
                                {
                                    //Debug.LogError("NO DEGREES!");
                                    //return;
                                    //"spot controller null".Log();
                                    if(ep.degrees == 0)
                                    {
                                        //sc.spot.degrees.Log("NO DEGREES FOR: ");
                                        //Debug.LogError("NO DEGREES!");
                                        //return;
                                    }

                                    degrees = ep.degrees;
                                }

                                bool c2 = sc.spot.degrees == degrees;

                                if ((lc.specialLetter && c1 && c2) || (!lc.specialLetter && c2))
                                    //if ( c1 &&c2)
                                    {
                                        //"!!--found".LogDev();
                                        if (!correctCandidates.Contains(s))
                                    {
                                            //s.groupID.LogDev("Adding cand g: ");
                                            correctCandidates.Add(s);
                                    }

                                    if (!spotsCandidates.Contains(sc))
                                    {
                                        sc.matchInLastIterationFound = true;
                                        spotsCandidates.Add(sc);
                                    }

                                    fitCounter++;
                                }
                                else
                                {
                                    //string.Format("NOT FOUND: {0} {1} {2} {3} SP:{4}", c1, c2, sc.spot.degrees, degrees, lc.specialLetter).Log();
                                    //if(lc.specialLetter && !c1 && c2)
                                    //{
                                    //    string.Format("-- {0} {1} {2}", dist, sc.transform.rotation.eulerAngles.z, ep.element.transform.rotation.eulerAngles.z).Log();
                                    //}
                                    //"!!--not found".LogDev();
                                }                          


                                //if (lc.specialLetter && c2)
                                //{
                                //    dist.LogDev("Distance: ");
                                //}
                            });
                        }

                        if (fitCounter == 0)
                        {
                            //"---- no spots".LogDev();
                            int d = 0;

                            if(ep.spotController != null)
                            {
                                d = ep.spotController.spot.degrees;
                            }
                            else
                            {
                                d = ep.degrees;
                            }

                            //string.Format("NOT FOUND: sp{0} d{1}",lc.specialLetter, d).Log();
                            if (iterations > 10)
                            {
                                Debug.LogError("Randomization error");                                
                                Application.Quit();
                                return null;
                            }                            
                            
                            iterations++;
                            
                            return RandomTest(assignDropSpots, iterations);
                        }
                        else
                        {
                         
                            SpotsGroup choosenTemp = correctCandidates[UnityEngine.Random.Range(0, correctCandidates.Count)];
                            List<SpotController> availableSpots = choosenTemp.spots.FindAll(o => o.matchInLastIterationFound);
                            choosenSpot = availableSpots[UnityEngine.Random.Range(0, availableSpots.Count)];
                            //choosenSpot.spot.degrees.Log("FOUND DEGREES FOR: ");
                            tempGroups.Remove(choosenTemp);
                            tempGroups.ForEach(o => o.spots.ForEach(i => i.matchInLastIterationFound = false));
                            
                            RandomizedPair rp = new RandomizedPair
                            {
                                elementPair = ep,
                                spotController = choosenSpot,
                                letterController = lc
                            };
                            randomizedPairs.Add(rp);
                            spotsList.Add(choosenSpot.spot);
                        }
                    }
                }
            }
        }

        groups.ForEach(o => o.spots.ForEach(i => i.matchInLastIterationFound = false));

        //CheckIfThereAreErrors();


        //int groupElementsActiveCounter = 0;

        //foreach (SpotsGroup sg in groups)
        //{
        //    foreach (SpotController sc in sg.spots)
        //    {
        //        if (sc.gameObject.activeSelf == true)
        //        {
        //            groupElementsActiveCounter++;
        //        }
        //    }
        //}

        //foreach(RandomizedPair rp in randomizedPairs)
        //{
        //    string.Format("{0} - {1}", rp.elementPair.element.name, rp.spotController.name).LogDev();
        //    //rp.ep.element.name.LogDev();

        //}


        return randomizedPairs;
    }

    public List<Spot> RandomizeSpots()
    {
        List<Spot> spotsList = new List<Spot>();
        List<RandomizedPair> randomizedPairs = RandomTest(true);

        foreach (RandomizedPair rp in randomizedPairs)
        {
            rp.spotController.gameObject.SetActive(true);
            rp.spotController.spot.figureImage.color = rp.letterController.letterColor;
            Color c = rp.spotController.shadowColor;
            c.a = .5f;
            rp.spotController.spot.figureImage.color = c;
            //Debug.Log("Changing color to " + c, rp.spotController.spot.figureImage);
            rp.elementPair.spotController = rp.spotController;
            
            spotsList.Add(rp.spotController.spot);
        }

        //"shaodws on".LogDev();

        return spotsList;
    }


    public void CheckIfThereAreErrors()
    {
        bool errors = false;

        TextOverlayController toc = TextDeconstructionController.Instance.GetCity(country, cityName);


        if (toc != null)
        {
            foreach (TextControllerSet tcs in toc.sets)
            {



                foreach (LetterController lc in tcs.letters)
                {
                    if (lc == null)
                        continue;

                    foreach (ElementsPair ep in lc.pairs)
                    {
                        string name1 = ep.element.GetComponent<Image>().sprite.name;
                        string name2 = ep.spotController.spot.figureImage.sprite.name;

                        if (name1 != name2 && name1 != "single")
                        {
                            string errorMsg = string.Format("!!! - {0} != {1} ", name1, name2);
#if UNITY_EDITOR
                            Debug.LogError(errorMsg);
#endif
                            errorMsg.LogDev();
                            errors = true;

                        }
                    }
                }
            }
        }

        if (errors)
            "--- Errors".LogDev();
    }
}

[System.Serializable]
public class SpotsGroup
{
    public int groupID = -1;
    public GameObject groupParent;
    public List<SpotController> spots = new List<SpotController>();
    public bool available = true;
    [HideInInspector]
    public bool wasUsed = false;
}

public class ListCheck
{

    public SpotController sc;
    public ElementsPair ep;

    public ListCheck(SpotController sc, ElementsPair ep)
    {
        this.sc = sc;
        this.ep = ep;
    }
}