using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Elements;
using System;

public class CoolOffScreen : View
{
    static private CoolOffScreen instance;

    static public CoolOffScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(CoolOffScreen))[0] as CoolOffScreen;

            return instance;
        }
    }

    public Text title, description, timer;
    public Text adsTitle, starsForSkip_text;

    private CountryGroup _group;
       

    public void Enable(CountryGroup group)
    {
        _group = group;
        Enable();
                      

        timer.text = string.Format(TextTranslationModule.GetWord("Wait until the plane is ready with refueling in {0}"), "<b>" + group.GetCooloffLeft() + "</b>");
        adsTitle.text = string.Format(TextTranslationModule.GetWord("Service") + " (-{0} min)", Rules.GameBalance.coolOffAdMinutes);
        starsForSkip_text.text = Rules.GameBalance.skipCooloffCost.ToString();

        StartCoroutine("UpdateCo");
    }

    IEnumerator UpdateCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);         
            timer.text = string.Format(TextTranslationModule.GetWord("Wait until the plane is ready with refueling in {0}"), "<b>" + _group.GetCooloffLeft() + "</b>");
        }
    }

    public void WatchAd()
    {
        AdsManager.Instance.ShowCooloffAd();
    }

    public void AdCallback()
    {
        // decrease time

        // check if there is still blockade

        DateTime cooloffDate = ServerTimeController.TryParseOrGetNow(_group.serializedFields.coolOffLeft);
        cooloffDate = cooloffDate.AddMinutes(-Rules.GameBalance.coolOffAdMinutes);
        _group.serializedFields.coolOffLeft = ServerTimeController.FormatDateTime(cooloffDate);

        if (_group.HasCoolOff())
        {
            timer.text = string.Format(TextTranslationModule.GetWord("Wait until the plane is ready with refueling in {0} min"), "<b>" + _group.GetCooloffLeft() + "</b>");
            FirebaseDatabaseController.Instance.SaveGroups();
        }
        else
        {
            ReleaseBlockade();
        }
    }

    public void SkipWithStars()
    {
        if (UserController.Instance.GetStars() < Rules.GameBalance.skipCooloffCost)
        {
            OutOfStarsScreen.Instance.Enable(LocationsView.Instance);
            AddViewToClose(OutOfStarsScreen.Instance);
            return;
        }

        UserController.Instance.RemoveStars(Rules.GameBalance.skipCooloffCost, "skipGroupBlockade", _group.id.ToString());
        ReleaseBlockade();
    }

    public void ReleaseBlockade()
    {
        _group.serializedFields.coolOffDone = true;
        FirebaseDatabaseController.Instance.SaveGroups();
        Disable();
        GroupLockIndicator.Instance.Disable();
    }

    public override void Disable()
    {
        "Disabling Cool Off".LogDev();
        StopCoroutine("UpdateCo");
        base.Disable();      
    }


}
