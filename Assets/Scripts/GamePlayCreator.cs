using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elements;
using System.Linq;
using UnityEngine.UI;

public class GamePlayCreator : Singleton<GamePlayCreator>
{
    [Header("Static elements")]
    public Transform mapMarkersParent;
    public Transform gameplayCountriesParent;

    [Header("Prefabs")]
    public GameObject mapMarkerPrefab;
    public GameObject citySample;
    public GameObject countrySample;
    public GameObject wordDeconstructionCountrySample;
    public GameObject wordDecontructionCitySample;
    public GameObject languageSample;
    public GameObject letterSample;
    public GameObject letterAngle;

    //public List<int> degrees = new List<int>();

    public void RemoveCountry(string name)
    {
        if (name == "")
            return;

        TextDecontruction_Country tdc = TextDeconstructionController.Instance.countries.Find(o => o.country.ToLower() == name.ToLower());
        if (tdc != null)
        {
            TextDeconstructionController.Instance.countries.Remove(tdc);
            DestroyImmediate(tdc.gameObject);
        }

        //RemoveMapMarker(name);

        Countries c = ElementsDatabase.Instance.GetCountryByName(name);
        if (c == null)
            return;

        if (c.mapLandscape != null)
            DestroyImmediate(c.mapLandscape);

        if (c.mapPortrait != null)
            DestroyImmediate(c.mapPortrait);

        if (c.gameplayParent != null)
            DestroyImmediate(c.gameplayParent);

        ElementsDatabase.Instance.countries.Remove(c);

    }

    public void RemoveCity(string city, string country)
    {
        if (city == "" || country == "")
            return;

        Countries countryObject = ElementsDatabase.Instance.countries.Find(o => o.name.ToLower() == country.ToLower());
        if (countryObject == null)
        {
            Debug.Log("No such country in database: " + country);
            return;
        }

        Cities cityObject = countryObject.cities.Find(o => o.name.ToLower() == city.ToLower());
        if (cityObject == null)
        {
            Debug.Log("No such city in database");
            return;
        }



        // destroy deconstruction for this city
        TextDecontruction_Country tdc = TextDeconstructionController.Instance.countries.Find(o => o.country.ToLower() == country.ToLower());

        if (tdc != null)
        {
            var t = tdc.deconstructions.Find(o => o.country.ToLower() == country.ToLower() && o.city.ToLower() == city.ToLower());
            if (t != null)
            {
                tdc.deconstructions.Remove(t);
                DestroyImmediate(t.gameObject);
            }
            else
                Debug.Log("City decon err");
        }
        else
        {
            Debug.Log("Country decon err");
        }


        // destroy city from gameplay
        DestroyImmediate(cityObject.gameplay);

        // destroy city from database
        countryObject.cities.Remove(cityObject);
    }

    public void AddNewCountry(string countryName)
    {
        if (countryName == "")
            return;

        if (ElementsDatabase.Instance.countries.Find(o => o.name.ToLower() == countryName.ToLower()) != null)
        {
            Debug.Log("Such country exists");
            return;
        }

        // Add country to the gameplayParent

        GameObject g = (GameObject)Instantiate(countrySample, gameplayCountriesParent);
        g.name = countryName;

        // Add word decontruction structure

        GameObject g1 = (GameObject)Instantiate(wordDeconstructionCountrySample, TextDeconstructionController.Instance.transform);
        g1.SetActive(true);
        g1.name = countryName;
        g1.GetComponent<TextDecontruction_Country>().country = countryName;
        TextDeconstructionController.Instance.countries.Add(g1.GetComponent<TextDecontruction_Country>());


        // Add marker for a map
        GameObject mapMarker = CreateMapMarkers(countryName);

        // Add country to the database

        Countries c = new Countries
        {
            name = countryName,
            gameplayParent = g,
            mapPortrait = mapMarker
        };

        ElementsDatabase.Instance.countries.Add(c);

        countryName = "";
    }

    public void AddNewCity(string countryName, string cityName, Sprite image, string wordDeconstruction_language, List<int> degrees)
    {
        if (countryName == "" || cityName == "" || image == null || wordDeconstruction_language == "")
        {
            Debug.Log("Missing some of arguments");
            return;
        }

        Countries country = ElementsDatabase.Instance.countries.Find(o => o.name.ToLower() == countryName.ToLower());
        if (country == null)
        {
            Debug.Log("Such country doesn't exsists");
            return;
        }

        TextDecontruction_Country deconstructionCountry = TextDeconstructionController.Instance.countries.Find(o => o.country.ToLower() == countryName.ToLower());

        if (deconstructionCountry == null)
        {
            Debug.Log("Such country for word deconstruction doesn't");
            return;
        }

        if (deconstructionCountry.deconstructions.Find(o => o.city.ToLower() == cityName.ToLower()) != null)
        {
            Debug.Log("Such city for word deconstruction exists");
            return;
        }

        foreach (Countries c1 in ElementsDatabase.Instance.countries)
        {
            if (c1.cities.Find(o => o.name.ToLower() == cityName.ToLower()) != null)
            {
                Debug.Log("Such city exists");
                return;
            }
        }

        if (country.cities.Find(o => o.name.ToLower() == cityName.ToLower()) != null)
        {
            Debug.Log("Such city exists in country");
            return;
        }

        // check if degrees in lists has prefabs


        foreach (int d in degrees)
        {
            if (ElementsDatabase.Instance.GetElementByDegree(d) == null)
            {
                Debug.Log("There is no prefab for: " + d.ToString());
                return;
            }
        }


        // Add city to gameplay country
        // instantiate one
        // set city name
        // set country name
        // set image sprite

        GameObject g1 = (GameObject)Instantiate(citySample, country.gameplayParent.transform);
        g1.name = cityName;
        g1.GetComponent<CityController>().country = countryName;
        g1.GetComponent<CityController>().cityName = cityName;
        g1.GetComponent<CityController>().image.sprite = image;
        g1.GetComponent<CityController>().image.rectTransform.sizeDelta = new Vector2(image.rect.width, image.rect.height);

        Cities c = new Cities
        {
            name = cityName,
            gameplay = g1
        };

        foreach (int i in degrees)
        {
            GameObject g = (GameObject)Instantiate(ElementsDatabase.Instance.GetElementByDegree(i).spotPrefab, g1.GetComponent<CityController>().spotsParent.transform);
            c.elementsDegrees.Add(i);
        }

        GameObject g2 = (GameObject)Instantiate(wordDecontructionCitySample, deconstructionCountry.transform);
        g2.name = cityName;
        g2.GetComponent<TextOverlayController>().country = countryName;
        g2.GetComponent<TextOverlayController>().city = cityName;

        deconstructionCountry.deconstructions.Add(g2.GetComponent<TextOverlayController>());

        country.cities.Add(c);

        countryName = "";
        cityName = "";
        image = null;
    }


    public void AddLanguageToCity()
    {

    }

    public string countryToRemoveName;

    //[NaughtyAttributes.Button("RemoveCountryFromDatabase")]
    public void RemoveCountryFromDatabase()
    {
        if (countryToRemoveName == "" || ElementsDatabase.Instance.countries.Find(o => o.name.ToLower() == countryToRemoveName.ToLower()) == null)
        {
            Debug.Log("Such country doesn't exists");
            return;
        }

        Countries c = ElementsDatabase.Instance.countries.Find(o => o.name.ToLower() == countryToRemoveName.ToLower());
        //ElementsDatabase.Instance.countries.Remove(c);
        countryToRemoveName = "";
    }

    public string mapMarkerName = "";
    // create map markers

    public bool RemoveMapMarker(string name)
    {

        if (name == "")
            return false;

        // required: country name, markers parent, list of current markers
        for (int i = 0; i < mapMarkersParent.childCount; i++)
        {
            if (mapMarkersParent.GetChild(i).GetComponent<CountryLocation>() != null)
            {
                if (mapMarkersParent.GetChild(i).GetComponent<CountryLocation>().name.ToLower() == mapMarkerName.ToLower())
                {
                    Destroy(mapMarkersParent.GetChild(i).gameObject);
                    return true;
                }
            }
        }

        return false;
    }


    public GameObject CreateMapMarkers(string name = null)
    {
        if (name != null)
            mapMarkerName = name;

        if (mapMarkerName == "")
            return null;

        // required: country name, markers parent, list of current markers
        for (int i = 0; i < mapMarkersParent.childCount; i++)
        {
            if (mapMarkersParent.GetChild(i).GetComponent<CountryLocation>() != null)
            {
                if (mapMarkersParent.GetChild(i).GetComponent<CountryLocation>().name.ToLower() == mapMarkerName.ToLower())
                    return null;
            }
        }

        // check

        GameObject g = null;
#if UNITY_EDITOR
        g = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(mapMarkerPrefab, mapMarkersParent);
        //GameObject g = (GameObject)Instantiate(mapMarkerPrefab, mapMarkersParent);
        g.name = mapMarkerName;
        g.GetComponent<CountryLocation>().name = mapMarkerName;
        g.GetComponentInChildren<UnityEngine.UI.Text>().text = mapMarkerName;

#endif

        mapMarkerName = "";

        return g;
    }



    public void AddLanguageToCity(string languageName, Cities city, List<LetterToMake> lettersToMake)
    {
        if (languageName == "")
        {
            Debug.Log("Empty language");
            return;
        }

        //foreach (LetterToMake l in letters)
        //{
        //    foreach (int d in l.degrees)
        //    {
        //        if (ElementsDatabase.Instance.GetElementByDegree(d) == null)
        //        {
        //            Debug.Log("There is no prefab for: " + d.ToString());
        //            return;
        //        }
        //    }
        //}


        // compare list of degrees to make with letters elements

        int lettersDegreesCounter = 0;
        lettersToMake.ForEach(o => lettersDegreesCounter += o.degrees.Count);

        if (lettersDegreesCounter != city.elementsDegrees.Count)
        {
            Debug.Log(string.Format("Degrees numbers are different. City degrees {0}, letters degrees {1}", city.elementsDegrees.Count, lettersDegreesCounter));
            return;
        }

        List<int> tempDegrees = new List<int>(city.elementsDegrees);
        //bool foreachResult = true;

        lettersToMake.ForEach(delegate (LetterToMake l)
        {
            foreach (int d in l.degrees)
            {
                if (tempDegrees.Contains(d))
                {
                    tempDegrees.RemoveAt(tempDegrees.IndexOf(d));
                }
                else
                {
                    Debug.Log("Missing " + d.ToString() + " element");
                    //foreachResult = false;
                    return;
                }
            }
        });

        if (tempDegrees.Count > 0)
        {
            Debug.Log("Some degrees left");
            return;
        }

        Countries country = ElementsDatabase.Instance.countries.Find(o => o.cities.Contains(city));

        if (country == null)
        {
            Debug.Log("No such country: ");
            return;
        }

        TextDecontruction_Country tdc = TextDeconstructionController.Instance.countries.Find(o => o.country.ToLower() == country.name.ToLower());

        if (tdc == null)
        {
            Debug.Log("No such country for deconstruction");
            return;
        }

        TextOverlayController toc = tdc.deconstructions.Find(o => o.city.ToLower() == city.name.ToLower());

        if (toc == null)
        {
            Debug.Log("No such country for deconstruction");
            return;
        }

        TextControllerSet tcs = toc.sets.Find(o => o.language.ToLower() == languageName.ToLower());

        if (tcs != null)
        {
            Debug.Log("Already has such language");
            return;
        }

        GameObject languageGO = (GameObject)Instantiate(languageSample, toc.transform);
        languageGO.SetActive(true);
        languageGO.name = languageName;


        TextControllerSet newSet = new TextControllerSet();
        newSet.language = languageName;
        newSet.parentGameObject = languageGO;


        lettersToMake.ForEach(delegate (LetterToMake l)
        {
            GameObject letterInstantiate = (GameObject)Instantiate(letterSample, languageGO.transform);
            letterInstantiate.SetActive(true);
            letterInstantiate.name = l.letterLabel;

            LetterController lc = letterInstantiate.GetComponent<LetterController>();

            lc.pairs.Clear();
            lc.letterColor = l.color;
            lc.helpMessage = l.letterLabel;
            lc.specialLetter = l.specialOne;
            lc.correctDropsExpected = l.degrees.Count;
            newSet.letters.Add(lc);

            foreach (int d in l.degrees)
            {
                ElementsPair ep = new ElementsPair();

                GameObject letterAng = (GameObject)Instantiate(letterAngle, letterInstantiate.transform);
                letterAng.name = d.ToString();
                letterAng.SetActive(true);
                letterAng.GetComponent<UnityEngine.UI.Image>().sprite = ElementsDatabase.Instance.GetElementByDegree(d).spriteFilled;
                letterAng.GetComponent<UnityEngine.UI.Image>().color = l.color;
                ep.degrees = d;
                ep.element = letterAng;
                lc.pairs.Add(ep);
            }
        });

        toc.sets.Add(newSet);

        //Debug.Log("Degrees validation: OK");


        // Add word deconstrucion for country and city
        // instantiate one
        // set city name
        // set country name
        // add one language
        // set letters count

        //GameObject g2 = (GameObject)Instantiate(wordDecontructionCitySample, deconstructionCountry.transform);
        //g2.name = cityName;
        //g2.GetComponent<TextOverlayController>().country = countryName;
        //g2.GetComponent<TextOverlayController>().city = cityName;

        //// isntantiate spots and letters
        //lettersToMake.ForEach(delegate (LetterToMake l) {
        //    GameObject letterInstantiate = (GameObject)Instantiate(letterSample, g1.GetComponent<CityController>().spotsParent.transform);
        //    LetterController lc = letterInstantiate.GetComponent<LetterController>();
        //    lc.helpMessage = l.letterLabel;
        //    lc.specialLetter = l.specialOne;
        //    lc.correctDropsExpected = l.degrees.Count;


        //    foreach (int d in l.degrees)
        //    {
        //        GameObject g = (GameObject)Instantiate(ElementsDatabase.Instance.GetElementByDegree(d).spotPrefab, g1.GetComponent<CityController>().spotsParent.transform);
        //        ElementsPair ep = new ElementsPair();
        //        ep.spotController = g.GetComponent<SpotController>();


        //    }
        //});

        // Add city to element database
        // find country
        // add city
        // set city name


        //lettersToMake.Clear()
    }

    [NaughtyAttributes.Button("Screens of all")]
    public void ScreensOfAllWords()
    {
        StartCoroutine("ScreensOfAll");
    }

    [NaughtyAttributes.Button("Screens of ##")]
    public void ScreensOfAllHashtags()
    {
        StartCoroutine("ScreensOfAllHashtagsCo");
    }

    IEnumerator ScreensOfAllHashtagsCo()
    {
        //GamePlayController.Instance.gameObject.SetActive(true);
        //menu.gameObject.SetActive(false);

        int counter = 0;
        yield return new WaitForEndOfFrame();

        GamePlayController.Instance.newLayoutTopBar.SetActive(false);
        GamePlayController.Instance.gameObject.SetActive(true);
        menu.gameObject.SetActive(false);


        foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
        {
            foreach (Elements.Cities city in c.cities)
            {
                city.name.Log("Processing: ");

                if (!city.IsReady())
                    continue;


                city.gameplay.SetActive(true);
                city.GetController().HideALLSPOTS();
                city.GetController().HideCloseAreas();
                city.GetController().HideDropAreas();
                yield return new WaitForEndOfFrame();
                city.GetController().ScaleImage();

                yield return new WaitForEndOfFrame();

                string hashTagSentence = city.GetController().GetCurrecntSentenceDots();
                string hashTagWord = city.GetController().GetCurrentWordWithHash();

                if (!string.IsNullOrEmpty(hashTagSentence))
                {
                    //HashtahSentenceController.Instance.Enable(string.Format(hashTagSentence, "<b>"+ hashTagWord + "</b>"));
                    //HashtahSentenceController.Instance.Enable(string.Format(hashTagSentence, " . . . "), city);
                }

                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(.5f);

                string pathGameplays = Application.persistentDataPath + "\\Hashtags\\";

                if (!System.IO.Directory.Exists(pathGameplays))
                {
                    System.IO.Directory.CreateDirectory(pathGameplays);
                }
                else
                {
                    // System.IO.Directory.Delete(pathGameplays);
                    //System.IO.Directory.CreateDirectory(pathGameplays);
                }

#if UNITY_EDITOR

                GameViewUtils.TakeScreenshot(pathGameplays + "\\", Elements.ElementsDatabase.Instance.GetCountryByCity(city).name + "_" + city.name);

#endif

                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                city.gameplay.SetActive(false);

                continue;

            }

            counter++;
        }



    }

    //[NaughtyAttributes.Button("LaunchGameplayForAll")]
    //public void LaunchGameplayForAll()
    //{
    //    StartCoroutine("LaunchGameplayForAllCo");
    //}

    //IEnumerator LaunchGameplayForAllCo()
    //{
    //    //GamePlayController.Instance.gameObject.SetActive(true);
    //    //menu.gameObject.SetActive(false);

    //    int counter = 0;
    //    yield return new WaitForEndOfFrame();

    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            if (!city.readyToGo || city.type == CitiesGameplayType.tutorial)
    //                continue;

    //            GamePlayController.Instance.Disable();
    //            yield return new WaitForEndOfFrame();
    //            GamePlayController.Instance.Enable(city);

    //            yield return new WaitForEndOfFrame();
    //            yield return new WaitForEndOfFrame();

    //        }

    //        counter++;
    //    }
    //}

    public MenuController menu;

    IEnumerator ScreensOfAll()
    {
        "scr".Log();
        GamePlayController.Instance.newLayoutTopBar.SetActive(false);
        GamePlayController.Instance.gameObject.SetActive(true);
        menu.gameObject.SetActive(false);

        int counter = 0;
        yield return new WaitForEndOfFrame();

        foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
        {
            foreach (Elements.Cities city in c.cities)
            {

                if (city.gameplay == null || city.GetController() == null)
                    continue;


                city.gameplay.SetActive(true);
                city.GetController().HideALLSPOTS();
                yield return new WaitForEndOfFrame();
                city.GetController().ScaleImage();
                //city.GetController().ScaleImageInvert();

                //Rect r =  city.GetController().GetComponent<RectTransform>().GetScreenRect(ViewsController.Instance.mainCanvas);


                //Texture2D tex = new Texture2D( (int)r.width, (int)r.height, TextureFormat.RGB24, false);

                //tex.ReadPixels(r, 0, 0);

                //tex.Apply();
                //string pathGameplays = Application.persistentDataPath + "\\Gameplays_Areas\\";

                //DKK.IO.SaveToFile(pathGameplays + "\\sample", tex.EncodeToPNG());

                //yield return null;

                //city.GetController().ShowCloseAreas();
                //city.GetController().ShowDropAreas();

                //foreach (Spot s in city.GetController().SpotsList())
                //{
                //    if (s == null)
                //        continue;

                //    s.figureImage.color = new Color(255, 0, 153);
                //}
                //yield return new WaitForEndOfFrame();
                //yield return new WaitForEndOfFrame();



                //if (!System.IO.Directory.Exists(pathGameplays))
                //{
                //    System.IO.Directory.CreateDirectory(pathGameplays);
                //}
                //else
                //{
                //    // System.IO.Directory.Delete(pathGameplays);
                //    //System.IO.Directory.CreateDirectory(pathGameplays);
                //}

#if UNITY_EDITOR

                //city.GetController().image.GetComponent<RectTransform>().

                //GameViewUtils.TakeScreenshot(pathGameplays + "\\", Elements.ElementsDatabase.Instance.GetCountryByCity(city).name + "_" + city.name);

#endif

                // yield return new WaitForEndOfFrame();
                //yield return new WaitForEndOfFrame();

                //city.gameplay.SetActive(false);

                //continue;

                TextOverlayController toc = TextDeconstructionController.Instance.GetCity(c.name.ToLower(), city.name.ToLower());


                if (toc == null)
                {
                    //city.name.Log("No # for ");
                    continue;
                }

                toc.gameObject.SetActive(true);

                yield return new WaitForEndOfFrame();

                //Debug.Log("Starting for " + city.name);

                foreach (TextControllerSet tcs in toc.sets)
                {
                    tcs.parentGameObject.SetActive(true);


                    foreach (LetterController lc in tcs.letters)
                    {
                        if (lc == null)
                            continue;

                        foreach (ElementsPair ep in lc.pairs)
                        {
                            if (ep == null)
                                continue;

                            ep.element.GetComponent<UnityEngine.UI.Image>().enabled = true;
                        }
                    }

                    yield return new WaitForSeconds(.2f);

                    string path = Application.persistentDataPath + "\\HashtagScreens\\" + c.name + "\\" + city.name + "\\" + tcs.language + "\\";

                    path = Application.persistentDataPath + "\\HashtagScreens\\All\\";

                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }

                    path += c.name + "_" + city.name + "_" + tcs.language + "_";


#if UNITY_EDITOR
                    GameViewUtils.TakeScreenshot(path);
#endif

                    yield return new WaitForSeconds(.2f);
                    tcs.parentGameObject.SetActive(false);
                }

                toc.gameObject.SetActive(false);

                yield return new WaitForEndOfFrame();
            }

            counter++;

            //if (counter > 3)
            //    break;
        }



    }


    //[NaughtyAttributes.Button("Check translations for cities")]
    //public void translationsCities()
    //{
    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        TextTranslationModule.GetWord(c.name);
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            if (city.readyToGo)
    //                TextTranslationModule.GetWord(city.name);
    //        }
    //    }
    //}

    //[NaughtyAttributes.Button("CheckProperMagentaRotations")]
    //public void CheckProperMagentaRotations()
    //{
    //    float degreesSpan = 10f;

    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            if (!city.readyToGo)
    //                continue;

    //            city.GetController().GetTextOverlayController().sets.ForEach(delegate (TextControllerSet set)
    //            {

    //                if (set.language.ToLower() != "中文(香港)")
    //                    return;

    //                set.letters.ForEach(delegate (LetterController l)
    //                {
    //                    if (!l.specialLetter)
    //                        return;

    //                    l.pairs.ForEach(delegate (ElementsPair pair)
    //                    {
    //                        float elementRotation = pair.element.transform.localEulerAngles.z;

    //                        //Quaternion q = pair.element.transform.rotation * Quaternion.Inverse(pair.spotController.transform.rotation);
    //                        float angle = Quaternion.Angle(pair.spotController.transform.rotation, pair.element.transform.rotation);
    //                        //angle.Log();

    //                        float min = angle - degreesSpan;
    //                        float max = angle + degreesSpan;
    //                        //min.Log("Min: ");
    //                        //max.Log("Max: ");

    //                        float spotRotation = pair.spotController.transform.localEulerAngles.z;
    //                        if (angle > degreesSpan)
    //                        {

    //                            Debug.Log(string.Format("{0} {1} {2} a:{3}", c.name, city.name, l.helpMessage, angle));
    //                        }
    //                    });
    //                });
    //            });

    //        }
    //    }
    //}

    //[NaughtyAttributes.Button("CheckForAnglesWithGaps")]
    //public void CheckForAnglesWithGaps()
    //{
    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            if (city.gameplay == null || city.GetController() == null)
    //            {
    //                Debug.Log("Skip for: " + city.name);
    //                continue;
    //            }

    //            foreach (Spot item in city.GetController().SpotsList())
    //            {
    //                if (item.figureImage.sprite == Elements.ElementsDatabase.Instance.dragables.Find(o => o.ID == item.degrees.ToString()).sprite)
    //                {
    //                    Debug.Log("Wrong for " + city.name);
    //                }
    //            }
    //        }
    //    }
    //}

    //public UnityEditor.AssetDatabase defAsset;

    //[NaughtyAttributes.Button("FillAvatars")]
    //public void FillAvatars()
    //{

    //}

    //[NaughtyAttributes.Button("Add groups for cities")]
    //public void GroupsCities()
    //{
    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            city.groupID = c.GetMyGroup().id;
    //        }
    //    }
    //}
#if UNITY_EDITOR

    [NaughtyAttributes.Button("validate cities")]
    public void GroupsCities()
    {
        foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
        {
            foreach (Elements.Cities city in c.cities)
            {
                if (city.attentionDistractor)
                {
                    city.name.LogDev("Att distr: ");
                }
            }
        }
    }


    [NaughtyAttributes.Button("BundleSize")]
    public void BundleSize()
    {
        return;
        foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
        {
            foreach (Elements.Cities city in c.cities)
            {
                if (city.gameplay == null || city.GetController() == null || !city.IsReady() || !city.bundle.requireBundle)
                {
                    Debug.Log("Skip for: " + city.name);
                    continue;
                }

                string path = System.IO.Path.Combine(Application.persistentDataPath, System.IO.Path.Combine( "AssetBundles", city.bundle.GetBundleName()));

                var fileInfo = new System.IO.FileInfo(path);
                float size = (float)fileInfo.Length / 1024f / 1024f;
                Debug.Log(city.name + " - "  +size);

                city.bundle.bundleSize = size;
                UnityEditor.EditorUtility.SetDirty(city.GetController());

                //System.IO.FileInfo


                continue;

                Transform t = city.GetController().image.transform.Find("EditorImage");

                if(t == null)
                {
                    Debug.Log("No editor image for: " + city.name);
                    continue;
                }
                //t.GetComponent<Image>().sprite.texture.

                //t.gameObject.name.Log("Name: ");
            }
        }

     
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }

    [NaughtyAttributes.Button("CheckIfSentenceSplitIsPossible")]
    public void CheckIfSentenceSplitIsPossible()
    {
        foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
        {
            foreach (Elements.Cities city in c.cities)
            {
                if (city.gameplay == null || city.GetController() == null || !city.IsReady() || !city.bundle.requireBundle)
                {
                    Debug.Log("NOT READY. Skip for: " + city.name);
                    continue;
                }

                TextControllerSet tcs = city.GetController().GetTextOverlayController().sets.Find(o => o.language.ToLower() == "english");

                if(tcs == null)
                {
                    Debug.Log("TCS NULL. Skip for: " + city.name);
                    continue;
                }

                string[] stringSeparators = new string[] { "\n\n" };
                string[] lines = tcs.hashTagSentence.Split(stringSeparators, System.StringSplitOptions.None);

                if(lines.Length < 2)
                {
                    city.name.Log("1 Length for: ");
                    lines[0].Log();
                }
                else
                {
                    city.name.Log("2 Length for: ");
                    lines[0].Log();
                    lines[1].Log();
                }
            }
        }
    }

#endif

    //[NaughtyAttributes.Button("Check Chinese with rand")]

    //public void ChineseWIthRand()
    //{
    //    Elements.ElementsDatabase.Instance.countries.ForEach(delegate(Countries c)
    //    {
    //        c.cities.ForEach(delegate(Cities city) {
    //            if (city.ranomizeDropSpots)
    //            {
    //                if(city.languageSettings.Find(o => o.language == Language.Chinese).readyToGo)
    //                {
    //                    Debug.LogError(city.name + " not ready for CHinese");
    //                }
    //            }
    //        });
    //    });
    //}

    public GameObject editorImage;

    //[NaughtyAttributes.Button("Bundle initialization")]
    //public void AddEditorImage()
    //{
    //    List<string> names = new List<string>();
    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            if (!city.readyToGo)
    //                continue;

    //            //city.bundle.bundleName = city.bundleName;
    //            //city.bundle.hasBundle = city.hasBundle;
    //            //city.bundle.imageName = city.imageName;
    //            //city.bundle.requireBundle = city.requireBundle;

    //            continue;

    //            string bundleName = string.Format("{0}.{1}", c.name.ToLower(), city.name.ToLower());
    //            //bundleName.LogDev();
    //            city.bundle.bundleName = bundleName;
    //            continue;

    //            CityController controller = city.GetController();
    //            if (controller == null || controller.gameObject == null)
    //                continue;

    //            string img_name = controller.image.sprite.name;
    //            if (!names.Contains(img_name))
    //            {
    //                img_name.LogDev("Adding: ");
    //                names.Add(img_name);
    //            }
    //            else
    //            {
    //                "ALREADY EXISTS".LogDev();
    //            }

    //            city.bundle.imageName = img_name;
    //            //controller.image.sprite.name.LogDev();
    //            //city.imageName = controller.image.sprite.name;

    //            continue;

    //            controller.gameObject.name.LogDev("Controller: ") ;
    //            Transform t = controller.image.transform.Find("EditorImage");
    //            if (t == null)
    //            {
    //                GameObject g = GameObject.Instantiate(editorImage, controller.image.transform);
    //                g.name = "EditorImage";                  
    //                g.GetComponent<Image>().sprite = controller.image.sprite;
    //                g.transform.SetAsFirstSibling();
    //               // g.transform.SetParent(controller.image.transform);
    //                //return;
    //            }

    //        }
    //    }
    //}

    //[NaughtyAttributes.Button("ZoomChange")]
    //public void ZoomChange()
    //{
    //    List<string> names = new List<string>();
    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            if (!city.readyToGo)
    //                continue;

    //          if(city.type == CitiesGameplayType.zooming)
    //            {
    //                if (city.zoomingMultiplier == 2f)
    //                    city.zoomingMultiplier = 1.5f;
    //            }

    //        }
    //    }
    //}

    //[NaughtyAttributes.Button("Specials counter")]
    //public void SpecialsCounter()
    //{
    //    List<string> names = new List<string>();
    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            if (!city.readyToGo)
    //                continue;

    //            string.Format("{0} : {1}", city.name, city.GetController().SpecialAnglesCounter()).LogDev();

    //        }
    //    }
    //}



    public void Console_ClearAll()
    {
        // clear saves
        UserController.Instance.DeleteSaves();

#if !UNITY_EDITOR
        UserController.Instance.DeleteAssetBundles();
#endif
        // clear player prefs

        PlayerPrefs.DeleteAll();

        // close application

        Application.Quit();
    }

    public void Console_Finish()
    {
        foreach (CountryGroup cg in ElementsDatabase.Instance.groups.OrderBy(x => x.id))
        {
            foreach (Countries c in ElementsDatabase.Instance.countries.FindAll(o => o.groupName.ToLower() == cg.groupName.ToLower()))
            {
                c.cities.ForEach(delegate (Cities city)
                {
                    if (Application.isPlaying && city.IsReady())
                    {
                        city.GetCurrentLanguageSet().isNew = false;
                        //city.GetCurrentLanguageSet().
                        UserController.Instance.AddEntry(city.GetController(), 300, 2);
                    }
                });
            }
        }
    }
}
[System.Serializable]
    public class LetterToMake
    {
        public string letterLabel;
        public Color color;
        public List<int> degrees = new List<int>();
        public bool specialOne = false;
    }