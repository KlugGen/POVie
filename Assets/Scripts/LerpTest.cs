using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpTest : MonoBehaviour
{
    public AnimationPair pairLerp, pairToward;
    public AnimationPair pairLerpCoroutine, pairTowardCoroutine;

    public float animationSpeed = 10f;
    float t1 = 0f, t2 = 0f;

    void Start() {
       //StartCoroutine("Animation1");
        //StartCoroutine("Animation2");

    }

    void Update() {

        t1 += Time.deltaTime / animationSpeed;

        float distance1 = Vector3.Distance(pairLerp.element.position, pairLerp.end.position);

        pairLerp.element.position = Vector3.Lerp(pairLerp.element.position, pairLerp.end.position, t1);
        if (distance1 < 2f) {
            pairLerp.element.position = pairLerp.start.position;
            t1 = 0f;
        }




        t2 += Time.deltaTime / animationSpeed;

        float distance2 = Vector3.Distance(pairToward.element.position, pairToward.end.position);
        
        pairToward.element.position = Vector3.MoveTowards(pairToward.element.position, pairToward.end.position, t2);
        if (distance2 < 2f)
        {
            pairToward.element.position = pairToward.start.position;
            t2 = 0f;
        }

    }

    IEnumerator Animation1() {
    
        while (Vector3.Distance(pairLerpCoroutine.element.position, pairLerpCoroutine.end.position) > 2f)
        {
            pairLerpCoroutine.element.position = Vector3.Lerp(pairLerpCoroutine.element.position, pairLerpCoroutine.end.position, Time.deltaTime * animationSpeed);
            yield return null;
        }

        pairLerpCoroutine.element.position = pairLerpCoroutine.start.position;
        StartCoroutine("Animation1");
    }

    IEnumerator Animation2()
    {

        while (Vector3.Distance(pairTowardCoroutine.element.position, pairTowardCoroutine.end.position) > 2f)
        {
            pairTowardCoroutine.element.position = Vector3.MoveTowards(pairTowardCoroutine.element.position, pairTowardCoroutine.end.position, Time.deltaTime * animationSpeed);
            yield return null;
        }

        pairTowardCoroutine.element.position = pairTowardCoroutine.start.position;
        StartCoroutine("Animation2");
    }
}

[System.Serializable]
public class AnimationPair {
    public RectTransform start, end, element;
}