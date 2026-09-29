using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TTM = TextTranslationModule;

public class NewGroupUnlocked : View
{
    static private NewGroupUnlocked instance;

    static public NewGroupUnlocked Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(NewGroupUnlocked))[0] as NewGroupUnlocked;

            return instance;
        }
    }

    public Text title_text, description_text;

    public List<NewGroupMessageSet> sets = new List<NewGroupMessageSet>();

    public Image icon;

    public override void Enable()
    {
        base.Enable();
        NewGroupMessageSet set = sets[UserController.Instance.maxUnlockdedGroup % 3];
        title_text.text = TTM.GetWord(set.title) + "!";
        description_text.text = TTM.GetWord(set.message) + "!";

        Elements.CountryGroup maxGroup = Elements.ElementsDatabase.Instance.groups.Find(o=> o.id == UserController.Instance.maxUnlockdedGroup);
        if(maxGroup == null)
        {
            maxGroup = Elements.ElementsDatabase.Instance.groups[0];
        }
        
        icon.sprite = maxGroup.boosterInfoSprite; 
    }     

    public void CloseButton()
    {
        MapViewController.Instance.StartAnimation();
        Disable();
    }

}

[System.Serializable]
public class NewGroupMessageSet
{
    public string title;
    public string message;
}
