using Elements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupLockIndicator : Singleton<GroupLockIndicator>
{
    public Text timer;

    private CountryGroup _group;

    private int starsToSkip = 15;

    public void Enable(CountryGroup group)
    {
        _group = group;
        gameObject.SetActive(true);
        timer.text = group.GetCooloffLeft();
        StartCoroutine("UpdateCo");
    }

    IEnumerator UpdateCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            //"update".LogDev();
            timer.text = _group.GetCooloffLeft();
            if (!_group.HasCoolOff())
            {
                _group.serializedFields.coolOffDone = true;
                FirebaseDatabaseController.Instance.SaveGroups();
                Disable();
                CoolOffScreen.Instance.Disable();
            }
        }
    }

    public void OpenPopUp()
    {
        CoolOffScreen.Instance.Enable(_group);
        MapViewController.Instance.AddViewToClose(CoolOffScreen.Instance);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        StopCoroutine("UpdateCo");
    }
}
