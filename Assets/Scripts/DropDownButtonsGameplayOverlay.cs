using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropDownButtonsGameplayOverlay : MonoBehaviour
{
    public Text capital_text;
    public Canvas topCenter_canvas, dropButton_canvas;

    public GameObject buttons_gameObject;

    public Image dropDownButton_image;
    public Sprite dropDowndefault_sprite, dropDownUp_sprite;

    public Button autoRotationBonus_button;
    public Button noDistractor_button;
    public Button endlessTimer_button;


    public GameObject autoRotationBonus_gameObject;
    public GameObject noDistractor_gameObject;
    public GameObject endlessTimer_gameObject;

    public Text autoRotationBonus_text;
    public Text noDistractor_text;
    public Text endlessTimer_text;

    public GameObject bonuses_button;


    public void Enable()
    {
        //capital_text.text = string.Format(TextTranslationModule.GetWord("{0} left"), UserController.Instance.GetStars());

        // set overide sorting for top center
        topCenter_canvas.overrideSorting = true;

        // set overide sorting for drop button
        dropButton_canvas.overrideSorting = true;

        // set overlay active

        gameObject.SetActive(true);
        buttons_gameObject.SetActive(true);
        dropDownButton_image.sprite = dropDownUp_sprite;

        ActualizeBonusesButtons();
    }

    public void ActualizeBonusesButtons()
    {
        bool condition = UserController.Instance.MaxUnlockedGroup() < 3;

        bonuses_button.SetActive(!condition);
        return;

        //autoRotationBonus_gameObject.SetActive(!condition);
        //noDistractor_gameObject.SetActive(!condition);
        //endlessTimer_gameObject.SetActive(!condition);

        //if (condition)
        //    return;

        //autoRotationBonus_text.text = string.Format("{0} ({1}{2})", TextTranslationModule.GetWord("Auto rotation"),UserController.GameBalance.autoRotationBonus, "\u2605");
        //noDistractor_text.text = string.Format("{0} ({1}{2})", TextTranslationModule.GetWord("No distractor"), UserController.GameBalance.noDistractorBonus, "\u2605");
        //endlessTimer_text.text = string.Format("{0} ({1}{2})", TextTranslationModule.GetWord("Endless timer"), UserController.GameBalance.endlessTimerBonus, "\u2605");

        //autoRotationBonus_button.interactable = !GamePlayController.Instance.bonuses.autoRotationActive;
        //noDistractor_button.interactable = !GamePlayController.Instance.bonuses.noDistractorActive;
        //endlessTimer_button.interactable = !GamePlayController.Instance.bonuses.endlessTimerActive;

    }

    public void BonusesButton()
    {
        Disable();
        GamePlayController.Instance.TopCenterElement_OnClick();
    }

    public void Disable()
    {
        dropDownButton_image.sprite = dropDowndefault_sprite;
       
        topCenter_canvas.overrideSorting = false;

        // set overide sorting for drop button
        dropButton_canvas.overrideSorting = false;

        // set overlay active

        gameObject.SetActive(false);
        buttons_gameObject.SetActive(false);
    }

    public bool IsEnabled()
    {
        return gameObject.activeSelf;
    }
}
