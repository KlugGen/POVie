using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveAreaAnchored : MonoBehaviour
{

    public RectTransform includedForOffset;
    public float offset = 0f;
    private float yMax = 0f;

    private void Awake()
    {
        float extraOffset = 0f;

        float scaler = ViewsController.Instance.mainCanvas.scaleFactor;
        //scaler.Log("SC: ");
        //Screen.safeArea.height.Log("SA: ");

        if (includedForOffset != null)
        {
            if (includedForOffset.rect.height > 0f)
                extraOffset = includedForOffset.rect.height * scaler + offset * scaler;
            else
            {
                if (includedForOffset.sizeDelta.y > 0f)
                    extraOffset = includedForOffset.sizeDelta.y * scaler + offset * scaler;
            }
        }

        //extraOffset.Log("E: ");

        this.yMax = (Screen.safeArea.height - (20f * scaler) - extraOffset);
        float yMax = this.yMax / (float)Screen.height;
       
        Vector2 vMax = GetComponent<RectTransform>().anchorMax;
        vMax.y = yMax;            
       
        GetComponent<RectTransform>().anchorMax = vMax;                       
    }

    public float GetYMax()
    {
        return yMax;
    }
}
