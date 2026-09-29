using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class FloatingPointPanelController : Singleton<FloatingPointPanelController>
{
    public bool isEnabled = true;

    private int lastValue, newValue;
    private float currentValue;

    public float speed = 10;
    public Text stars_text;
    public Text starsAddition_text;    
    public LayoutGroup  starsGroup;
    float tempSpeed = 0f;

    public void Show(int lastValue, int newValue, int addition)
    {      
        if (!isEnabled || lastValue == newValue)
        {
            return;
        }

        string amountText = "";

        if (addition > 0)
        {
            amountText = "+" + addition;          
        }
        else
        {
            amountText = addition.ToString();
        }

        starsAddition_text.text = string.Format("{0}", amountText);

        tempSpeed = 1.5f / (float)Mathf.Abs(addition);

        this.lastValue = lastValue;
        this.newValue = newValue;
        this.currentValue = lastValue;
        //lastValue.LogDev("LV: ");
        stars_text.text = lastValue.ToString("F0");

        gameObject.SetActive(true);
        StartCoroutine("ShowCoroutine");

        if(starsGroup != null)
        LayoutRebuilder.ForceRebuildLayoutImmediate(starsGroup.GetComponent<RectTransform>());
    }   

    public void Hide()
    {
        StopCoroutine("ShowCoroutine");
        gameObject.SetActive(false);      
    }

    IEnumerator ShowCoroutine()
    {
        yield return new WaitForSeconds(.2f);        

        float abs = Mathf.Abs(currentValue - newValue);

        while (abs > .2f)
        {                      
            currentValue = Mathf.MoveTowards(currentValue, newValue, 1f);
            //currentValue.LogDev("C: ");
            abs = Mathf.Abs(currentValue - newValue);
            stars_text.text = currentValue.ToString("F0");
            yield return new WaitForSeconds(tempSpeed);
            
        }

        stars_text.text = newValue.ToString("F0");

        yield return new WaitForSeconds(.5f);

        Hide();
    }
}
