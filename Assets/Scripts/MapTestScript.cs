using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTestScript : MonoBehaviour
{
    public MapController MapController;
    public GameObject MapLabelToFocus;

    [Button("Enable & Focus")]
    public void Enable() 
    {
        gameObject.SetActive(true);
        MapController.Enable();
        MapController.Focus(MapLabelToFocus);
    }

    [Button("Disable")]
    public void Disable()
    {
        gameObject.SetActive(false);
        MapController.Disable();
    }
}