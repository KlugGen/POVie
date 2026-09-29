//using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyFbScript : MonoBehaviour
{
    //public Text console, infoText;
    //public InputField fieldsInput;

    //private void Update()
    //{
    //    infoText.text = "Initialized: " + FB.IsInitialized + ",\nLoggedIn: " + FB.IsLoggedIn; 
    //}

    //public void OnInitButtonClicked() 
    //{
    //    FB.Init(OnInitComplete, OnHideUnity);
    //}

    //private void OnInitComplete()
    //{
    //    Debug.Log("Success - Check log for details");
    //    string logMessage = string.Format(
    //        "OnInitCompleteCalled IsLoggedIn='{0}' IsInitialized='{1}'",
    //        FB.IsLoggedIn,
    //        FB.IsInitialized);
    //    Debug.Log(logMessage);
    //}

    //private void OnHideUnity(bool isGameShown)
    //{
    //    Debug.Log("Success - Check log for details (OnHideUnity)");
    //}

    //public void CallFBLogout() => FB.LogOut();

    //public void CallFBLogin()
    //{
    //    FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email"}, (result) => 
    //    {
    //        console.text = "";
    //        foreach (KeyValuePair<string, object> keyValue in result.ResultDictionary) 
    //        {
    //            console.text += (keyValue.Key + ": " + keyValue.Value);
    //        }
    //    }
    //    );;
    //}

    //public void OnMeButtonClicked()
    //{
    //    FB.API("/me?fields=" + fieldsInput.text, HttpMethod.GET, result => console.text = "Data: " + result.RawResult);
    //}

    //public void OnGetAllDataButtonClicked()
    //{
    //    FB.API("/me?fields=id,first_name,last_name,middle_name,name,name_format,picture,short_name,email", HttpMethod.GET, result => console.text = "Data: " + result.RawResult);
    //}
}
