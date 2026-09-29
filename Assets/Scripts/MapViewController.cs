using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Elements;

public class MapViewController : View
{
    public MapViewControllerFields landscapeFields, portraitFields;
    public float doneLevelsScale = .9f;
    public float drainedLevelScale = .8f;

    private ViewAttributes<MapViewControllerFields> attributes = new ViewAttributes<MapViewControllerFields>();
    public GameObject animationStop_gameObject;
    public Canvas dropDownButton_canvas;
    public DropDownMapController dropDown_script;
    public CapitalStateScreen capitalScreen;

    public bool markersSaclingEnabled = false;

    public ProgressBarPercentage progressNumber, progressComponents;
    public MapExplenation mapExplenation;

    static private MapViewController instance;

    static public MapViewController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(MapViewController))[0] as MapViewController;

            return instance;
        }
    }

    public override void EnableLandscape()
    {
        base.EnableLandscape();

        SetMapPosition();
        landscapeFields.InitializeCapitaText(UserController.Instance.GetStars());

        //if (landscapeFields.Cities != null && landscapeFields.Cities.Count > 0) 
        //{
        //    landscapeFields.map.ZoomMap(0.5f);
        //    landscapeFields.map.Focus(landscapeFields.Cities[0]);
        //}
    }

    public override void EnablePortrait()
    {
        base.EnablePortrait();

        SetMapPosition();

        portraitFields.InitializeCapitaText(UserController.Instance.GetStars());
    }

    public override void Enable()
    {
        attributes.Initialize(portraitFields, landscapeFields);
        base.Enable();

        DisableExplentation();
        //if (!UserController.Instance.GetStats().HasFlag("mapExplained"))
        //{
        //    UserController.Instance.GetStats().AddFlag("mapExplained");
        //    EnableExplentation();
        //}

        //attributes.Get().starCounter.SetActive(UserController.Instance.AnyPointsWereGiven());
        attributes.Get().starCounter.SetActive(true);

        dropDown_script.Disable();
        capitalScreen.SetStarButtonState();

        //#if UNITY_EDITOR
        //        PlayerPrefs.SetString("unlockedCountry" + UserController.Instance.GetLanguage(), "Germany");
        //        StartCoroutine("AnimationForScroll");
        //#endif

    }

    public void UpdateMarkersScale()
    {
        if (!markersSaclingEnabled)
            return;

        foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
        {
            if (c.mapPortrait != null)
            {
                GameObject g = c.GetMapMarker();
                CountryLocation cl;
                if (g != null)
                {
                    cl = g.GetComponent<CountryLocation>();

                    if (cl != null)
                        cl.UpdateScale();
                }
            }
        }
    }

    public void SetMapPosition()
    {
        infoDisplayed = false;
        attributes.Get().map.Enable();

        if (UserController.Instance.GetStats().HasDictionary("unlockedCountry" + UserController.Instance.GetLanguage()))
        {
            int maxUnlocked = UserController.Instance.GetStats().GetInt("maxUnlockedId_saved" + UserController.Instance.GetLanguage(), -2);

            UserController.Instance.GetStats().RemoveFromDictionary("unlockedCountry" + UserController.Instance.GetLanguage());

            if (maxUnlocked > -1)
            {
                NewGroupUnlocked.Instance.Enable();
            }
            else
            {
                StartAnimation();
            }

            return;
        }

        CoolOffScreen.Instance.Disable();
        SetMapCountryFocus();
    }

    public void StartAnimation()
    {
        StartCoroutine("AnimationForScroll");
    }

    //public MapViewControllerFields GetAttributes()
    //{
    //    if (ViewsController.Instance.GetCurrentOrientation() == ScreenOrientation.Portrait)
    //        return portraitFields;

    //    return landscapeFields;
    //}

    public void OpenCapitalScreen()
    {
        capitalScreen.Initialize(UserController.Instance.GetStars());
    }

    public void DropDownButton()
    {
        //dropDownButton_canvas.overrideSorting = true;
        if (dropDown_script.gameObject.activeInHierarchy)
        {
            dropDown_script.Disable();
            DropDownAdditionalView.Instance.Disable();
        }
        else
            dropDown_script.Enable();
    }

    IEnumerator AnimationForScroll()
    {
        yield return new WaitForEndOfFrame();

        animationStop_gameObject.SetActive(true);

        attributes.Get().map.ZoomMap(0f);
        attributes.Get().scrollRect.horizontalNormalizedPosition = 0f;

        yield return new WaitForSeconds(.4f);

        while (attributes.Get().scrollRect.horizontalNormalizedPosition < .95f)
        {
            attributes.Get().scrollRect.horizontalNormalizedPosition = Mathf.Lerp(attributes.Get().scrollRect.horizontalNormalizedPosition, 1f, Time.deltaTime * 0.5f);
            yield return null;
        }

        AnimationEnd();
    }

    private bool firstMapEnable = true;

    /// <summary>
    /// Set focus of map on country
    /// </summary>
    public void SetMapCountryFocus()
    {
        if (UserController.Instance.GetStats().HasFlag("blockade", true))
        {
            int maxUnlocked = UserController.Instance.GetStats().GetInt("maxUnlockedId_saved" + UserController.Instance.GetLanguage(), -2);
            Elements.CountryGroup group = Elements.ElementsDatabase.Instance.groups.Find(o => o.id == maxUnlocked);
            GroupLockIndicator.Instance.Enable(group);
        }
        else
        {
            GroupLockIndicator.Instance.Disable();
        }

        CheckPostAnimationScreenDisplay();

        //"focus".Log();
        string key = "maxUnlockedId_saved" + UserController.Instance.GetLanguage();
        int unlockedId = UserController.Instance.GetStats().GetInt(key, -2);
        //string.Format("Id: {0}, key: {1}", unlockedId, key).Log();

        Elements.ElementsDatabase.Instance.GetCountriesByGroupId(-1).ForEach(o => o.GetMapMarker().GetComponent<Animator>().enabled = false);
        Elements.ElementsDatabase.Instance.GetCountriesByGroupId(-1).ForEach(o => o.GetMapMarker().GetComponent<CountryLocation>().ResetScale());


        // if group was just unlocked
        string key1 = "unlockedCountry" + UserController.Instance.GetLanguage();
        if (UserController.Instance.GetStats().HasDictionary(key1))
        {
            string countryName = (UserController.Instance.GetStats().GetString(key1));
            Elements.Countries cntry = Elements.ElementsDatabase.Instance.GetCountryByName(countryName);
            if (cntry != null)
            {
                //countryName.Log("Zooming on: ");
                attributes.Get().map.ZoomMap(0.3f);
                attributes.Get().map.Focus(cntry.GetMapMarker());
            }
            else
            {
                countryName.Log("NULL!: ");
            }

            return;
        }

        /// else
        /// if buffor level is last unlocked

        if (unlockedId == -1)
        {
            List<string> countriesOrder = new List<string>() { "Norway", "Qatar", "Estonia" };
            Elements.Countries country = null;
            List<Countries> groupCountries = Elements.ElementsDatabase.Instance.GetCountriesByGroupId(unlockedId);

            foreach (Countries c1 in groupCountries)
            {
                if (!c1.IsDone(unlockedId))
                {
                    c1.mapPortrait.DeActivate();
                }
            }

            foreach (string cnt in countriesOrder)
            {
                Countries tempC = groupCountries.Find(o => o.name.ToLower() == cnt.ToLower());
                if (tempC != null)
                {
                    if (tempC.IsNew_V2(unlockedId))
                    {
                        tempC.name.Log();
                        tempC.mapPortrait.Activate();
                        country = tempC;
                        break;
                    }
                }
            }

            if (country == null)
            {
                country = ElementsDatabase.Instance.GetCountriesByGroupId(unlockedId).OrderBy(x => x.name).ToList<Elements.Countries>().Find(o => o.IsNew());
            }

            country.GetMapMarker().GetComponent<Animator>().enabled = true;

            attributes.Get().map.ZoomMap(0.3f);
            attributes.Get().map.Focus(country.GetMapMarker());
            return;
        }
        else
        {
            if (firstMapEnable)
            {
                firstMapEnable = false;
                Elements.Countries country = GetFirstNewFromGroup(unlockedId);

                if (country != null)
                {
                    UserController.Instance.GetStats().AddString("lastPlayedCity", country.name);
                    attributes.Get().map.ZoomMap(0.3f);
                    attributes.Get().map.Focus(country.GetMapMarker());
                }

                return;
            }
        }

        /// if norway not finished than focus on norway
        /// else focus on qatar

        string lastPlayed = UserController.Instance.GetStats().GetString("lastPlayedCity", "Malta");
        //lastPlayed.Log("Last played country: ");

        Elements.Countries c = Elements.ElementsDatabase.Instance.GetCountryByName(lastPlayed);

        if (c != null)
        {
            if (!c.IsNew_V2(unlockedId))
            {
                //c.name.LogDev("!!Is not new: ");
                c = GetFirstNewFromGroup(unlockedId);
                //c.name.LogDev("New not played found: ");
            }
            else
            {
                //c.name.LogDev("Is already new: ");
            }
        }
        else
        {
            //"country null".LogDev();
        }

        if (c == null || c.GetMapMarker().activeInHierarchy == false)
        {
            c = Elements.ElementsDatabase.Instance.GetCountryByName("Malta");
        }

        if (c != null)
        {
            attributes.Get().map.ZoomMap(0.3f);
            attributes.Get().map.Focus(c.GetMapMarker());
        }
    }

    private Countries GetFirstNewFromGroup(int unlockedId)
    {
        Elements.Countries country = Elements.ElementsDatabase.Instance.GetCountriesByGroupId(unlockedId).OrderBy(x => x.name).ToList<Elements.Countries>().Find(o => o.IsNew());

        if (country != null)
        {
            return country;
        }

        return null;
    }

    private bool infoDisplayed = false;

    public void AnimationEnd()
    {
        StopCoroutine("AnimationForScroll");
        attributes.Get().scrollRect.horizontalNormalizedPosition = .5f;
        CoolOffScreen.Instance.Disable();

        if (UserController.Instance.GetStats().HasFlag("blockade", true))
        {
            int maxUnlocked = UserController.Instance.GetStats().GetInt("maxUnlockedId_saved" + UserController.Instance.GetLanguage(), -2);

            string blockadePopUpKey = "blockadeAutoPopUp" + maxUnlocked + UserController.Instance.GetLanguage();
            if (!UserController.Instance.GetStats().HasFlag(blockadePopUpKey))
            {
                UserController.Instance.GetStats().AddFlag(blockadePopUpKey);
                // display blockade pop up
                Elements.CountryGroup group = Elements.ElementsDatabase.Instance.groups.Find(o => o.id == maxUnlocked);
                CoolOffScreen.Instance.Enable(group);
                infoDisplayed = true;
            }
        }

        SetMapCountryFocus();
        animationStop_gameObject.SetActive(false);
    }

    private void CheckPostAnimationScreenDisplay()
    {
        string lang = UserController.Instance.GetLanguage();

        // check if there are any new levels

        //if (UserController.Instance.hasNewCities)
        //{
        //    NewLevelsScreen.Instance.Enable();           
        //    return;
        //}

        // check for tutorials to show
        int id = UserController.Instance.GetStats().GetInt("tutorialShow" + lang, -1);

        //"tutorialShowDelete".LogDev();


        // tutorial sub screens
        switch (id)
        {
            case -1:
                break;
            case 4:
                break;
            case 2:
                UserController.Instance.GetStats().RemoveFromDictionary("tutorialShow" + lang);
                TutorialAdditionalsController.Instance.SecondTutorial();
                break;
            case 3:
                UserController.Instance.GetStats().RemoveFromDictionary("tutorialShow" + lang);
                TutorialAdditionalsController.Instance.ThirdTutorial();
                break;
        }

        // modes sub screens

        // count all done cities
        List<Elements.Cities> cities = Elements.ElementsDatabase.Instance.GetAllDoneCities();
        //cities.Count.Log("Unlocked counter: ");

        if (cities.Count >= 8 && !infoDisplayed)
        {
            bool relaxModeEnabled = UserController.Instance.GetStats().GetInt("mode_relax_" + lang, 0) == 1;
            bool engagModeEnabled = UserController.Instance.GetStats().GetInt("mode_engaging_" + lang, 0) == 1;
            bool enterModeEnabled = UserController.Instance.GetStats().GetInt("mode_entertaining_" + lang, 0) == 1;

            if (cities.Count >= 31)
            {
                if (!relaxModeEnabled)
                {
                    UserController.Instance.GetStats().AddInt("mode_relax_" + lang, 1);
                    ModeUnlockedScreen.Instance.RelaxingUnlocked();
                    AddViewToClose(ModeUnlockedScreen.Instance);
                }
            }
            else if (cities.Count >= 27)
            {
                if (!enterModeEnabled)
                {
                    UserController.Instance.GetStats().AddInt("mode_entertaining_" + lang, 1);
                    ModeUnlockedScreen.Instance.EntertainingUnlocked();
                    AddViewToClose(ModeUnlockedScreen.Instance);
                }
            }
            else if (cities.Count >= 24)
            {
                if (!engagModeEnabled)
                {
                    UserController.Instance.GetStats().AddInt("mode_engaging_" + lang, 1);
                    ModeUnlockedScreen.Instance.EngagingUnlocked();
                    AddViewToClose(ModeUnlockedScreen.Instance);
                }
            }
        }

        if (!infoDisplayed)
        {
            var stats = UserController.Instance.GetTopScores();

            if (!UserController.Instance.GetStats().HasFlag("777reached", true))
            {
                if (stats >= 10)
                {
                    UserController.Instance.GetStats().AddFlag("777reached", true);
                    Golden777ReachedView.Instance.Enable();
                }
            }
            else
            {
                if (stats > 10)
                {
                    int modulo = (stats - 10) / 7;
                    if (modulo >= 1)
                    {
                        for (int i = 1; i <= modulo; i++)
                        {
                            string key = "777reached_part" + i;
                            if (!UserController.Instance.GetStats().HasFlag(key, true))
                            {
                                UserController.Instance.GetStats().AddFlag(key, true);
                                int stars = 50;
                                UserController.Instance.AddStars(stars, key);
                                Next777Reward.Instance.Enable(modulo, stars);
                                return;
                            }
                        }
                    }
                }
            }

            var counter = ElementsDatabase.Instance.allCities.FindAll(o => o.IsReady() && !o.IsDone()).Count;
            var allCities = ElementsDatabase.Instance.allCities.Count;
            //counter.LogDev("Undone coutner: ");

            if (counter == 0)
            {
                string key = "levelsCompleted" + allCities;
                if (!UserController.Instance.GetStats().HasFlag(key, true))
                {
                    ViewsController.Instance.sharedViews.allLevelsCompleted.Enable();
                    UserController.Instance.GetStats().AddFlag(key, true);
                }
            }
        }
    }

    public void EnableExplentation()
    {
        if (GroupLockIndicator.Instance.gameObject.activeInHierarchy)
            return;

        mapExplenation.parent.SetActive(true);
        mapExplenation.overlay.SetActive(true);
        mapExplenation.button.SetActive(true);
    }

    public void DisableExplentation()
    {
        mapExplenation.parent.SetActive(false);
        mapExplenation.overlay.SetActive(false);
        mapExplenation.button.SetActive(false);
    }
}

[System.Serializable]
public class MapViewControllerFields
{
    public UnityEngine.UI.ScrollRect scrollRect;
    public MapController map;
    public List<GameObject> Cities;
    public UnityEngine.UI.Text capitalText;

    public GameObject starCounter;

    public void InitializeCapitaText(int points)
    {
        if (capitalText != null)
            capitalText.text = "x" + points.ToString();
    }
}

[System.Serializable]
public class MapExplenation
{
    public GameObject parent;
    public GameObject overlay;
    public GameObject button;
}