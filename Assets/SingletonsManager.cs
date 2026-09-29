using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonsManager : MonoBehaviour
{
    public OnboardingController onboarding;

    public void Awake()
    {
        OnboardingController.SetExternalSingleton(onboarding); 
    }
}
