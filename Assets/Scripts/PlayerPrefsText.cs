using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof( Text))]
public class PlayerPrefsText : MonoBehaviour
{
    private Text _text;
    public string playerPrefsKey;
    public string format;

    private void Awake()
    {
       
      
        return;
        if (!string.IsNullOrEmpty(playerPrefsKey))
        {
            // TODO: change to get from playerprefs
            _text.text = string.Format(format,  UserController.Instance.GetStars());
        }
    }

    public void OnEnable()
    {
        _text = GetComponent<Text>();
        _text.text = string.Format(format, UserController.Instance.GetStars());
    }
}
