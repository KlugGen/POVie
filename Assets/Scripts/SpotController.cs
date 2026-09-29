using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpotController : MonoBehaviour
{
    //[HideInInspector]
    public bool matchInLastIterationFound = false;
    [HideInInspector]
    public GameObject tempLetterElement= null;

    public bool basicSpot = true;
    public bool alternativeForBasic = false;
    public Spot spot;
    public CloseArea closeArea;
    public VanishAnimation vanishAnimation;

    public LetterController letter;

    public CityController _city;

    public Sprite animationSprite;

    public Color shadowColor = Color.white;

    public Image shadowImage;

    public void Initialize(CityController city)
    {
        if (vanishAnimation == null)
        {
            vanishAnimation = gameObject.GetComponent<VanishAnimation>();
            if (vanishAnimation == null)
                vanishAnimation = gameObject.AddComponent<VanishAnimation>();

            vanishAnimation.vanishButton = gameObject.GetComponent<Button>();
            if (vanishAnimation.vanishButton == null)
                vanishAnimation.vanishButton = gameObject.AddComponent<Button>();
        }
        
        vanishAnimation.enabled = false;
        
        droppedCorrectly = false;
        closeArea.flashEnabled = false;
        _city = city;
        ResetTarget();
        if (_city.textOverlayController != null)
        {
            letter = _city.textOverlayController.FindMyLetter(this);
            animationSprite = _city.textOverlayController.FindMySprite(this);
        }
    }

    public void CorrectDrop()
    {
        if (letter.specialLetter)
            return;

        droppedCorrectly = true;
        //if(vanishAnimation!=null)
        // vanishAnimation.Enable();
    }


    public bool droppedCorrectly = false;

    public float animationSpeed = 3f;

    public void Update()
    {
        if (spot.deconstructionAnimation)
        {         
            spot.animationElement.transform.position = Vector3.Lerp(spot.animationElement.transform.position, animationTarget.transform.position, Time.deltaTime* animationSpeed);
            spot.animationElement.transform.rotation = Quaternion.Lerp(spot.animationElement.transform.rotation, animationTarget.transform.rotation, Time.deltaTime * animationSpeed);
            spot.animationElement.color = Color.Lerp(spot.animationElement.color, letter.letterColor, Time.deltaTime * animationSpeed);

            //spot.animationElement.transform.localScale = Vector3.Lerp(spot.animationElement.transform.localScale, animationTarget.transform.localScale, Time.deltaTime * animationSpeed);
            spot.animationElement.transform.localScale = Vector3.Lerp(spot.animationElement.transform.localScale,Vector3.one, Time.deltaTime * animationSpeed);
            spot.animationElement.rectTransform.sizeDelta = Vector2.Lerp(spot.animationElement.rectTransform.sizeDelta, animationTarget.GetComponent<RectTransform>().sizeDelta, Time.deltaTime * animationSpeed);
        }
    }

    private GameObject animationTarget;

    public void ResetTarget()
    {
        animationTarget = null;
    }

    public void InitializeTarget(GameObject target)
    {
        animationTarget = target;
    }

    public void EnableAnimation()
    {
        if (animationTarget != null)
        {
            spot.animationElement.transform.SetParent(animationTarget.transform);

            spot.deconstructionAnimation = true;
            spot.animationElement.gameObject.SetActive(true);
        }
        else
        {
            "No animation target".Log();
        }  
    }   
    
    //public LetterController FindMyLetter()
    //{

    //    return _city.textOverlayController.FindMyLetter(this);
    //}
}
