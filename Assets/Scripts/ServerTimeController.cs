using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerTimeController : Singleton<ServerTimeController>
{

    public string serverDateUrl = "";
    private DateTime serverDateTime;
    private Coroutine serverDateTimeCoroutine = null;
    public bool serverTimeInitialized = false;
    private int serverInitCounter = 0;
    private bool errorRaised = false;
    public bool serverReady = false;

    public void Awake()
    {
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        InitializeServerDate();
    }

    public bool IsReady()
    {
        return serverReady;
    }

    public void InitializeServerDate()
    {      
        DKK.ServerManager.SendField("", "", serverDateUrl, false, delegate (bool success, object data)
        {
            if (success)
            {
                serverDateTime = DateTime.ParseExact(data.ToString(), "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                serverDateTimeCoroutine = StartCoroutine(ServerDateTimeCoroutine());
                serverDateTime.Log("Server time initialized: ");
                serverTimeInitialized = true;
                serverReady = true;
                SignInController.Instance.AfterLoadingSavesAction();
            }
            else
            {
                if(serverInitCounter < 5)
                {
                    Debug.LogError("[Server time not initialized][Warning] Reinitialization: " + serverInitCounter.ToString(), transform);
                    serverInitCounter++;
                    InitializeServerDate();
                }
                else
                {
                    Debug.LogError("[Server time not initialized][Error] No success with initialization after 5 attempts.", transform);
                    serverReady = true;
                    SignInController.Instance.AfterLoadingSavesAction();
                }
            }
        });
    }

    public IEnumerator ServerDateTimeCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            serverDateTime = serverDateTime.AddSeconds(1);
        }
    }

    public static DateTime GetTime()
    {
        if(Instance.serverTimeInitialized)
            return Instance.serverDateTime;

        if (!Instance.errorRaised)
        {
            Instance.errorRaised = true;
            Debug.LogError("[Server time not initialized][Error] Call with not initialized.", ServerTimeController.Instance.transform);
        }

        return DateTime.UtcNow;
    }
    
    public static string FormatDateTime(DateTime dt)
    {
        return dt.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string GetFormattedTime()
    {       
        return GetTime().ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static DateTime TryParseOrGetNow(string date_string)
    {
        DateTime dateTime;

        try
        {
            dateTime = DateTime.ParseExact(date_string, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            if (date_string != "" && date_string != null)
            {
                try
                {
                    dateTime = DateTime.Parse(date_string);
                }
                catch
                {
                    dateTime = DateTime.ParseExact(GetFormattedTime(), "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    dateTime = dateTime.AddDays(-1);
                }
            }
            else
            {
                try
                {
                    dateTime = DateTime.Parse(GetTime().ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                    dateTime = dateTime.AddDays(-1);
                }
                catch
                {
                    dateTime = DateTime.ParseExact(GetFormattedTime(), "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    dateTime = dateTime.AddDays(-1);
                }
            }
        }

        return dateTime;
    }
}