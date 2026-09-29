using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashRotator : MonoBehaviour
{
    public float speed = 1f;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0f, 0f, 1f), speed*Time.deltaTime);
    }
}
