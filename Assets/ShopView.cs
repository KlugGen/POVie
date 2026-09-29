using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopView : View
{
    static private ShopView instance;

    static public ShopView Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(ShopView))[0] as ShopView;

            return instance;
        }
    }

    public List<UnityEngine.Purchasing.IAPButton> bundlesButtons = new List<UnityEngine.Purchasing.IAPButton>();

    public override void Enable()
    {
        base.Enable();

#if UNTITY_ANDROID || UNITY_EDITOR
       
#else
       bundlesButtons.ForEach(o => Destroy(o));
#endif
    }

    public void BundleButtonClick()
    {
#if UNTITY_ANDROID || UNITY_EDITOR
       
#else
    
        string title = "";
        string message = TextTranslationModule.GetWord("Transaction failed");

        UniversalInfoPopUp.Instance.Enable(title, message);       
        Instance.AddViewToClose(UniversalInfoPopUp.Instance);

#endif
    }


}
