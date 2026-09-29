using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextDeconstructionController : Singleton<TextDeconstructionController>
{
    public Text sentenceText;
    public List<TextDecontruction_Country> countries = new List<TextDecontruction_Country>();

    public TextOverlayController GetMyOverlay(CityController city) {
        foreach(TextDecontruction_Country t in countries)
        {
            if(t.country.ToLower() == city.country.ToLower())
            {
                foreach(TextOverlayController o in t.deconstructions)
                {
                    if (o.city.ToLower() == city.cityName.ToLower())
                    {
                        o.myCity = city;
                        return o;
                    }
                }
            }
        }

        Debug.Log("Overlay null");
        return null;
    }

    public TextDecontruction_Country GetCountry(string countryName)
    {
        return countries.Find(o=>o.country.ToLower() == countryName.ToLower());
    }

    public TextOverlayController GetCity(string countryName, string cityName)
    {
        TextDecontruction_Country country = GetCountry(countryName);
        if (country == null)
            return null;
        
        return country.deconstructions.Find(o => o.city.ToLower() == cityName.ToLower());
    }
}
