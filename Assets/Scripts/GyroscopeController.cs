using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroscopeController : Singleton<GyroscopeController>
{
    public Vector3 correctAttitude;
    private Gyroscope _gyroscope;


    // debug
    public UnityEngine.UI.Text debugText, debugTextGryoscope;
    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            _gyroscope = Input.gyro;
            _gyroscope.enabled = true;
            ActualizeGyroAttitude();
        }
    }

    public void ActualizeGyroAttitude()
    {
        if (!SystemInfo.supportsGyroscope || _gyroscope == null)
        {
            correctAttitude = Vector3.zero;
            return;
        }

        correctAttitude = new Vector3(_gyroscope.attitude.x, _gyroscope.attitude.y, _gyroscope.attitude.z);
    }

    public Vector3 GetCurrentCalculatedAttitude()
    {
        if (!SystemInfo.supportsGyroscope)
            return Vector3.one;


        return new Vector3(correctAttitude.x - Mathf.Abs(_gyroscope.attitude.x), correctAttitude.y - Mathf.Abs(_gyroscope.attitude.y), correctAttitude.z - Mathf.Abs(_gyroscope.attitude.z));

        //return new Vector3(correctAttitude.x - _gyroscope.attitude.x, correctAttitude.y - _gyroscope.attitude.y, correctAttitude.z - _gyroscope.attitude.z);
    }

    private Vector3 GetVectorAttitude() {

        return new Vector3(_gyroscope.attitude.x, _gyroscope.attitude.y, _gyroscope.attitude.z);
    }

    private void Update()
    {

        if (!SystemInfo.supportsGyroscope || _gyroscope == null)
        {
            
            return;
        }

        debugText.text = GetVectorAttitude().ToString() + " " + _gyroscope.attitude.eulerAngles.ToString();

       //debugText.text = correctAttitude.ToString() + "-" + GetVectorAttitude().ToString() + "=" + GetCurrentCalculatedAttitude().ToString();
       //debugText.text = correctAttitude.ToString() -  "C:" +GetCurrentCalculatedAttitude().ToString() + " Clamped: " + Vector3.ClampMagnitude(GetCurrentCalculatedAttitude(),1f).ToString();
        debugTextGryoscope.text = "G: "+_gyroscope.attitude.eulerAngles.ToString();
    }
}
