using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewAttributes<T> 
{
    public T portraitFields, landscapeFields;

    public void Initialize(T portrait, T landscape)
    {
        portraitFields = portrait;
        landscapeFields = landscape;
    }
    
    public T Get()
    {
        if (ViewsController.Instance.GetCurrentOrientation() == ScreenOrientation.Portrait)
            return portraitFields;

        return landscapeFields;
    }

}
