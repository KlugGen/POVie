using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneGathererController : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<DroneController>() != null)
        {
            DronesDistractorController.Instance.DroneGathered(other.gameObject.GetComponent<DroneController>());
        }

    }
}
