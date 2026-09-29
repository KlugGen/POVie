using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toturial : MonoBehaviour
{

    public List<RectTransform> toRebuild = new List<RectTransform>();
    public Toturial nextStep;
    public Toturial previousStep;

    public virtual void Disable()
    {     
        gameObject.SetActive(false);
    }

    public virtual void OnClick() {
        if (nextStep != null)
            nextStep.Enable();

        Disable();
    }

    public virtual void PrevStep()
    {
        if (previousStep != null)
                    previousStep.Enable();     
        

        Disable();
    }

    public virtual void Enable()
    {
        gameObject.SetActive(true);
        toRebuild.ForEach(o => UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(o));    

    }
}
