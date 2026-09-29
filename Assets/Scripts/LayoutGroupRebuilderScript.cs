using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayoutGroupRebuilderScript : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(RebuildCoroutine());
    }

    IEnumerator RebuildCoroutine()
    {
        yield return new WaitForEndOfFrame();
        LayoutGroup lg = GetComponent<LayoutGroup>();
        RectTransform rt = GetComponent<RectTransform>();
        if (lg != null && rt != null)
        {
            //"Layout group found".LogDev();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

    }
}
