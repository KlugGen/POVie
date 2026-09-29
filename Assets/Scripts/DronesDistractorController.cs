using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

public class DronesDistractorController : Singleton<DronesDistractorController>
{
    public List<DroneController> drones = new List<DroneController>();
    public List<FormationController> formations = new List<FormationController>();

    public List<GameObject> neutralSpots = new List<GameObject>();

    private float timerBetweenFormations = 5f;
    public float defaultTimerBetweenFormations = 5f;
    private float animationTimer = 0f;

    public float distractorTimer = 0f;

    public bool allowAnimation = false;

    public UnityAction OnComplete, OnClicked;

    public void RegisterOnComplete(UnityAction action) => OnComplete += action;
    public void RegisterOnClicked(UnityAction action) => OnClicked += action;

    public float level_easy = 8f;
    public float level_medium = 5f;
    public float level_hard = 2f;

    public float treshold_goodResult = 5f;
    public float treshold_badResult = 10f;

    public bool progression = false;


    void Update()
    {
        // wait for time and change formation
        if (allowAnimation)
        {
            animationTimer -= Time.deltaTime;
            if (animationTimer < 0f)
            {
                //allowAnimation = false;
                NextFormation();
                animationTimer = timerBetweenFormations;
            }
        }

    }

    private int lastFormation = -1;

    [Button("Test Random")]
    public void TestRandom()
    {
        lastFormation = Random.Range(0, formations.Count - 1);
        Enable(lastFormation);
    }

    public void EnableRandom(bool resetDrones = true, bool progression = false)
    {
        this.progression = progression;
        distractorTimer = Time.realtimeSinceStartup;
        SetProgression();

        gameObject.SetActive(true);
        lastFormation = Random.Range(0, formations.Count - 1);
        Enable(lastFormation, resetDrones);
    }

    public void SetProgression()
    {
        if (progression)
        {
            defaultTimerBetweenFormations = level_medium;
            string key = "drones_progression_" + UserController.Instance.GetLanguage();
            if (!UserController.Instance.GetStats().HasDictionary(key))
            {
                UserController.Instance.GetStats().AddInt(key, 2);
                timerBetweenFormations = level_medium;
                return;
            }

            int level = UserController.Instance.GetStats().GetInt(key,1);
            switch (level)
            {
                case 1:
                    timerBetweenFormations = level_easy;
                    break;
                case 2:
                    timerBetweenFormations = level_medium;
                    break;
                case 3:
                    timerBetweenFormations = level_hard;
                    break;
            }            
        }
        else
        {
            timerBetweenFormations = defaultTimerBetweenFormations;
        }
    }
    

    private void NextFormation() {
        //Debug.Log("Next formation");

        List<DroneController> tempDrones = new List<DroneController>(dronesToClick);

        // get list of formations with enough poisitons

        List<FormationController> properFormations = new List<FormationController>();
              properFormations = formations.FindAll(o => o.spots.Count>= tempDrones.Count);
     

        lastFormation = Random.Range(0, properFormations.Count - 1);
       
       List <Vector3> availablePosition = new List<Vector3>();
        properFormations[lastFormation].spots.
            ForEach(delegate(GameObject g)
            {
                RectTransform r = g.GetComponent<RectTransform>();
                if (r != null) {
                    Vector3 pos = g.transform.localPosition;                  
                    pos.y -= r.rect.height / 2;
                    availablePosition.Add(pos);
                }
                else
                    availablePosition.Add(g.transform.localPosition);
            });

        for (int i = 0; i < tempDrones.Count; i++)
        {
            int randomIndex = Random.Range(0, availablePosition.Count - 1);          
            tempDrones[i].Enable(availablePosition[randomIndex]);
            availablePosition.RemoveAt(randomIndex);
        }

        animationTimer = timerBetweenFormations;
        allowAnimation = true;
    }

    public void Enable(int formationID, bool resetDrones = true)
    {
        if (resetDrones)
            ResetElements();

        List<Vector3> availablePosition = new List<Vector3>();
        formations[formationID].spots.ForEach(delegate (GameObject g)
        {
            RectTransform r = g.GetComponent<RectTransform>();
            
            if (r != null)
            {
                Vector3 pos = g.transform.localPosition;              
                pos.y -= r.rect.height / 2;
                availablePosition.Add(pos);
            }
            else
                availablePosition.Add(g.transform.localPosition);
        });

        for (int i = 0; i < formations[formationID].spots.Count; i++)
        {
            int randomIndex = Random.Range(0, availablePosition.Count - 1);
            drones[i].Enable(availablePosition[randomIndex]);
            availablePosition.RemoveAt(randomIndex);
            dronesToClick.Add(drones[i]);
        }

        dronesToGather = new List<DroneController>(dronesToClick);

        animationTimer = timerBetweenFormations;
        allowAnimation = true;
    }

    List<DroneController> dronesToClick = new List<DroneController>();
    List<DroneController> dronesToGather = new List<DroneController>();

    public void DroneClicked(DroneController drone)
    {
        dronesToClick.Remove(drone);

        if (dronesToClick.Count == 0)
        {
            if (OnClicked != null)
                OnClicked.Invoke();

            // Debug.Log("All clicked");
        }
    }

    public void DroneGathered(DroneController drone)
    {
        dronesToGather.Remove(drone);

        drone.ResetToLastPosition();
        if (dronesToGather.Count == 0)
        {
            if(OnComplete!=null)
                OnComplete.Invoke();

            UpdateProgression();
            ResetElements();
            Disable();
        }
    }

    public void UpdateProgression()
    {
        if (progression)
        {
            string key = "drones_progression_" + UserController.Instance.GetLanguage();
            float time = Time.realtimeSinceStartup - distractorTimer;

            if (time <= treshold_goodResult)
            {
                "Harder distractor".LogDev();
                UserController.Instance.GetStats().AddInt(key, 3);
                return;
            }
            
            if (time >= treshold_badResult)
            {
                "Easier distractor".LogDev();
                UserController.Instance.GetStats().AddInt(key, 1);
                return;
            }

            "Medium distractor".LogDev();
            UserController.Instance.GetStats().AddInt(key, 2);
        }
    }
    
    public void ResetElements()
    {      
        dronesToClick.Clear();
        dronesToGather.Clear();
        drones.ForEach(o => o.Reset(neutralSpots[Random.Range(0, neutralSpots.Count - 1)].transform.localPosition));
    }

    public void Disable()
    {
        ResetElements();
        gameObject.SetActive(false);
    }
}
