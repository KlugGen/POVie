using Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
//using Firebase;
//using Firebase.Unity.Editor;


public class UserController : Singleton<UserController>
{
    public string fileName = "saves.json";

    public int maxUnlockdedGroup = -2;
    public int gameMode = 0;
    public int additionalPoints = 10;

    public bool hasNewCities = false;
    public bool logInReward = false;

    public Dictionary<string, UserStatistics> statistics = new Dictionary<string, UserStatistics>();
    public UserGeneralInformation generalInfo = new UserGeneralInformation();

    public List<Text> starTexts = new List<Text>();

    public GameModeDependencies gameModeDependencies;

    private void Awake()
    {        
        BuildBundlesToDownload(true);
    }

    public void LoadStatistics(string str)
    {
        statistics = UserStatistics.LoadStatistics(str);
    }

    public void LoadStatisticsAndSaves()
    {      
        gameMode = GetGameModeID();
        InitializeGameModeDependencies();
        MaxUnlockedGroup();

        ActualizeStarTexts();
#if !UNITY_EDITOR
                       
        Dictionary<string, object> customParams = new Dictionary<string, object>();
        customParams.Add("star_balance", UserController.Instance.GetStars());
        AnalyticsEvent.GameStart(customParams);
#endif
    }

    #region GameMode

    public JSONObject GetStatisticsJson()
    {
        return UserStatistics.GetStatisticsJson(statistics);
    }

    public JSONObject GetGeneralInformation()
    {
        return new JSONObject(JsonUtility.ToJson(generalInfo));
    }

    public void LoadGeneralInformation(string str)
    {
        if (string.IsNullOrEmpty(str))
            generalInfo = new UserGeneralInformation();

        generalInfo = JsonUtility.FromJson<UserGeneralInformation>(str);
    }

    public int GetGameModeID()
    {
        if (FirebaseDatabaseController.Instance.IsStatisticLoaded())
            return UserController.Instance.GetStats().GetInt("gameMode", 0);
        else
        {
            "save not loaded".Log();
            return 0;
        }
    }

    public struct UsersGameplayStats
    {
        public int engaging, entertaining, relaxing;
        public int counter;
        public int citiesCount;
    }

    public UsersGameplayStats GetCurrentStatisticsForScoreboard()
    {
        var cities = Elements.ElementsDatabase.Instance.GetAllDoneCitiesChallenging().ToList();

        UsersGameplayStats stats = new UsersGameplayStats() { engaging = 0, entertaining = 0, relaxing = 0, counter = 0, citiesCount =0 };
        stats.citiesCount = cities.Count;

        foreach (Cities city in cities)
        {
            SingleSave save = city.FirstTopScorePair(UserController.Instance.GetLanguage());
            if (save != null && save.stars > 0)
            {
                stats.counter++;
                switch (save.stats.gameMode)
                {
                    case 0:
                        if (save.GotMaxPointsForMode(1))
                        {
                            //city.name.LogDev("eng+ for ");
                            stats.engaging++;
                        }

                        if (save.GotMaxPointsForMode(2))
                        {
                            //city.name.LogDev("ent+ for ");
                            stats.entertaining++;
                        }

                        if (save.GotMaxPointsForMode(3))
                        {
                            //city.name.LogDev("rel+ for ");
                            stats.relaxing++;
                        }
                        break;
                    case 1:
                        //eng
                        if (save.GotMaxPointsForMode(1))
                        {
                            //city.name.LogDev("eng+ for ");
                            stats.engaging += 2;
                        }
                        break;
                    case 2:
                        //ent
                        if (save.GotMaxPointsForMode(2))
                        {
                            //city.name.LogDev("ent+ for ");
                            stats.entertaining += 2;
                        }
                        break;
                    case 3:
                        //rel

                        if (save.GotMaxPointsForMode(3))
                        {
                            //city.name.LogDev("rel+ for ");
                            stats.relaxing += 2;
                        }
                        break;

                }
            }
        }

        return stats;
    }

    public int CNT = -1;

    public int GetTopScores()
    {
#if UNITY_EDITOR
        if (CNT > -1)
            return CNT;

#endif

        return GetCurrentStatisticsForScoreboard().counter;
    }

    public GameMode GetGameMode()
    {
        return ElementsDatabase.Instance.GetGameModeById(GetGameModeID());
    }

    public void SetGameMode(int mode)
    {
        if (GetGameModeID() == mode)
            return;

        UserController.Instance.GetStats().GetInt("gameMode", mode);
        GetStats().gameMode = mode;
        GetStats().AddInt("gameMode", mode);
        SaveStatistics();
        InitializeGameModeDependencies();
    }

    private void InitializeGameModeDependencies()
    {
        int gameMode = GetGameModeID();

        GameMode mode = ElementsDatabase.Instance.GetGameModeById(gameMode);

        gameModeDependencies.images.ForEach(delegate (Image i)
        {
            i.color = mode.color;
        });

        gameModeDependencies.texts.ForEach(delegate (Text i)
        {
            i.color = mode.color;
        });
    }

    #endregion

    public int GetStars()
    {
        string lang = GetLanguage();
        if (!statistics.ContainsKey(lang))
        {
            statistics.Add(lang, new UserStatistics(lang));
            UserStatistics.SaveStatistics(statistics);
        }

        return statistics[lang].stars;
    }

    public UserStatistics GetStats()
    {
        string lang = GetLanguage();
        if (!statistics.ContainsKey(lang))
        {
            statistics.Keys.ToList().ForEach(o => o.LogDev("Has: "));
            statistics.Keys.ToList().ForEach(o => System.Text.Encoding.Unicode.GetString(System.Text.Encoding.Default.GetBytes(o)).LogDev("Hasu: "));
            
           lang.LogDev("adding statistics: ");
            statistics.Add(lang, new UserStatistics(lang));
            UserStatistics.SaveStatistics(statistics);
        }

        return statistics[lang];
    }

    public void AddStars(int amount, string title = "", bool showAnimation = false)
    {

        int oldStars = GetStats().stars;

        if (amount > 0)
        {
            GetStats().stars = GetStars() + amount;
            if (showAnimation)
                FloatingPointPanelController.Instance.Show(oldStars, GetStats().stars, amount);
        }

        if (!string.IsNullOrEmpty(title))
        {
#if !UNITY_EDITOR
                       
            Dictionary<string, object> customParams = new Dictionary<string, object>();
            customParams.Add("user_id", SystemInfo.deviceUniqueIdentifier);
            customParams.Add("amount", amount);
          
            AnalyticsEvent.Custom(title, customParams);
#endif

            Firebase.Analytics.Parameter[] parameters = new Firebase.Analytics.Parameter[] {
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterVirtualCurrencyName, title),
                   new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, amount),
            };

            AnalyticsController.Instance.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventEarnVirtualCurrency, parameters);
        }

        UserStatistics.SaveStatistics(statistics);

        ActualizeStarTexts();
    }

    public void SaveStatistics()
    {
        UserStatistics.SaveStatistics(statistics);
    }

    public void ActualizeStarTexts()
    {
        int s = GetStars();

        starTexts.ForEach(delegate (Text t)
        {
            if (t != null)
            {
                t.text = s.ToString();
            }
        });
    }

    public bool AnyPointsWereGiven()
    {
        return UserController.Instance.GetStats().HasDictionary("starsGiven" + GetLanguage());
    }

    public void RemoveStars(int amount, string purchaseTitle = "", string param = "", bool showAnimation = false)
    {
        //amount.LogDev("Removing stars for " + purchaseTitle+ " : ");

        int oldStars = GetStats().stars;

        if (amount > 0)
        {
            GetStats().stars = GetStars() - amount;

            if (showAnimation)
                FloatingPointPanelController.Instance.Show(oldStars, GetStats().stars, -amount);
        }



        UserStatistics.SaveStatistics(statistics);

        int stars = GetStars();

        if (UserController.Instance.GetStats().HasDictionary("oneBonusAdd") == false)
        {
            if (stars <= 0)
            {
                UserController.Instance.GetStats().AddInt("oneBonusAdd", 1);
                AddStars(10, "stars_one_time_bonus");
                OneTimeBonusScreen.Instance.Enable(10);
                ViewsController.Instance.AddToCurrentContextToClose(OneTimeBonusScreen.Instance);
            }
        }

        if (!string.IsNullOrEmpty(purchaseTitle))
        {
#if !UNITY_EDITOR
                       
            Dictionary<string, object> customParams = new Dictionary<string, object>();
            if(param != ""){
              customParams.Add("name", param);
            }
            customParams.Add("user_id", SystemInfo.deviceUniqueIdentifier);
            customParams.Add("amount", amount);
            AnalyticsEvent.Custom(purchaseTitle, customParams);
#endif

            //Firebase.Analytics.Parameter[] parameters = new Firebase.Analytics.Parameter[] {
            //    new Firebase.Analytics.Parameter("title", purchaseTitle),
            //    new Firebase.Analytics.Parameter("param", param)                
            //};

            Firebase.Analytics.Parameter[] parameters = new Firebase.Analytics.Parameter[] {
                 new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterItemName, ""),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterVirtualCurrencyName, purchaseTitle),
                   new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, amount),
            };

            AnalyticsController.Instance.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventSpendVirtualCurrency, parameters);
        }

        ActualizeStarTexts();
    }

    public int MaxUnlockedGroup()
    {
        int maxUnlockedId = -1;

        foreach (CountryGroup cg in ElementsDatabase.Instance.groups.OrderBy(x => x.id))
        {
            List<Cities> cities = ElementsDatabase.Instance.GetCitiesByGroup(cg.id);

            bool undoneFound = false;

            cities.ForEach(delegate (Cities c)
            {
                if (!c.IsDone() && c.IsReady())
                {
                    undoneFound = true;
                }
            });

            maxUnlockedId = cg.id;

            if (undoneFound)
            {
                break;
            }
        }

        maxUnlockdedGroup = maxUnlockedId;

        return maxUnlockedId;
    }

    private string SavesPath()
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    private void ResetStatistics()
    {
        statistics = new Dictionary<string, UserStatistics>();
    }

    private void ResetGroups()
    {
        foreach(Elements.CountryGroup group in Elements.ElementsDatabase.Instance.groups)
        {
            group.serializedFields = new CountryGroup.GroupSerializedFields();
        }
    }

    private void ResetGeneral()
    {
        generalInfo = new UserGeneralInformation();
    }

    private void ResetCitiesParameters()
    {
        ElementsDatabase.Instance.groups.ForEach(o => o.cities.Clear());

        foreach (Countries country in ElementsDatabase.Instance.countries)
        {
            country.isNew = true;

            foreach (Cities city in country.cities)
            {
                city.wasInSaves = false;
                city.languageFields.Clear();
                city.unsuccessfulPlays.Clear();
                city.saves.Clear();
            }
        }
    }

    public void LoadSaves(string savesExternal = null)
    {
        //List<string> bundlesToDownload = new List<string>();

        //savesExternal.Log("Loadign saves from string: ");

        ResetCitiesParameters();

        string savesText = "";

        if (savesExternal == null)
            savesText = DKK.IO.ReadFromFile(SavesPath());
        else
        {
            //savesExternal.Log("External: ");
            savesText = savesExternal;
        }

        if (savesText != null)
        {
            JSONObject saves = new JSONObject(savesText);

            if (saves != null && saves.list != null)
            {
                //saves.list.Count.Log("List count: ");
                foreach (JSONObject country in saves.list)
                {
                    if (!country.HasField("name"))
                    {
                        "No name".Log();
                        continue;
                    }

                    string name = country["name"].str;

                    //name.Log("Name found: ");

                    if (!country.HasField("isNew"))
                    {
                        "Doesn't have new".Log();
                    }

                    Countries countryObj = ElementsDatabase.Instance.GetCountryByName(name);
                    if (countryObj != null)
                    {
                        countryObj.isNew = country["isNew"].b;
                        if (country.HasField("cities") && country["cities"].list != null)
                        {
                            foreach (JSONObject city in country["cities"].list)
                            {
                                Cities cityObj = countryObj.GetCityByName(city["name"].str);
                                if (cityObj == null)
                                    continue;

                                cityObj.wasInSaves = true;
                                cityObj.country = countryObj;

                                CountryGroup group = ElementsDatabase.Instance.groups.Find(o => o.id == cityObj.GetMyGroup().id);
                                if (group != null)
                                {
                                    if (!group.cities.Contains(cityObj))
                                        group.cities.Add(cityObj);
                                }

                                if (cityObj != null)
                                {
                                    if (city.HasField("languageFields") && city["languageFields"].IsArray)
                                    {
                                        //cityObj.languageFields.Count.Log("Before loading: ");
                                        foreach (JSONObject unit in city["languageFields"].list)
                                        {
                                            CityLanguageSet cls1 = JsonUtility.FromJson<CityLanguageSet>(unit.ToString());
                                            cityObj.languageFields.Add(cls1);

                                            #region OldSaveMethod
                                            continue;

                                            CityLanguageSet cls = new CityLanguageSet();

                                            if (unit.HasField("isNew"))
                                            {
                                                cls.isNew = unit["isNew"].b;
                                            }

                                            if (unit.HasField("wasBought"))
                                            {
                                                cls.wasBought = unit["wasBought"].b;
                                            }

                                            if (unit.HasField("lastSuccessfullPlayDate"))
                                            {
                                                cls.lastSuccessfullPlayDate = unit["lastSuccessfullPlayDate"].str;
                                            }

                                            if (unit.HasField("allRuns"))
                                            {
                                                cls.allRuns = (int)unit["allRuns"].f;
                                            }

                                            if (unit.HasField("allStars"))
                                            {
                                                cls.allRuns = (int)unit["allStars"].f;
                                            }

                                            if (unit.HasField("succesfullRuns"))
                                            {
                                                cls.succesfullRuns = (int)unit["succesfullRuns"].f;
                                            }

                                            if (unit.HasField("language"))
                                            {
                                                cls.language = unit["language"].str;
                                                cityObj.languageFields.Add(cls);
                                            }

                                            #endregion
                                        }
                                    }

                                    if (city.HasField("unsuccessfulPlays") && city["unsuccessfulPlays"].IsArray)
                                    {
                                        foreach (JSONObject unit in city["unsuccessfulPlays"].list)
                                        {
                                            cityObj.unsuccessfulPlays.Add(unit.str);
                                        }
                                    }

                                    if (city.HasField("saves") && city["saves"].IsArray)
                                    {
                                        foreach (JSONObject save in city["saves"].list)
                                        {
                                            //if (save.HasField("stars") && save.HasField("score") && save.HasField("date"))
                                            if (save.HasFields(new string[] { "stars", "score", "date" }))
                                            {
                                                string lang = "";
                                                if (save.HasField("language"))
                                                    lang = save["language"].str;


                                                SingleSave singleSave = JsonUtility.FromJson<SingleSave>(save.ToString());
                                                //SingleSave singleSave = new SingleSave((int)save["score"].f, (int)save["stars"].f, save["date"].str, lang);
                                                cityObj.saves.Add(singleSave);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                "not an array".Log();
            }
        }
        else
        {
            "null".Log();
        }

        //BuildBundlesToDownload(prepareAssets);       

        //Elements.ElementsDatabase.Instance.allCities.Count.LogDev("Cities: ");

        //if (PlayerPrefs.HasKey("newCitiesFirstCheck") == false)
        //{
        //    //"no cities first check key".LogDev();
        //    PlayerPrefs.SetInt("newCitiesFirstCheck", 1);
        //    Elements.ElementsDatabase.Instance.allCities.ForEach(o => o.wasInSaves = true);
        //    UserController.Instance.SaveGame();
        //    UserController.Instance.hasNewCities = false;
        //}
        //else
        //{
        //    if (Elements.ElementsDatabase.Instance.allCities.Find(o => o.wasInSaves == false) != null)
        //    {
        //        hasNewCities = true;
        //        //"has new cities".LogDev();
        //    }
        //    else
        //    {
        //       // "no new cities".LogDev();
        //    }
        //}

        //if (prepareAssets)
        //{
        //    string s = bundlesToDownload.Count.ToString();
        //    bundlesToDownload.ForEach(o => s += "\n" + o);
        //    s.LogDev();
        //}
    }

    public void LogOut()
    {
        ResetCitiesParameters();
        ResetStatistics();
        ResetGeneral();
        ResetGroups();
    }

    public void BuildBundlesToDownload(bool prepareAssets)
    {
        foreach (Countries c in Elements.ElementsDatabase.Instance.countries)
        {
            foreach (Cities ci in c.cities)
            {
                if (ci.IsReady())
                    Elements.ElementsDatabase.Instance.allCities.Add(ci);

                if (prepareAssets && ci.bundle.requireBundle && ci.IsReady())
                {
                    //string bundleName = string.Format("{0}.{1}", name.ToLower(), cityObj.name.ToLower());
                    bool hasBudle = DKK.AssetsBundleManager.Instance.Exists(ci.bundle.GetBundleName());
                    if (!hasBudle)
                    {
                        ci.bundle.DeleteOldBundles();
                        AssetBundleUnitInfo abui = new AssetBundleUnitInfo(ci.bundle.GetBundleName(), ci.bundle.GetBundleSize());
                        AssetBundleDownloader.Instance.AddAssetToQueue(abui);                       
                    }
                }

                if (ci.country == null)
                    ci.country = c;
            }
        }
    }

    internal float GetGameModeMultiplier()
    {
        // todo: define multiplier for distractors
        int gameMode = GetGameModeID();
        if (gameMode == 0)
            return 1f;

        return 1.2f;
    }

    public void SaveGame()
    {
        if (!FirebaseDatabaseController.Instance.IsSaveLoaded())
        {
            "SAVES NOT LOADED - Skip saving".Log();
            return;
        }

        FirebaseDatabaseController.Instance.SaveGame();
        return;
        JSONObject countriesJson = new JSONObject();

        foreach (Countries c in ElementsDatabase.Instance.countries)
        {
            JSONObject countryJson = new JSONObject();
            countryJson.AddField("name", c.name);
            countryJson.AddField("isNew", c.isNew);
            JSONObject citiesJson = new JSONObject();

            foreach (Cities city in c.cities)
            {
                JSONObject cityJson = new JSONObject();
                cityJson.AddField("name", city.name);

                JSONObject languageFields = new JSONObject();
                JSONObject languageFastFields = new JSONObject();

                foreach (CityLanguageSet cls in city.languageFields)
                {
                    languageFields.Add(new JSONObject(JsonUtility.ToJson(cls)));
                    /*
                    JSONObject languageField = new JSONObject();
                    languageField.AddField("language", cls.language);
                    languageField.AddField("wasBought", cls.wasBought);
                    languageField.AddField("isNew", cls.isNew);
                    languageField.AddField("allRuns", cls.allRuns);
                    languageField.AddField("succesfullRuns", cls.succesfullRuns);
                    languageField.AddField("lastSuccessfullPlayDate", cls.lastSuccessfullPlayDate);
                    languageField.AddField("allStars", cls.allStars);                    
                    languageFields.Add(languageField);
                    */


                }

                cityJson.AddField("languageFields", languageFields);
                //cityJson.AddField("languageFastFields", languageFastFields);

                JSONObject unsucsesfullPlays = new JSONObject();

                foreach (string s in city.unsuccessfulPlays)
                {
                    unsucsesfullPlays.Add(s);
                }

                cityJson.AddField("unsuccessfulPlays", unsucsesfullPlays);

                JSONObject saves = new JSONObject();

                foreach (SingleSave singleSave in city.saves)
                {
                    JSONObject savesJson = new JSONObject();
                    savesJson.AddField("stars", singleSave.stars);
                    savesJson.AddField("score", singleSave.score);
                    savesJson.AddField("language", singleSave.language);
                    savesJson.AddField("date", singleSave.date);
                    saves.Add(savesJson);
                }

                cityJson.AddField("saves", saves);
                citiesJson.Add(cityJson);
            }

            countryJson.AddField("cities", citiesJson);
            countriesJson.Add(countryJson);
        }

        DKK.IO.SaveToFile(SavesPath(), countriesJson.ToString());
    }

    public JSONObject GetSavesInJson()
    {
        JSONObject countriesJson = new JSONObject();

        foreach (Countries c in ElementsDatabase.Instance.countries)
        {
            JSONObject countryJson = new JSONObject();
            countryJson.AddField("name", c.name);
            countryJson.AddField("isNew", c.isNew);
            JSONObject citiesJson = new JSONObject();

            foreach (Cities city in c.cities)
            {
                JSONObject cityJson = new JSONObject();
                cityJson.AddField("name", city.name);

                JSONObject languageFields = new JSONObject();
                JSONObject languageFastFields = new JSONObject();

                foreach (CityLanguageSet cls in city.languageFields)
                {
                    if (CityLanguageSet.IsInitialized(cls))
                    {
                        //city.name.Log("Initialized for: ");
                        languageFields.Add(new JSONObject(JsonUtility.ToJson(cls)));
                    }
                    else
                    {
                        if (city.name.ToLower() == "mdina")
                            city.name.LogDev("City skipped: ");
                    }
                    /*
                    JSONObject languageField = new JSONObject();
                    languageField.AddField("language", cls.language);
                    languageField.AddField("wasBought", cls.wasBought);
                    languageField.AddField("isNew", cls.isNew);
                    languageField.AddField("allRuns", cls.allRuns);
                    languageField.AddField("succesfullRuns", cls.succesfullRuns);
                    languageField.AddField("lastSuccessfullPlayDate", cls.lastSuccessfullPlayDate);
                    languageField.AddField("allStars", cls.allStars);                    
                    languageFields.Add(languageField);
                    */
                }

                cityJson.AddField("languageFields", languageFields);
                //cityJson.AddField("languageFastFields", languageFastFields);

                JSONObject unsucsesfullPlays = new JSONObject();

                foreach (string s in city.unsuccessfulPlays)
                {
                    unsucsesfullPlays.Add(s);
                }

                cityJson.AddField("unsuccessfulPlays", unsucsesfullPlays);

                JSONObject saves = new JSONObject();

                foreach (SingleSave singleSave in city.saves)
                {
                    //JSONObject savesJson = new JSONObject();
                    //savesJson.AddField("stars", singleSave.stars);
                    //savesJson.AddField("score", singleSave.score);
                    //savesJson.AddField("language", singleSave.language);
                    //savesJson.AddField("date", singleSave.date);
                    //saves.Add(savesJson);
                    saves.Add(new JSONObject(JsonUtility.ToJson(singleSave)));
                }

                cityJson.AddField("saves", saves);

                citiesJson.AddField(city.name, cityJson);
                //citiesJson.Add(cityJson);
            }

            countryJson.AddField("cities", citiesJson);

            countriesJson.AddField(c.name, countryJson);
            //countriesJson.Add(countryJson);
        }

        return countriesJson;
    }

    public void LoadGroups(string data = "")
    {
        if (string.IsNullOrEmpty(data))
            return;

        JSONObject groups = new JSONObject(data);

        if (groups != null && groups.list != null)
        {
            //groups.list.Count.Log("Groups count: ");
            foreach (JSONObject group in groups.list)
            {
                if (group.HasField("id") && group.HasField("field"))
                {
                    int gID = (int)group["id"].i;
                    JSONObject fieldObj = group["field"];
                    ElementsDatabase.Instance.groups.Find(o => o.id == gID).serializedFields = JsonUtility.FromJson<CountryGroup.GroupSerializedFields>(fieldObj.ToString());
                    //gID.Log("Loaded for: ");
                }
                //else
                //{
                //    "No id field".Log();
                //}
            }
        }
    }

    public JSONObject GetGroupsInJson()
    {

        JSONObject groups = new JSONObject();

        foreach (CountryGroup c in ElementsDatabase.Instance.groups)
        {
            JSONObject group = new JSONObject();
            group.AddField("id", c.id);
            group.AddField("field", new JSONObject(JsonUtility.ToJson(c.serializedFields)));

            groups.AddField(c.groupName, group);
        }

        return groups;
    }
       


    public void DeleteSaves()
    {
        if (System.IO.File.Exists(SavesPath()))
        {
            System.IO.File.Delete(SavesPath());
        }
    }

    public void DeleteAssetBundles()
    {
        if (System.IO.Directory.Exists(DKK.AssetsBundleManager.Instance.bundleDirectory))
        {
            System.IO.Directory.Delete(DKK.AssetsBundleManager.Instance.bundleDirectory, true);
        }
    }

    public void AddEntry(CityController cityController, int score, int stars, SuccessScreenController.ScoreStatistics stats = null)
    {
        Cities city = ElementsDatabase.Instance.GetCityByName(cityController.cityName);
        if (city == null)
            return;

        AddEntry(city, score, stars, true, stats);
    }

    public void AddEntry(Cities city, int score, int stars, bool save = true, SuccessScreenController.ScoreStatistics stats = null)
    {
        if (stars > 0)
        {
            UserController.Instance.GetStats().AddInt("starsGiven" + GetLanguage(), 1);
        }

        city.GetLanguageSetOrCreate(UserController.Instance.GetLanguage()).succesfullRuns++;
        city.GetLanguageSetOrCreate(UserController.Instance.GetLanguage()).allStars += stars;
        // limit only for the best

        city.saves.Add(new SingleSave(score, stars, GetLanguage(), stats));

        if (save)
            SaveGame();
    }

    public string GetLanguage()
    {
        return PlayerPrefs.GetString("lastLang", "English").ToLower();
    }

    [NaughtyAttributes.Button("Add stars")]
    public void EditorAddStars()
    {
        if (additionalPoints > 0)
            AddStars(additionalPoints, "", true);
        else
        {
            RemoveStars(Math.Abs(additionalPoints), "", "", true);
        }

        FloatingTextPanel.Instance.Show(new FloatingTextParameters()
        {
            text = "Test message from points"
        });
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {

        }
    }

    public class UserGeneralInformation
    {
        public int stars = 0;
        public string name = "";
        public string accountType = "";
    }

    public class UserStatistics
    {
        public static string userStatsFileName = "user.json";

        public static Dictionary<string, UserStatistics> LoadStatistics(string statisticsString = "")
        {
            //statisticsString.Log("Loadign stats from string: ");
            Dictionary<string, UserStatistics> stats = new Dictionary<string, UserStatistics>();

            string savesText = statisticsString;

            if (savesText != null)
            {
                JSONObject saves = new JSONObject(savesText);
                if (saves != null && saves.IsArray)
                {
                    foreach (JSONObject stat in saves.list)
                    {
                        JSONObject obj = stat["obj"];
                        //obj.ToString().Log("US: ");
                        //if(obj.ToString())
                        UserStatistics us = JsonUtility.FromJson<UserStatistics>(obj.ToString());
                        string key = stat["language"].str;
                        if (key == "chinese")
                            key = "中文(香港)";

                        //key.LogDev("Key language: ");
                        if (!stats.ContainsKey(key))
                        {
                            stats.Add(key, us);
                        }
                        else
                        {
                            key.LogDev("Already had key: ");
                        }
                    }
                }
            }


            return stats;
        }

        public static bool SaveStatistics(Dictionary<string, UserStatistics> stats)
        {
            if (!FirebaseDatabaseController.Instance.IsStatisticLoaded())
            {
                "STATS NOT LOADED - Skip saving".Log();
                return false;
            }


            FirebaseDatabaseController.Instance.SaveStatistics();
            return true;
            JSONObject j = new JSONObject();

            foreach (KeyValuePair<string, UserStatistics> entry in stats)
            {
                JSONObject langObj = new JSONObject();
                langObj.AddField("language", entry.Value.language);
                langObj.AddField("obj", new JSONObject(JsonUtility.ToJson(entry.Value)));
                j.Add(langObj);
            }

            DKK.IO.SaveToFile(UserStatistics.SavePath(), j.ToString());

            return true;
        }

        public static JSONObject GetStatisticsJson(Dictionary<string, UserStatistics> stats)
        {
            JSONObject j = new JSONObject();            

            foreach (KeyValuePair<string, UserStatistics> entry in stats)
            {
                entry.Value.dictionary.RemoveAll( o => string.IsNullOrEmpty(o.key));
                JSONObject langObj = new JSONObject();
                //entry.Value.language.LogDev("Lang added: ");
                string key = entry.Value.language;

                if (key == "中文(香港)")
                    key = "chinese";

                langObj.AddField("language", key);
                langObj.AddField("obj", new JSONObject(JsonUtility.ToJson(entry.Value)));
                j.Add(langObj);
            }

            return j;
        }

        public UserStatistics(string lang)
        {
            language = lang;
        }

        public static string SavePath()
        {
            return Path.Combine(Application.persistentDataPath, userStatsFileName);
        }

        public bool HasFlag(string flag, bool addLanguageMarker = false)
        {
            string f = flag;
            if (addLanguageMarker)
            {
                f = flag + UserController.Instance.GetLanguage();
            }

            return flags.Contains(f);
        }

        public void AddFlag(string flag, bool addLanguageMarker = false)
        {
            string f = flag;
            if (addLanguageMarker)
            {
                f = flag + UserController.Instance.GetLanguage();
            }

            if (!flags.Contains(f))
            {
                flags.Add(f);
                UserController.Instance.SaveStatistics();
            }
        }

        public void RemoveFlag(string flag, bool addLanguageMarker = false)
        {
            string f = flag;
            if (addLanguageMarker)
            {
                f = flag + UserController.Instance.GetLanguage();
            }

            if (flags.Contains(f))
            {
                flags.Remove(f);
                UserController.Instance.SaveStatistics();
            }
        }

        //public void AddToDictionary(string key, object value, KeyType type)
        //{
        //    if (!HasDictionary(key))
        //    {
        //        switch (type)
        //        {
        //            case KeyType.INT:
        //                dictionary.Add(new KeyValue() { key = key, intValue = value, type = type });
        //                break;
        //            case KeyType.STRING:
        //                dictionary.Add(new KeyValue() { key = key, stringVal = value, type = type });
        //                break;
        //        }

        //        UserController.Instance.SaveStatistics();
        //    }
        //}

        public string GetString(string key, string defaultValue = "")
        {
            if (HasDictionary(key, KeyType.STRING))
            {
                return GetKeyValue(key).stringVal;
            }

            return defaultValue;
        }

        public void AddString(string key, string value)
        {
            if (!HasDictionary(key, KeyType.STRING))
            {
                dictionary.Add(new KeyValue() { key = key, stringVal = value, type = KeyType.STRING });
                UserController.Instance.SaveStatistics();
            }
            else
            {
                RemoveFromDictionary(key, true);
                dictionary.Add(new KeyValue() { key = key, stringVal = value, type = KeyType.STRING });
                UserController.Instance.SaveStatistics();
            }
        }

        public void AddInt(string key, int value)
        {
            if (!HasDictionary(key, KeyType.INT))
            {
                dictionary.Add(new KeyValue() { key = key, intValue = value, type = KeyType.INT });
                UserController.Instance.SaveStatistics();
            }
            else
            {
                RemoveFromDictionary(key, true);
                dictionary.Add(new KeyValue() { key = key, intValue = value, type = KeyType.STRING });
                UserController.Instance.SaveStatistics();
            }
        }

        public int GetInt(string key, int defaultValue)
        {
            if (HasDictionary(key, KeyType.INT))
            {
                return GetKeyValue(key).intValue;
            }

            return defaultValue;
        }

        public void RemoveFromDictionary(string keySearch, bool avoidSaving = false)
        {
            KeyValue kv = dictionary.Find(o => o.key == keySearch);
            if (kv != null)
            {
                dictionary.Remove(kv);
                if (!avoidSaving)
                    UserController.Instance.SaveStatistics();
            }
            else
            {
                "trying to remove but nothing found".LogDev();
            }
        }

        public KeyValue GetKeyValue(string key)
        {
            return dictionary.Find(o => o.key == key);
        }

        public bool HasDictionary(string keySearch, KeyType type = KeyType.STRING)
        {
            KeyValue kv = dictionary.Find(o => o.key == keySearch);
            return kv != null;
        }

        public void SetGameMode(int value)
        {
            gameMode = value;
        }

        public int GameMode()
        {
            return gameMode;
        }

        public string language;
        public int stars = 0;
        public int gameMode = 0;

        public int autoRotation_balance = 0;
        public int noDistractor_balance = 0;
        public int endlessTimer_balance = 0;
        public int highlights_balance = 0;

        public bool freeAutorotation = false;
        public bool freeNoDistractor = false;
        public bool freeEndlessTimer = false;
        public bool freeHighlight = false;

        public int tickets_balance = 0;

        public List<string> flags = new List<string>();
        public List<KeyValue> dictionary = new List<KeyValue>();

        public enum KeyType
        {
            STRING = 0,
            INT = 1,
            FLOAT = 2
        }

        [System.Serializable]
        public class KeyValue
        {
            public string key = "";
            public string stringVal = "";
            public int intValue;
            public KeyType type;
        }
    }

}

[System.Serializable]
public class GameModeDependencies
{
    public List<Image> images = new List<Image>();
    public List<Text> texts = new List<Text>();
}


