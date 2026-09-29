using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardController : View
{

    static private DailyRewardController instance;

    static public DailyRewardController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(DailyRewardController))[0] as DailyRewardController;

            return instance;
        }
    }

    public ScrollRect scrollRect;

    public int focusedId = 2;

    public Text rewardAmount_text, totalAmount_text;
    public HorizontalLayoutGroup lGroup1, lGroup2;

    public void EnableSimpleVersion(int reward)
    {
        Enable();
        rewardAmount_text.text = reward.ToString();
        totalAmount_text.text = TextTranslationModule.GetWord("Total") + ": " + UserController.Instance.GetStars().ToString();

        LayoutRebuilder.ForceRebuildLayoutImmediate(lGroup1.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(lGroup2.GetComponent<RectTransform>());
    }

    private void OnEnable()
    {
        //SetElementsWidth();
        //focusedId = 1;
        //FocusScrollRect();
    }

   [NaughtyAttributes.Button("Set width")]
   public void SetElementsWidth()
    {
        for(int i = 0; i< scrollRect.content.childCount; i++)
        {          
            Vector2 v = scrollRect.content.GetChild(i).GetComponent<RectTransform>().sizeDelta;           
            float width = scrollRect.GetComponent<RectTransform>().rect.width;           
            scrollRect.content.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(width,v.y);

            //scrollRect.GetComponent<RectTransform>().rect.width.Log("W: ");
            //width.Log("W: ");
            //scrollRect.content.GetChild(i).GetComponent<RectTransform>().sizeDelta.Log("Size: ");
            // v.Log("setting width");
            //Rect r = scrollRect.content.GetChild(i).GetComponent<RectTransform>().rect;
            //r.width = scrollRect.GetComponent<RectTransform>().rect.width;
        }
    }

    [NaughtyAttributes.Button("Focus")]
    public void FocusScrollRect()
    {
        float k = scrollRect.GetComponent<RectTransform>().rect.width;
        float betweenSpan = 0f;
        float leftSpan = 0f;
        
        float val = (focusedId * (k + betweenSpan) + leftSpan - k - 2f* betweenSpan) / (scrollRect.content.rect.width-k - (2f * betweenSpan));   
        scrollRect.horizontalNormalizedPosition = val;
    }
}
