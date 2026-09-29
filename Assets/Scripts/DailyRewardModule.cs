
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyRewardModule : Singleton<DailyRewardModule>
{
    // Start is called before the first frame update

    public bool ShouldDisplay()
    {
        string today = ServerTimeController.GetTime().FormatSimple();

        if (!UserController.Instance.GetStats().HasDictionary("dailyRewardDate"))
            UserController.Instance.GetStats().AddString("dailyRewardDate", today);

        string date = UserController.Instance.GetStats().GetString("dailyRewardDate", today);

        if (date == today)
        {
            return false;
        }

        return true;
    }

    public void Check()
    {
        if (ShouldDisplay())
        {
            string today = ServerTimeController.GetTime().FormatSimple();
            UserController.Instance.GetStats().AddString("dailyRewardDate", today);
            GiveReward();
        }
    }

    private void GiveReward()
    {
        //"Adding daily reward".Log();
        UserController.Instance.AddStars(2, "stars_reward");
        DailyRewardController.Instance.EnableSimpleVersion(2);
    }
}
