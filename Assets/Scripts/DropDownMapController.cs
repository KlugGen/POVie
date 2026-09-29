using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropDownMapController : MonoBehaviour
{
    public Canvas dropButton_canvas;
    public Text capital_text;
    public Image volume_image;
    public Image dropDownButton_image;
    public Sprite dropDowndefault_sprite, dropDownUp_sprite;
 
    public GameObject buttons_gameObject;
    public void Enable()
    {      
        // set overide sorting for drop button
        dropButton_canvas.overrideSorting = true;

        //capital_text.text = string.Format(TextTranslationModule.GetWord("{0} left"),UserController.Instance.GetStars());
        // set overlay active

        gameObject.SetActive(true);
        buttons_gameObject.SetActive(true);
        dropDownButton_image.sprite = dropDownUp_sprite;
    }

    public void Disable()
    {
        //CapitalScreen.Instance.Disable();
        //TutorialAdditionalsController.Instance.Disable();
        dropDownButton_image.sprite = dropDowndefault_sprite;
        // set overide sorting for drop button
        dropButton_canvas.overrideSorting = false;
        // set overlay active

        gameObject.SetActive(false);
        buttons_gameObject.SetActive(false);
        
    }
}
