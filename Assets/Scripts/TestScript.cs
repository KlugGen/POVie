using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        SwipeDetector.Instance.RegisterOnSwipeStarted(OnSwipeStarted);
        SwipeDetector.Instance.RegisterOnSwipe(OnSwipe);
        SwipeDetector.Instance.RegisterOnSwipeEnded(OnSwipeEnded);
        ShakeDetector.Instance.RegisterOnShakeStarted(OnShakeStarted);
        ShakeDetector.Instance.RegisterOnShake(OnShake);
        ShakeDetector.Instance.RegisterOnShakeEnded(OnShakeEnded);
    }

    private void OnSwipe(SwipeData obj)
    {
        Debug.Log("On Swipe: " + obj.SwipeTime);
        GetComponent<Image>().color = Color.yellow;
    }

    private void OnSwipeStarted(SwipeData obj)
    {
        Debug.Log("Swipe started: " + obj.Direction + "  " + obj.SwipeTime);
        GetComponent<Image>().color = Color.green;
    }

    private void OnSwipeEnded(SwipeData obj)
    {
        Debug.Log("Swipe ended: " + obj.Direction + "  " + obj.SwipeTime);
        GetComponent<Image>().color = Color.red;
    }

    private void OnShakeStarted(ShakeData shakeData)
    {
        Debug.Log("Shake started: " + shakeData.ShakePower);
        GetComponent<Image>().color = Color.yellow;
    }

    private void OnShake(ShakeData shakeData)
    {
        Debug.Log("Shake: " + shakeData.ShakePower);
        GetComponent<Image>().color = Color.red;
    }

    private void OnShakeEnded(ShakeData shakeData)
    {
        Debug.Log("Shake ended: ");
        GetComponent<Image>().color = Color.green;
    }
}
