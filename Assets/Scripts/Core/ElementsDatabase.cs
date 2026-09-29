using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Elements
{
    public class ElementsDatabase : Singleton<ElementsDatabase>
    {
        public enum AmbientSound
        {
            UNDEFINED = -1,
            SOUND_1 = 0,
            SOUND_2 = 1,
            SOUND_3 = 2,
            SOUND_4 = 3,
            SOUND_5 = 4,
            SOUND_6 = 5,
            FUTURE = 6,
            INDUSTRIAL = 7,
            ISLANDS = 8,
            NIGHT = 9,
            ROCKS = 10,
            VIDEO_GAME = 11,
            LOGIC = 12
        };

        [NaughtyAttributes.ReorderableList]
        public List<GameMode> modes = new List<GameMode>();

        [NaughtyAttributes.ReorderableList]
        public List<Countries> countries = new List<Countries>();

        [NaughtyAttributes.ReorderableList]
        public List<DragableElements> dragables = new List<DragableElements>();

        //[NaughtyAttributes.ReorderableList]
        //public List<DragableElements> dragablesColor = new List<DragableElements>();

        [NaughtyAttributes.ReorderableList]
        public List<CountryGroup> groups = new List<CountryGroup>();

        [NaughtyAttributes.ReorderableList]
        public List<GameplayScenario> scenarios = new List<GameplayScenario>();

        public List<Cities> allCities = new List<Cities>();
              

        public Countries GetCountryByCity(Cities city)
        {
            foreach(Countries c in countries)
            {
                if (c.cities.Contains(city))
                    return c;
            }

            return null;
        }

        public List<Cities> GetPlayableCities()
        {
            List<Cities> cities = new List<Cities>();

            foreach (Countries c in countries)
            {
                cities.AddRange(c.cities.FindAll(o => o.IsReady()));
            }

            return cities;
        }

    public GameMode GetGameModeById(int id)
        {
            return modes.Find(o=>o.id == id);
        }

        public List<Countries> GetCountriesByGroupId(int id)
        {
            return countries.FindAll(o => o.groupName.ToLower() == groups.Find(u => u.id == id).groupName.ToLower());
        }

        public Cities GetCityByName(Countries country, string name)
        {
            foreach (Cities city in country.cities)
                if (city.name.ToLower() == name.ToLower())
                        return city;           

            return null;
        }

        public CountryGroup GetCountryGroupByName(string name)
        {
            return groups.Find(o => o.groupName.ToLower() == name.ToLower());
        }

        public Cities GetCityByName(string name)
        {
            foreach (Countries c in countries)
            {
                foreach (Cities city in c.cities)
                    if (city.name.ToLower() == name.ToLower())
                        return city;
            }

            return null;
        }

        public Countries GetCountryByName(string name)
        {
            Countries country = countries.Find(o => o.name.ToLower() == name.ToLower());

            if (country == null)
                return null;

            return country;
        }

        public DragableElements GetElementByDegree(int degree)
        {
            DragableElements element = dragables.Find(o => o.ID.ToLower() == degree.ToString().ToLower());
            
            if (element == null)
            {
                //Debug.LogError("No such e;lement as: " + name);
                return null;
            }
              
            return element;
        }

        public List<Cities> GetCitiesByGroup(int group)
        {
            List<Cities> citiesReturn = new List<Cities>();
            foreach(Countries c in countries)
            {
                foreach(Cities city in c.cities)
                {
                    if (city.GetMyGroup()!=null &&  city.GetMyGroup().id == group && city.IsReady())
                    {
                        citiesReturn.Add(city);
                    }
                }
            }

            return citiesReturn;
        }

        public List<Cities> GetAllDoneCities()
        {
            List<Cities> citiesReturn = new List<Cities>();

            foreach (CountryGroup cg in groups.OrderBy(x => x.id))
            {
                List<Cities> cities = GetCitiesByGroup(cg.id);

                bool undoneFound = false;

                cities.ForEach(delegate (Cities c)
                {
                    if (!c.IsDone() && c.IsReady())
                    {
                        undoneFound = true;
                    }
                    else
                    {
                        citiesReturn.Add(c);
                    }
                });

                if (undoneFound)
                {
                    break;
                }
            }

            return citiesReturn;
        }

        public List<Cities> GetAllDoneCitiesChallenging()
        {
            List<Cities> citiesReturn = new List<Cities>();

            foreach (Countries c in countries)
            {
                foreach(Cities city in c.cities)
                {
                    if (city.IsReady() && city.IsDone() && city.GetMyGroup().id >= 1)
                        citiesReturn.Add(city);
                }
            }

            return citiesReturn;
        }

        public void ResetDailyPlay()
        {
            List<Cities> citiesReturn = new List<Cities>();
            foreach (Countries c in countries)
            {
                foreach (Cities city in c.cities)
                {
                    city.GetCurrentLanguageSet().lastSuccessfullPlayDate = "";
                }
            }
        }
    }

    [System.Serializable]
    public class Countries
    {        
        public string name;        
       
        public string groupName;
        //public int groupID  = -1;

        public List<Cities> cities = new List<Cities>();
        public GameObject mapPortrait, mapLandscape;
        public GameObject gameplayParent;

        public Elements.ElementsDatabase.AmbientSound ambientSound = ElementsDatabase.AmbientSound.UNDEFINED;

        [HideInInspector]
        public bool isNew = true; // unchecked

        public GameObject GetMapMarker()
        {
            if (ViewsController.Instance.GetCurrentOrientation() == ScreenOrientation.Portrait)
                return mapPortrait;

            return mapLandscape;
        }

        public bool IsNew(int maxGroup = -5)
        {
            List<Cities> myCities = cities.FindAll(o => o.GetMyGroup().id >= maxGroup && o.IsReady());
            if(myCities == null || myCities.Count == 0)
            {
                return false;
            }

            foreach (Cities c in myCities)
            {
                if (c.IsDone() && c.IsReady())
                    return false;
            }

            //name.Log("Country new: ");
            return true;
        }

        public bool IsNew_V2(int maxGroup = -5, bool debug = false)
        {
            List<Cities> myCities = cities.FindAll(o => o.GetMyGroup().id <= maxGroup);

            if (debug)
            {
                maxGroup.LogDev("Group: ");
                cities.Count.Log("Cities all: ");
                cities.ForEach(o => o.name.LogDev());
                cities.FindAll(o => o.IsReady() == true).Count.Log("Cities ready: ");
                cities.FindAll(o => o.IsReady() == true).ForEach(o => o.name.LogDev());
                myCities.Count.Log("Cities in group: ");
                myCities.ForEach(o => o.name.LogDev());
            }

            if (myCities == null || myCities.Count == 0)
            {
                return false;
            }                                   

            foreach (Cities c in myCities)
            {
                //if (debug)
                //{
                //    c.name.Log("Is new city: ");
                //}

                if (!c.IsDone() && c.IsReady())
                    return true;
            }

            //name.Log("Country new: ");
            return false;
        }

        public bool IsDrained(int maxGroup = -5)
        {
            //cities.Count.LogDev("Cities for " +  name +": ");
            List<Cities> myCities = cities.FindAll(o => o.GetMyGroup().id >= maxGroup);
            if (myCities == null || myCities.Count == 0)
            {
                return false;
            }

            foreach (Cities c in myCities)
            {
                if (c.CheckIfStarsAvailable() && c.IsReady())
                    return false;
            }

            return true;
        }
       
        public bool IsDone(int maxGroup = -5)
        {
            List<Cities> myCities = cities.FindAll(o => o.GetMyGroup().id >= maxGroup);
            if (myCities == null || myCities.Count == 0)
            {
                return true;
            }

            foreach (Cities c in myCities)
            {
                if (!c.IsDone() && c.IsReady())
                    return false;
            }

            return true;
        }
        

        public bool AreAllStarsGathered()
        {
            foreach (Cities c in cities)
            {
                if (c.IsReady() && c.CheckIfStarsAvailable())
                    return false;
            }

            return true;
        }

        public Cities GetCityByName(string name)
        {
            return cities.Find(o => o.name.ToLower() == name.ToLower());
        }       

        public void EnableMarkers()
        {
            if (mapLandscape != null)
                mapLandscape.SetActive(true);

            if (mapPortrait != null)
                mapPortrait.SetActive(true);

            if(mapPortrait.GetComponent<DKK.SimpleAnimation>() != null)
            {                
                mapPortrait.GetComponent<DKK.SimpleAnimation>().animationDelay = GetMyGroup().id*.1f;
                mapPortrait.GetComponent<DKK.SimpleAnimation>().PreInitialize();
                mapPortrait.GetComponent<DKK.SimpleAnimation>().AnimateCondition();
            }
        }

        public void DisableMarkers()
        {
            if (mapLandscape != null)
                mapLandscape.SetActive(false);

            if (mapPortrait != null)
                mapPortrait.SetActive(false);
        }

        public CountryGroup GetMyGroup()
        {
           return ElementsDatabase.Instance.GetCountryGroupByName(groupName);
        }

        public bool HasTutorial()
        {
            foreach (Cities c in cities)
                if (c.type == CitiesGameplayType.tutorial)
                    return true;


            return false;
        }
    }

    [System.Serializable]
    public class Cities
    {
        public enum GroupSource
        {
            PARENT,
            DIFFERENT
        }

        [System.Serializable]
        public class LanguageSettings
        {
            public Language language = Language.English;
            public bool readyToGo = true;

            public ParametersInheritance ranomizeDropSpotsInheritance = ParametersInheritance.Inherit;  
            public bool ranomizeDropSpots = false;
        }
              
        public static float replayTimeoutHours = 24f;
        public static float restartTimeoutHours = 24f;
        public string name;

        public List<LanguageSettings> languageSettings = new List<LanguageSettings>() { new LanguageSettings() { readyToGo = true, language = Language.English }, new LanguageSettings() { readyToGo = true, language = Language.Chinese } };
        
        [HideInInspector]
        public bool wasInSaves = false;                
        public bool readyToGo = true;        

        [HideInInspector]
        public string groupName = "";     

        public bool attentionDistractor = false;      

        public BundleInfo bundle;

        public Sprite avatar = null;
        public string author = "";

        [Header("Group")]
        public GroupSource groupSource = GroupSource.PARENT;
        public int groupID;

        [Header("Audio")]
        public Elements.ElementsDatabase.AmbientSound ambientSound = ElementsDatabase.AmbientSound.UNDEFINED;
        public float audioOffset = 0f;

        [Header("Type")]
        public CitiesGameplayType type = CitiesGameplayType.regular;

        public float zoomingMultiplier = 2f;
        public float mimicsTransparency = .5f;
        public bool showMimics = false;
        public bool ranomizeDropSpots = false;
        public bool progressiveShadowsHint = false;
      
        public bool helpLogic = false;

        [System.NonSerialized]
        public Countries country;         
        
        public bool HasRandomizedDropSpots()
        {
            LanguageSettings ls = GetLanguageSettings();

            if(ls != null)
            {
                if (ls.ranomizeDropSpotsInheritance == ParametersInheritance.Override)
                    return ls.ranomizeDropSpots;
            }

            return ranomizeDropSpots;
        }
        
        public bool IsReady(string language = null)
        {           
            LanguageSettings ls = GetLanguageSettings(language);

            if(ls == null)
            {
                Debug.LogWarning("No settings!");
                return readyToGo;
            }

            return ls.readyToGo;
        }

        private LanguageSettings GetLanguageSettings(string language = null)
        {
            string tempLanguage = language;
            if (language == null)
                tempLanguage = UserController.Instance.GetLanguage();

            Language l = LanguageFromString(tempLanguage);

            LanguageSettings ls = languageSettings.Find(o => o.language == l);
            return ls;
        }

        public bool IsReadyInAnyLanguage()
        {
            LanguageSettings ls = languageSettings.Find(o => o.readyToGo);

            if (ls == null)
            {
                return false;
            }

            return true;
        }

        public static Language LanguageFromString(string langString)
        {
            switch (langString.ToLower())
            {
                case "english":                  
                    return Language.English;
                case "中文(香港)":
                    return Language.Chinese;
            }

            return Language.English;

        }

        public static string StringFromLanguage(Language lang)
        {
            switch (lang)
            {
                case Language.English:
                    return "english";
                case Language.Chinese:
                    return "中文(香港)";
            }

            return "english";
        }

        public bool IsDone()
        {
            return GetCurrentLanguageSet().succesfullRuns > 0;         
        }

        [HideInInspector]
        public List<string> unsuccessfulPlays = new List<string>();

        public List<CityLanguageSet> languageFields = new List<CityLanguageSet>();

        public GameObject gameplay;

        public List<int> elementsDegrees = new List<int>();
        public List<SingleSave> saves = new List<SingleSave>();

        public CityLanguageSet GetCurrentLanguageSet()
        {          
            return GetLanguageSetOrCreate(UserController.Instance.GetLanguage().ToLower());
        }

        public CityLanguageSet GetLanguageSetOrCreate(string language)
        {
            CityLanguageSet cls = languageFields.Find( o => o.language.ToLower() == language.ToLower());
            if(cls != null)
            {
                return cls;
            }

            //name.LogDev("create current language set for: ");
            //language.Log("creating new language set: ");
            
            cls = new CityLanguageSet();
            cls.language = UserController.Instance.GetLanguage().ToLower();
            languageFields.Add(cls);
            //UserController.Instance.SaveGame();
            return cls;
        }

        public void AddUnsuccesfullPlay()
        {
            string lang = UserController.Instance.GetLanguage().ToLower();
            if (!unsuccessfulPlays.Contains(lang))
            {
                unsuccessfulPlays.Add(lang);
            }
        }

        public bool CheckIfUnlocked(int maxUnlockedGroup)
        {
            if (maxUnlockedGroup >= GetMyGroup().id)
                return true;

            return false;
        }

        public void MarkAsPlayedToday()
        {
            string date = ServerTimeController.GetTime().FormatFull();

            //date.Log("Marking as played: ");

            GetCurrentLanguageSet().lastSuccessfullPlayDate = date;
        }

        public bool WasPlayedToday()
        {
            //GetCurrentLanguageSet().lastSuccessfullPlayDate.Log("Last s playdate: ");

            if (GetCurrentLanguageSet().lastSuccessfullPlayDate == "")
                return false;

            DateTime now = ServerTimeController.GetTime();
            
            DateTime lastSuccessfullPlayDate = ServerTimeController.TryParseOrGetNow(GetCurrentLanguageSet().lastSuccessfullPlayDate);            

            TimeSpan timeSpan = (now - lastSuccessfullPlayDate);                                 

            float daysDiff = (float)timeSpan.TotalDays;
            daysDiff = Mathf.Abs(daysDiff);

            if (daysDiff <= 1f)            
            {              
                return true;
            }

            return false;
        }

        public string GetReplayTimeoutInString()
        {
            if(GetCurrentLanguageSet().lastSuccessfullPlayDate == "")
            {
                return "";
            }

            System.DateTime now = ServerTimeController.GetTime();            
            DateTime lastSuccessfullPlayDate = ServerTimeController.TryParseOrGetNow(GetCurrentLanguageSet().lastSuccessfullPlayDate);          
            TimeSpan timeSpan = (now - lastSuccessfullPlayDate);

            timeSpan = timeSpan.Subtract(TimeSpan.FromDays(1));
                 
                      

            string timeLeft_string = "";

            int hours = Mathf.Abs(timeSpan.Hours);        
            int minutes = Mathf.Abs(timeSpan.Minutes);
            int seconds = Mathf.Abs(timeSpan.Seconds);
            

            timeLeft_string = string.Format("{0}:{1}:{2}", hours.ToString("D2"),
      minutes.ToString("D2"),
       seconds.ToString("D2"));

            return timeLeft_string + " h";
        }

        public void ReportUnsuccesfullPlay()
        {
            string todayDate = ServerTimeController.GetTime().FormatSimple();

            if (GetCurrentLanguageSet().lastRestartingDate == "")
            {
                GetCurrentLanguageSet().lastRestartingDate = ServerTimeController.GetTime().FormatFull();
            }
            
            string simplifiedDate = ServerTimeController.TryParseOrGetNow(GetCurrentLanguageSet().lastRestartingDate).FormatSimple();

            if (simplifiedDate != todayDate)
            {               
                GetCurrentLanguageSet().restartingCount = 1;
            }
            else
            {
                GetCurrentLanguageSet().restartingCount++;
            }

            GetCurrentLanguageSet().lastRestartingDate = ServerTimeController.GetTime().FormatFull();
            UserController.Instance.SaveGame();
        }

        public bool WasRestartLimitReached()
        {
            string todayDate = ServerTimeController.GetTime().FormatSimple();
            CityLanguageSet cls = GetCurrentLanguageSet();

            if (cls.lastRestartingDate == "")
                return false;
                       
            string lastRestartingDate_string = ServerTimeController.TryParseOrGetNow(cls.lastRestartingDate).FormatSimple();
           
            if (lastRestartingDate_string == todayDate && cls.restartingCount >= 3)
            {
                return true;
            }

            return false;
        }

        public string GetRestartTimeoutInString()
        {
            if(GetCurrentLanguageSet().lastRestartingDate == "")
            {
                return "";
            }

            DateTime now = ServerTimeController.GetTime();

            DateTime lastSuccessfullPlayDate = ServerTimeController.TryParseOrGetNow(GetCurrentLanguageSet().lastRestartingDate);
            TimeSpan timeSpan = (now - lastSuccessfullPlayDate);

            timeSpan = timeSpan.Subtract(TimeSpan.FromDays(1));
       //     string timeLeft_string = string.Format("{0}:{1}:{2}", Mathf.Abs(timeSpan.Hours).ToString("D2"),
       //Mathf.Abs(timeSpan.Minutes).ToString("D2"),
       //Mathf.Abs(timeSpan.Seconds).ToString("D2"));

            int hours = Mathf.Abs(timeSpan.Hours);
            int minutes = Mathf.Abs(timeSpan.Minutes);
            int seconds = Mathf.Abs(timeSpan.Seconds);

            string timeLeft_string = string.Format("{0}:{1}:{2}", hours.ToString("D2"),
     minutes.ToString("D2"),
      seconds.ToString("D2"));

            return timeLeft_string + " h";
        }

        public bool WasUnsuccesfullyPlayed()
        {
            return unsuccessfulPlays.Contains(UserController.Instance.GetLanguage().ToLower());
        }

        public void DisableGameplayCityCotroller()
        {
            if (gameplay != null && gameplay.GetComponent<CityController>() != null)
            {
                CityController controller = gameplay.GetComponent<CityController>();

                controller.DisableInEditor();
                controller.HideImages();

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(controller);
#endif
            }
        }

        private CityController controller = null;

        public CityController GetController()
        {
            if (controller == null)
                controller = gameplay.GetComponent<CityController>();

            return controller;
        }

        public int GetBestScore(string language)
        {
            if (saves.Count == 0)
                return 0;

            int maxStars = 0;

            List<SingleSave> languageSaves = saves.FindAll(o => o.language.ToLower() == language.ToLower());

            foreach (SingleSave ss in languageSaves)
            {
                if (ss.stars > maxStars)
                    maxStars = ss.stars;
            }

            return maxStars;
        }

        //public int GetBestScoreSave(string language)
        //{
        //    if (saves.Count == 0)
        //        return 0;

        //    int maxStars = 0;

        //    List<SingleSave> languageSaves = saves.FindAll(o => o.language.ToLower() == language.ToLower());

        //    foreach (SingleSave ss in languageSaves)
        //    {
        //        if (ss.stars > maxStars)
        //            maxStars = ss.stars;
        //    }

        //    return maxStars;
        //}

        public struct BestScorePair
        {
           public int stars;
            public int maxStars;
            public int mode;
        }

        public BestScorePair GetBestScorePair(string language)
        {
            BestScorePair bsp = new BestScorePair() { stars = 0, maxStars = 0, mode = -1 };
         
            if (saves.Count == 0)
                return bsp;
                     

            List<SingleSave> languageSaves = saves.FindAll(o => o.language.ToLower() == language.ToLower());

            foreach (SingleSave ss in languageSaves)
            {
                if (ss.stars > bsp.stars)
                {
                    bsp.maxStars = ss.GetMaxPossibleStars();
                    bsp.stars = ss.stars;
                }
            }

            return bsp;
        }

        public SingleSave FirstTopScorePair(string language)
        {           
            if (saves.Count == 0)
                return null;


            List<SingleSave> languageSaves = saves.FindAll(o => o.language.ToLower() == language.ToLower());

            foreach (SingleSave ss in languageSaves)
            {
                if (ss.stars == ss.GetMaxPossibleStars())
                {
                    return ss;
                }
            }

            return null;
        }

        public int GetAllStars(string language)
        {
            if (saves.Count == 0)
                return 0;

            int maxStars = 0;

            List<SingleSave> languageSaves = saves.FindAll(o => o.language.ToLower() == language.ToLower());

            foreach (SingleSave ss in languageSaves)
            {               
                    maxStars += ss.stars;
            }

            return maxStars;
        }

        public int GetBestPoints(string language)
        {
            if (saves.Count == 0)
                return 0;

            int maxPoints = 0;

            List<SingleSave> languageSaves = saves.FindAll( o => o.language.ToLower() == language.ToLower());

            foreach (SingleSave ss in languageSaves)
            {
                if (ss.score > maxPoints)
                    maxPoints = ss.score;
            }

            return maxPoints;
        }

        [System.NonSerialized]
        private CountryGroup _myGroup = null;

        public CountryGroup GetMyGroup()
        {
            if (_myGroup == null)
            {
                if (groupID > -1 && groupSource == GroupSource.DIFFERENT)
                    _myGroup= ElementsDatabase.Instance.groups.Find(o => o.id == groupID);
                else
                    _myGroup = ElementsDatabase.Instance.GetCountryGroupByName(ElementsDatabase.Instance.GetCountryByCity(this).groupName);
            }          

            return _myGroup;

        }

        public TextOverlayController GetTextOverlayController()
        {
            TextOverlayController toc = TextDeconstructionController.Instance.GetCity(ElementsDatabase.Instance.GetCountryByCity(this).name, name);
            
            return toc;
        }

        public int GetCompleteRunsCount(string language)
        {
            //List<SingleSave> languageSaves = saves.FindAll(o => o.language.ToLower() == language.ToLower());
            return GetLanguageSetOrCreate(language).succesfullRuns ;
        }

        public float GetScoreTarget(string language)
        {
            int runs = GetCompleteRunsCount(language);

            float scoreToBeat = 0;

            if (runs > 3)
            {
                int lastScore = GetBestPoints(language);
                scoreToBeat = 0;
                switch (runs)
                {
                    case 4:
                        scoreToBeat = (float)lastScore * 1.15f;
                        break;
                    case 5:
                        scoreToBeat = (float)lastScore * 1.2f;
                        break;

                    case 6:
                        scoreToBeat = (float)lastScore * 1.25f;
                        break;
                }
            }

            return scoreToBeat;
        }

        public bool CheckResult(int points)
        {
            CountryGroup group = GetMyGroup();

            int runs = GetCompleteRunsCount(UserController.Instance.GetLanguage());



            if (group.id > 1 && runs == 0)
            {
                //"Star - free run".Log();
                return true;
            }

            if(group.id == 1)
            {
                // free run
                if (runs == 0)
                {
                    //"Star - free run".Log();
                    return true;
                }
                  

                // distractors
                if(runs > 0 && runs < 4)
                {
                    //"Star - distractor".Log();
                    return true;
                }

                if(runs==4 && GetScoreTarget(UserController.Instance.GetLanguage()) <= points)
                {
                    //"Star - You beat the score".Log();
                    return true;
                }


                if (runs == 5 || runs == 6)
                {
                    //"Star - Free stars".Log();
                    return true;
                }

            }
         
            return false;
        }

        public bool CheckIfStarsAvailable()
        {
            //CountryGroup group = GetMyGroup();
            //int runs = GetCompleteRunsCount(UserController.Instance.GetLanguage());
            ////runs.Log("R: ");
          
            if(type == CitiesGameplayType.zooming)
                return GetCurrentLanguageSet().succesfullRuns <= 5;


            return GetCurrentLanguageSet().succesfullRuns <= 3;

            //if (group == null)
            //{
            //    name.Log("Missing group: ");
            //}
            //if (group.id > 1)
            //{      
            //    if(runs == 0 && (!WasUnsuccesfullyPlayed() || (WasUnsuccesfullyPlayed() && UserController.Instance.GetStars()>0)))
            //        return true;
            //}

            //if(group.id == 1 && runs< 7)
            //{
            //    return true;
            //}

            //return false;
        }

        private GameplayScenario scenario = null;

        public GameplayScenario GetScenario()
        {
            // get scenario
            if (scenario != null)
            {
                //scenario.id.Log("Id scenario: ");
                return scenario;
            }            
          
             if(GetMyGroup().scenarioID > -1)
            {
                if (scenario == null)
                    scenario = ElementsDatabase.Instance.scenarios.Find(o => o.id == GetMyGroup().scenarioID);
            }

            if(scenario == null)
            {
                scenario = ElementsDatabase.Instance.scenarios[0];
                Debug.LogWarning("Missing scenario for " + name);
            }
            
            return scenario;
        }
    }

    [System.Serializable]
    public class BundleInfo
    {
        public bool requireBundle = true;
        public bool hasBundle = false;
        public string bundleName = "";
        public string imageName = "";
        public float bundleSize = 0f;

        public List<BundleEntry> bundles = new List<BundleEntry>();

        public string GetBundleName()
        {
            if(bundles.Count>0 && bundles.Last() != null && !string.IsNullOrEmpty(bundles.Last().name))
            {
                return bundles.Last().name;
            }

            return bundleName;
        }

        public float GetBundleSize()
        {
            if (bundles.Count > 0 && bundles.Last() != null)
            {
                return bundles.Last().size;
            }

            return bundleSize;
        } 

        public string GetImageName()
        {
            if (bundles.Count > 0 && bundles.Last() != null)
            {
                return bundles.Last().imageName;
            }

            return imageName;
        }

        public void DeleteOldBundles()
        {
            //"deleting old bundles".Log();
            if (bundles.Count > 0 && bundles.Last() != null && !string.IsNullOrEmpty(bundles.Last().name))
            {
                List<BundleEntry> entries = new List<BundleEntry>(bundles);
                entries.Remove(bundles.Last());

                if (entries.Count > 0)
                {
                    //"something to remove".LogDev();
                    entries.ForEach( o=> RemoveBundle(o.name));
                }

                RemoveBundle(bundleName);
            }
        }

        public void RemoveBundle(string bundleName) {

          
            if (string.IsNullOrEmpty(bundleName))
                return;

            string bundlesPath = DKK.AssetsBundleManager.Instance.bundleDirectory;
            string bundle = System.IO.Path.Combine(bundlesPath, bundleName);

            bundle.LogDev("Removing: ");
            //return;

            if (System.IO.File.Exists(bundle))
            {
                System.IO.File.Delete(bundle);
            }
        }

        [System.Serializable]
        public class BundleEntry
        {
            public string name = "";
            public float size = 0f;
            public string imageName = "";
        }
    }

    [System.Serializable]
    public class DragableElements
    {
        public string ID;
        public Sprite sprite;

        public Sprite spriteColorfull;

        public Sprite spriteFilled;

        public int angleIncrementation = 90;

        public GameObject spotPrefab;
    }

    [System.Serializable]
    public class CountryGroup
    {
        public GroupSerializedFields serializedFields;
        public bool cooloff = false;
        public int cooloffTime = 0;

        public string groupName;
        public Color groupColor;
        public int scenarioID = -1;
        public int id;
        public int ticketPrice = 2;

        public Sprite boosterInfoSprite;

        public List<Cities> cities = new List<Cities>();       
        
        [System.Serializable]
        public class GroupSerializedFields
        {            
            //[HideInInspector]
            public bool coolOffDone = false;
            public string coolOffLeft;
        }

        public bool HasCoolOff()
        {
            if (string.IsNullOrEmpty(serializedFields.coolOffLeft) || !cooloff)
                return false;

            System.DateTime now = ServerTimeController.GetTime();
            DateTime cooloffDate = ServerTimeController.TryParseOrGetNow(serializedFields.coolOffLeft);

            if (now > cooloffDate)
                return false;

            return true;
        }

        public bool IsCoolOffSetUp()
        {
            return !string.IsNullOrEmpty(serializedFields.coolOffLeft);
        }

        public string GetCooloffLeft()
        {
            if (string.IsNullOrEmpty(serializedFields.coolOffLeft))
                return "";

            System.DateTime now = ServerTimeController.GetTime();

            DateTime cooloffDate = ServerTimeController.TryParseOrGetNow(serializedFields.coolOffLeft);

            TimeSpan timeSpan = (cooloffDate - now);

            //timeSpan = timeSpan.Subtract(TimeSpan.FromDays(1));


            string timeLeft_string = "";

            int hours = Mathf.Abs(timeSpan.Hours);
            int minutes = Mathf.Abs(timeSpan.Minutes);
            int seconds = Mathf.Abs(timeSpan.Seconds);


            timeLeft_string = string.Format("{0}:{1}:{2}", hours.ToString("D2"),
      minutes.ToString("D2"),
       seconds.ToString("D2"));

            return timeLeft_string + " h";
        }
    }

    public class CityLanguageSet
    {
        public string language;

        public bool wasBought = false;

        public int succesfullRuns = 0;
        public int allRuns = 0;        

        public string lastSuccessfullPlayDate = "";
        public int allStars = 0;

        public bool isNew = true; // unchecked

        public string allBlinkingUsedDate = "";
        public int blinkingUsageStatus = 0;

        public string lastRestartingDate;
        public int restartingCount = 0;

        public static bool IsInitialized(CityLanguageSet cls)
        {
            if (cls.allRuns > 0)
                return true;

            if (cls.succesfullRuns > 0)
                return true;

            if (cls.lastSuccessfullPlayDate != "")
                return true;

            if (cls.isNew != true)
                return true;
            
            if (cls.wasBought != false)
                return true;

            if (cls.allBlinkingUsedDate != "")
                return true;

            if (cls.blinkingUsageStatus != 0)
                return true;

            if (!string.IsNullOrEmpty(cls.lastRestartingDate))
                return true;

            if (cls.restartingCount != 0)
                return true;

            return false;
        }
    }

    public class SingleSave
    {
        public string language;
        public int score;
        public string date;
        public int stars;
        public SuccessScreenController.ScoreStatistics stats = null;

        public SingleSave(int score, int stars, string language, SuccessScreenController.ScoreStatistics stats = null)
        {
            this.stars = stars;
            this.score = score;
            this.date = ServerTimeController.GetTime().FormatFull();
            this.language = language;
            this.stats = stats;

            this.date.Log("Single save date: ");
        }

        public SingleSave(int score, int stars, string date, string language, SuccessScreenController.ScoreStatistics stats = null)
        {
            //"Creating save".Log();
            this.stars = stars;
            this.score = score;
            this.date = ServerTimeController.TryParseOrGetNow(date).FormatFull();
            this.language = language;
            this.stats = stats;
        }        

        public int GetMaxPossibleStars()
        {
            int maxStars = 0;
            if (stats != null)
            {
                if (stats.engagingMax >= 0)
                    maxStars += stats.engagingMax;

                if (stats.relaxingMax >= 0)
                    maxStars += stats.relaxingMax;

                if (stats.entertainingMax >= 0)
                    maxStars += stats.entertainingMax;
            }

            return maxStars;
        }

        public bool GotMaxPointsForMode(int mode)
        {
            switch (mode)
            {                
                case 1:
                    //eng
                    if (stats.engagingScore > 0 && stats.engagingScore == stats.engagingMax)
                        return true;
                    break;
                case 2:
                    //ent
                    if (stats.entertainingScore > 0 && stats.entertainingScore == stats.entertainingMax)
                        return true;
                    break;
                case 3:
                    //rel
                    if (stats.relaxingScore > 0 && stats.relaxingScore == stats.relaxingMax)
                        return true;
                    break;
            }

            return false;
        }
    }

    [System.Serializable]
    public class GameplayScenario
    {
        //public enum ScoringType {
        //    simple = 0,
        //    notSimple = 1
        //};

        public string scenarioName;
        public int id;
        public bool highlightedAreas = false;
        public float draggingTime = 5f;
        public bool randomizationDragables = false;
        public bool movesReward = false;
        public bool decreasedHighlightedAreas = false;
        public bool animationOnAny = true;
        //public ScoringType scoringType = ScoringType.simple;
    }

    public enum ParametersInheritance
    {
        Inherit = 0,
        Override = 1
    }
    public enum Language
    {
        English = 1,
        Chinese = 2
    }

    public enum CitiesGameplayType
    {
        tutorial=0,
        regular = 1,
        zooming = 2
    }

    [System.Serializable]
    public class GameMode
    {
        public string name;
        public int id;
        public Color color;
        public float rotationSpeedMultiplier = 1f;
        public Sprite overlayForHashtag;

        public List<GameModeLanguageObject> languageObjects = new List<GameModeLanguageObject>();

        public Sprite GetHashtagOverlaySprite()
        {
            return languageObjects.Find( o=> o.language.ToString() == UserController.Instance.GetLanguage
            ()).overlayForHashtag;
        }

        public string GetModeIsOnMessage()
        {
            return TextTranslationModule.GetWord(name+ " mode is ON");
            //string translatedWord = TextTranslationModule.GetWord(name);
            //return string.Format(TextTranslationModule.GetWord("{0} mode is ON"), translatedWord);
        }

        public static void DisplayExplentation(int mode)
        {
            mode.LogDev("Display explenation: ");
            if (mode != 0)
            {
                //PlayerPrefs.SetInt("mode_used_" + mode + "_" + lang, 1);
                switch (mode)
                {
                    case 1:
                        ModesDescriptionScreen.Instance.Engaging();
                        //ModesDescriptionScreen.Instance.AddViewToClose(externalContext);
                        break;
                    case 2:
                        ModesDescriptionScreen.Instance.Entertaining();
                        //ModesDescriptionScreen.Instance.AddViewToClose(externalContext);
                        break;

                    case 3:
                        ModesDescriptionScreen.Instance.Relaxing();
                        //ModesDescriptionScreen.Instance.AddViewToClose(externalContext);
                        break;
                }

            }
        }

        public static void DisplayPopUpExplenation(int mode)
        {
            if (mode != 0)
            {
                FloatingTextPanel.Instance.Show(
                             new FloatingTextParameters()
                             {
                                 text = Elements.ElementsDatabase.Instance.GetGameModeById(mode).GetModeIsOnMessage(),
                                 time = 3f,
                                 generalClickAction = delegate () { FloatingTextPanel.Instance.Hide(); } 
                             }
                             );
            }
        }
    }

    [System.Serializable]
    public class GameModeLanguageObject
    {
        public Language language;
        public Sprite overlayForHashtag;
    }
}

