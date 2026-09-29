using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropArea : MonoBehaviour, IMovable
{
    public int shiftID = 1;
    public int orderID = 1;

    public int GetOrderID()
    {
        return orderID;
    }

    public virtual int GetShiftID()
    {
        return shiftID;
    }

    public virtual bool IsInteractable()
    {
        return true;
    }

    public MyEvent onDropActions = null;

    public virtual void OnDrop(GameObject target)
    {      
        if(onDropActions!=null)
         onDropActions.Invoke(target);    

         Debug.Log("Dropped " + target.name + " on " + gameObject.name);    
    }

    public virtual void OnHover(GameObject target)
    {

    }

    public virtual void OnHoverStart(GameObject target)
    {
    }

    public virtual void OnHoverEnd(GameObject target)
    {
    }

    public virtual void OnStart()
    {

    }

    public virtual void OnEnd(int movablesDetected = -1)
    {

    }

    public virtual void OnDrag() { }

    public virtual bool ShouldInterractWithObject(GameObject target)
    {
        return true;
    }
}

[System.Serializable]
public class MyEvent : UnityEngine.Events.UnityEvent<GameObject>
{
}
