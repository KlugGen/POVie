using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerGamePlayController : MonoBehaviour
{
    public Text timerText;

    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        ActualizeTimer();
    }

    public void Enable()
    {
        enabled = true;
    }
       
    public void Disable()
    {
        enabled = false;
    }

    public void Reset()
    {
        timer = 0f;
        ActualizeTimer();
    }

    public void ActualizeTimer()
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(timer);
        string timeText = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        timerText.text = timeText;
    }

    public void AddTime(float timerAddition)
    {
        timer += timerAddition;
    }

    public float GetTime()
    {
        return timer;
    }
}
