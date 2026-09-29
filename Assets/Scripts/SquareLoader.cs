using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareLoader : Loader
{
    public List<SquareLoaderUnit> squares = new List<SquareLoaderUnit>();
    
    public override void SetPercentage(int percentage)
    {
        
        if (squares.Count == 0) {
            Debug.LogError("No squares in loader");
        }
        float percentageCounter = (float)squares.Count*((float)percentage/100f);
        squares.ForEach(o => o.SetState(false));       

        foreach (SquareLoaderUnit g in squares.GetRange(0, (int)percentageCounter)) {
            g.SetState(true);
        }
    }
}
