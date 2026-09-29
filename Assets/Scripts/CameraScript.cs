using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Camera _camera;

    [SerializeField]
    private float zoomSpeed = 1f;

    [SerializeField]
    private float cameraMoveSpeed = 1f;

    [SerializeField]
    private float minCameraSize = 1;

    [SerializeField]
    private float maxCameraSize = 8;

    Vector3 initialMousePosition;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Application.isEditor)
            HandleEditorCameraMove();
        else
            HandleMobileDeviceCameraMove();
    }

    private void HandleEditorCameraMove()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        //PC ZOOM
        Zoom(scroll * zoomSpeed);

        //PC ZOOM MOVE
        if (scroll != 0f)
            Move(Input.mousePosition, cameraMoveSpeed);

        //PC PAN
        if (Input.GetMouseButtonDown(0))
            initialMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Vector3 direction = initialMousePosition - _camera.ScreenToWorldPoint(Input.mousePosition);
            transform.position += direction;
        }
    }

    private void HandleMobileDeviceCameraMove()
    {
        if (Input.touchCount == 2)
        {
            //MOBILE ZOOM
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Zoom(difference * zoomSpeed * 0.005f);

            //MOBILE ZOOM MOVE
            Vector2 pointBetweenTouches = GetCenteredTouch(touchZero.position, touchOne.position);

            if (difference != 0f)
                Move(pointBetweenTouches, cameraMoveSpeed * 0.3f);
        }
        else if(Input.touchCount == 1)
        {
            //MOBILE PAN
            if (Input.GetMouseButtonDown(0))
                initialMousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButton(0))
            {
                Vector3 direction = initialMousePosition - _camera.ScreenToWorldPoint(Input.mousePosition);
                transform.position += direction;
            }
        }
    }

    private Vector2 GetCenteredTouch(Vector2 position1, Vector2 position2)
    {
        float x = (position1.x + position2.x) / 2f;
        float y = (position1.y + position2.y) / 2f;

        return new Vector2(x, y);
    }

    private void Zoom(float amount)
    {
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - amount, minCameraSize, maxCameraSize);
    }

    private void Move(Vector3 referencePoint, float speed)
    {
        transform.position = Vector2.Lerp(transform.position, _camera.ScreenToWorldPoint(referencePoint), speed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, -20f);
    }
}
