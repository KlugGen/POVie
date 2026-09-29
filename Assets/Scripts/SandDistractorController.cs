using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandDistractorController : Singleton<SandDistractorController>
{

    public List<Transform> DistractorsParents;
    private List<SandRow> Rows = new List<SandRow>();
    public GameObject SandParticleSystemLeftGameObject, SandParticleSystemRightGameObject;
    public ParticleDetector ParticleDetector;

    private bool noEmptyPlaces = false;
    [SerializeField] private bool debugEnabled = false;
    private List<string> addedSandBlocks = new List<string>();
    [Range(0.01f, 2f)] public float SecsBetweenSquares = 0.5f;
    private float secsBetweenSquares_private = .5f;
    [Range(0f, 1f)] public float GetFromLowestChance = 0.2f;
    [Range(0f, 1f)] public float JumpChance = 0.25f;

    private Coroutine distractorCoroutine, distractorCoroutineRight;
    private Coroutine startSandBuilderCoroutine;

    private int lastBlockXLeft = -1;
    private int lastBlockYLeft = -1;
    private int lastBlockXRight = -1;
    private int lastBlockYRight = -1;

    public float level_easy = 0.44f;
    public float level_medium = 0.13f;
    public float level_hard = .01f;

    public float treshold_goodResult = 2f;
    public float treshold_badResult = 4f;

    public bool LeftEnabled = true;
    public bool RightEnabled = true;

    //[Button("Enable")]
    public void Enable(bool progression = false)
    {
        this.progression = progression;
        distractorTimer = Time.realtimeSinceStartup;
        SetProgression();
        gameObject.SetActive(true);
        StartCoroutine(StartDistractor());
    }

    [Button("Disable")]
    public void Disable()
    {
        gameObject.SetActive(false);
        ResetDistractor();
    }

    private void AssignBlocksToRows()
    {
        var listOfRowsYs = GetListOfRowsYs();

        for (int i = 0; i < listOfRowsYs.Count; i++)
        {
            Rows.Add(new SandRow());
        }

        foreach (var distractorParent in DistractorsParents)
        {
            for (int i = 0; i < listOfRowsYs.Count; i++)
            {
                if (distractorParent.position.y == listOfRowsYs[i])
                {
                    Rows[i].sandElements.Add(distractorParent);
                    break;
                }
            }
        }

        if (debugEnabled)
            Debug.Log("Blocks assigned to rows, rows count: " + Rows.Count);
    }

    private List<float> GetListOfRowsYs()
    {
        List<float> YsWithNoRepetitions = new List<float>();

        foreach (var distractorParent in DistractorsParents)
        {
            if (!YsWithNoRepetitions.Contains(distractorParent.position.y))
                YsWithNoRepetitions.Add(distractorParent.position.y);
        }

        YsWithNoRepetitions.Sort();

        return YsWithNoRepetitions;
    }

    private void FindAndSetAnotherSandBlockFromTheLowestPossibleLayer(SandBuilderSide side)
    {
        if (debugEnabled)
            Debug.Log("Finding another initial block");

        bool found = false;
        List<Tuple<int, int>> foundBlocks = new List<Tuple<int, int>>();

        for (int y = 0; y < Rows.Count; y++)
        {
            for (int x = 0; x < Rows[y].sandElements.Count; x++)
            {
                if (CanNextBlockBeSet(x, y, side))
                {
                    foundBlocks.Add(new Tuple<int, int>(x, y));
                    found = true;
                }
            }

            if (found)
                break;
        }

        if (found)
        {
            //int randomIndexFromFoundBlocks = UnityEngine.Random.Range(0, foundBlocks.Count);
            int randomIndexFromFoundBlocks = (side == SandBuilderSide.Left) ? 0 : (foundBlocks.Count - 1);
            ShowSandBlock(foundBlocks[randomIndexFromFoundBlocks].Item1, foundBlocks[randomIndexFromFoundBlocks].Item2, side);
        }
        else
        {
            noEmptyPlaces = true;

            if (debugEnabled)
                Debug.Log("FindAndSetAnotherSandBlockFromTheLowestPossibleLayer(): No empty blocks left");
        }
    }

    private void ShowSandBlock(int x, int y, SandBuilderSide side)
    {
        if (Rows.Count < y)
            Debug.LogError("Error: ShowSandBlock(): Rows.Count < y");

        if (Rows[y].sandElements.Count < x)
            Debug.LogError("Error: ShowSandBlock(): Rows[y].sandElements.Count < x");

        Rows[y].sandElements[x].GetChild(0).gameObject.SetActive(true);
        addedSandBlocks.Add(x + " " + y);

        if (side == SandBuilderSide.Left)
        {
            lastBlockXLeft = x;
            lastBlockYLeft = y;
        }
        else
        {
            lastBlockXRight = x;
            lastBlockYRight = y;
        }
    }

    private void AddAnotherSandBlock(SandBuilderSide side)
    {
        if (noEmptyPlaces)
            return;

        List<int> possibleDirections = new List<int> { 0, 1, 2, 3 };
        int direction; //0 - left, 1 - right, 2 - left top, 3 - right top
        bool sandBlockAdded = false;

        while (possibleDirections.Count > 0)
        {
            direction = UnityEngine.Random.Range(0, possibleDirections.Count);
            int xToCheck = -1, yToCheck = -1;
            int lastBlockXAccordingToSide, lastBlockYAccordingToSide;

            if (side == SandBuilderSide.Left)
            {
                lastBlockXAccordingToSide = lastBlockXLeft;
                lastBlockYAccordingToSide = lastBlockYLeft;
            }
            else
            {
                lastBlockXAccordingToSide = lastBlockXRight;
                lastBlockYAccordingToSide = lastBlockYRight;
            }

            switch (possibleDirections[direction])
            {
                case 0:
                    xToCheck = lastBlockXAccordingToSide - 1;
                    yToCheck = lastBlockYAccordingToSide;
                    break;
                case 1:
                    xToCheck = lastBlockXAccordingToSide + 1;
                    yToCheck = lastBlockYAccordingToSide;
                    break;
                case 2:
                    xToCheck = lastBlockXAccordingToSide;
                    yToCheck = lastBlockYAccordingToSide + 1;
                    break;
                case 3:
                    xToCheck = lastBlockXAccordingToSide + 1;
                    yToCheck = lastBlockYAccordingToSide + 1;
                    break;
            }

            if (CanNextBlockBeSet(xToCheck, yToCheck, side))
            {
                ShowSandBlock(xToCheck, yToCheck, side);
                sandBlockAdded = true;
                break;
            }
            else possibleDirections.RemoveAt(direction);
        }

        if (!sandBlockAdded)
        {
            if (debugEnabled)
                Debug.Log("AddAnotherSandBlock: No possible places");

            FindAndSetAnotherSandBlockFromTheLowestPossibleLayer(side);
        }
    }

    private bool IsBlockAlreadySet(int x, int y) => addedSandBlocks.Contains(x + " " + y);

    private bool DoesBlockHaveBase(int x, int y)
    {
        bool hasLeftBase = true;
        bool hasRightBase = true;

        int leftBaseX, leftBaseY, rightBaseX, rightBaseY;
        leftBaseY = rightBaseY = y - 1;

        //outer condition added because of possibility of deleting some rows
        if (Rows.Count % 2 == 0)
        {
            if (y % 2 == 1)
            {
                //odd row
                leftBaseX = x - 1;
                rightBaseX = x;
            }
            else
            {
                //even row
                leftBaseX = x;
                rightBaseX = x + 1;
            }
        }
        else
        {
            if (y % 2 == 1)
            {
                //odd row
                leftBaseX = x;
                rightBaseX = x + 1;
            }
            else
            {
                //even row
                leftBaseX = x - 1;
                rightBaseX = x;
            }
        }

        //if there are no bottom blocks
        if (y == 0)
            return true;

        //check left base
        if (leftBaseX < 0 || leftBaseX >= Rows[leftBaseY].sandElements.Count)   //block doesn't exist
            hasLeftBase = true;
        else if (IsBlockAlreadySet(leftBaseX, leftBaseY))   //block already set
            hasLeftBase = true;
        else
            hasLeftBase = false;

        //check right base
        if (rightBaseX < 0 || rightBaseX >= Rows[rightBaseY].sandElements.Count)   //block doesn't exist
            hasRightBase = true;
        else if (IsBlockAlreadySet(rightBaseX, rightBaseY))   //block already set
            hasRightBase = true;
        else
            hasRightBase = false;

        return hasLeftBase && hasRightBase;
    }

    private bool CanNextBlockBeSet(int x, int y, SandBuilderSide side)
    {
        if (y < 0 || y >= Rows.Count)
            return false;

        if (x < 0 || x >= Rows[y].sandElements.Count)
            return false;

        if (IsBlockAlreadySet(x, y))
            return false;

        if (!DoesBlockHaveBase(x, y))
            return false;

        return true;
    }

    private void AddAllSandBlocks(SandBuilderSide side)
    {
        distractorCoroutine = StartCoroutine(AddAllSandBlocksCoroutine(side));
    }

    private IEnumerator AddAllSandBlocksCoroutine(SandBuilderSide initialSide)
    {
        SandBuilderSide currentSide = initialSide;

        while (!noEmptyPlaces)
        {
            yield return new WaitForSeconds(secsBetweenSquares_private);

            if (LeftEnabled || RightEnabled)
            {
                if (UnityEngine.Random.value < GetFromLowestChance)
                    FindAndSetAnotherSandBlockFromTheLowestPossibleLayer(currentSide);
                else
                {
                    if (UnityEngine.Random.value <= JumpChance)
                    {
                        if (!Jump(currentSide))
                            AddAnotherSandBlock(currentSide);
                    }
                    else
                        AddAnotherSandBlock(currentSide);
                }

                if (currentSide == SandBuilderSide.Left && RightEnabled)
                    currentSide = SandBuilderSide.Right;
                else if (currentSide == SandBuilderSide.Right && LeftEnabled)
                    currentSide = SandBuilderSide.Left;
            }

            SandParticleSystemLeftGameObject.SetActive(LeftEnabled);
            SandParticleSystemRightGameObject.SetActive(RightEnabled);
        }

        if (debugEnabled)
            Debug.Log("End of coroutine");

        SandParticleSystemLeftGameObject.SetActive(false);
        SandParticleSystemRightGameObject.SetActive(false);
    }

    /// <summary>
    /// Returns true if sand block added.
    /// </summary>
    /// <returns></returns>
    private bool Jump(SandBuilderSide side)
    {
        if (side == SandBuilderSide.Left)
        {
            for (int x = 0; x < Rows[0].sandElements.Count; x++)
            {
                for (int y = 0; y < Rows.Count; y++)
                {
                    if (CanNextBlockBeSet(x, y, side))
                    {
                        ShowSandBlock(x, y, side);
                        return true;
                    }
                }
            }
        }
        else
        {
            for (int x = Rows[0].sandElements.Count - 1; x >= 0; x--)
            {
                for (int y = 0; y < Rows.Count; y++)
                {
                    if (CanNextBlockBeSet(x, y, side))
                    {
                        ShowSandBlock(x, y, side);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Starts sand distractor.
    /// </summary>
    public IEnumerator StartDistractor()
    {
        yield return new WaitForEndOfFrame();

        ResetDistractor();
        AssignBlocksToRows();

        RemoveInvisibleRowsFromBelow();
        RemoveInvisibleRowsFromRight();

        if (LeftEnabled)
            SandParticleSystemLeftGameObject.SetActive(true);

        if (RightEnabled)
            SandParticleSystemRightGameObject.SetActive(true);

        if (LeftEnabled || RightEnabled)
            startSandBuilderCoroutine = StartCoroutine(StartSandBuilderAfterDelay());
    }

    private IEnumerator StartSandBuilderAfterDelay()
    {
        while (!ParticleDetector.CollisionDetected)
        {
            yield return new WaitForSeconds(0.5f);
        }

        //TODO jak się zatrzyma obie strony, to żeby przestało
        //TODO czemu prawą stronę szybciej buduje????
        SandBuilderSide initialSide = LeftEnabled ? SandBuilderSide.Left : SandBuilderSide.Right;

        FindAndSetAnotherSandBlockFromTheLowestPossibleLayer(initialSide);
        AddAllSandBlocks(initialSide);
        //TODO dlaczego nie buduje ciągle od lewej jak ten jeden parametr jest na 0, a tylko jump jest dodatni???!?!??!?!!
    }

    /// <summary>
    /// Resets sand distractor.
    /// </summary>
    public void ResetDistractor()
    {
        SandParticleSystemLeftGameObject.SetActive(false);
        SandParticleSystemRightGameObject.SetActive(false);

        if (distractorCoroutine != null)
            StopCoroutine(distractorCoroutine);

        if (distractorCoroutineRight != null)
            StopCoroutine(distractorCoroutineRight);

        if (startSandBuilderCoroutine != null)
            StopCoroutine(startSandBuilderCoroutine);

        ParticleDetector.CollisionDetected = false;

        DistractorsParents.ForEach(x => x.GetChild(0).gameObject.SetActive(false));

        Rows.ForEach(x => x.sandElements.Clear());
        Rows.Clear();

        noEmptyPlaces = false;
        addedSandBlocks.Clear();
        lastBlockXLeft = lastBlockYLeft = -1;
        lastBlockXRight = lastBlockYRight = -1;
    }

    private void RemoveInvisibleRowsFromBelow()
    {
        if (debugEnabled)
            Debug.Log("DeleteInvisibleRowsFromBelow()...");

        var corners = new Vector3[4];

        for (int i = 0; i < Rows.Count; i++)
        {
            Rows[i].sandElements[0].GetComponent<RectTransform>().GetWorldCorners(corners);
            if (corners[1].y > 0f)
            {
                Rows.RemoveRange(0, i);
                break;
            }
            else Rows[i].sandElements.Clear();
        }
    }

    private void RemoveInvisibleRowsFromRight()
    {
        if (debugEnabled)
            Debug.Log("DeleteInvisibleRowsFromRight()...");

        var corners = new Vector3[4];
        var parentCorners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(parentCorners);

        for (int y = 0; y < Rows.Count; y++)
        {
            for (int x = Rows[y].sandElements.Count - 1; x >= 0; x--)
            {
                Rows[y].sandElements[x].GetComponent<RectTransform>().GetWorldCorners(corners);

                if (corners[1].x < parentCorners[2].x)
                {
                    int count = Rows[y].sandElements.Count - 1 - x;
                    Rows[y].sandElements.RemoveRange(x + 1, count);
                    break;
                }
            }
        }
    }

    public void EnableAllBlocks() => DistractorsParents.ForEach(x => x.GetChild(0).gameObject.SetActive(true));

    private enum SandBuilderSide
    {
        Left,
        Right,
    }

    public bool progression = false;
    public float distractorTimer = 0f;

    public void SetProgression()
    {
        if (progression)
        {
            secsBetweenSquares_private = level_medium;
            string key = "sand_progression_" + UserController.Instance.GetLanguage();
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
            string key = "sand_progression_" + UserController.Instance.GetLanguage();
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
}


[Serializable]
public class SandRow
{
    public List<Transform> sandElements = new List<Transform>();
}