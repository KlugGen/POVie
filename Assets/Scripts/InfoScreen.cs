using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoScreen : Singleton<InfoScreen>
{
    public Text infoText;

    public void Enable(string message)
    {
        infoText.text = message;
        Enable();
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public List<UnityEngine.Events.UnityAction> onDisable = new List<UnityEngine.Events.UnityAction>();

    public void Disable()
    {
        gameObject.SetActive(false);
        onDisable.ForEach(o => o.Invoke());
        onDisable.Clear();
    }
}
