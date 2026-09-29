using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetController : Singleton<InternetController>
{
    public bool active = false;
    public bool allowOffline = false;

    //private void Awake()
    //{
    //    if(Application.internetReachability == NetworkReachability.NotReachable)
    //    {
    //        "no internet".Log();
    //        StartCoroutine(CheckInternetSimple());
    //    }
    //    else
    //    {
    //        "internet ok".Log();
    //    }
    //}

    public bool CheckInternetStatus()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable && !allowOffline)
        {
            "internet ok".Log();
            YesNoPopupController.Instance.Enable(delegate() {
                CheckInternetStatus();
            }, "Please connect to the Internet. Retry now?", false, Application.Quit);
            return false;
        }

        return true;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        //pauseStatus.LogDev();

        if (!active)
            return;
       
        if (!pauseStatus)
        {
            CheckInternetStatus();
        }
    }

    private IEnumerator CheckInternetSimple()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            "no internet".Log();
        }
        else
        {
            "internet ok".Log();
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(CheckInternetSimple());
    }

    public static bool IsInternetReachable() => Application.internetReachability != NetworkReachability.NotReachable;

    public static bool IsReachableByCarrier() => Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork;
}
