using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DroneController : MonoBehaviour
{
    Vector3 targetPosition;
    private Rigidbody2D _rigidbody2D = null;

    public Image droneImage;

    public DroneFollower follower;

    public void Enable(Vector3 position)
    {
        gameObject.SetActive(true);

        // Marcin:
        follower.Enable();

        targetPosition = position;
        enabled = true;
        transform.GetComponentInChildren<ParticleSystem>().Play();
    }

    public void ResetToLastPosition()
    {
        Reset(lastPositionFromReset);
    }

    private Vector3 lastPositionFromReset;
    public void Reset(Vector3 position)
    {

        transform.localRotation = Quaternion.identity;

        if (_rigidbody2D == null)
            _rigidbody2D = GetComponent<Rigidbody2D>();

        droneImage.color = Color.white;

        lastPositionFromReset = position;
        
        // reset position outside the screen view
        transform.localPosition = position;
        follower.position = transform.position;
        _rigidbody2D.bodyType = RigidbodyType2D.Static;
        enabled = false;
    }

    public float speed = 10f;

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, speed * Time.deltaTime);
        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.1f)
        {
            transform.localPosition = targetPosition;
            enabled = false;
        }
        follower.position = transform.position;
    }

    public void OnClick()
    {
        // Marcin:
        follower.Drop(transform);

        enabled = false;
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        float torque = Random.Range(3000f, 6000f);
        float multiplier = -1f;

        if (Random.Range(0f, 100f) > 50f)
            multiplier = 1f;

        //Debug.Log(torque * multiplier);
        droneImage.color = Color.gray;
        _rigidbody2D.AddTorque(torque * multiplier);
        DronesDistractorController.Instance.DroneClicked(this);

    }
}
