#if UNITY_ANDROID

using System.Collections;
using System.Collections.Generic;
using Unity.Notifications.Android;
using UnityEngine;

public class LocalNotification : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //SendNote();
    }

    public void SendNote()
    {

        var channel =
            new AndroidNotificationChannel()
            {
                Id = "default_channel",
                Name = "SpeedySquare",
                Importance = Importance.Default,
                Description = "channel_description"
            };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        var notification = new AndroidNotification();
        notification.Title = "Speedy Square";
        notification.Text = "Someone beat the highscore!";        
        notification.FireTime = System.DateTime.Now.AddSeconds(5);
        //AndroidNotificationCenter.SendNotification(notification, "default_channel");
        //AndroidNotificationCenter.canc
        //AndroidNotificationCenter.GetNotificationChannel("")
        //AndroidNotificationCenter.GetNotificationChannel("").
        Debug.Log("Send note");


    }
}

#endif
