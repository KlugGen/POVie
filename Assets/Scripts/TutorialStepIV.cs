using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStepIV : Toturial
{
    public RectTransform startAnimation, endAtnimation;
    public RectTransform animatedElement;

    public float animationSpeed = 2f;

    public bool looped = true;

    public Image elementImage;
    public Sprite whiteSprite, colorSprite;

    public List<DKK.SimpleAnimation> animations = new List<DKK.SimpleAnimation>();

    //int animationState = 0;
    float t = 0;

    IEnumerator Animation()
    {
        yield return new WaitForEndOfFrame();
        t = 0;
        elementImage.sprite = whiteSprite;
        animatedElement.localEulerAngles = new Vector3(0f,0f,-90f);
        animatedElement.position = startAnimation.position;
        animatedElement.transform.SetParent(startAnimation.transform);
        animatedElement.localScale = Vector3.one;

        yield return new WaitForSeconds(1f);

        while (Vector3.Distance(animatedElement.position, endAtnimation.position) > 2f)
        {
            t += Time.deltaTime / animationSpeed;
            animatedElement.position = Vector3.Lerp(animatedElement.position, endAtnimation.position, t);
            yield return null;
        }

     
        yield return new WaitForSeconds(0.5f);

        animatedElement.position = endAtnimation.position;
        animations.ForEach(o => o.Animate());

        animatedElement.Rotate(new Vector3(0f, 0f, 1f), -45f);

        yield return new WaitForSeconds(0.3f);

        animatedElement.Rotate(new Vector3(0f, 0f, 1f), -45f);
        elementImage.sprite = colorSprite;

        yield return new WaitForSeconds(1f);

        if (looped)
            ResetAnimation();
        else
            enabled = false;
    }

    public override void OnClick()
    {
        base.OnClick();
    }

    private void ResetAnimation()
    {
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
