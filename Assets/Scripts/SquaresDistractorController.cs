using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SquaresDistractorController : Singleton<SquaresDistractorController>
{
    public SaveAreaAnchored saveArea;
    public List<Transform> DistractorsParents;
    //private List<Image> Distractors;
    private List<SquareGridUnit> units;

    public float level_easy = 0.8f;
    public float level_medium = 0.5f;
    public float level_hard = .15f;

    public float treshold_goodResult = 5f;
    public float treshold_badResult = 10f;

    public Font font;
    
    private Coroutine distractorCoroutine;
    [Range(0.01f, 2f)] public float SecsBetweenSquares = 0.5f;
    private float secsBetweenSquares_private = .5f;

    public int targetToFinish = 10;

    [Button("Enable")]
    public void Enable(bool progression = false)
    {
        if (!Application.isPlaying)
            return;

        distractorTimer = Time.realtimeSinceStartup;
        this.progression = progression;
        SetProgression();
        gameObject.SetActive(true);
        DistractorsParents.ForEach( o=> o.GetChild(0).GetComponent<Image>().enabled = false);
        StartCoroutine("EnableCoroutine");
    }

    public IEnumerator EnableCoroutine()
    {
        yield return new WaitForEndOfFrame();

        RemoveInvisibleRowsFromBelowAndRight();

        //DistractorsParents.ForEach(o => o.GetChild(0).GetComponent<Image>().enabled = true);

        if (units is null || units.Count == 0)
            InitializeDistractors();

        StartDistractor();
    }

    //[Button("Disable")]
    public void Disable()
    {
        StopCoroutine("EnableCoroutine");
        gameObject.SetActive(false);
        ResetDistractor();
    }

    public void StartDistractor()
    {
        ChangeAllAtOnce(false);
        distractorCoroutine = StartCoroutine(ShowAllWithBreaks());
    }

    public void ResetDistractor()
    {
        if (distractorCoroutine != null)
            StopCoroutine(distractorCoroutine);

        ChangeAllAtOnce(false);
        clickedSquaresCounter = 0;
    }

    private IEnumerator ShowAllWithBreaks()
    {
        for (int i = 0; i < units.Count; i++)
        {
            yield return new WaitForSeconds(secsBetweenSquares_private);
            ShowAnotherSquare();
        }
    }

    private void InitializeDistractors()
    {
        units = new List<SquareGridUnit>();

        foreach (Transform distractorParent in DistractorsParents)
        {
            SquareGridUnit unit = distractorParent.GetComponent<SquareGridUnit>();
            unit.Initialize();
            units.Add(unit);
            //Distractors.Add(distractorParent.GetChild(0).GetComponent<Image>());
        }
    }

    private void ShowAnotherSquare()
    {
        SquareGridUnit chosenSquare = GetRandomSquare(false);            
       

        if (chosenSquare != null)
        {
            chosenSquare.displayed = true;
            chosenSquare.image.enabled = true;
            chosenSquare.button.enabled = true;
        }
    }

    private SquareGridUnit GetRandomSquare(bool fromEnabledSquares)
    {
        List<SquareGridUnit> filteredImages = units.FindAll(x => x.displayed == false);
        int randomIndex = UnityEngine.Random.Range(0, filteredImages.Count);

        if (filteredImages.Count == 0)
            return null;

        return filteredImages[randomIndex];
    }

    private void ChangeAllAtOnce(bool enabled)
    {
        if (units != null)
            units.ForEach(delegate(SquareGridUnit i) {
                if(i != null)
                {
                    if (!enabled)
                    {
                        i.displayed = false;
                        i.button.enabled = false;
                    }

                    i.image.enabled = enabled;     
                }
            });
    }

    private void RemoveInvisible()
    {
        List<int> idsOfBlocksToDelete = new List<int>();

        for (int i = 0; i < DistractorsParents.Count; i++)
        {
            RectTransform r = DistractorsParents[i].GetComponent<RectTransform>();

            if (r.position.y < 0f)
            {
                "rem".Log();
                r.transform.parent.GetChild(0).GetComponent<Image>().enabled = false;
                idsOfBlocksToDelete.Add(i);
            }
        }

        idsOfBlocksToDelete.ForEach(o => DistractorsParents[o] = null);
        DistractorsParents.RemoveAll(x => x == null);
    }

    private void RemoveInvisibleRowsFromBelowAndRight()
    {
        var corners = new Vector3[4];
        var parentCorners = new Vector3[4];

        GetComponent<RectTransform>().GetWorldCorners(parentCorners);


        //GetComponent<RectTransform>().rect.height.Log("H: ");
        float heighScaled = GetComponent<RectTransform>().rect.height * ViewsController.Instance.mainCanvas.scaleFactor;
        //heighScaled.Log("Scaled: ");

        List<int> idsOfBlocksToDelete = new List<int>();

        float scaler = ViewsController.Instance.mainCanvas.scaleFactor;

        for (int i = 0; i < DistractorsParents.Count; i++)
        {
            RectTransform rt = DistractorsParents[i].GetComponent<RectTransform>();
            rt.GetWorldCorners(corners);

            //GameObject g = new GameObject();
            //g.transform.SetParent(rt.transform);
            //g.transform.localPosition = Vector3.zero;
            //Text t = g.AddComponent<Text>();
            //t.text = (rt.rect.height * scaler).ToString();
            //t.alignment = TextAnchor.MiddleCenter;
            //t.color = Color.black;
            //t.font = font;

           // rt.gameObject.AddComponent<Text>().text = rt.position.y.ToString();


            if (corners[1].y <= 0f || corners[1].x >= parentCorners[2].x)
                idsOfBlocksToDelete.Add(i);
            else
            {
                // if (saveArea.GetYMax()  < rt.position.y)
                //- (rt.sizeDelta.y * ViewsController.Instance.mainCanvas.scaleFactor)

                //if ((saveArea.GetYMax() ) < rt.position.y + rt.rect.height )
                // if ((saveArea.GetYMax()) < rt.position.y  + rt.rect.height )   
                float scaledHeight = rt.rect.height * scaler;
                if (heighScaled < rt.position.y + scaledHeight/2f)                              
                {
                    if (inDebug)
                    {
                    //    //rt.rect.height.Log("H: ");
                    //    //rt.position.y.Log("Pos: ");
                    //    //rt.sizeDelta.y.Log("Del: ");
                    }
                    //rt.rect.Log("H: ");
                    //saveArea.GetYMax().Log("YM: ");
                    //Screen.height.Log("SH: ");
                    //chosenSquare.rectTransform.position.y.Log("Apos: ");
                    //"avoid".Log();
                    idsOfBlocksToDelete.Add(i);
                    //return;
                }
            }
        }
        //Debug.Log(idsOfBlocksToDelete.Count + " to remove..");
        foreach (var id in idsOfBlocksToDelete)
        {
            //DistractorsParents[id].GetChild(0).gameObject.AddComponent<Outline>();
            //DistractorsParents[id].GetChild(0).GetComponent<Image>().enabled = false;
            DistractorsParents[id] = null;
        }

        DistractorsParents.RemoveAll(x => x == null);
    }

    private int clickedSquaresCounter = 0;

    public UnityAction OnComplete, OnClicked;

    public void RegisterOnComplete(UnityAction action) => OnComplete += action;
    public void RegisterOnClicked(UnityAction action) => OnClicked += action;

    public void SquareClicked(SquareGridUnit unit)
    {       
        clickedSquaresCounter++;
     
        unit.image.enabled = false;
        if(OnClicked!=null)
            OnClicked.Invoke();

        unit.displayed = false;
        CheckProgression();
    }

    private void CheckProgression()
    {
        if (clickedSquaresCounter >= targetToFinish)
        {
            if (OnComplete != null)
                OnComplete.Invoke();

            UpdateProgression();
            //Debug.Log("Distractor finished");
            Disable();          
        }
    }

    public bool progression = false;
    public float distractorTimer = 0f;

    public void SetProgression()
    {
        if (progression)
        {
            secsBetweenSquares_private = level_medium;
            string key = "squares_progression_" + UserController.Instance.GetLanguage();
            if (!UserController.Instance.GetStats().HasDictionary(key))
            {
                UserController.Instance.GetStats().AddInt(key, 2);
                secsBetweenSquares_private = level_medium;
                return;
            }

            int level = UserController.Instance.GetStats().GetInt(key,0);
            switch (level)
            {
                case 1:
                    secsBetweenSquares_private = level_easy;
                    break;
                case 2:
                    secsBetweenSquares_private = level_medium;
                    break;
                case 3:
                    secsBetweenSquares_private = level_hard;
                    break;
            }
        }
        else
        {
            secsBetweenSquares_private = SecsBetweenSquares;
        }
    }

    public void UpdateProgression()
    {
        if (progression)
        {
            string key = "squares_progression_" + UserController.Instance.GetLanguage();
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

            UserController.Instance.GetStats().AddInt(key, 2);
        }
    }
}