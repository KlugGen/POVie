using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CapitalScreen : View
{
    static private CapitalScreen instance;

    static public CapitalScreen Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(CapitalScreen))[0] as CapitalScreen;

            return instance;
        }
    }

    public GameObject advertiseButton;
    public Text capital_text, adsWatchingRewardAmount;

    public override void Enable()
    {
        base.Enable();
        // check rules for ads        
        advertiseButton.SetActive(Rules.AdsRules.AddToBalance());        
        capital_text.text = UserController.Instance.GetStars().ToString();
        adsWatchingRewardAmount.text =  "+" +Rules.GameBalance.adsBalanceReward;

        // check rules for shop
    }

    public void AdsRewardCallback()
    {
        UserController.Instance.AddStars(Rules.GameBalance.adsBalanceReward, "ads_balance");
        capital_text.transform.DOScale(Vector3.one * 1.5f, .5f).SetLoops(2, LoopType.Yoyo);
        advertiseButton.SetActive(Rules.AdsRules.AddToBalance());
    }
}
