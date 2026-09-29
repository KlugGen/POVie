using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VanishAnimation : MonoBehaviour
{
    private SpotController sController;
   
    public float vanishTime = 3f;
    private float vanishTimer = 0f;
    private CanvasGroup vanishCanvasGroup = null;
    public Button vanishButton;

    public void Disable()
    {
        enabled = false;
        vanishButton.enabled = false;
        ResetVanishTimer();

    }

    public void Enable()
    {
        sController = GetComponent<SpotController>();
        vanishButton.enabled = true;
        ResetVanishTimer();

        if (vanishCanvasGroup == null)
            vanishCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        enabled = true;
    }

    public void ResetVanishTimer()
    {
        vanishTimer = vanishTime;
//#if UNITY_EDITOR
//        vanishTimer = 3f;
//#endif
        if (vanishCanvasGroup != null)
        {
            vanishCanvasGroup.alpha = 1f;
        }
    }

    private void Update()   
    {
        if (sController.droppedCorrectly)
        {
            if (vanishTimer < 0)
            {
                vanishCanvasGroup.alpha = Mathf.Lerp(vanishCanvasGroup.alpha, 0f, Time.deltaTime/2f);
            }
            else
            {
                vanishTimer -= Time.deltaTime;
            }
        }
    }

    public void OnClick()
    {
        if (sController != null && sController.droppedCorrectly)
        {
            ResetVanishTimer();
        }
    }
}
