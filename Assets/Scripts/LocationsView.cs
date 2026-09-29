using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elements;

public class LocationsView : View
{

    static private LocationsView instance;

    static public LocationsView Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(LocationsView))[0] as LocationsView;

            return instance;
        }
    }
    private Countries _country;

    public GameObject prefab;

    List<GameObject> trash = new List<GameObject>();

    public LocationViewAttributes landscapeAttributes, portraitAttributes;
    public void Enable(Countries country)
    {
        if(country.isNew == true)
        {
            country.isNew = false;
            UserController.Instance.SaveGame();
        }   

        Enable();
        _country = country;

        foreach (Cities c in country.cities)
        {
            if (!c.IsReady() || !c.CheckIfUnlocked(UserController.Instance.maxUnlockdedGroup))
                continue;

            //GameObject g1 = (GameObject)Instantiate(prefab, landscapeAttributes.container.transform);
            //g1.GetComponent<LocationPrefab>().Initialize(c);
            //trash.Add(g1);

            GameObject g2 = (GameObject)Instantiate(prefab, portraitAttributes.container.transform);
            g2.GetComponent<LocationPrefab>().Initialize(c);
            trash.Add(g2);

            portraitAttributes.countryTitle.text = landscapeAttributes.countryTitle.text = TextTranslationModule.GetWord(country.name);
        }

       // int id = PlayerPrefs.GetInt("tutorialShow" + UserController.Instance.GetLanguage(), -1);
        int id  = UserController.Instance.GetStats().GetInt("tutorialShow" + UserController.Instance.GetLanguage(), -1);
        //id.LogDev("Tutorial Show check: ");
        if (id == 4 && _country.GetMyGroup().id >= 4)
        {
            "tutorialShowDelete".LogDev();
            TutorialAdditionalsController.Instance.FourthTutorial();
            UserController.Instance.GetStats().RemoveFromDictionary("tutorialShow" + UserController.Instance.GetLanguage());
        }
    }

    public override void Disable()
    {
        base.Disable();

        // clear elements from trash list        
        trash.CompleteClear(true);
    }
}

[System.Serializable]
public class LocationViewAttributes
{
    public GameObject container;
    public Text countryTitle;
}