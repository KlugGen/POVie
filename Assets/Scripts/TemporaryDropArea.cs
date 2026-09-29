using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryDropArea : DropArea
{
    public override void OnDrop(GameObject target)
    {
        DragableElement de = target.GetComponent<DragableElement>();
        if (de != null)
        {
            de.OnTemporaryAreaDrop();
            //GamePlayController.Instance.UnlockDragging(target);
        }
    }

    public override void OnHoverStart(GameObject target)
    {
        if (target.GetComponent<DragableElement>() != null)
            target.GetComponent<DragableElement>().OnHoverTemporaryAreaStartFeedback();
    }

    public override void OnHoverEnd(GameObject target)
    {
        if (target.GetComponent<DragableElement>() != null)
            target.GetComponent<DragableElement>().OnHoverTemporaryAreaEndFeedback();
    }
}
