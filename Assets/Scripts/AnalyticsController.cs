using Firebase.Analytics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsController : Singleton<AnalyticsController>
{
    public bool sendInEditor = false;
    public bool sendData = true;
    public void LogEvent(string name, params Parameter[] parameters)
    {
        if(!sendData)
        {
            name.Log("Disabled. Analytics with name skipped: ");
            return;
        }

#if UNITY_EDITOR
        if (!sendInEditor)
        {
            name.Log("Editor. Analytics with name skipped: ");
            return;
        }
#endif

        FirebaseAnalytics.LogEvent(name, parameters);
    }
}
