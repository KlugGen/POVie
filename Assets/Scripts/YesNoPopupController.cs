using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class YesNoPopupController : View
{

    static private YesNoPopupController instance;

    static public YesNoPopupController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(YesNoPopupController))[0] as YesNoPopupController;

            return instance;
        }
    }

    public ViewAttributes<YesNoPopupControllerFields> attributes = new ViewAttributes<YesNoPopupControllerFields>();

    public YesNoPopupControllerFields portraitFields, landscapeFields;
    private UnityAction confirmAction = null;
    private UnityAction noAction = null;

    public void Enable(UnityAction action, string confirmationText, bool q = false, UnityAction noAction = null, bool avoidTranslation = false)
    {
        confirmAction = null;
        this.noAction = null;

        attributes.Initialize(portraitFields, landscapeFields);

        this.confirmAction = action;
        this.noAction = noAction;

        Enable();

        if(avoidTranslation)
            attributes.Get().messageText.text = confirmationText;
        else
            attributes.Get().messageText.text = TextTranslationModule.GetWord(confirmationText);

        if (q)
        {
            attributes.Get().messageText.text += "?";
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)){
            YesButton();
        }
    }

    public void YesButton()
    {
        Disable();

        if (confirmAction != null)
        {
            confirmAction.Invoke();
           
        }              
    }

    public void NoButton()
    {
        Disable();

        if (noAction!= null)
        {
            noAction.Invoke();
        }        
    }
}

[System.Serializable]
public class YesNoPopupControllerFields
{
    public Text messageText;
}
