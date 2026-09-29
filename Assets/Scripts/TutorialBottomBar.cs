using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialBottomBar : MonoBehaviour
{
    public Text text;
    public Image leftArrowImage, rightArrowImage;

    public Sprite arrowSingle, arrowDouble;
    public GameObject elementWithText;


    public void Enable(string message, bool singleArrow = true)
    {
        //Debug.Log("Enabled bar");
        leftArrowImage.enabled = rightArrowImage.enabled = true;
        Enable(message);
        leftArrowImage.sprite = rightArrowImage.sprite = singleArrow ? arrowSingle : arrowDouble;   
    }

    public void EnableWithoutArrows(string message)
    {
        leftArrowImage.enabled = rightArrowImage.enabled = false;
        Enable(message);
    }

    public void EnableWithRightArrow(string message, bool singleArrow = true)
    {
        leftArrowImage.enabled = false;
        rightArrowImage.enabled = true;

        Enable(message);
        rightArrowImage.sprite = singleArrow ? arrowSingle : arrowDouble;
    }

    public void EnableWithLeftArrow(string message, bool singleArrow = true)
    {
        leftArrowImage.enabled = true;
        rightArrowImage.enabled = false;

        Enable(message);
        leftArrowImage.sprite = singleArrow ? arrowSingle : arrowDouble;
    }

    public void Enable(string message)
    {
        if (elementWithText != null)
            elementWithText.SetActive(false);
        onRightArrowClick.Clear();
        gameObject.SetActive(true);
        text.text = message;
    }

    public void EnableExtraElement()
    {
        if(elementWithText!=null)
        elementWithText.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);      
    }

    public List<UnityEngine.Events.UnityAction> onRightArrowClick = new List<UnityEngine.Events.UnityAction>();

    public void RightArrowButton()
    {
        onRightArrowClick.ForEach(o => o.Invoke());
    }
}
