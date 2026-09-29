using Elements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityStatisticPrefab : MonoBehaviour
{
    public Text name_text, points_text;
    public Image background;
    private Cities _city; 

    public void Initialize(Cities city)
    {
        _city = city;
        name_text.text = string.Format("{0} [{1}]", city.name, city.country.name);

        Cities.BestScorePair pair = city.GetBestScorePair(UserController.Instance.GetLanguage());
        if (city.type == CitiesGameplayType.tutorial)
            points_text.text = TextTranslationModule.GetWord("tutorial");
        else
            points_text.text = string.Format("{0}/{1}", pair.stars, pair.maxStars);
       
        //background.color = backgroundColor;
    }

    public void SetColor(Color c)
    {
        background.color = c;
    }
}
