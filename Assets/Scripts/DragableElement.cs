using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elements;
using UnityEngine.EventSystems;

public class DragableElement : MonoBehaviour, IMovable
{
    public Image backgroundImage, elementImage, timerImage;
    public Image shadow;

    private DragableElements _element;

    public bool magentaAvailable = true;
    private Vector3 startRotation;
    public Transform baseContainer;
    public Text timerText;
    public GameObject rotationClickObject;
    private bool shadowEnabled = true;
    private Coroutine animationCoroutine = null;
    public Outline magentaOutline;
    public Image magentaOutline_image;

    public GameObject backgroundsParent;

    private int siblingIdBeforeDraging = 0;

    private float draggingTimer = 0f;
    private float draggingTime = 2f;
    
    public int GetShiftID()
    {
        return 2;
    }

    public int GetOrderID()
    {
        return 0;
    }

    public void SetDraggingTime(float time)
    {
        draggingTime = time;
        ResetTimer();
    }

    public bool testEvent = false;

    private ScrollRect _scrollRect;

    private GameObject animationTarget;
    private float animationSpeed = 5;

    private bool animate = false;

    private bool markedAsMagenta = false;

    public void ExternalAnimation(GameObject target)
    {
        GetComponent<ElementReplacement>().enabled = false;
        backgroundsParent.SetActive(false);
        backgroundImage.enabled = false;
        animationTarget = target;       
        animate = true;

#if UNITY_EDITOR
        if (testEvent)
        {
           UnityEditor.Selection.activeGameObject = gameObject;
        }

#endif
    }
    
    private bool animationDone = false;
    private bool rotationTimerEnabled = false;

    // set for animation of rotating
    private bool rotationAnimation = false;
    private Quaternion targetRotation;
    private float rotaitonAnimationSpeed = 7f;
    private float modeRotationSpeedMultiplier = 1f;

    public bool IsAnimationDone()
    {
        return animationDone;
    }

    public void LockDragging()
    {
        GetComponent<ElementReplacement>().enabled = false;
        GetComponent<CanvasGroup>().alpha = .5f;
    }

    public void UnlockDragging()
    {
        GetComponent<ElementReplacement>().enabled = true;
        GetComponent<CanvasGroup>().alpha = 1f;
    }

    public void SetAsMagenta()
    {
        //if (magentaOutline != null)
        //    magentaOutline.enabled = true;

        if (magentaOutline_image != null)
            magentaOutline_image.enabled = true;

        markedAsMagenta = true;
    }

    public void ClearMagentaFeatures()
    {
        //if (magentaOutline != null)
        //    magentaOutline.enabled = false;


        if (magentaOutline_image != null)
            magentaOutline_image.enabled = false;

        markedAsMagenta = false;
    }

    private void Update()
    {
        if (rotationAnimation)
        {
            elementImage.transform.localRotation = Quaternion.SlerpUnclamped(elementImage.transform.localRotation, targetRotation, Time.deltaTime * rotaitonAnimationSpeed* modeRotationSpeedMultiplier);
            float diff = Quaternion.Angle(elementImage.transform.localRotation, targetRotation);

            if (diff < 5)
            {
                OnRotationAnimationEnd();
                elementImage.transform.localRotation = targetRotation;               
                rotationAnimation = false;
            }

            shadow.transform.localRotation = elementImage.transform.localRotation;
        }

        if (rotationTimerEnabled)
        {
            draggingTimer -= Time.deltaTime;
            AcutalizeTimerText();

            if (draggingTimer < 0f)
            {              
                OnTemporaryAreaDrop();
            }                
        }

        if (!animate)
            return;

        elementImage.transform.position = Vector3.Lerp(elementImage.transform.position, animationTarget.transform.position, Time.deltaTime * animationSpeed);

        float distance = Vector3.Distance(elementImage.transform.position, animationTarget.transform.position);

        if (distance < 1f)
        {
            elementImage.transform.position = animationTarget.transform.position;

            Spot spot = animationTarget.GetComponent<Spot>();
            if(spot != null)
            {
                spot.figureImage.color = spot.parentSpotController.letter.letterColor;
                spot.parentSpotController.gameObject.SetActive(true);
                gameObject.SetActive(false);
            }

            animationDone = true;
            animate = false;
        }
    }

    public void Initialize(DragableElements element, ScrollRect scrollRect = null)
    {
        _element = element;
        elementImage.sprite = element.spriteFilled;
        ClearMagentaFeatures();
        magentaAvailable = true;
        shadow.sprite = element.spriteFilled;
        shadow.enabled = false;

        startRotation = elementImage.transform.localEulerAngles;

        GameMode mode =  ElementsDatabase.Instance.modes.Find(o => o.id == UserController.Instance.gameMode);
        if(mode != null)
        {
            modeRotationSpeedMultiplier = mode.rotationSpeedMultiplier;
        }        

        ResetTimer();

        rotationAnimation = false;
        targetRotation = elementImage.transform.localRotation;

        animationDone = false;
        timerImage.enabled = false;
        timerText.enabled = false;
        rotationClickObject.SetActive(false);
        _scrollRect = scrollRect;

        if (_scrollRect != null)
            _scrollRect.enabled = true;

        AcutalizeTimerText();
        enabled = true;       
    }

    public void RandomRotation()
    {
        int randomMultiplier = Random.Range(1, 4);
        elementImage.transform.Rotate(new Vector3(0f, 0f, 1f), Rules.GameBalance.angleRotationStep * randomMultiplier);
        targetRotation = elementImage.transform.localRotation;
        shadow.transform.localRotation = elementImage.transform.localRotation;
    }

    public void SetRotation(float rotZ)
    {
        elementImage.transform.Rotate(new Vector3(0f, 0f, 1f), rotZ);
        targetRotation = elementImage.transform.localRotation;
        startRotation = targetRotation.eulerAngles;
        shadow.transform.localRotation = elementImage.transform.localRotation;
    }

    public void OnDrag()
    {
        //transform.position.LogDev("DP: ");
        draggingTimer -= Time.deltaTime;

        if (draggingTimer < 0f)
        {
            OnDraggingTimerEnd();          
        }
        
        AcutalizeTimerText();
        timerImage.fillAmount = draggingTimer / draggingTime;

        CheckDistanceToTheEdges();
    }
    
    float percentageScrolltreshold = .1f;

    private void CheckDistanceToTheEdges()
    {
        if (GamePlayController.Instance.GetCurrentCity().type == CitiesGameplayType.tutorial)
            return;

        float positionX = transform.position.x;
        float positionY = transform.position.y;

        float tresholdX = Screen.width * percentageScrolltreshold;
        float tresholdY = Screen.height * percentageScrolltreshold;

        if (positionX < tresholdX)
        {       
            GamePlayController.Instance.GetCurrentCity().GetController().ExternalScroll(-.5f);
        }
        else if (positionX > Screen.width - tresholdX)
        {
            GamePlayController.Instance.GetCurrentCity().GetController().ExternalScroll(.5f);
        }

        if (positionY < (tresholdY + tresholdY*1.2f))
        {
            GamePlayController.Instance.GetCurrentCity().GetController().ExternalScroll(-.5f, false);
        }
        else if (positionY > Screen.height - tresholdY)
        {
            GamePlayController.Instance.GetCurrentCity().GetController().ExternalScroll(.5f, false);
        }


    }

    private void AcutalizeTimerText()
    {
        //if (!GamePlayController.Instance.bonuses.endlessTimerActive)
            timerText.text = string.Format("{0}s", draggingTimer.ToString("F1")).Replace(",", ".");
       // else
       //     timerText.text = "";
    }

    public Sprite GetElementSprite()
    {
        return _element.spriteFilled;
    }

    public int GetDegrees()
    {
        return int.Parse(_element.ID);
    }
   
    public void CorrectDrop(Spot spot, bool letterCompleted = false)
    {
        AudioController.Instance.PlayLockingTone();
        onCompleteActions.ForEach(o => o.Invoke());
        GamePlayController.Instance.DeleteElement(gameObject, spot);
        GamePlayController.Instance.CheckBlinkingLimitationDisplayStatus();
    }
    
    public bool IsInteractable()
    {
        return true;
    }

    public void OnDrop(GameObject target)
    {
      
    }

    private bool uniqueBlinkFlag = false;

    public void OnStart()
    {
        //ResetTimer();
        GamePlayController.Instance.distractorSuspended = true;

        if (shadowEnabled)
        {
            shadow.enabled = true;
        }

        if (!wasOnceDropped)
            siblingIdBeforeDraging = transform.GetSiblingIndex();

        //"on start".LogDev();
        
        onDragActions.ForEach(o => o.Invoke());

        GamePlayController.Instance.ElementPicked();
        GamePlayController.Instance.LockDragging(gameObject);
        elementImage.color = Color.white;
        backgroundsParent.SetActive(false);
        backgroundImage.enabled = false;
        timerText.enabled = true;
        rotationTimerEnabled = false;

        magentaOutline.enabled = false;
        GetComponent<Animator>().Play("Idle");      

        if (onCloseAreaHovered)
        {           
            MarkImageAsHoverOnCloseArea();
        }

        if (wasOnceDropped)
        {
            if (GamePlayController.Instance.bonuses.endlessTimerActive)
                draggingTimer = Rules.GameBalance.extendedTimer;
            else
            {
                if (draggingTimer < 3f)
                    draggingTimer = 3f;
            }
        }
        else
        {
            ResetTimer();
        }

        //shadow.transform.localRotation = elementImage.transform.localRotation;
    }

    public void OnEnd(int movablesDetected = -1)
    {      
        Vector3 pos = transform.position;
       
        elementImage.transform.localScale = Vector3.one;
        elementImage.color = Color.white;

        // check if dropped outside the screen
        if (pos.x <= 0 || pos.x >= Screen.width || pos.y <= 0 || pos.y >= Screen.height || movablesDetected ==0)
        {            
            OnTemporaryAreaDrop();
        }
    }

    public void OnHover(GameObject target)
    {
    }

    private bool wasOnceDropped = false;

    public List<UnityEngine.Events.UnityAction> onDragActions = new List<UnityEngine.Events.UnityAction>();
    public List<UnityEngine.Events.UnityAction> onRotateActions = new List<UnityEngine.Events.UnityAction>();
    public List<UnityEngine.Events.UnityAction> onCorrectDropActions = new List<UnityEngine.Events.UnityAction>();
    public List<UnityEngine.Events.UnityAction> correctFitActions = new List<UnityEngine.Events.UnityAction>();
    public List<UnityEngine.Events.UnityAction> onCompleteActions = new List<UnityEngine.Events.UnityAction>();

    public List<UnityEngine.Events.UnityAction> onHighlightedStart = new List<UnityEngine.Events.UnityAction>();
    public List<UnityEngine.Events.UnityAction> onHighlightedEnd =  new List<UnityEngine.Events.UnityAction>();

    public void OnClick()
    {
        if (onCloseArea)
        {
            //targetRotation.eulerAngles.LogDev("BEF: Euler target: ");
            targetRotation *= Quaternion.Euler(0, 0, Rules.GameBalance.angleRotationStep);
            //targetRotation.eulerAngles.LogDev("AFTER: Euler target: ");
            //elementImage.transform.localRotation.eulerAngles.LogDev("EL rotation: ");
            rotationAnimation = true;
            return;
            //elementImage.transform.Rotate(new Vector3(0f, 0f, 1f), _element.angleIncrementation);
            //if (IsOnRightSpot())
            //{
            //    onRotateActions.ForEach(o => o.Invoke());
            //    foreach (GameObject g in rightSpotshoveredObjects) {
            //        if (g != null && g.GetComponent<Spot>() != null) {
            //            if (g.GetComponent<Spot>().CheckProperRotation(this))
            //            {
            //                correctFitActions.ForEach(o => o.Invoke());
            //                break;
            //            }
            //        }
            //    }
            //}
        }
    }

    private void OnRotationAnimationEnd()
    {
        "rotation animation end".LogDev();
        if (IsOnRightSpot())
        {
            onRotateActions.ForEach(o => o.Invoke());
            foreach (GameObject g in rightSpotshoveredObjects)
            {
                if (g != null && g.GetComponent<Spot>() != null)
                {
                    if (g.GetComponent<Spot>().CheckProperRotation(this))
                    {
                        correctFitActions.ForEach(o => o.Invoke());
                        break;
                    }
                }
            }
        }
    }



    public void OnHoverEnd(GameObject target)
    {
    }

    public void SetCorrectRotation(Vector3 v)
    {
        elementImage.transform.localEulerAngles = v;
        targetRotation = elementImage.transform.localRotation;
        shadow.transform.localRotation = elementImage.transform.localRotation;
    }

    public Vector3 GetCurrentRotation()
    {
        return elementImage.transform.localEulerAngles;
    }

    public Quaternion GetCurrentRotationQuaternion()
    {
        return elementImage.transform.localRotation;
    }

    public void OnRightSpotDrop()
    {
        //onCompleteActions.ForEach(o => o.Invoke());
        //"Right spot drop".LogDev();
    }

    public bool OnTemporaryArea = false;

    private void OnDraggingTimerEnd() {

        //"DTend".LogDev();
        //"end".Log();
        PointerEventData pointerEventData1 = new PointerEventData(EventSystem.current);
        //Input.mousePosition.Log("Pos touch: ");
        //if(Input.GetTouch)
        pointerEventData1.position = Input.mousePosition;
        
        //pointerEventData1.position.Log("End drag sim pos: ");
        ////gameObject.GetComponent<ElementReplacement>().beginDragData.pointerDrag = null;  
        ////ExecuteEvents.Execute(gameObject, pointerEventData1, ExecuteEvents.dropHandler);
        ExecuteEvents.Execute(gameObject, pointerEventData1, ExecuteEvents.endDragHandler);
        return;
       // gameObject.GetComponent<ElementReplacement>().OnEndDrag(pointerEventData1);
       
        //ExecuteEvents.Execute(gameObject, pointerEventData1, ExecuteEvents.dropHandler);

        //GamePlayController.Instance.UnlockDragging(gameObject);
        //GamePlayController.Instance.RebuildDragableScroll();
        //return;

        //PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        //pointerEventData.position = Vector3.zero;       
        //gameObject.GetComponent<ElementReplacement>().beginDragData.pointerDrag = null;
        //ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.endDragHandler);

        ////return;

        //OnTemporaryAreaDrop();
        //GamePlayController.Instance.UnlockDragging(gameObject);
        //GamePlayController.Instance.RebuildDragableScroll();
    }

    public List<UnityEngine.Events.UnityAction> onTemporaryAreaDropActions = new List<UnityEngine.Events.UnityAction>();

    public void OnTemporaryAreaDrop()
    {
        //"Temp drop".LogDev();
        //transform.position.LogDev("TP: ");

        //"Drop".LogDev();

        if (shadowEnabled)        
            shadow.enabled = false;
        

        onTemporaryAreaDropActions.ForEach(o => o.Invoke());
        GetComponent<ElementReplacement>().enabled = false;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        animationCoroutine = StartCoroutine(PlayAnimationWithDelay("Idle"));
        magentaOutline.enabled = false;
        rotationClickObject.SetActive(false);
        wasOnceDropped = false;
        OnTemporaryArea = true;
        onCloseArea = false;
        backgroundsParent.SetActive(true);
        onCloseAreaHovered = false;
        backgroundImage.enabled = true;
        timerImage.enabled = false;
        timerText.enabled = false;
        rotationTimerEnabled = false;
        uniqueBlinkFlag = false;
        transform.SetParent(baseContainer);
        
        // set sibling order
        if(markedAsMagenta)
            transform.SetAsFirstSibling();
        else
            transform.SetSiblingIndex(siblingIdBeforeDraging);

        //siblingIdBeforeDraging.LogDev("Sibling: ");

        elementImage.transform.localEulerAngles = startRotation;
        targetRotation = elementImage.transform.localRotation;
        shadow.transform.localRotation = elementImage.transform.localRotation;

        ResetTimer();

        hoveredObjects.Clear();
        rightSpotshoveredObjects.Clear();

        GamePlayController.Instance.UnlockDragging(gameObject);
        GamePlayController.Instance.distractorSuspended = false;
        GamePlayController.Instance.AddSeconds(15);
        GamePlayController.Instance.AddPoints(-15);
        GamePlayController.Instance.IncorrectDrop(gameObject);
    }

    private void ResetTimer()
    {
        if (GamePlayController.Instance.bonuses.endlessTimerActive)
            draggingTimer = Rules.GameBalance.extendedTimer;
        else
            draggingTimer = draggingTime;

        //#if UNITY_EDITOR
        //        draggingTimer = 10f;
        //#endif
    }

    public bool onCloseArea = false;
    

    public void OnCloseAreaDrop(GameObject g)
    {
        //"close".Log();
        onCorrectDropActions.ForEach(o => o.Invoke());

        rotationClickObject.SetActive(true);
        onCloseArea = true;
        backgroundsParent.SetActive(false);
        backgroundImage.enabled = false;
        timerImage.enabled = false;

        transform.SetParent(GamePlayController.Instance.GetCurrentCity().gameplay.GetComponent<CityController>().image.transform);

        if (wasOnceDropped) {
            OnTemporaryAreaDrop();
        }
        else
        {
            rotationTimerEnabled = true;

            if(draggingTimer<8f)
                draggingTimer = 8f;          

            AcutalizeTimerText();
            wasOnceDropped = true;
            GetComponent<ElementReplacement>().enabled = true;
        }     
    }

    private bool onCloseAreaHovered = false;
    private List<GameObject> MyCloseAreas = new List<GameObject>();

    public void OnHoverCloseAreaStartFeedback(GameObject target)
    {
        //target.name.LogDev("Target name: ", target);
        onHighlightedStart.ForEach( o => o.Invoke());

        if (GamePlayController.Instance.CanBlink())
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
           
            string animation_name = "Dragable_Hovering";

            if (markedAsMagenta)
            {
                CloseArea ca = target.GetComponent<CloseArea>();
                if(ca != null && ca.spotController.letter != null)
                {
                    if (ca.spotController.letter.specialLetter)
                    {
                        animation_name = "dragable_magenta";
                        magentaOutline.enabled = true;
                    }
                }
                else
                {
                    "Close area or letter null".LogDev();
                }
                //animationCoroutine = StartCoroutine(PlayAnimationWithDelay("dragable_magenta", true));
            }
            

            animationCoroutine = StartCoroutine(PlayAnimationWithDelay(animation_name, true));
        }

        onCloseAreaHovered = true;
        int lastArrayCount = hoveredObjects.Count;

        if (!hoveredObjects.Contains(target) && target.GetComponent<CloseArea>() != null)
        {
            hoveredObjects.Add(target);
            if (lastArrayCount == 0 && hoveredObjects.Count > 0)
            {
                MarkImageAsHoverOnCloseArea();
            }
        }
    }

    IEnumerator PlayAnimationWithDelay(string name, bool closeAreaAnimation = false)
    {
        yield return new WaitForEndOfFrame();
        GetComponent<Animator>().Play(name);

        if (closeAreaAnimation)
        {
            yield return new WaitForSeconds(.3f);
            if (!uniqueBlinkFlag && onCloseAreaHovered)
            {
                uniqueBlinkFlag = true;
                GamePlayController.Instance.ReportUniqueBlinking();
            }
        }
    }

    public void RemoteHoveringAnimation()
    {
        animationCoroutine = StartCoroutine(PlayAnimationWithDelay("Dragable_Hovering", false));
    }

    private void MarkImageAsHoverOnCloseArea()
    {
        elementImage.color = AssetsDatabase.AssetsDatabase.Instance.colors.dragableColorHoverOnCloseArea;
    }

    public void OnHoverCloseAreaEndFeedback(GameObject target)
    {
        onHighlightedEnd.ForEach(o => o.Invoke());

        onCloseAreaHovered = false;
        if (hoveredObjects.Contains(target) && target.GetComponent<CloseArea>() != null)
        {
            hoveredObjects.Remove(target);
            if (hoveredObjects.Count == 0)
            {

                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                    animationCoroutine = null;
                }

                magentaOutline.enabled = false;
                animationCoroutine = StartCoroutine(PlayAnimationWithDelay("Idle"));
                elementImage.color = Color.white;
                elementImage.transform.localScale = Vector3.one;

            }
        }
    }

    private bool onTemporaryAreaHovered = false;
    public void OnHoverTemporaryAreaStartFeedback()
    {
        onTemporaryAreaHovered = true;
    }

    public void OnHoverTemporaryAreaEndFeedback()
    {
        onTemporaryAreaHovered = false;
    }

    private bool onRightSpotAreaHovered = false;
    private Spot lastRightSpot = null;

    private List<GameObject> rightSpotshoveredObjects = new List<GameObject>();

    public void OnRightSpotHoverStartFeedback(GameObject target)
    {
        if (target.GetComponent<Spot>() != null)
        {
            if (!rightSpotshoveredObjects.Contains(target))
                rightSpotshoveredObjects.Add(target);
                       
            onRightSpotAreaHovered = true;
            lastRightSpot = target.GetComponent<Spot>();
        }
    }

    public void OnRightSpotHoverEndFeedback(GameObject target)
    {
        //"Right spot end hover".Log();
        if (rightSpotshoveredObjects.Contains(target))
            rightSpotshoveredObjects.Remove(target);

        if(rightSpotshoveredObjects.Count==0)
            onRightSpotAreaHovered = false;
    }

    public bool IsOnRightSpot()
    {
        return onRightSpotAreaHovered && lastRightSpot != null;
    }

    private List<GameObject> hoveredObjects = new List<GameObject>();

    public void OnHoverStart(GameObject target)
    {
    }

    public bool ShouldInterractWithObject(GameObject target)
    {
        return true;
    }
}