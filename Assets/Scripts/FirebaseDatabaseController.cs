using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using UniRx;
//using Firebase.Unity.Editor;

public class FirebaseDatabaseController : Singleton<FirebaseDatabaseController>
{
    FirebaseDatabase database;
    DatabaseReference reference;

    private static string databaseRootKey = "users";
    private static string databaseSavesKey = "saves";
    public static string databaseStatsKey = "stats";
    public static string databaseGeneralKey = "general";
    public static string databaseGroupsKey = "groups";

    private bool initialized = false;

    public bool savesReadyToLoad = false;
    private bool saveLoaded = false;
    private bool statisticsLoaded = false;
    private bool generalLoaded = false;
    private bool allDataReady = false;

    public bool IsAllDataReady() => allDataReady;
    public bool IsGeneralLoaded() => generalLoaded;
    public bool IsStatisticLoaded() => statisticsLoaded;
    public bool IsSaveLoaded() => saveLoaded;
    public bool IsInitialized() => initialized;

    private DatabaseReference usersDatabaseReference = null;
    private DatabaseReference savesDatabaseReference = null;
    private DatabaseReference statsDatabaseReference = null;
    private DatabaseReference groupsDatabaseReference = null;
    private DatabaseReference generalDatabaseReference = null;


    public void LoggedOut()
    {
        saveLoaded = false;
        statisticsLoaded = false;
        generalLoaded = false;
        allDataReady = false;
        UserController.Instance.logInReward = false;
    }

    public void Initialize()
    {       
        if (initialized)
        {
            return;
        }
      
        database = FirebaseDatabase.DefaultInstance;
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        
        initialized = true;
        InitializeDatabaseReferences();
    }    

    public void ReinitializeAfterBeingRelogged()
    {
        InitializeDatabaseReferences();
    }

    private void InitializeDatabaseReferences()
    {
        usersDatabaseReference = reference.Child(databaseRootKey).Child(FirebaseController.Instance.GetFirebaseAuth().CurrentUser.UserId);
        savesDatabaseReference = usersDatabaseReference.Child(databaseSavesKey);
        statsDatabaseReference = usersDatabaseReference.Child(databaseStatsKey);
        generalDatabaseReference = usersDatabaseReference.Child(databaseGeneralKey);
        groupsDatabaseReference = usersDatabaseReference.Child(databaseGroupsKey);
    }
    
    public void LoadSaves()
    {       
        var taskScheduler = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();   

        savesDatabaseReference.GetValueAsync().ContinueWith(task => {                       
         
            if (task.IsFaulted)
            {
                "TASK FAULTED - SAVES".Log();
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Value == null)
                {                    
                    UserController.Instance.LoadSaves("");
                    savesDatabaseReference.SetRawJsonValueAsync(UserController.Instance.GetSavesInJson().ToString()).ContinueWith(task1 => {                    
                                       
                        LoadStatistics(taskScheduler);
                        saveLoaded = true;
                    }, taskScheduler);
                }
                else
                {                   
                    UserController.Instance.LoadSaves(snapshot.GetRawJsonValue());
                    LoadStatistics(taskScheduler);
                    saveLoaded = true;
                }
            }
            else
            {
                task.Result.Log("Else: ");
            }
        }, taskScheduler);
    }

    public void GetDataForCity(string country, string city)
    {
        DatabaseReference dr = savesDatabaseReference.Child(country).Child("cities").Child(city);

        dr.GetValueAsync().ContinueWith(task => {

            if (task.IsFaulted)
            {
                "TASK FAULTED - SAVING".Log();
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Value == null)
                {
                    "NO DATA FOR CITY".Log();
                }
            }
            else
            {
                task.Result.Log("Else: ");
            }
        });
    }

    public void LoadStatistics(System.Threading.Tasks.TaskScheduler taskScheduler)
    {
        statsDatabaseReference.GetValueAsync().ContinueWith(task => {

            //task.Result.Log("users: ");
            if (task.IsFaulted)
            {
                "TASK FAULED - STATISTICS".Log();
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Value == null)
                {
                    UserController.Instance.logInReward = true;
                    "NO CLOUD STATS FOR USER".Log();
                    statsDatabaseReference.SetRawJsonValueAsync(UserController.Instance.GetStatisticsJson().ToString()).ContinueWith(task1 => {
                        
                        UserController.Instance.LoadStatistics("");
                        statisticsLoaded = true;
                        LoadGeneral(taskScheduler);                    
                        "new users".Log();
                    }, taskScheduler);
                }
                else
                {
                    try
                    {
                        statisticsLoaded = true;
                        UserController.Instance.LoadStatistics(snapshot.GetRawJsonValue());
                    }catch(System.Exception e)
                    {
                        e.ToString().Log();
                    }

                    LoadGeneral(taskScheduler);                    
                }               
            }
            else
            {
                task.Result.Log("Else: ");
            }
        }, taskScheduler);
    }

    public void LoadGeneral(System.Threading.Tasks.TaskScheduler taskScheduler)
    {
        generalDatabaseReference.GetValueAsync().ContinueWith(task => {

            if (task.IsFaulted)
            {
                "TASK FAULTED - GENERAL".Log();
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Value == null)
                {
                    UserController.Instance.LoadGeneralInformation("");
                    "NO CLOUD GENERAL FOR USER".Log();
                    generalDatabaseReference.SetRawJsonValueAsync(UserController.Instance.GetGeneralInformation().ToString()).ContinueWith(task1 => {

                        generalLoaded = true;
                        LoadGroups(taskScheduler);              

                    }, taskScheduler);
                }
                else
                {
                    try
                    {
                        generalLoaded= true;
                        UserController.Instance.LoadGeneralInformation(snapshot.GetRawJsonValue());
                    }
                    catch (System.Exception e)
                    {
                        e.ToString().Log();
                    }

                    LoadGroups(taskScheduler);
                }
            }
            else
            {
                task.Result.Log("Else: ");
            }
        }, taskScheduler);
    }

    public void LoadGroups(System.Threading.Tasks.TaskScheduler taskScheduler)
    {
        groupsDatabaseReference.GetValueAsync().ContinueWith(task => {            
            if (task.IsFaulted)
            {
                "TASK FAULTED - GROUPS".Log();
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Value == null)
                {
                    "NO CLOUD GROUPS FOR USER".Log();
                    groupsDatabaseReference.SetRawJsonValueAsync(UserController.Instance.GetGroupsInJson().ToString()).ContinueWith(task1 => {

                        UserController.Instance.LoadGroups("");
                        allDataReady = true;
                        UserController.Instance.LoadStatisticsAndSaves();
                        SignInController.Instance.AfterLoadingSavesAction();                    
                    }, taskScheduler);
                }
                else
                {
                    allDataReady = true;
                    UserController.Instance.LoadGroups(snapshot.GetRawJsonValue());
                    UserController.Instance.LoadStatisticsAndSaves();
                    SignInController.Instance.AfterLoadingSavesAction();
                }
            }
            else
            {
                task.Result.Log("Else: ");
            }
        }, taskScheduler);
    }

    bool saveStatistics = false;
    bool saveGame = false;
    bool saveGroups = false;
    bool saveGeneral = false;

    public static bool savingInCloudActive = true;

    public void SaveStatistics()
    {
        if (!savingInCloudActive) {
            "CLOUD STATS SAVING TEMPORARY BLOCKED".LogDev();
            return;
        }         

        "CLOUD STATS SAVING FLAG RAISED".Log();
        saveStatistics = true;      
    }

    public void LateUpdate()
    {
        if (saveStatistics)
        {
            saveStatistics = false;
            string str = UserController.Instance.GetStatisticsJson().ToString();
            statsDatabaseReference.SetRawJsonValueAsync(str).ContinueWith(task1 => {});
            return;
        }

        if (saveGame)
        {
            saveGame = false;
            string savesString = UserController.Instance.GetSavesInJson().ToString();
            savesDatabaseReference.SetRawJsonValueAsync(savesString).ContinueWith(task1 => {});
            return;
        }

        if (saveGroups)
        {
            saveGroups = false;
            string savesString = UserController.Instance.GetGroupsInJson().ToString();
            groupsDatabaseReference.SetRawJsonValueAsync(savesString).ContinueWith(task1 => {});
            return;
        }

        if (saveGeneral)
        {            
            string savesString = UserController.Instance.GetGeneralInformation().ToString();
            generalDatabaseReference.SetRawJsonValueAsync(savesString).ContinueWith(task1 => {});
            return;
        }
    }

    //[NaughtyAttributes.Button("save game")]
    public void SaveGame() => saveGame = true;

    public void SaveGroups() => saveGroups = true;

    public void SaveGeneral() => saveGeneral = true;

    public string userID;
    public int resultScore;

    public object Timestamp { get; private set; }
}
