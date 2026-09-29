using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Elements;
using System.IO;
using System.Globalization;

#if UNITY_EDITOR
public class GameplayAutomation : EditorWindow
{
    [MenuItem("Window/Gameplay Creator")]
    static void Init()
    {
        choosenCity = choosenCountry = newCountryName = newCityName = "";
        GetWindow(typeof(GameplayAutomation));
        standardGUIColor = GUI.backgroundColor;
    }
   
    
    static Color standardGUIColor;
    static string choosenCountry = "", choosenCity = "";

    static string newCountryName = "", newCityName = "";

    public Texture mapIcon, spotIcon, imageIcon;
    Vector2 scrollPos;

    void OnGUI()
    {
        scrollPos =
           EditorGUILayout.BeginScrollView(scrollPos);
      
        GUILayout.BeginHorizontal();

        /*
        if (GUILayout.Button("Fix maps markers")) {
            foreach(Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
            {
                if(c.mapLandscape!= null) {
                    CountryLocation cl = c.mapLandscape.GetComponent<CountryLocation>();
                    if (cl.outlineImage == null)
                    {
                        for (int i = 0; i < cl.transform.childCount; i++)
                        {
                            Transform t = cl.transform.GetChild(i);
                            if (t.name.Contains("outline") && t.GetComponent<Image>() != null)
                            {
                                cl.outlineImage = t.GetComponent<Image>();
                            }
                        }
                    }
                }
                if(c.mapPortrait != null)
                {
                    CountryLocation cl= c.mapPortrait.GetComponent<CountryLocation>();
                    if (cl.outlineImage == null)
                    {
                        for(int i = 0; i< cl.transform.childCount; i++)
                        {
                            Transform t = cl.transform.GetChild(i);
                            if (t.name.Contains("outline") && t.GetComponent<Image>()!=null)
                            {
                                cl.outlineImage = t.GetComponent<Image>();
                            }
                        }
                    }
                }
            }
        }
        */


        if (!IsCountryPicked())
        {
            GUILayout.BeginVertical();
            GUILayout.BeginVertical("box");
            CountriesList();
            GUILayout.EndVertical();


            GUILayout.BeginVertical("box");
            PrintGroupsWithCountries();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            GeneralActions();
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }
        

        if (choosenCountry != null && choosenCountry != "")
        {
            GUILayout.BeginVertical();
            EditorGUILayout.Separator();

            GUILayout.BeginVertical("box");
            CitiesList();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        if (choosenCity != "")
        {            
            GUILayout.BeginVertical();
            EditorGUILayout.Separator();
            GUILayout.BeginVertical("box");
            CityDetails();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void ViewsList()
    {
        GUILayout.Label("Views");
    }

    private void ViewButton(GameObject g)
    {
        if (GUILayout.Button(g.name))
        {
            g.SetActive(!g.activeSelf);
        }
    }

    private void PrintGroupsWithCountries()
    {
        GUILayout.BeginVertical();
         
        foreach(CountryGroup cg in ElementsDatabase.Instance.groups.OrderBy(x => x.id))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("{0} [T:{1}][GID:{2}]",cg.groupName , cg.ticketPrice, cg.id));
            List<Cities> citiesReturn = new List<Cities>();
            foreach (Countries c in Elements.ElementsDatabase.Instance.countries)
            {
                foreach (Cities city in c.cities)
                {
                    if (city.GetMyGroup() != null && city.GetMyGroup().id == cg.id && city.IsReadyInAnyLanguage())
                    {
                        citiesReturn.Add(city);
                    }
                }
            }
           
            List<Cities> cities = citiesReturn;

            if (GUILayout.Button("Finish group"))
            {
                cities.ForEach(delegate (Cities c)
                {
                    if (Application.isPlaying && c.IsReady())
                    {
                        c.GetCurrentLanguageSet().allRuns++;
                        c.GetCurrentLanguageSet().isNew = false;
                        c.MarkAsPlayedToday();
                        UserController.Instance.AddEntry(c.GetController(), 300, 2);
                    }
                });
            }

            GUILayout.EndHorizontal();


            cities.ForEach(delegate (Cities c) {
                //if (c.type != CitiesGameplayType.zooming)
                //    return;
                GUILayout.BeginHorizontal();

                string country = ElementsDatabase.Instance.GetCountryByCity(c).name;

                int counter = c.GetTextOverlayController().SpecialLetterElementsCounter();



                if (GUILayout.Button(string.Format("{0} [{1}][{2}]{3}{4}", c.name, country, c.type, c.ranomizeDropSpots ? "[RAND]" : "", c.showMimics ? "[SHAD]" : "")))
                {
                    choosenCountry = country;                 
                    this.country = ElementsDatabase.Instance.GetCountryByCity(c);
                    choosenCity = c.name;
                    choosenCityObject = c;
                }
                
                if (GUILayout.Button("F"))
                {
                    if (Application.isPlaying && c.IsReady())
                    {
                        c.GetCurrentLanguageSet().allRuns++;
                        c.GetCurrentLanguageSet().isNew = false;
                        c.MarkAsPlayedToday();
                        UserController.Instance.AddEntry(c.GetController(), 300, 2);
                    }
                }

                //if (GUILayout.Button("I"))
                //    Selection.activeGameObject = c.gameplay.GetComponent<CityController>().image.gameObject;

                //if (GUILayout.Button("C"))
                //{
                //    if (Application.isPlaying && c.readyToGo)
                //    {
                //        c.GetCurrentLanguageSet().wasBought = false;
                //       c.saves.Clear();
                //    }
                //}



                //if (GUILayout.Button("Z"))
                //{
                //    c.type = CitiesGameplayType.zooming;                
                //}

                GUILayout.EndHorizontal();

            });

            //    foreach (Countries c in ElementsDatabase.Instance.countries.FindAll( o => o.groupName.ToLower() == cg.groupName.ToLower()))
            //{
            //    GUILayout.BeginHorizontal();
            //    GUILayout.Button(c.name);


            //    if (GUILayout.Button("F"))
            //    {
            //        cg.id.Log("Group id: ");
            //        c.cities.FindAll(o => o.groupID >= cg.id).ForEach(delegate (Cities city)
            //        {
            //            Debug.Log("Completing for " + city.name + " with group " + city.groupID);
            //            //city.name.Log("Completing for");
            //            if (Application.isPlaying && city.readyToGo)
            //            {
            //                UserController.Instance.AddEntry(city.GetController(), 300, 2);
            //            }
            //        });
            //    }

            //    if (GUILayout.Button("C"))
            //    {
            //        c.cities.ForEach(delegate (Cities city)
            //        {
            //            if (Application.isPlaying && city.readyToGo)
            //            {
            //                city.GetCurrentLanguageSet().wasBought = false;
            //                city.saves.Clear();
            //            }
            //        });

            //    }

            //    GUILayout.EndHorizontal();
            }
        

        //List<Countries> withoutGroup = ElementsDatabase.Instance.countries.FindAll( o => o.GetMyGroup() == null);
        //if (withoutGroup.Count > 0)
        //{
        //    GUILayout.Label("Unassigned!!");
        //    foreach (Countries c in withoutGroup)
        //    {
        //        GUILayout.Button(c.name);
        //    }
        //}

        GUILayout.EndVertical();
    }


    //private void EnableMagentaEverywhere()
    //{
    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            city.GetController().magentaFirstRequired = true;
    //            EditorUtility.SetDirty(city.GetController());
    //            EditorUtility.SetDirty(city.gameplay.gameObject);
    //        }
    //        }

    //    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    //}

    //private void ReplaceAnglesInGameplays()
    //{
    //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
    //    {
    //        foreach (Elements.Cities city in c.cities)
    //        {
    //            if(city.gameplay == null)
    //            {
    //                Debug.Log("City controller null for " + city.name);
    //                continue;
    //            }
    //            foreach(Spot s in city.GetController().SpotsList())
    //            {
    //                Elements.DragableElements de = Elements.ElementsDatabase.Instance.GetElementByDegree(s.degrees);
    //                if(de == null)
    //                {
    //                    Debug.Log("No for " + s.degrees.ToString());
    //                    continue;
    //                }

    //                if(s.figureImage.sprite == de.sprite)
    //                {
    //                    s.figureImage.sprite = de.spriteFilled;

    //                    //Undo.RecordObject(s.parentSpotController.gameObject, "Via script revert");

    //                    EditorUtility.SetDirty(s.parentSpotController.gameObject);
    //                    EditorUtility.SetDirty(s.figureImage.sprite);
    //                    EditorUtility.SetDirty(s.figureImage);

    //                    Debug.Log("Ok sprite");
    //                }
    //                else
    //                {
    //                    if (s.figureImage.sprite == de.spriteFilled)
    //                    {

    //                    }else
    //                        Debug.Log("Different sprite");
    //                }
    //            }
    //        }
    //    }

    //    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    //    //UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    //}

    private void ReplaceAngles()
    {
        
        foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
        {
            foreach (Elements.Cities city in c.cities)
            {
                TextOverlayController toc = TextDeconstructionController.Instance.GetCity(c.name.ToLower(), city.name.ToLower());
              
                if (toc == null)
                {
                    //city.name.Log("No # for ");
                    continue;
                }

                //Debug.Log("Starting for " + city.name);

                foreach (TextControllerSet tcs in toc.sets)
                {
                    //tcs.letters[0].
                    foreach(LetterController l in tcs.letters)
                    {
                        if (l == null)
                            continue;

                        foreach(ElementsPair ep in l.pairs)
                        {
                            int tryParseResult = -1;
                            
                            if(int.TryParse(ep.element.GetComponent<Image>().sprite.name, out tryParseResult) == false)
                            {
                                Debug.Log("--Cannot change for " + city.name + " in language " + tcs.language + " with sprite " + ep.element.GetComponent<Image>().sprite.name + " spot degrees " + ep.spotController.spot.degrees);

                                continue;
                            }

                            Elements.DragableElements de = Elements.ElementsDatabase.Instance.GetElementByDegree(tryParseResult);
                            if(de == null)
                            {
                                Debug.Log("--Cannot change for " + city.name + " in language " + tcs.language + " with sprite " + ep.element.GetComponent<Image>().sprite.name + " spot degrees " + ep.spotController.spot.degrees);
                            }

                            ep.element.GetComponent<Image>().sprite = de.spriteFilled;
                            //Elements.DragableElements de = Elements.ElementsDatabase.Instance.GetElementByDegree(ep.spotController.spot.degrees);


                            //if(ep.element.GetComponent<Image>().sprite == de.sprite)
                            //{
                            //    //Debug.Log("++Can change");

                            //}
                            //else
                            //{
                            //    //Debug.Log("--Cannot change for " + city.name + " in language " + tcs.language + " with sprite " + ep.element.GetComponent<Image>().sprite.name + " spot degrees " + ep.spotController.spot.degrees);
                            //}
                        }
                    }
                }
            }
        }
    }

    private void CountriesList()
    {
        GUILayout.Label("Countries");
        //if (GUILayout.Button("Find fullstops"))
        //{
        //    foreach (Elements.Countries c in Elements.ElementsDatabase.Instance.countries)
        //    {               
        //        foreach (Elements.Cities city in c.cities)
        //        {
        //            TextOverlayController toc = TextDeconstructionController.Instance.GetCity(c.name.ToLower(), city.name.ToLower());

        //            if (toc == null)
        //            {
        //                city.name.Log("No # for ");
        //                continue;
        //            }

        //                toc.sets.ForEach(delegate(TextControllerSet o) {
        //                if (o.hashTagSentence.Contains(".")) Debug.Log(city.name + " contains .");
        //            });  
        //        }
        //    }
        //}
        //if (GUILayout.Button("EnableMagentaEverywhere"))
        //{
        //    EnableMagentaEverywhere();
        //}
       

        //if (GUILayout.Button("Replace in gameplays"))
        //{
        //    ReplaceAnglesInGameplays();
        //}

     

        GUILayout.BeginHorizontal();
        newCountryName = GUILayout.TextField(newCountryName);

        if (GUILayout.Button("+"))
        {
            //GamePlayCreator.Instance.countryName = newCountryName;
            GamePlayCreator.Instance.AddNewCountry(newCountryName);
            newCountryName = "";
        }

        //ResetGUIColor();

        if (GUILayout.Button("-"))
        {
            if (newCountryName != "")
            {
                GamePlayCreator.Instance.RemoveCountry(newCountryName);
                newCountryName = "";
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        List<Elements.Countries> countries = new List<Elements.Countries>(Elements.ElementsDatabase.Instance.countries);

        countries.Sort(delegate (Elements.Countries x, Elements.Countries y)
        {
            //return 0;
            return x.name.CompareTo(y.name);
        });

        foreach (Elements.Countries c in countries)
        {
            GUILayout.BeginHorizontal();
            string name = c.name; // + string.Format(" [{0}]", c.cities.Count.ToString());

            bool onSomehow = false;
            //bool hasZoomLevels = false;
            bool hasRanodmized = false;
            bool hasMimics = false;

            foreach (Elements.Cities city in c.cities)
            {
                //if (city.readyToGo && city.type == CitiesGameplayType.zooming)
                //    hasZoomLevels = true;

                //if (Application.isPlaying) {
                //    if (city.readyToGo && city.GetController().randomizedDropSpots && !city.ranomizeDropSpots)
                //        hasRanodmized = true;

                //    if (city.readyToGo && city.GetController().showShadows && !city.showMimics)
                //        hasMimics = true;
                //} 

                if (city.gameplay != null && city.gameplay.activeInHierarchy) {
                    onSomehow = true;
                    name += "[ON]";
                    break;
                }
            }

            //if (hasZoomLevels)
            //{
            //    name += "--[Z]";
            //}

            if (hasRanodmized)
            {
                name += "--[RAND]";
            }


            if (hasMimics)
            {
                name += "--[SHAD]";
            }




            if (GUILayout.Button(name))
            {
                Selection.activeGameObject = c.gameplayParent;
                choosenCountry = c.name;
                choosenCity = "";
                country = Elements.ElementsDatabase.Instance.GetCountryByName(choosenCountry);


                //Selection.activeGameObject = c.gameplay;
                if (c.cities.Count == 1)
                {
                    choosenCity = c.cities[0].name;
                    choosenCityObject = c.cities[0];
                }

            }

            //GUIContent content = new GUIContent();
            //content.image = mapIcon;
            
                     
            Elements.Countries tCountry = Elements.ElementsDatabase.Instance.GetCountryByName(c.name);

            if (onSomehow)
            {
                if (GUILayout.Button("Off", GUILayout.Width(20f)))
                {                  
                    foreach (Elements.Cities tCity in tCountry.cities)
                    {
                        tCity.gameplay.GetComponent<CityController>().DisableInEditor();

                        TextOverlayController toc = TextDeconstructionController.Instance.GetCity(c.name, tCity.name);

                        if (toc != null)
                        {
                            foreach (TextControllerSet tcs in toc.sets)
                            {
                                toc.gameObject.SetActive(false);
                                tcs.parentGameObject.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }

            bool somethingMissing = false;
            string missingReason = "";

            foreach (Elements.Cities tCity in tCountry.cities)
            {
                bool shouldSkip = false;

                if (tCity.languageSettings.Find(o => o.readyToGo) == null)
                {
                    //missingReason += "|NR A A";
                    //somethingMissing = true;
                    shouldSkip = true;
                    continue;
                }
                //if (!tCity.IsReady())
                //{

                //    if (tCity.languageSettings.Find(o => o.language == Language.English).readyToGo != tCity.IsReady())
                //    {
                //        missingReason += "|settings";
                //        somethingMissing = true;
                //    }
                //    else
                //    {

                //    }

                //    missingReason += "|NR";
                //    somethingMissing = true;

                //    continue;
                //}

                missingReason += "|" + tCity.name + "|";

                if (tCity.avatar == null)
                {
                    missingReason += "|avatar";
                    somethingMissing = true;
                }

                if (string.IsNullOrEmpty(tCity.author) )
                {
                    missingReason += "|author";
                    somethingMissing = true;
                }

                if (tCity.bundle.requireBundle)
                {
                    if (string.IsNullOrEmpty(tCity.bundle.GetBundleName()) || string.IsNullOrEmpty(tCity.bundle.GetImageName())  || tCity.bundle.GetBundleSize() == 0f)
                    {
                        missingReason += "|bundle";
                        somethingMissing = true;
                    }                      
                }

               // check proper scales

                if(!tCity.GetController().CheckScales())
                {
                    missingReason += "|!!scale";
                    somethingMissing = true;
                }

                TextOverlayController toc = TextDeconstructionController.Instance.GetCity(c.name, tCity.name);

              

                if (toc != null)
                {

                    foreach(Cities.LanguageSettings ls in tCity.languageSettings)
                    {
                        if (ls.readyToGo)
                        {
                            if(toc.sets.Find(o=>o.language.ToLower() == Cities.StringFromLanguage(ls.language)) == null){
                                missingReason += "|settings." + ls.language + tCity.name;
                                somethingMissing = true;
                            }
                        }
                        else
                        {

                        }
                    }

                    foreach (TextControllerSet tcs in toc.sets)
                    {
                        //if (!tcs.minAddDots)
                        //{
                        //    missingReason += "|no-add-dots";
                        //    somethingMissing = true;
                        //}

                        Cities.LanguageSettings ls = tCity.languageSettings.Find(o => Cities.StringFromLanguage(o.language) == tcs.language.ToLower());
                        if (ls == null)
                        {
                            missingReason += "|no language set";
                            somethingMissing = true;
                            continue;
                        }

                        if (!ls.readyToGo)
                        {
                            //missingReason += "|NR-";
                            //somethingMissing = true;
                            continue;
                        }

                        if (tcs.hashTagWord == "")// || tcs.hashTagSentence == "" || !tcs.hashTagSentence.Contains("{0}") )
                        {
                            missingReason += "|hash0";
                            somethingMissing = true;
                        }
                                              
                        if (string.IsNullOrEmpty(tcs.htSentence_min))
                        {
                            missingReason += "|hash1-1";
                            somethingMissing = true;
                        }
                     

                        if (string.IsNullOrEmpty(tcs.htSentence_full))
                        {
                            missingReason += "|hash1-2";
                            somethingMissing = true;
                        }

                        if (string.IsNullOrEmpty(tcs.personalTouch))
                        {
                            missingReason += "|hash1-3";
                            somethingMissing = true;
                        }

                        if (!tcs.htSentence_full.Contains("{0}"))
                        {
                            missingReason += "|hash1-4";
                            somethingMissing = true;
                        }

                        if (tcs.htSentence_min.Contains("...") && tcs.minAddDots)
                        {
                            missingReason += "|hash1-5";
                            somethingMissing = true;
                        }

                        if (tcs.htSentence_min.Contains("{0}"))
                        {
                            missingReason += "|hash1-6";
                            somethingMissing = true;
                        }

                        if (tcs.htSentence_min.Contains("more"))
                            {
                                missingReason += "|hash2";
                                somethingMissing = true;
                            }

                            if (tcs.hashTagWord.Contains(" "))
                            {
                                missingReason += "|hash3";
                                somethingMissing = true;
                            }

                        if (tcs.htSentence_full.Contains("#") || tcs.htSentence_min.Contains("#"))
                        {
                            missingReason += "|##";
                            somethingMissing = true;
                        }                      

                        if (tcs.letters.Find(o => o.specialLetter == true) == null)
                        {
                            missingReason += "|no-special";
                            somethingMissing = true;
                        }
                        int letterSpecialCounter = 0;

                        foreach (LetterController l in tcs.letters)
                        {
                           
                            //if (tcs.language.ToLower() == "english")
                            //{

                            if (tcs.language.ToLower() == "english") { 
                                for (int i = 0; i < l.transform.childCount; i++)
                                {
                                    Transform t = l.transform.GetChild(i);
                                    if (t.name.ToLower().Contains("underline") && t.gameObject.activeSelf == false)
                                    {
                                        missingReason += "|under";
                                        somethingMissing = true;
                                    }
                                }
                        }
                            //}

                            if (l.specialLetter)
                                letterSpecialCounter++;

                            if (l.letterColor.a == 0 || l.letterColor == Color.black)
                            {
                                missingReason += "|c";
                                somethingMissing = true;
                            }

                            foreach (ElementsPair ep in l.pairs)
                            {
                                if(!tCity.ranomizeDropSpots && ep.spotController == null)
                                {
                                    missingReason += "|epNULL";
                                    somethingMissing = true;
                                }

                                if (ep.element.GetComponent<Image>().color == Color.black || ep.element.GetComponent<Image>().color.a == 0)
                                {
                                    missingReason += "|c";
                                    somethingMissing = true;
                                }
                            }
                        }


                        if ( letterSpecialCounter  != 1)
                        {
                            somethingMissing = true;
                            missingReason += "|Special Counter";
                        }
                        //else
                        //{
                        //    somethingMissing = true;
                        //    missingReason += "|Special good";
                        //}

                       
                    }
                }



            }

            if (tCountry.groupName == "") {
                missingReason += "|no-group";
                somethingMissing = true;
            }

            if (!tCountry.HasTutorial() && tCountry.ambientSound == ElementsDatabase.AmbientSound.UNDEFINED)
            {
                missingReason += "|no-sound";
                somethingMissing = true;
            }




            if (somethingMissing)
            {
                if (GUILayout.Button("Miss: " + missingReason, GUILayout.Width(400f)))
                {

                }
            }

            GUILayout.EndHorizontal();
        }
    }

    private void GeneralActions()
    {
        GUILayout.Label("General actions");
        if (GUILayout.Button("Disable all in editor"))
        {
            Elements.ElementsDatabase.Instance.countries.ForEach(o => o.cities.ForEach(city => city.DisableGameplayCityCotroller()));
        }
    }

    private void ActionsForCountry()
    {
        if (country != null)
        {
            GUILayout.Label("Actions");

            if (GUILayout.Button("Select country"))
            {
                Selection.activeGameObject = country.gameplayParent;
            }

            if (GUILayout.Button("Select map marker"))
            {
                Selection.activeGameObject = country.mapPortrait;

            }
        }
    }

    Elements.Countries country = null;

    Elements.Cities choosenCityObject = null;

    private bool IsCountryPicked()
    {
        return choosenCountry != null && choosenCountry != "";
    }

    private void CitiesList()
    {
        if (IsCountryPicked())
        {

            if (country != null)
            {
                if (GUILayout.Button("< BACK"))
                {
                    choosenCountry = null;
                    choosenCountry = "";
                    choosenCity = "";
                }

                GUILayout.Label("Cities for " + choosenCountry);             

                //GUILayout.Space(20);

                foreach (Elements.Cities city in country.cities)
                {

                    GUILayout.BeginVertical();

                    if (GUILayout.Button(city.name))
                    {
                        Selection.activeGameObject = city.gameplay;
                        choosenCity = city.name;
                        choosenCityObject = city;
                    }

                    GUILayout.Space(10f);

                    if (GUILayout.Button("Map portrait"))
                    {
                        Selection.activeGameObject = country.mapPortrait;
                    }

                    if (GUILayout.Button("Img"))
                    {
                        Selection.activeGameObject = city.gameplay.GetComponent<CityController>().image.gameObject;
                    }

                    if (GUILayout.Button("Spots"))
                    {
                        Selection.activeGameObject = city.gameplay.GetComponent<CityController>().spotsParent;
                    }

                    if (GUILayout.Button("Finish"))
                    {
                        if (Application.isPlaying)
                        {
                            UserController.Instance.AddEntry(city.GetController(), 300, 2);
                        }
                    }
                    
                    if (GUILayout.Button("Clear results"))
                    {
                        if (Application.isPlaying)
                        {
                            city.GetCurrentLanguageSet().wasBought = false;
                            city.saves.Clear(); 
                        }
                    }

                    GUILayout.EndVertical();
                }

                EditorGUILayout.Separator();

                if (GUILayout.Button("+ City"))
                {
                    NewCityWindow.choosenCity = choosenCity;
                    NewCityWindow.choosenCountry = choosenCountry;
                    NewCityWindow.Init();
                }
            }
        }
    }

    private void RepaintScene() {
        //SceneView.RepaintAll();
       // EditorWindow view = EditorWindow.GetWindow<SceneView>();
        //Debug.Log(view.position);
        //view.Repaint();
        //SceneView.currentDrawingSceneView.Repaint();
       
    }

    private void CityDetails()
    {
        if (choosenCity != "")
        {
            GUILayout.Label("Details for " + choosenCity);

            string degrees = "";

            choosenCityObject.elementsDegrees.ForEach(o => degrees += o.ToString()+ ",");
            //degrees = degrees.Substring(degrees.Length - 1, 1);

            //GUILayout.Label(string.Format("Angles: {0}", degrees));
            EditorGUILayout.Separator();

            Elements.Countries country = Elements.ElementsDatabase.Instance.GetCountryByName(choosenCountry);
            Elements.Cities city = country.GetCityByName(choosenCity);
            if (city != null)
            {

                GUILayout.Label("Areas Close");
                GUILayout.BeginVertical();
                if (GUILayout.Button("Show"))
                {
                    city.gameplay.GetComponent<CityController>().ShowCloseAreas();
                    EditorUtility.SetDirty(city.gameplay);
                   // RepaintScene();                  
                }

                if (GUILayout.Button("Hide"))
                {
                    city.gameplay.GetComponent<CityController>().HideCloseAreas();
                      EditorUtility.SetDirty(city.gameplay);
                    RepaintScene();
                }

                GUILayout.EndVertical();

                //GUILayout.Label("Images");
                //GUILayout.BeginVertical();
                //if (GUILayout.Button("Show"))
                //{
                //    city.gameplay.GetComponent<CityController>().SpotsList().ForEach(o => o.figureImage.enabled = true);
                //    EditorUtility.SetDirty(city.gameplay);
                //    // RepaintScene();                  
                //}

                //if (GUILayout.Button("Hide"))
                //{
                //    city.gameplay.GetComponent<CityController>().SpotsList().ForEach(o => o.figureImage.enabled = false);
                //    EditorUtility.SetDirty(city.gameplay);
                //    RepaintScene();
                //}

                //GUILayout.EndVertical();

                GUILayout.Label("Areas Drop");
                GUILayout.BeginVertical();
                if (GUILayout.Button("Show"))
                {
                    city.gameplay.GetComponent<CityController>().ShowDropAreas();
                    EditorUtility.SetDirty(city.gameplay);
                }

                if (GUILayout.Button("Hide"))
                {
                    city.gameplay.GetComponent<CityController>().HideDropAreas();
                    EditorUtility.SetDirty(city.gameplay);
                }
                GUILayout.EndVertical();

                GUILayout.Label("Gameplay");
                GUILayout.BeginVertical();
                if (GUILayout.Button("On"))
                {
                    city.gameplay.GetComponent<CityController>().EnableInEditor();
                }

                if (GUILayout.Button("Off"))
                {
                    city.gameplay.GetComponent<CityController>().DisableInEditor();
                }

                GUILayout.EndVertical();

                GUILayout.Label("Text Deconstruction");

                TextOverlayController toc = TextDeconstructionController.Instance.GetCity(choosenCountry, choosenCity);

                if (toc != null)
                {
                    foreach (TextControllerSet tcs in toc.sets)
                    {
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button(tcs.language + " #" +  tcs.hashTagWord))
                        {
                            Selection.activeGameObject = tcs.parentGameObject;
                        }

                        if (GUILayout.Button("On/Off")) {
                            bool isActive = toc.gameObject.activeSelf;
                            toc.gameObject.SetActive(!isActive);
                            tcs.parentGameObject.gameObject.SetActive(!isActive);

                            if (tcs.parentGameObject.gameObject.activeSelf)
                            {
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
                            }
                            else
                            {
                                foreach (LetterController lc in tcs.letters)
                                {
                                    if (lc == null)
                                        continue;

                                    foreach (ElementsPair ep in lc.pairs)
                                    {
                                        if (ep == null)
                                            continue;

                                        ep.element.GetComponent<UnityEngine.UI.Image>().enabled = false;
                                    }
                                }
                            }
                        }

                        GUILayout.EndHorizontal();
                    }
                                      

                }
                
                EditorGUILayout.Separator();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Fit image"))
                {
                    city.gameplay.GetComponent<CityController>().ScaleImage();
                }

                GUILayout.EndHorizontal();


                if (GUILayout.Button("Remove city"))
                {
                    if (EditorUtility.DisplayDialog("Remove city?", "Remove ciy?", "Ok"))
                    {
                        GamePlayCreator.Instance.RemoveCity(choosenCity, choosenCountry);
                    }
                }

                EditorGUILayout.Separator();

                if (GUILayout.Button("+ Language"))
                {
                    NewLanguageWindow.choosenCity = choosenCity;
                    NewLanguageWindow.choosenCountry = choosenCountry;
                    NewLanguageWindow.choosenCityObject = choosenCityObject;
                    NewLanguageWindow.Init();
                }

            }
        }
    }

    public void CityLanguage()
    {
        if (choosenCityObject != null && choosenCountry != "")
        {

            if (GUILayout.Button("Image gameobject"))
            {
                //Selection.activeGameObject = city.gameplay.GetComponent<CityController>().image.gameObject;
            }
        }
    }  

    private float GetWidth(float percnetage)
    {
        return (position.width * percnetage) / 100f;
    }

    private void SetGUIColor(Color c)
    {
        GUI.backgroundColor = c;
    }

    private void ResetGUIColor()
    {
        GUI.backgroundColor = standardGUIColor;
    }

    [MenuItem("DKK/Build Test Client #F5")]
    public static void BuildClient()
    {
        //KillClients(); // You can kill the running processes here before build.
        //SignVersionFile();
        string mClientBuildDirectory = "C:/Users/Jacek/Desktop";
        string mClientBinaryName = VersionName();
        UnityEngine.Debug.Log("Building Client, Directory: " + mClientBuildDirectory);

        PlayerSettings.productName.LogDev("P name:");
        //return;

        try
        {
            //PlayerSettings.productName = "TestClient";
            string[] scenes = new string[] { "Assets/Scenes/MainScene.unity" };

            // Build player
            string fileFullPath = mClientBuildDirectory + "/" + mClientBinaryName;
            BuildPipeline.BuildPlayer(scenes, fileFullPath, BuildTarget.Android, BuildOptions.AllowDebugging);

            UnityEngine.Debug.Log("Built!, File: " + fileFullPath);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    static void SignVersionFile()
    {
        UnityEngine.Debug.Log("Signing version file...");

        int MainVersion = 0;
        int SubVersion = 99;

        string VersionFile = PlayerSettings.bundleVersion;
        try
        {
            string versionFilePath = Application.dataPath + "/Resources/" + VersionFile;

            // Writes with UTF-8 without BOM (byte order mark)
            using (StreamWriter writer = new StreamWriter(versionFilePath))
            {
                string versionDate = System.DateTime.Now.ToString("ddMM-HHmm", CultureInfo.InvariantCulture);
                string versionLine = string.Format("{0}.{1}.{2}", MainVersion, SubVersion, versionDate);
                writer.Write(versionLine);
            }

            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("Signed!");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    static string VersionName()
    {

        int MainVersion = 0;
        int SubVersion = 99;
        string versionDate = System.DateTime.Now.ToString("ddMM-HHmm", CultureInfo.InvariantCulture);
        string versionLine = string.Format("{0}.{1}.{2}.apk", MainVersion, SubVersion, versionDate);

        return versionLine;
    }

}

#endif