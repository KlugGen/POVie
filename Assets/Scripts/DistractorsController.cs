using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elements;

public class DistractorsController : Singleton<DistractorsController>
{
    public bool showLogs = false;
    [HideInInspector]
    public int lastEnabledDistractor = 0;

    public float timeSinceLastStart = 0f;
    private bool hintShown = false;
            
    public int ShouldTimerLimitedBeTriggered()
    {
        if (!MainConditions())
            return -1;

        float idleResult = Time.realtimeSinceStartup - GamePlayController.Instance.statistics.idleTimer;

        if (idleResult >= GamePlayController.Instance.statistics.idleTime)
        {
            // sand
            return 1;
        }

        if (Time.realtimeSinceStartup - GamePlayController.Instance.statistics.lastPortalDistractor >= 30f)
        {
            Cities c = GamePlayController.Instance.GetCurrentCity();

            int completedRuns = c.GetCompleteRunsCount(UserController.Instance.GetLanguage());

            if (c.GetMyGroup().id > 0 && completedRuns > 0)
            {
                return 4;
            }
        }

        return -1;
    }

    public int ShouldBeTriggered()
    {
        if (!MainConditions())
        {
            return -1;
        }

        if (!GamePlayController.Instance.statistics.halfElementsCompletedTrigger && GamePlayController.Instance.GetCorrectDrops() == GamePlayController.Instance.GetDropsTarget() / 2)
        {
            if (GamePlayController.Instance.GetCurrentCity().attentionDistractor == true)
            {
                GamePlayController.Instance.statistics.halfElementsCompletedTrigger = true;

                return 3;
            }
        }

        if (Time.realtimeSinceStartup - GamePlayController.Instance.statistics.idleTimer >= GamePlayController.Instance.statistics.idleTime)
        {
            return 1;
        }

        if (GamePlayController.Instance.statistics.movesTimers.Count >= 5)
        {
            float time = Time.realtimeSinceStartup - GamePlayController.Instance.statistics.movesTimers[0];

            if (time < 15f)
            {
                return 2;
            }
        }

        // game mode check if entertainging
        if (UserController.Instance.GetGameModeID() == 2)
        {
            if (Time.realtimeSinceStartup - GamePlayController.Instance.statistics.entertainingTimer > 12f)
            {
                // pollution               
                return UnityEngine.Random.Range(1, 3);
            }
        }

        // no triggered
        return -1;
    }

    public void CheckHowLongEnabled()
    {
        if(hintShown == false && Time.realtimeSinceStartup - timeSinceLastStart > Rules.GameBalance.timeForDistractorHint)
        {
            if (UserController.Instance.GetStats().HasFlag("distractorHintShown_" + lastEnabledDistractor))
            {
                hintShown = true;
                return;
            }

            EnableHint();
        }
    }

    public void EnableHint()
    {
        FloatingTextParameters parameters = new FloatingTextParameters();

        switch (lastEnabledDistractor)
        {
            case 1:
                // sand
                parameters.text = TextTranslationModule.GetWord( "Swipe to remove the sand");
                break;
            case 2:
                // trash
                parameters.text = TextTranslationModule.GetWord("Shake your device");
                break;
            case 3:
                // drones
                parameters.text = TextTranslationModule.GetWord("Tap on drones to remove");
                break;
            case 4:
                // portal
                parameters.text = TextTranslationModule.GetWord("Tap on squares to remove portal");
                break;
        }

        parameters.category = "distractor_hint";
        parameters.time = 2f;
        parameters.generalClickAction = delegate () {
            FloatingTextPanel.Instance.Hide();
        };
        UserController.Instance.GetStats().AddFlag("distractorHintShown_" + lastEnabledDistractor);
        hintShown = true;
        FloatingTextPanel.Instance.Show(parameters);
    }

    public void RunLastDestractor()
    {
        RunDistratctor(lastEnabledDistractor);
    }

    public void RunDistratctor(int i)
    {
        hintShown = false;
        lastEnabledDistractor = i;
        timeSinceLastStart = Time.realtimeSinceStartup;

        switch (i)
        {
            case 3:
                GamePlayController.Instance.distractorsEnabled = true;
                DronesDistractorController.Instance.EnableRandom(true, true);
                DronesDistractorController.Instance.OnComplete = null;
                DronesDistractorController.Instance.RegisterOnClicked(delegate ()
                {
                    GamePlayController.Instance.statistics.Reset();
                    GamePlayController.Instance.distractorsEnabled = false;
                    FloatingTextPanel.Instance.Hide("distractor_hint");
                });

                break;
            case 2:
                GamePlayController.Instance.distractorsEnabled = true;
                ShakeDetector.Instance.enabled = true;
                ShakeDetector.Instance.gameObject.SetActive(true);
                TrashDestractor.Instance.Enable();

                ShakeDetector.Instance.RegisterOnShake(delegate (ShakeData data)
                {
                    GamePlayController.Instance.statistics.Reset();
                    ShakeDetector.Instance.gameObject.SetActive(false);
                    GamePlayController.Instance.distractorsEnabled = false;
                    FloatingTextPanel.Instance.Hide("distractor_hint");
                });

                break;

            case 1:
                GamePlayController.Instance.distractorsEnabled = true;
                SwipeDetector.Instance.OnSwipe = null;
                SwipeDetector.Instance.RegisterOnSwipe(delegate (SwipeData data)
                {
                    if (data.SwipeTime > .2f)
                    {
                        GamePlayController.Instance.statistics.Reset();
                        GamePlayController.Instance.distractorsEnabled = false;
                        SandDistractorController.Instance.UpdateProgression();
                        SandDistractorController.Instance.Disable();
                        FloatingTextPanel.Instance.Hide("distractor_hint");
                    }
                });

                SwipeDetector.Instance.gameObject.SetActive(true);
                SandDistractorController.Instance.Enable(true);

                break;
            case 4:
                GamePlayController.Instance.distractorsEnabled = true;
                SquaresDistractorController.Instance.Enable(true);
                SquaresDistractorController.Instance.OnComplete = null;
                SquaresDistractorController.Instance.RegisterOnComplete(delegate ()
                {
                    GamePlayController.Instance.statistics.Reset();
                    GamePlayController.Instance.distractorsEnabled = false;
                    FloatingTextPanel.Instance.Hide("distractor_hint");
                });
                break;
        }
    }

    public bool MainConditions()
    {
        if (GamePlayController.Instance.bonuses.noDistractorActive)
        {
            return false;
        }

        //if (GamePlayController.Instance.GetCurrentCity().GetMyGroup().id <= -1)
        //{
        //    return false;
        //}

        if (UserController.Instance.maxUnlockdedGroup < 2)
        {
            return false;
        }

        if (GamePlayController.Instance.GetCurrentCity().type == CitiesGameplayType.tutorial)
        {
            return false;
        }

        if (GamePlayController.Instance.distractorsEnabled || 
            GamePlayController.Instance.distractorSuspended || 
            GamePlayController.Instance.distractorSuspendedByAnimation)
        {
            //"DIS: distractor Enabled ro suspended".LogDev();
            return false;
        }

        return true;
    } 
}
