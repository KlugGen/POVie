using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplacementInteractionLayers : Singleton<ReplacementInteractionLayers>
{
    public List<InteractionPair> allowedInteractionsLayers = new List<InteractionPair>();

    public bool IsInteractionAllowed(int layer1, int layer2)
    {
        if (CheckIfContains(new Vector2(layer2, layer1)))
            return true;

        return false;
    }

    private bool CheckIfContains(Vector2 v)
    {
        foreach (InteractionPair ip in allowedInteractionsLayers)
            if ((ip.layerTwo == v.x && ip.layerOne == v.y) || (ip.layerTwo == v.y && ip.layerOne == v.x))
                return true;

        return false;
    }
}

[System.Serializable]
public class InteractionPair 
{
    public int layerOne, layerTwo;
}
