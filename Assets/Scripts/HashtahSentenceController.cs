using Elements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HashtahSentenceController : View
{
    static private HashtahSentenceController instance;

    static public HashtahSentenceController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(HashtahSentenceController))[0] as HashtahSentenceController;

            return instance;
        }
    }

    public HashTagSentenceControllerFields portraitFields, landscapeFields;

    public void Enable(string sentence, Cities city)
    {
        //"en".Log();
        Enable();

        if (city.avatar != null)
        {
            //GetAttributes().avatar_mask.enabled = true;
            GetAttributes().avatar_image.enabled = true;
            GetAttributes().avatar_image.sprite = city.avatar;
            string author_name = string.IsNullOrEmpty(city.author) ? "" : string.Format("@{0}", city.author);
            GetAttributes().author.text = author_name;
        }
        else
        {
           // GetAttributes().avatar_mask.enabled = false;
            GetAttributes().avatar_image.enabled = false;
        }

        //if (UserController.Instance.GetGameModeID() != 0)
        //{
        GetAttributes().modeText.text = TextTranslationModule.GetWord(string.Format("{0} mode", UserController.Instance.GetGameMode().name));
        //}
        //else
        //{
        //    GetAttributes().modeText.text = "";
        //}
        GetAttributes().sentence.text = sentence;
    }

    public List<UnityEngine.Events.UnityAction> onClick = new List<UnityEngine.Events.UnityAction>();

    public void RegisterOnClickCallback(UnityEngine.Events.UnityAction action)
    {
        onClick.Add(action);
    }

    public void OnClick()
    {
        onClick.ForEach(o => o.Invoke());
        onClick.Clear();
        Disable();
    }

    public HashTagSentenceControllerFields GetAttributes()
    {
        if (ViewsController.Instance.GetCurrentOrientation() == ScreenOrientation.Portrait)
            return portraitFields;

        return landscapeFields;
    }
}

[System.Serializable]
public class HashTagSentenceControllerFields
{
    public Text sentence;
    public Image avatar_image;
    public Image avatar_mask;
    public Text modeText;
    public Text author;

    public Text sentence_partI;
    public Text sentence_partII;

    public Text personal_touch;
}
