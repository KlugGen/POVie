using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScorePrefabUnit : MonoBehaviour
{
    public Text name_text, points_text;
    public Image background;

    public void Initialize(JSONObject j, Color backgroundColor) {
       if(j.HasField("id") && j.HasField("nick") && j.HasField("points"))
        {
            name_text.text = j["id"].i.ToString() + ". " + j["nick"].str;
            points_text.text = j["points"].i.ToString();
        }
        background.color = backgroundColor;
    }
}
