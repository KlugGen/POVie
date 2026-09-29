using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Spot : DropArea
{
    public int degrees;
    public Vector3 rotation;

    public Image figureImage;

    public SpotController parentSpotController;

    private bool wasUsed = false;
    private bool wasDropped = false;

    private DragableElement droppedElement;
    private Vector3 correctRotation;
    private Quaternion correctRotationQuaternion;

    public bool skipAnimationOnComplete = false;

    void Start()
    {
        correctRotation = transform.parent.localEulerAngles;
        correctRotationQuaternion = transform.parent.localRotation;
    }

    // ANIMATION

    public bool deconstructionAnimation = false;
    public Image animationElement;

    public GameObject animationTarget;

    public void EnableDeconstructionAnimation(GameObject target)
    {
        animationElement.gameObject.SetActive(true);
        animationTarget = target;
        // enable animation
        deconstructionAnimation = true;
    }

    public override void OnDrop(GameObject target)
    {
        if (wasDropped == false)
        {            
            DragableElement de = target.GetComponent<DragableElement>();
            if (de != null)
            {
                if (parentSpotController != null)
                {
                    if (degrees == de.GetDegrees())
                    {
                        droppedElement = de;
                        de.OnRightSpotDrop();

                        if (!CheckProperRotation())
                        {
                            de.OnCloseAreaDrop(parentSpotController.closeArea.gameObject);
                        }
                    }
                    else
                    {
                        de.OnTemporaryAreaDrop();
                    }
                }
            }
        }
    }
       
    public bool CheckProperRotation(DragableElement dropped = null)
    {
        if (droppedElement == null)
        {
            droppedElement = dropped;

            if (droppedElement == null)
                return false;
        }

        float distance = Quaternion.Angle(droppedElement.GetCurrentRotationQuaternion(), correctRotationQuaternion);
        float treshold = (float)Rules.GameBalance.rotationAccuracyCheck;


        if ((distance < treshold || GamePlayController.Instance.bonuses.autoRotationActive )&& droppedElement.GetDegrees() == degrees)
        {
            droppedElement.SetCorrectRotation(correctRotation);
            GamePlayController.Instance.UnlockDragging(droppedElement.gameObject);
            GamePlayController.Instance.distractorSuspended = false;
            MarkAsCorrect();
            return true;
        }

        return false;
    }

    public bool IsAvailable()
    {
        return !wasUsed && !wasDropped;
    }

    private void MarkAsCorrect()
    {
        // ANIMATION
     
        figureImage.color = AssetsDatabase.AssetsDatabase.Instance.colors.dragableElementColorEnabled;        

        wasUsed = wasDropped = true;
        parentSpotController.CorrectDrop();
        droppedElement.magentaAvailable = false;

        if (parentSpotController.letter != null)
        {
            figureImage.color = parentSpotController.letter.letterColor;

            if (parentSpotController.letter.CorrectDrop(parentSpotController))
            {            
                // all word is finished
                if (parentSpotController._city.textOverlayController.IsCompleted())
                {                  
                    GamePlayController.Instance.AllLettersCollected();
                    GamePlayController.Instance.PrePreparingCheck(this, true);
                    GamePlayController.Instance.SuspendDistractorWithAnimation();

                    parentSpotController._city.textOverlayController.EnableWithSentence(parentSpotController._city.GetCurrecntSentenceDots());

                    if(!skipAnimationOnComplete)
                        parentSpotController._city.AnimateAll();
                }
                else // only that letter is finished
                {
                    GamePlayController.Instance.PrePreparingCheck(this, true);
                    GamePlayController.Instance.SuspendDistractorWithAnimation();
               
                    if (!skipAnimationOnComplete)
                        parentSpotController.letter.ShowLetter();
                }
            }
            else
            {
                GamePlayController.Instance.PrePreparingCheck(this, false);
            }
        }

        droppedElement.CorrectDrop(this);

        parentSpotController.closeArea.gameObject.SetActive(false);
        parentSpotController.spot.gameObject.SetActive(false);

        parentSpotController._city.ReportCorrectDrop(droppedElement.gameObject);
    }

    public void OnClick()
    {
        if (wasDropped && !wasUsed)
        {
            IncrementRotation();
            CheckProperRotation();
        }
    }

    private void IncrementRotation()
    {       
        transform.parent.Rotate(new Vector3(0f, 0f, 1f), 45f);
        CheckProperRotation();
    }

    public void Reset()
    {
        //"Reset".LogDev();
        wasUsed = wasDropped = false;
        figureImage.color = AssetsDatabase.AssetsDatabase.Instance.colors.dragableElementColorDisabled;
        parentSpotController.closeArea.gameObject.SetActive(true);
        parentSpotController.spot.gameObject.SetActive(true);        

        ResetAnimation();
    }

    public void ResetAnimation()
    {
        //"reset".Log();
        // ANIMATION
        animationElement.gameObject.SetActive(false);      
        animationElement.transform.SetParent(figureImage.transform);
        animationElement.rectTransform.sizeDelta = figureImage.rectTransform.sizeDelta;
        animationElement.transform.localPosition = Vector3.zero;
        animationElement.transform.localRotation = Quaternion.identity;
        animationElement.transform.localPosition = Vector3.one;

        if (parentSpotController.animationSprite == null)
        {
            //"geting sprite".Log();
            animationElement.sprite = Elements.ElementsDatabase.Instance.GetElementByDegree(degrees).spriteFilled;
        }
        else
            animationElement.sprite = parentSpotController.animationSprite;

        animationElement.color = Color.white;
        deconstructionAnimation = false;
    }

    public override void OnHoverStart(GameObject target)
    {
        DragableElement dragable = target.GetComponent<DragableElement>();
        if (dragable != null)
        {
            if (parentSpotController != null)
            {
                if (parentSpotController.spot.degrees == dragable.GetDegrees())
                {
                    dragable.OnRightSpotHoverStartFeedback(gameObject);
                }
            }
            else
            {
                dragable.OnRightSpotHoverStartFeedback(gameObject);
            }
        }     
    }

    public override void OnHoverEnd(GameObject target)
    {
        DragableElement dragable = target.GetComponent<DragableElement>();
        if (dragable != null)
        {
            if (parentSpotController != null)
            {
                if (parentSpotController.spot.degrees == dragable.GetDegrees())
                {
                    dragable.OnRightSpotHoverEndFeedback(gameObject);
                }
            }
            else
            {
                dragable.OnRightSpotHoverEndFeedback(gameObject);
            }
        }
    }

    public override bool ShouldInterractWithObject(GameObject target)
    {
        if (wasDropped == false)
        {
            DragableElement de = target.GetComponent<DragableElement>();
            if (de != null)
            {
                if (parentSpotController != null)
                {
                    if (degrees == de.GetDegrees())
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
