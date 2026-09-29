using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rules
{
    public static class AdsRules
    {
        public static bool MultiplyReward(int stars)
        {
#if UNITY_IOS
            return false;
#endif
            if (stars <= 0)
                return false;

            return true;
        }

        public static bool AddToBalance()
        {
#if UNITY_IOS
            return false;
#endif
            UserController.Instance.GetStars().LogDev("Ads balance check: ");
            return UserController.Instance.GetStars() < Rules.GameBalance.adsBalanceAvailableTreshold;
        }
    }

  
    public static class BoostersRules
    {
        public static bool BoosterButtonActive(int currentCityGroupID)
        {
            return FirstBoosterAvailable();
        }

        public static bool FirstBoosterAvailable()
        {
            int maxUnlcocked = UserController.Instance.MaxUnlockedGroup();
            return maxUnlcocked >= 1;
        }

        public static bool SecondBoosterAvailable()
        {

            int maxUnlcocked = UserController.Instance.MaxUnlockedGroup();
            return maxUnlcocked >= 2;

            List<Elements.Cities> cities =  Elements.ElementsDatabase.Instance.GetCitiesByGroup(2);

            //cities.Count.LogDev("Cities in g2: ");
            List<Elements.Cities> citiesDone = cities.FindAll(o => o.GetCurrentLanguageSet().succesfullRuns > 0);

            //citiesDone.Count.LogDev("Done: ");

            if (cities.Count == 1)
                return true;

            if (cities.Count < 4)
            {              
                //citiesDone.Count.LogDev("Cities less than 3. Done: ");
                if(citiesDone.Count > 1)
                    return true;
            }
            else
            {
                if (citiesDone.Count >= 3)
                {
                    return true;
                }
            }
                     

            return false;           
        }

        public static bool ThirdBoosterAvailable()
        {
            return UserController.Instance.maxUnlockdedGroup >= 3;
        }

        public static bool AllBoostersAvailable()
        {                   
            return UserController.Instance.maxUnlockdedGroup >= 4;
        }

        public static bool FirstBoosterTutorial()
        {                 
                return UserController.Instance.maxUnlockdedGroup >= 1;            
        }

        public static bool SecondBoosterTutorial()
        {
            return UserController.Instance.maxUnlockdedGroup >= 2;

            List<Elements.Cities> cities = Elements.ElementsDatabase.Instance.GetCitiesByGroup(2);

            //cities.Count.LogDev("Cities in g2: ");
            List<Elements.Cities> citiesDone = cities.FindAll(o => o.GetCurrentLanguageSet().succesfullRuns > 0);

            //citiesDone.Count.LogDev("Done: ");

            if (cities.Count == 1)
                return true;

            if (cities.Count < 4)
            {
                //citiesDone.Count.LogDev("Cities less than 3. Done: ");
                if (citiesDone.Count > 1)
                    return true;
            }
            else
            {
                if (citiesDone.Count >= 3)
                {
                    return true;
                }
            }


            return false;
        }

        public static bool ThirdBoosterTutorial()
        {
            return UserController.Instance.maxUnlockdedGroup >= 3;
        }

        public static bool AllBoostersTutorial()
        {         
            return UserController.Instance.maxUnlockdedGroup >= 4;
        }
    }

    public class GameRules : MonoBehaviour
    {

    }

    public static class GameplayRules
    {
        /// <summary>
        /// Decide if visualisation explentation is available.
        /// </summary>
        /// <returns></returns>
        public static bool VisualisationExplenation()
        {
            if (UserController.Instance.MaxUnlockedGroup() < 2)
                return false;

            Elements.GameMode gameMode = UserController.Instance.GetGameMode();

            if (gameMode.id == 1 || gameMode.id == 2)
            {
                //"no visualisation explentaion because of mode".LogDev();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Decide if blinking animation is available.
        /// </summary>
        /// <returns></returns>
        public static bool BlinkingAnimation()
        {
            int maxUnlcocked = UserController.Instance.MaxUnlockedGroup();

            if (maxUnlcocked >= 3)
                return true;

            return false;
        }
    }

    [System.Serializable]
    public class GameBalance
    {
        public static int extendGameplayCost = 1;

        public static int autoRotationBonus = 10;
        public static int highlightBonus = 5;
        public static int noDistractorBonus = 5;
        public static int endlessTimerBonus = 5;

        public static float extendedTimer = 15f;

        public static int replayCost = 10;
        public static int restartCost = 10;

        public static int adsScoreMultiplier = 2;
        public static int adsBalanceReward = 2;

        public static int angleRotationStep = 45;
        public static int rotationAccuracyCheck = 25;
        public static int adsBalanceAvailableTreshold = 15;

        public static float timeForDistractorHint = 5f;

        public static int skipCooloffCost = 15;
        public static int coolOffAdMinutes = 15;

        public static int logInReward = 10;

    }
}