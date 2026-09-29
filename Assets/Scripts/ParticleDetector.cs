using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDetector : MonoBehaviour
{
    public bool CollisionDetected = false;
    private RectTransform rectTransform;
    private BoxCollider2D boxCollider2D;

    private void OnParticleCollision(GameObject other) => CollisionDetected = true;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        //TODO wywoływać też po każdej zmianie orientacji
        SetColliderSize();
    }

    private void SetColliderSize() => boxCollider2D.size = rectTransform.rect.size;
}
