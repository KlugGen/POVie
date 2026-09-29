using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ProgressBarPercentage : MonoBehaviour
{
    public enum ProgressBarType
    {
        NUMBER = 0,
        COMPONENTS = 1
    }

    public enum NumberDisplayType
    {
        PERCENT = 0,
        QTY = 1,
    }

    public ProgressBarType type = ProgressBarType.NUMBER;
    public NumberDisplayType displayType = NumberDisplayType.PERCENT;

    public Image barBackground_image;
    public Image bar_image;

    public Text barStatus_text;

    public List<Image> barComponents_list = new List<Image>();
    public Sprite componentEmpty_sprite, componentFull_sprite;

    private Vector2 barTarget = Vector2.zero;
  
    private float lastMaxValue = -1f;
    private float animationSpeed = 2f;
    public bool animate = false;

    public bool animateOnlyWithBiggerValue = true;
    private float oldValue = 0;

    public bool debug = false;

    private void Update()
    {
        if (!animate)
            return;

        bar_image.rectTransform.sizeDelta = Vector2.Lerp(bar_image.rectTransform.sizeDelta, barTarget, Time.deltaTime * animationSpeed);

        float distance = Vector2.Distance(bar_image.rectTransform.sizeDelta, barTarget);
        if(distance < 0.1f)
        {
            FitBarToTarget();           
        }

        //bar_image.rectTransform.sizeDelta = newDelta;
    }

    private void FitBarToTarget()
    {     
        bar_image.rectTransform.sizeDelta = barTarget;
        enabled = false;
    }

    public void Initialize(float maxValue)
    {
        //if (debug)
        //{
        //    maxValue.LogDev("Initialize. Max val: ");
        //}

        enabled = false;
        if (type == ProgressBarType.NUMBER)
        {
            Vector2 newDelta = bar_image.rectTransform.sizeDelta;
            
            newDelta.x = 0;
            bar_image.rectTransform.sizeDelta = newDelta;

            if (displayType == NumberDisplayType.PERCENT)
            {
                barStatus_text.text = "0%";
            }
            else
            {
                barStatus_text.text = "0/" + maxValue.ToString("F0");
            }                       
        }
        else
        {
            barComponents_list.ForEach(delegate(Image i) {
                i.gameObject.SetActive(false);                
            });

            barStatus_text.text = "0/" + maxValue.ToString("F0");

            for (int i = 0; i < maxValue; i++)
            {
                barComponents_list[i].gameObject.SetActive(true);
                barComponents_list[i].sprite = componentEmpty_sprite;
            }
        }

        lastMaxValue = maxValue;
    }

    public void ActualizeValue(float val, float maxValue= -1, bool animateLast = false)
    {
        //val.LogDev("Val: ");
        //maxValue.LogDev("Max: ");
        if (maxValue != -1)
            lastMaxValue = maxValue;

        if (type == ProgressBarType.NUMBER)
        {
            FitBarToTarget();
            float maxWidth = barBackground_image.rectTransform.rect.width;
            Vector2 newDelta = bar_image.rectTransform.sizeDelta;
            newDelta.x = (maxWidth  * val) / lastMaxValue;
            barTarget = newDelta;
            
            if (displayType == NumberDisplayType.PERCENT)
            {
                float percentageValue = (100 * val) / lastMaxValue;
                barStatus_text.text = percentageValue.ToString("F0") + "%";
            }
            else
            {
                barStatus_text.text = val.ToString() + "/" + lastMaxValue.ToString();
            }

            //oldValue.LogDev("Old value: ");
            //maxValue.LogDev("Max value: ");

            if (animate && ((animateOnlyWithBiggerValue && oldValue < val) || !animateOnlyWithBiggerValue))
                enabled = true;
            //else
            //{
            //    "Dont animate".LogDev();
            //}

            oldValue = val;
            return;
        }
        else
        {            
            barComponents_list.ForEach( o => o.gameObject.SetActive(false));

            for(int i = 0; i<maxValue; i++)
            {
                barComponents_list[i].gameObject.SetActive(true);
                barComponents_list[i].sprite = componentEmpty_sprite;
                Color c = barComponents_list[i].color;
                c.a = .3f;
                barComponents_list[i].color= c;
                barComponents_list[i].DORestart();
            }

            for (int i = 0; i < val; i++)
            {
                barComponents_list[i].sprite = componentFull_sprite;
                Color c = barComponents_list[i].color;
                c.a = 1f;
                barComponents_list[i].color = c;
            }

            if (animateLast && val>0)
            {
                barComponents_list[(int)val - 1].DOFade(.2f, 0f);
                barComponents_list[(int)val - 1].DOFade(1f, 1f).SetDelay(1f);
            }

            barStatus_text.text = val.ToString() + "/" + maxValue.ToString();
        }

       
    }
}
