using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InteractiveTutorial : Singleton<InteractiveTutorial>
{
    public Button helpButton1, helpButton2;
    // Elements:
    // scr,pub: text line on dragables - with text and sprites replace
    public TutorialBottomBar onDragableBar;

    // scr,pub: text line up to the dragables - with text and sprites replace
    public TutorialBottomBar upToDragableBar;

    /// <summary>
    /// scr,pub: overlay between dragables and picture
    /// </summary>
    public GameObject generalOverlay;

    public GameObject userDropOverlay;
    public GameObject autoDropOverlay;
    public GameObject autorotationDescription;

    // prv: canvas group for first element from dragables

    // substract gameobject
    public GameObject substractGameObject;
    public GameObject substractText1, substractText2, substractText3, substractText4;
    // scr,pub: grab text right to dragable
    public GameObject grabPanelGameObject;
    public GameObject grabAndDrag_text;
    public GameObject yourTurn_text;
    public GameObject wathHowToGrab_text;

    public GameObject tapToRotate_user;

    // scr,pub: place to drop with description - with text replace

    // scr,pub: continue bar on bottom
    public GameObject continueButtonScreenGameObject;
    public GameObject continueButtonAfterAllAnimation;
    public GameObject continueButton_Scrolling;

    // scr,pub: finish screen on top of everythin 
    public GameObject finishButtonScreenGameObject;
    public ScrollRect pictureScrollRect, dragableScrollRect;
    public SpotController choosenSpot;

    public GameObject dropSpot;
    public GameObject userDropSpot;

    public Transform hand;
    public Transform handDefaultParent;

    public Transform midAnimation;

    public List<RectTransform> arrows = new List<RectTransform>();

    // MEthods catch: scrollRectDragables, scrollRectPicture

    // Delegates
    // -grab element
    // -drop element
    // -rotate element
    // -correct rotation
    // -distractor shake 
    // -distractor swipe
    // -distractor drones

    int currentS = 0;
    private List<object> trashToDeleteOnReset = new List<object>();

    public void Disable()
    {
        hand.SetParent(handDefaultParent);
        // resume and reset everuthing
        StopCoroutine("DelayedEnable");
        StopCoroutine("DelayedS");
        StopCoroutine("MoveElements");
        StopCoroutine("ArrowsAdjusting");
        StopCoroutine("AutoTapping");        
        tutorialEnabled = false;
        Reset();
    }

    public void Reset()
    {
        // disable elements

        // clear delegates

        // reset distractors

        tapToRotate_user.SetActive(false);
        hand.gameObject.SetActive(false);
        userDropSpot.SetActive(false);
        autoDropOverlay.SetActive(false);
        grabAndDrag_text.SetActive(false);
        yourTurn_text.SetActive(false);
        wathHowToGrab_text.SetActive(false);
        userDropOverlay.SetActive(false);
        autorotationDescription.SetActive(false);
        upToDragableBar.Disable();
        onDragableBar.Disable();
        pictureScrollRect.enabled = dragableScrollRect.enabled = true;
        generalOverlay.SetActive(false);
        grabPanelGameObject.SetActive(false);
        grabPanelGameObject.SetActive(false);
        sentenceScreen_gameObject.SetActive(false);

        substractGameObject.SetActive(false);
        substractText1.SetActive(false);


        scrollDragablesunlocked = false;
        scrollPictureDelegateUnlocked = false;
        tutorialEnabled = false;
        substractText2.SetActive(false);
        substractText3.SetActive(false);
        substractText4.SetActive(false);

        continueButtonScreenGameObject.SetActive(false);
        continueButtonAfterAllAnimation.SetActive(false);

        ShakeDetector.Instance.enabled = false;
        ShakeDetector.Instance.gameObject.SetActive(false);
        ShakeDetector.Instance.OnShake = null;

        
        SandDistractorController.Instance.Disable();
        SwipeDetector.Instance.gameObject.SetActive(false);
        SwipeDetector.Instance.OnSwipe = null;

        DronesDistractorController.Instance.Disable();
        DronesDistractorController.Instance.OnClicked = null;
        DronesDistractorController.Instance.OnComplete = null;
        finishButtonScreenGameObject.SetActive(false);

        if (currentCity != null && currentCity.onAnimationEnd != null)
            currentCity.onAnimationEnd.Clear();

        timerExplanation_gameObject.SetActive(false);
        highlightedExplanation_gameObject.SetActive(false);

        GamePlayController.Instance.attributes.Get().EnableBottonHorizontalGroup();

        //if (GamePlayController.Instance.portraitFields.dragableStartList.Count > 0)
        //{
        //    if (GamePlayController.Instance.portraitFields.dragableStartList[0].GetComponent<DragableElement>() != null)
        //    {
        //        GamePlayController.Instance.portraitFields.dragableStartList[0].GetComponent<DragableElement>().onDragActions.Clear();
        //        GamePlayController.Instance.portraitFields.dragableStartList[0].GetComponent<DragableElement>().onCorrectDropActions.Clear();
        //        GamePlayController.Instance.portraitFields.dragableStartList[0].GetComponent<DragableElement>().onCorrectDropActions.Clear();
        //        GamePlayController.Instance.portraitFields.dragableStartList[0].GetComponent<DragableElement>().correctFitActions.Clear();
        //    }
        //}

        currentS = 0;

//#if UNITY_EDITOR

//        helpButton1.gameObject.SetActive(true);
//        helpButton2.gameObject.SetActive(true);
//#endif
    }

    bool tutorialEnabled = false;
    private CityController currentCity;
    private DragableElement currentDragable;

    public void PreEnable()
    {
        GamePlayController.Instance.attributes.Get().dragableStartList.ForEach(o => o.GetComponent<ElementReplacement>().motionAllowed = false);
    }

    // Start is called before the first frame update
    public void Enable(CityController city)
    {
        currentCity = city;
        Reset();


       
        GamePlayController.Instance.attributes.Get().dragableStartList.ForEach(o => o.GetComponent<ElementReplacement>().motionAllowed = false);
        currentDragable = GamePlayController.Instance.attributes.Get().dragableStartList[0].GetComponent<DragableElement>();
        GamePlayController.Instance.attributes.Get().dragableStartList[1].GetComponent<DragableElement>().SetRotation(90f);
        

        StartCoroutine("ArrowsAdjusting");
        StartCoroutine("DelayedEnable");
    }

    IEnumerator ArrowsAdjusting()
    {
        yield return new WaitForEndOfFrame();

        RectTransform dragableRT = currentDragable.GetComponent<RectTransform>();
       // dragableRT.rect.width.Log("W: ");
        //dragableRT.sizeDelta.x.Log("SDX: ");

        arrows.ForEach(delegate (RectTransform r) {
            Vector3 v3 = r.anchoredPosition3D;
            v3.x = dragableRT.rect.width / 2f - 30f;          
            r.anchoredPosition3D = v3;
        });
    }

    IEnumerator DelayedEnable()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        tutorialEnabled = true;
        S1();
        grabPanelGameObject.GetComponent<RectTransform>().ForceRebuildLayoutImmediate();
    }

    // scroll picture
    public void S1()
    {
        currentS = 1;
        scrollPictureDelegateUnlocked = true;
        onDragableBar.EnableWithoutArrows(TextTranslationModule.GetWord("Swipe the picture left and right to navigate"));
    }

    public void S1Disable()
    {
        currentS = 1;
    }

    // scroll dragables
    public void S2()
    {
        //Debug.Log("S1");
        currentS = 2;
        onDragableBar.Disable();
        generalOverlay.SetActive(true);
        scrollDragablesunlocked = true;
        upToDragableBar.EnableWithoutArrows(TextTranslationModule.GetWord("Swipe along the <b>bottom</b> bar left and right to navigate angles"));
        upToDragableBar.EnableExtraElement();
    }

    public void S2Disable()
    {
        generalOverlay.SetActive(false);
        currentS = 1;
    }

    // grab element preparation
    public void S3()
    {
        currentS = 3;
        upToDragableBar.Disable();
        pictureScrollRect.enabled = dragableScrollRect.enabled = false;

        generalOverlay.SetActive(true);

        grabPanelGameObject.SetActive(true);
        wathHowToGrab_text.SetActive(true);
        grabPanelGameObject.GetComponent<RectTransform>().ForceRebuildLayoutImmediate();

        dragableScrollRect.normalizedPosition = Vector2.zero;
        pictureScrollRect.content.anchoredPosition = Vector2.zero;


        hand.localScale = Vector3.one;
        hand.SetParent(GamePlayController.Instance.attributes.Get().dragableStartList[0].transform);
        hand.localPosition = Vector3.zero;
        hand.localScale = Vector3.one;    
        hand.gameObject.SetActive(true);

        nextSDelegate =  S4;
        RunDelayedSeconds(4f);


        return;
    
        // TODO: orientation
        if (GamePlayController.Instance.portraitFields.dragableStartList.Count > 0)
        {
            //"Add deleg".Log();
            currentDragable.onDragActions.Clear();
            currentDragable.onDragActions.Add(delegate ()
            {
                S4();
            });

            currentDragable.onTemporaryAreaDropActions.Clear();
            currentDragable.onTemporaryAreaDropActions.Add(delegate ()
            {
                S5_D();
                S4_D();
                nextSDelegate = S3;
                RunDelayedFrame();
            });
        }

        GamePlayController.Instance.attributes.Get().dragableStartList.ForEach(o => o.GetComponent<ElementReplacement>().motionAllowed = true);
    }

    public void S3_D()
    {
        generalOverlay.SetActive(false);
        grabPanelGameObject.SetActive(false);
    }

    private DragableElement dragable;

    // place elememnt
    public void S4()
    {
        S3_D();

        generalOverlay.SetActive(false);
        substractGameObject.SetActive(true);
        substractText1.SetActive(true);
        autorotationDescription.SetActive(true);
        autoDropOverlay.SetActive(true);

        currentDragable.onHighlightedStart.Clear();
        currentDragable.onHighlightedEnd.Clear();

        hand.localScale = Vector3.one * .9f;
        wathHowToGrab_text.SetActive(false);

        GamePlayController.Instance.attributes.Get().DisableBottonHorizontalGroup();

        if (GamePlayController.Instance.portraitFields.dragableStartList.Count > 0)
        {
            dragable = GamePlayController.Instance.attributes.Get().dragableStartList[0].GetComponent<DragableElement>();
            if(dragable.rotationClickObject != null && dragable.rotationClickObject.GetComponent<Button>() != null)
            {
                dragable.rotationClickObject.GetComponent<Button>().enabled = false;
            }
        
            dragable.OnStart();
            dragable.timerText.enabled = false;
            //hand.transform.DOMove(dropSpot.transform.position, 4f);

            dragable.transform.DOMove(midAnimation.position, 3f).SetEase(Ease.Linear).OnComplete(() => {

                dragable.RemoteHoveringAnimation();
                dragable.transform.DOMove(dropSpot.transform.position, 1f).OnComplete(() => {
                 dropSpot.GetComponent<SpotController>().spot.OnDrop(dragable.gameObject);                
                 StartCoroutine("AutoTapping");
             });
        });
            
        }           
      
    }

    public IEnumerator AutoTapping()
    {
        yield return new WaitForSeconds(2f);
        GamePlayController.Instance.attributes.Get().EnableBottonHorizontalGroup();
        S4_D();
        substractText1.SetActive(false);
        autorotationDescription.SetActive(false);

        substractText2.SetActive(true);
        hand.localScale = Vector3.one;
        dragable.OnRightSpotHoverStartFeedback(dropSpot.GetComponent<SpotController>().spot.gameObject);        
        substractGameObject.SetActive(true);
        substractText2.SetActive(true);
        hand.SetParent(handDefaultParent);

        yield return new WaitForSeconds(2f);
        hand.DOScale(Vector3.one*.5f,.2f).SetLoops(2, LoopType.Yoyo);
        dragable.OnClick();
        yield return new WaitForSeconds(1f);
        hand.DOScale(Vector3.one * .5f, .2f).SetLoops(2, LoopType.Yoyo);
        dragable.OnClick();
        yield return new WaitForSeconds(1f);
        hand.DOScale(Vector3.one * .5f, .2f).SetLoops(2, LoopType.Yoyo);
        dragable.OnClick();
        yield return new WaitForSeconds(1f);
        hand.DOScale(Vector3.one * .5f, .2f).SetLoops(2, LoopType.Yoyo);     
        dragable.OnClick();
        substractText2.SetActive(false);
        yield return new WaitForSeconds(2f);
        //substractGameObject.SetActive(false);
        //substractText2.SetActive(false);
        UserDragging();

      
    }
    
    public void UserDragging()
    {
        hand.localScale = Vector3.one;
        hand.gameObject.SetActive(false);

        autoDropOverlay.SetActive(false);

        wathHowToGrab_text.SetActive(false);
        // enable overlay for first element     
        substractText1.SetActive(false);
        substractText4.SetActive(false);

        yourTurn_text.SetActive(true);
        userDropOverlay.SetActive(true);
        grabPanelGameObject.SetActive(true);
    

        grabPanelGameObject.GetComponent<RectTransform>().ForceRebuildLayoutImmediate();

        // allow dragging only first element
        GamePlayController.Instance.attributes.Get().dragableStartList[1].GetComponent<ElementReplacement>().motionAllowed = true;
        DragableElement usersDragableElement = GamePlayController.Instance.attributes.Get().dragableStartList[1].GetComponent<DragableElement>();

        //Debug.Log("Seting second dragable", GamePlayController.Instance.attributes.Get().dragableStartList[1].gameObject);
        // detect correct dragging

        userDropSpot.GetComponent<SpotController>().spot.skipAnimationOnComplete = true;

        usersDragableElement.onCorrectDropActions.Clear();
        usersDragableElement.onCorrectDropActions.Add(delegate ()
        {
            tapToRotate_user.SetActive(true);
        });     

        usersDragableElement.onCompleteActions.Clear();
        usersDragableElement.onCompleteActions.Add(delegate ()
        {
            "complete".LogDev();
            grabPanelGameObject.SetActive(false);
            substractGameObject.SetActive(false);

            nextSDelegate = S10;
            RunDelayedSeconds(1f);
            //S10();
        });


        usersDragableElement.onDragActions.Clear();
        usersDragableElement.onDragActions.Add(delegate ()
        {
            yourTurn_text.SetActive(false);
            grabPanelGameObject.SetActive(false);
        });

        usersDragableElement.onTemporaryAreaDropActions.Clear();
        usersDragableElement.onTemporaryAreaDropActions.Add(delegate ()
        {
            yourTurn_text.SetActive(true);
            tapToRotate_user.SetActive(false);
            grabPanelGameObject.SetActive(true);
        });

        userDropSpot.SetActive(true);
     

        // program incorrect dragging

    }

    public void S4_D()
    {
        //currentCity.EnableFlashesOnAll();
        substractGameObject.SetActive(false);
        substractText1.SetActive(false);
        substractText4.SetActive(false);
    }

    // tap to rotate
    public void S5()
    {      
        GamePlayController.Instance.attributes.Get().dragableStartList[0].GetComponent<ElementReplacement>().motionAllowed = false;

        S4_D();
        substractGameObject.SetActive(true);
        substractText2.SetActive(true);

        GamePlayController.Instance.portraitFields.dragableStartList[0].GetComponent<DragableElement>().correctFitActions.Add(delegate ()
        {
            S6();
        });
    }

    public void S5_D()
    {
        substractGameObject.SetActive(false);
        substractText2.SetActive(false);
    }

    // good job
    public void S6()
    {
        S5_D();
        substractGameObject.SetActive(true);
        substractText3.SetActive(true);
        continueButtonScreenGameObject.SetActive(true);
    }

    public void S6_D()
    {
        continueButtonScreenGameObject.SetActive(false);
        substractGameObject.SetActive(false);
        substractText3.SetActive(false);
    }

    public void S7_D()
    {
        ShakeDetector.Instance.gameObject.SetActive(false);
    }

    public void ScrollingState()
    {
        bool undone = false;

        GamePlayController.Instance.attributes.Get().dragableStartList.ForEach(delegate (GameObject g)
        {
            if (g != null)
            {
                DragableElement de = g.GetComponent<DragableElement>();
                if (de != null)
                {
                    if (!de.IsAnimationDone())
                        undone = true;
                }
            }
        });

        if (undone)
        {
            return;
        }

        //continueButton_Scrolling.SetActive(true);
        //pictureScrollRect.enabled = true;
        currentCity.onAnimationEnd.Remove(ScrollingStateDisable);
        currentCity.onAnimationEnd.Add(ScrollingStateDisable);
        currentCity.StartExternalAnimation();
        S10_D();
    }

    public void ScrollingStateDisable()
    {
        S11();
    }

    // shake to remove
    public void S7()
    {
        S6_D();
        S10();
        return;

        ShakeDetector.Instance.enabled = true;
        ShakeDetector.Instance.gameObject.SetActive(true);
        onDragableBar.EnableWithoutArrows(TextTranslationModule.GetWord("Shake your device to remove objects"));
        ShakeDetector.Instance.OnShake = null;
        ShakeDetector.Instance.RegisterOnShake(delegate (ShakeData data)
        {
            ShakeDetector.Instance.gameObject.SetActive(false);
            S8();
        });
    }

    public void S8_D()
    {
        SandDistractorController.Instance.Disable();
        SwipeDetector.Instance.gameObject.SetActive(false);
    }

    // swipe to remove sand
    public void S8()
    {
        S7_D();
        //Debug.Log("S8");
        onDragableBar.EnableWithoutArrows(TextTranslationModule.GetWord("Swipe left or right to remove sand"));
        SwipeDetector.Instance.OnSwipe = null;
        SwipeDetector.Instance.RegisterOnSwipe(delegate (SwipeData data)
        {
            if (data.SwipeTime > .2f)
            {
                S9();
            }
        });

        SwipeDetector.Instance.gameObject.SetActive(true);
        SandDistractorController.Instance.Enable();
    }

    public void S9_D()
    {
        onDragableBar.Disable();
        DronesDistractorController.Instance.Disable();
    }

    // tap on drones
    public void S9()
    {
        //Debug.Log("S9");
        S8_D();
        onDragableBar.EnableWithoutArrows(TextTranslationModule.GetWord(TextTranslationModule.GetWord("Tap on drones to shoot them down")));
        DronesDistractorController.Instance.OnClicked = null;
       DronesDistractorController.Instance.RegisterOnClicked(delegate ()
        {
            S10();
        });
        DronesDistractorController.Instance.EnableRandom();
    }

    // move all texts
    public void S10()
    {
        S9_D();
        GamePlayController.Instance.attributes.Get().DisableBottonHorizontalGroup();

        MoveAllElements();
        continueButtonAfterAllAnimation.SetActive(true);
    }

    public void S10_D()
    {
        continueButtonAfterAllAnimation.SetActive(false);
    }

    // word decontstruction animation
    public void S11()
    {
        GamePlayController.Instance.attributes.Get().EnableBottonHorizontalGroup();

        continueButtonAfterAllAnimation.SetActive(false);
        upToDragableBar.Disable();
        onDragableBar.EnableWithRightArrow(TextTranslationModule.GetWord("Letters and then a hashtag appear upon angles matching"));


        currentCity.ForceOverlayDisplaying();
        currentCity.textOverlayController.onClick.Add(delegate ()
        {
            onDragableBar.Disable();
            S11_1();
        });

        onDragableBar.onRightArrowClick.Add(delegate ()
        {
            currentCity.textOverlayController.OnClick();
        });
    }

    public GameObject sentenceScreen_gameObject;
    public Text sentenceScreen_text;
    public Text personalTouch_text;
    public Image avatar;
    public Text author;

    public void S11_1()
    {
        sentenceScreen_gameObject.SetActive(true);
        string hashTagSentence = currentCity.GetCurrentSentenceWithWord();
        sentenceScreen_text.text = hashTagSentence;
        personalTouch_text.text = currentCity.CurrentSet().personalTouch;
        avatar.sprite = currentCity.cityObject.avatar;
        string author_name = string.IsNullOrEmpty(currentCity.cityObject.author) ? "" : string.Format("@{0}", currentCity.cityObject.author);
        author.text = author_name;
    }

    // finish
    public void S12()
    {
        sentenceScreen_gameObject.SetActive(false);
        finishButtonScreenGameObject.SetActive(true);
    }

    public GameObject timerExplanation_gameObject;

    public void EnableTimerExplanation()
    {
        S2Disable();
        timerExplanation_gameObject.SetActive(true);
    }

    public void DisableTimerExplanation()
    {
        timerExplanation_gameObject.SetActive(false);
        EnableHighlightedExplanation();
    }

    public GameObject highlightedExplanation_gameObject;

    public void EnableHighlightedExplanation()
    {
        highlightedExplanation_gameObject.SetActive(true);
    }

    public void DisableHighlightedExplanation()
    {
        highlightedExplanation_gameObject.SetActive(false);
        finishButtonScreenGameObject.SetActive(true);
    }

    [NaughtyAttributes.Button("Finish tutorial")]
    public void FinishButton()
    {
        //currentCity.
        //Reset();
        Disable();
        GamePlayController.Instance.EndGameWithoutPopUps();
    }

    public void MoveAllElements()
    {
        // start coroutine of moving
        //StartCoroutine("MoveElements");
        dragableScrollRect.enabled = false;
        currentCity.GamePlaySimulation();
    }

    public IEnumerator MoveElements()
    {
        dragableScrollRect.enabled = false;

        yield return null;

        //GamePlayController.Instance.portraitFields.dragableStartList.ForEach( o=> o.GetComponent<DragableElement>().OnStart());

        // currentCity.ForceOverlayDisplaying();
        currentCity.GamePlaySimulation();
        //foreach()

        // move elements to positions

        // when all on right spots set them as fixed

        // show deconstructed text - maybe after button
    }

    bool scrollPictureDelegateUnlocked = true;
    bool scrollDragablesunlocked = true;

    public void PictureScrollRectChange(Vector2 v)
    {
        if (!scrollPictureDelegateUnlocked || !tutorialEnabled || currentS != 1)
            return;

        nextSDelegate = S2;
        StartCoroutine("DelayedS");
        scrollPictureDelegateUnlocked = false;
    }

    public void DragablesScrollRectChange(Vector2 v)
    {
        if (!scrollDragablesunlocked || !tutorialEnabled || currentS != 2)
            return;

        nextSDelegate = S3;
        StartCoroutine("DelayedS");
        scrollDragablesunlocked = false;
    }

    public delegate void SDelegate(); // This defines what type of method you're going to call.
    private SDelegate nextSDelegate;

    private void RunDelayedSeconds(float time = 1f)
    {
        StartCoroutine(DelayedSTime(time));       
    }

    private void RunDelayedFrame()
    {
       StartCoroutine("DelayedS");
}

    IEnumerator DelayedS()
    {
        yield return new WaitForSeconds(1f);
        nextSDelegate();
    }

    IEnumerator DelayedSTime(float time = 1f)
    {
        yield return new WaitForSeconds(time);
        nextSDelegate();
    }

    IEnumerator DelayedSFrame()
    {
        yield return new WaitForEndOfFrame();
        nextSDelegate();
    }
}
