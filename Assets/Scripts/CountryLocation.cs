using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.UI;
 

public class CountryLocation : MonoBehaviour
{
    public string name; 
    public Image outlineImage;
    public Text stateText;
    public Text countryName_text;

    public Text starCounter;
    public Image starImage;
        
    public GameObject newLabel_gameObject;
    private Vector3 initialScale = Vector3.zero;
    private Vector3 scaleInInit;
    private Animator _animator;
    public bool animateOnNew = true;

    public GameObject lockLabel;

    public bool debug = false;

    //public List<Image> stars = new List<Image>();
    public float FixeScale = 1;

    private void Awake()
    {
        initialScale = transform.localScale;
        //initialScale.Log("Awake initial scale: ");
    }

    private Vector3 GetInitialScale()
    {
        if(initialScale == Vector3.zero)
        {
            initialScale = transform.localScale;
            //initialScale.Log("I Scale: ");
        }

        return initialScale;
    }

    public void UpdateScale()
    {
        transform.localScale = new Vector3( FixeScale / transform.parent.transform.localScale.x, FixeScale / transform.parent.transform.localScale.y, FixeScale / transform.parent.transform.localScale.z)/ ViewsController.Instance.mainCanvas.scaleFactor ;
    }

    public void Initliaze(Elements.Countries country)
    {
        _animator = GetComponent<Animator>();
        scaleInInit = transform.lossyScale;
        FixeScale = scaleInInit.x;

        stateText.gameObject.SetActive(false);
        newLabel_gameObject.SetActive(false);

        Elements.CountryGroup group = Elements.ElementsDatabase.Instance.GetCountryGroupByName(country.groupName);

        if (group != null)
        {            
            bool isNew = country.IsNew_V2(UserController.Instance.maxUnlockdedGroup, debug);

            if (debug)
                isNew.LogDev("Is new: ");

            newLabel_gameObject.SetActive(isNew);
            stateText.color = outlineImage.color = group.groupColor;

            if (_animator != null && animateOnNew)
            {
                _animator.enabled = false;
            }

            if (!isNew && !country.HasTutorial())
            {
                bool isDrained = country.IsDrained(UserController.Instance.maxUnlockdedGroup);

                if (isDrained)
                {
                    GetComponent<CanvasGroup>().alpha = .6f;
                    transform.localScale = GetInitialScale() * MapViewController.Instance.drainedLevelScale;
                }
                else
                {
                    transform.localScale = GetInitialScale() * MapViewController.Instance.doneLevelsScale;
                }
            }
            else
            {
                if (_animator != null && animateOnNew)
                {
                    _animator.enabled = true;
                }

                transform.localScale = GetInitialScale();
            }
        }
        else
        {
            outlineImage.color = Color.black;
            "group null".Log();
        }

        //if (country.AreAllStarsGathered())
        //{
        //    if (!country.HasTutorial())
        //        GetComponent<CanvasGroup>().alpha = .8f;
        //}
        
        //int starsCount = 0;
        //int savesCount = 0;

        //country.cities.ForEach(o => o.saves.ForEach(delegate (Elements.SingleSave s)
        //{
        //    starsCount += s.stars;
        //    savesCount++;
        //}));

        starImage.enabled = false;
        starCounter.text = "";

        //if (savesCount > 0)
        //{
        //    int starsAverage = starsCount / savesCount;
        //    starsAverage = Mathf.Clamp(starsAverage, 0, 3);

            
        //    if (starsAverage > 0)
        //    {
        //        starImage.enabled = true;
        //        starCounter.text = starsAverage.ToString() + "x";           
        //    }
        //}       
    }

    public void ResetScale()
    {      
        transform.localScale = GetInitialScale();
    }

    public void OnClick()
    {

        UserController.Instance.GetStats().AddString("lastPlayedCity",this.name);

        Elements.Countries c = Elements.ElementsDatabase.Instance.GetCountryByName(this.name);

        if (c == null)
        {
            return;
        }


        Animator a = c.GetMapMarker().GetComponent<Animator>();
        if(a != null)
        {
            a.enabled = false;
        }

        

        MapViewController.Instance.Disable();
        LocationsView.Instance.Enable(Elements.ElementsDatabase.Instance.GetCountryByName(this.name));      
    }
}