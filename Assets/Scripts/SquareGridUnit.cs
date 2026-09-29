using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquareGridUnit : MonoBehaviour
{
    public bool displayed = false;

    [HideInInspector]
    public Image image;
    public Button button;

    public RectTransform rectTransform;

    public void Initialize()
    {     
        if (image == null)        
            image = transform.GetChild(0).GetComponent<Image>();            
       

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        button = image.GetComponent<Button>();
        if (button == null)
        {
            button = image.gameObject.AddComponent<Button>();
            button.onClick.AddListener(delegate() { SquaresDistractorController.Instance.SquareClicked(this); });
            button.enabled = false;
        }        
    }    

    [NaughtyAttributes.Button("Calculate")]
    public void CalculateOffset(){
        RectTransform child = transform.GetChild(0).GetComponent<RectTransform>();
        child.localPosition = new Vector3(child.rect.width/2f,child.localPosition.y,child.localPosition.z );
    }


}
