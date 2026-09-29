using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneFollower : MonoBehaviour
{
    private Transform drone;

    public Vector3 position
    {
        set
        {
            transform.position = value;
        }
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Drop(Transform t)
    {
        drone = t;
        Fix();
        enabled = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(drone.gameObject.activeSelf)
        {
            Fix();
        }
        else
        {
            enabled = false;
            gameObject.SetActive(false);
        }
    }

    private void Fix()
    {
        transform.position = drone.position;
        transform.rotation = drone.rotation;
    }
}