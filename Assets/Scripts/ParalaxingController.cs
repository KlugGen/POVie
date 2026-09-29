using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParalaxingController : MonoBehaviour
{
    public float shiftMultiplier = 5f;
    // Start is called before the first frame update
    public float scaleMultiplier = 2f;

    public float constantOffset = .1f;

    public float gyroscopeMultiplier=100f;

    public float constantMultiplier = 2f;

    public bool debug = false;

    public float speedOfLerp = 5f;

    private Vector3 startPosition;

    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            startPosition = transform.localPosition;
            Input.gyro.enabled = true;
            enabled = true;
        }
        else
            enabled = false;

        transform.localScale = transform.localScale + transform.localScale * shiftMultiplier * scaleMultiplier;

#if UNITY_EDITOR
        // for unity editor

        startPosition = transform.localPosition;
        enabled = true;
#endif
    }

    // Update is called once per frame
    void Update()
    {

        float centerX = Screen.width / 2;
        float centerY = Screen.height / 2;

        Vector3 newPosition = Vector3.zero;

        //Vector2 shiftVector = Vector2.zero;
        //Vector3 gV3_temp = GyroscopeController.Instance.GetCurrentCalculatedAttitude();
        //Vector3 gV3 = new Vector3(gV3_temp.y, gV3_temp.x, gV3_temp.z);
        //shiftVector = gV3;
        // Marcin change for:
        Vector2 shiftVector = new Vector2(-(GyroForParallax.Instance.x-.5f), -(GyroForParallax.Instance.y-.5f));
        //GyroForParallax.Instance.logs.text = shiftVector.ToString();

        //shiftVector = new Vector2(centerX - gV3.x, centerY - gV3.y);

        newPosition = new Vector3(startPosition.x + (shiftVector.x * shiftMultiplier*constantMultiplier), startPosition.y + (shiftVector.y * shiftMultiplier * constantMultiplier), 0f);
        // Marcin change for:


#if UNITY_EDITOR
        if ((Input.mousePosition.y < 0 || Input.mousePosition.x > Screen.height) || (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width))
            return;

        shiftVector = new Vector2((centerX - Input.mousePosition.x)/centerX, (centerY - Input.mousePosition.y) / centerY);
        //Debug.Log(shiftVector);

        //shiftVector = new Vector2(shiftVector.x);
        newPosition = new Vector3(startPosition.x + (shiftVector.x * shiftMultiplier), startPosition.y + (shiftVector.y * shiftMultiplier), 0f);
       //newPosition = new Vector3(startPosition.x + (shiftVector.x) * (shiftMultiplier * constantOffset), startPosition.y + (shiftVector.y) * (shiftMultiplier * constantOffset), 0f);
        //Debug.Log("EDITOR");
#endif

        if (debug)
        {
           // Debug.Log("Shift: "+shiftVector + " and position: " + transform.localPosition);

        }
        float distance = Vector3.Distance(transform.localPosition, newPosition);

        //distance.LogDev();
        if(distance>0.2f)
            transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, Time.deltaTime* speedOfLerp);

        //transform.localPosition = newPosition;
    }


    void PrepareForStandalone() {
       
    }
}
