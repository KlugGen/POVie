using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FloatingTextPanel : Singleton<FloatingTextPanel>
{
    public TextFloatingPanel textPanel;
    public LayoutGroup textInfoGroup;

    public Sprite infoSprite, interactableSprite;

    private FloatingTextParameters currentParameters = null;

    private Coroutine stopCoroutine = null;

    public HorizontalLayoutGroup layoutGroup;

    public void Show(FloatingTextParameters parameters)
    {
        if(stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
            stopCoroutine = null;
        }

        currentParameters = parameters;

        if (currentParameters.type == FloatingTextParameters.Type.INTERACTABLE)
        {
            layoutGroup.padding.right = 90;
            textPanel.iconParent.gameObject.SetActive(true);
            textPanel.icon.transform.localScale = Vector3.one;
            textPanel.icon.transform.DOScale(Vector3.one * .7f, .5f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            layoutGroup.padding.right = 50;
            textPanel.iconParent.gameObject.SetActive(false);
            textPanel.icon.transform.DORewind();
        }

        textPanel.background.sprite = currentParameters.type == FloatingTextParameters.Type.INFO ? infoSprite : interactableSprite;

        textPanel.text.text = currentParameters.text;
        gameObject.SetActive(true);

        if (textInfoGroup != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(textInfoGroup.GetComponent<RectTransform>());

        //Invoke("Hide", currentParameters.time);
        stopCoroutine = StartCoroutine(CloseDelay(currentParameters.time));
    }

    [NaughtyAttributes.Button("Test 1")]
    public void TextTest()
    {
        textPanel.icon.transform.DOScale(Vector3.one * .7f, .5f).SetLoops(-1, LoopType.Yoyo);
    }

    [NaughtyAttributes.Button("Test 2")]
    public void TextTest1()
    {
        textPanel.icon.transform.DORewind();
    }

    public void GeneralClick()
    {
        if (currentParameters.generalClickAction != null)
        {
            currentParameters.generalClickAction.Invoke();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void RightMarkButton()
    {
        if (currentParameters.rightIconClickAction != null)
        {
            currentParameters.rightIconClickAction.Invoke();
        }
    }

    IEnumerator CloseDelay(float time)
    {
        yield return new WaitForSeconds(time);
        Hide();
    }

    public void Hide(string category)
    {
        if (CheckIfCategoryEnabled(category))
            Hide();
    }

    public bool CheckIfCategoryEnabled(string category)
    {
        if(currentParameters == null)
            return false;

        if (string.IsNullOrEmpty(currentParameters.category))
            return false;

        if(gameObject.activeInHierarchy && currentParameters.category == category)
            return true;

        return false;
    }
}

public class FloatingTextParameters
{
    public enum Type
    {
        INFO,
        INTERACTABLE
    }

    public Type type = Type.INFO;
    public string text = "";
    public float time = 5f;
    public UnityAction rightIconClickAction = null;
    public UnityAction generalClickAction = null;
    public string category = "";
    
}


[System.Serializable]
public class TextFloatingPanel
{
    public Text text;
    public GameObject icon;
    public GameObject iconParent;
    public Image background;
}