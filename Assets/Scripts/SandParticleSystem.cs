using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandParticleSystem : MonoBehaviour
{
    private ParticleSystem ps;
    private RectTransform parentRectTransform;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        var shape = ps.shape;
        shape.radius = parentRectTransform.rect.width;
    }
}
