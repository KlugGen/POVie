 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustToSaveArea : MonoBehaviour
{
    public float marginTop = 20f;
    private RectTransform _rectTransform;

    private Coroutine _coroutine = null;
    public bool awaken = false;
     
    void Start()
    {
        awaken = true;
      
        if (gameObject.activeInHierarchy)
        {
                     _coroutine = StartCoroutine(SetMarginCoroutine());
        }       
    }

    IEnumerator SetMarginCoroutine()
    {
        //"set margin coroutine - before".LogDev();
        yield return new WaitForEndOfFrame();
        //"set margin coroutine - aft".LogDev();
        SetMargin();
    }

    //[NaughtyAttributes.Button("Set margin")]
    public void SetMargin()
    {
        _rectTransform = GetComponent<RectTransform>();

        Vector3 v3 = _rectTransform.position;

        v3.y = Screen.safeArea.yMax;
        _rectTransform.position = v3;
        Vector2 v2 = _rectTransform.anchoredPosition;
        v2.y = v2.y - marginTop;

        _rectTransform.anchoredPosition = v2;

        DKK.SimpleAnimation simpleAnim = GetComponent<DKK.SimpleAnimation>();
        if (simpleAnim != null)
        {
            if (!simpleAnim.intitializeOnAwake)
            {
                simpleAnim.PreInitialize();
                simpleAnim.AnimateCondition();
            }
        }
    }

    private void OnDisable()
    {
        if (_coroutine!=null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }
}
