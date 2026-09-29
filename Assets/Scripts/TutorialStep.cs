using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStep : Toturial
{
    public RectTransform startAnimation, endAtnimation;
    public RectTransform animatedElement;
    
    public float animationSpeed = 2f;

    public bool looped = true;

    public Image elementImage;
    public Sprite whiteSprite, colorSprite;

    float t = 0;

    IEnumerator Animation() {

        yield return new WaitForEndOfFrame();
        t = 0;
        elementImage.sprite = whiteSprite;
        animatedElement.position = startAnimation.position;
        animatedElement.transform.SetParent(startAnimation.transform);
        animatedElement.localScale = Vector3.one;

        yield return new WaitForSeconds(0.5f);

        while (Vector3.Distance(animatedElement.position, endAtnimation.position) > 2f)
        {
            t += Time.deltaTime / animationSpeed;
            //float distance = Vector3.Distance(animatedElement.position, endAtnimation.position);
            //float finalSpeed = (distance / animationSpeed);
            animatedElement.position = Vector3.Lerp(animatedElement.position, endAtnimation.position, t);
            yield return null;
        }

        elementImage.sprite = colorSprite;
        yield return new WaitForSeconds(1f);

        animatedElement.position = endAtnimation.position;
        if (looped)
            ResetAnimation();
        else
            enabled = false;
            }

    public override void OnClick() {
        base.OnClick();
    }

    private void ResetAnimation() {
        animatedElement.position = startAnimation.position;
        StartCoroutine("Animation");
    }

    public override void Enable()
    {
        base.Enable();

        if (animatedElement != null && startAnimation != null && endAtnimation != null)
        {
            animatedElement.position = startAnimation.position;
            StartCoroutine("Animation");
        }     
    }
}
