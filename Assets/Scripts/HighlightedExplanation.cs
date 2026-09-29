using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightedExplanation : MonoBehaviour
{
    public Image image;

    // Start is called before the first frame update
   IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        RectTransform _imageRectTransform = image.GetComponent<RectTransform>();
        RectTransform parentsRectTransform = _imageRectTransform.transform.parent.GetComponent<RectTransform>();

        if (_imageRectTransform == null || parentsRectTransform == null)
        {
            parentsRectTransform = GetComponent<RectTransform>();
            _imageRectTransform = image.GetComponent<RectTransform>();
        }

        float mapWidthHeightRatio = _imageRectTransform.rect.width / _imageRectTransform.rect.height;
        float screenWidthHeightRatio = Screen.safeArea.width / Screen.safeArea.height;
        float newScale;

        if (mapWidthHeightRatio > screenWidthHeightRatio)
        {
            //Debug.Log("Scaled map to parent height.");
            newScale = parentsRectTransform.rect.height / _imageRectTransform.rect.height;
        }
        else
        {
            newScale = parentsRectTransform.rect.width / _imageRectTransform.rect.width;
        }

        _imageRectTransform.transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    
}
