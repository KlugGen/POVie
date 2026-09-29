using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroForParallax : MonoBehaviour
{
    static private GyroForParallax instance;

    //public bool autoenable = false;

    static public GyroForParallax Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(GyroForParallax))[0] as GyroForParallax;

            return instance;
        }
    }

    public Transform cross;
    public Transform forward, forwardP;
    public Transform backward, backwardP;
    public Transform leftdir, leftdirP;
    public Transform rightdir, rightdirP;
    public Transform gyrodir, gyrodirP;
    public Transform down, left, up;
    public Transform Xt, Yt;

    public float x
    {
        get
        {
            return Vector3.Distance(Xt.position, left.position)/4;
        }
    }

    public float y
    {
        get
        {
            return Vector3.Distance(Yt.position, down.position)/4;
            //return (10-distUp)/10;
        }
    }


    static public void Enbale()
    {
        Instance.gameObject.SetActive(true);
    }

    static public void Disable()
    {
        Instance.gameObject.SetActive(false);
    }

    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            StartCoroutine(WaitForNonZero());
        }
        else
            enabled = false;
    }

    public void Reset()
    {
        gyrodir.rotation = Input.gyro.attitude;

        float distToForward = Vector3.Distance(gyrodirP.position, forwardP.position);
        float distToBackward = Vector3.Distance(gyrodirP.position, backwardP.position);

        if (distToBackward < distToForward)
        {
            float distToLeftdir = Vector3.Distance(gyrodirP.position, leftdirP.position);
            float distToRightDir = Vector3.Distance(gyrodirP.position, rightdirP.position);

            if (distToLeftdir < distToRightDir)
            {
                Vector3 planeGyroDir = new Vector3(gyrodirP.position.x, gyrodirP.position.y, cross.position.z) - cross.position;
                float angle = Vector3.Angle(planeGyroDir, leftdir.rotation.eulerAngles);
                cross.Rotate(Vector3.up, -angle);
            }
            else
            {
                Vector3 planeGyroDir = new Vector3(gyrodirP.position.x, gyrodirP.position.y, cross.position.z) - cross.position;
                float angle = Vector3.Angle(planeGyroDir, rightdir.rotation.eulerAngles);
                cross.Rotate(Vector3.up, angle);
            }
        }
    }

    IEnumerator WaitForNonZero()
    {
        while (true)
        {
            yield return null;
            if (Input.gyro.attitude.eulerAngles.x != 0 || Input.gyro.attitude.eulerAngles.y != 0 || Input.gyro.attitude.eulerAngles.z != 0)
            {
                enabled = true;
                Reset();
                break;
            }

        }
    }

    void Update()  
    {
        gyrodir.rotation = Input.gyro.attitude; 

        float distToForward = Vector3.Distance(gyrodirP.position, forwardP.position);
        float distToBackward = Vector3.Distance(gyrodirP.position, backwardP.position);

        if (distToBackward < distToForward)
        {
            float distToLeftdir = Vector3.Distance(gyrodirP.position, leftdirP.position);
            float distToRightDir = Vector3.Distance(gyrodirP.position, rightdirP.position);

            if(distToLeftdir < distToRightDir)
            {
                Vector3 planeGyroDir = new Vector3(gyrodirP.position.x, gyrodirP.position.y, cross.position.z) - cross.position;
                float angle = Vector3.Angle(planeGyroDir, leftdir.rotation.eulerAngles);
                cross.Rotate(Vector3.up, -angle * 0.01f);
            }
            else
            {
                Vector3 planeGyroDir = new Vector3(gyrodirP.position.x, gyrodirP.position.y, cross.position.z) - cross.position;
                float angle = Vector3.Angle(planeGyroDir, rightdir.rotation.eulerAngles);
                cross.Rotate(Vector3.up, angle * 0.01f);
            }
        }

        Xt.position = Project(leftdirP.position, rightdirP.position, gyrodirP.position);
        Yt.position = Project(down.position, up.position, gyrodirP.position);

        Logs();
    }

    public UnityEngine.UI.Text logs;

    private void Logs()
    {
        if (logs != null && SystemInfo.supportsGyroscope && Input.gyro.enabled)
        {
            try
            {
                string t = string.Format("Gyro: {0}, {1}", x, y);
                logs.text = t;
                Debug.Log(t);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    private Vector3 Project(Vector3 w0, Vector3 w1, Vector3 P)
    {
        return Vector3.Project((P - w0), (w1 - w0)) + w0;
    }
}
