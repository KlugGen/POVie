using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using UnityEngine.Events;

public class ShakeDetector : Singleton<ShakeDetector>
{
    public float shakeDetectionThreshold = 2.0f;

    public UnityAction<ShakeData> OnShakeStarted;
    public UnityAction<ShakeData> OnShake;
    public UnityAction<ShakeData> OnShakeEnded;

    private float accelerometerUpdateInterval = 1.0f / 60.0f;
    // The greater the value of LowPassKernelWidthInSeconds, the slower the
    // filtered value will converge towards current input sample (and vice versa).
    public float lowPassKernelWidthInSeconds = 1.0f;

    private float lowPassFilterFactor;
    private Vector3 lowPassValue;

    [SerializeField] private bool shakeStarted = false;
    [SerializeField] private float timeSinceLastShake = 0f;
    [SerializeField] private float maxTimeSinceLastShake = 0.5f;
    [SerializeField] private float shakingTime = 0f;

    public void RegisterOnShakeStarted(UnityAction<ShakeData> action) => OnShakeStarted += action;

    public void UnregisterOnShakeStarted(UnityAction<ShakeData> action) => OnShakeStarted -= action;

    public void RegisterOnShake(UnityAction<ShakeData> action) => OnShake += action;

    public void UnregisterOnShake(UnityAction<ShakeData> action) => OnShake -= action;

    public void RegisterOnShakeEnded(UnityAction<ShakeData> action) => OnShakeEnded += action;

    public void UnregisterOnShakeEnded(UnityAction<ShakeData> action) => OnShakeEnded -= action;

    private void Start()
    {        
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
    }

    private void Update()
    {
        if (shakeStarted)
        {
            timeSinceLastShake += Time.deltaTime;
            shakingTime += Time.deltaTime;
        }

        Vector3 acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        Vector3 deltaAcceleration = acceleration - lowPassValue;

        ShakeData shakeData = new ShakeData
        {
            ShakePower = deltaAcceleration.sqrMagnitude,
            ShakingTime = shakingTime,
        };

        if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
        {
            if (!shakeStarted)
            {
                shakeStarted = true;
                shakingTime = 0f;
                shakeData.ShakingTime = shakingTime;
                OnShakeStarted?.Invoke(shakeData);
            }
            else 
                OnShake?.Invoke(shakeData);

            timeSinceLastShake = 0f;
        }
        else if (shakeStarted && timeSinceLastShake > maxTimeSinceLastShake)
        {
            OnShakeEnded?.Invoke(shakeData);
            shakeStarted = false;
        }
    }
    
    [NaughtyAttributes.Button("Force Shake")]
    public void ForceOnShake()
    {
        ShakeData shakeData = new ShakeData
        {
            ShakePower = 2f,
            ShakingTime =2f,
        };

        OnShake?.Invoke(shakeData);
    }
}

public struct ShakeData
{
    public float ShakingTime;
    public float ShakePower;
}