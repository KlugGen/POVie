using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseMessaging : Singleton<FirebaseMessaging>
{
    public void Initialize()
    {
        //"Messaging Init".Log();
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("From: " + e.Message.From);
        UnityEngine.Debug.Log("Message ID: " + e.Message.MessageId);

        
    }
}
