using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FAQController : View
{
    public List<VerticalLayoutGroup> layoutGroup;
    public ScrollRect scrollRect;

    public GameObject faqEN, faqCH;

    public override void Enable()
    {
        base.Enable();
        faqCH.DeActivate() ;
        faqEN.DeActivate();

        if (UserController.Instance.GetLanguage() == "english")
            faqEN.Activate();
        else
        {
            faqCH.Activate();
        }

        scrollRect.verticalNormalizedPosition = 1f;
        layoutGroup.ForEach( o => LayoutRebuilder.ForceRebuildLayoutImmediate(o.GetComponent<RectTransform>()));
        
    }
}
