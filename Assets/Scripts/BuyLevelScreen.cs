using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyLevelScreen : View
{
    private LocationPrefab _locaiton;

    static private BuyLevelScreen instance;

    public Text buyButton_text;

    static public BuyLevelScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(BuyLevelScreen))[0] as BuyLevelScreen;

            return instance;
        }
    }

    public void Enable(LocationPrefab location)
    {
        _locaiton = location;
        Enable();
        buyButton_text.text = string.Format(
            TextTranslationModule.GetWord("Buy this level {0} stars"),
            _locaiton._group.ticketPrice);
    }

    public void Confirm()
    {       
        _locaiton.BuyConfirm();
        Disable();
    }
}
