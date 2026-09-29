using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DKK;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndScroll : MonoBehaviour
{
    PointerEventData pointer;
    public ScrollRect scrollrect;
    bool dragging = false;
    Vector3 clickPosition;
    public float minDiff = 2;
    private Transform _transform;

    void Awake()
    {
        _transform = transform;
    }

    public void OnMouseDown()
    {

        // Debug.Log("On mouse down");
        if (!dragging)
        {
            //Debug.Log("Click");
            clickPosition = Mouse.Position;
            StartCoroutine(DirectionJudgement());
        }
    }

    IEnumerator DirectionJudgement()
    {
        while (true)
        {
            if (Mouse.Btn)
            {
                //Debug.Log("BTN-----------------"+ Mouse.Btn);

                Vector3 diff = Mouse.Position - clickPosition;
                float x = Mathf.Abs(diff.x);
                float y = Mathf.Abs(diff.y);
                if(x>y)
                {
                    if(x>=minDiff)
                    {
                        //Debug.Log("X");
                        CreatePointer();
                        PointerDragBegin(scrollrect.gameObject);
                        break;
                    }
                }
                else
                {
                    if(y>=minDiff)
                    {
                        //Debug.Log("Y");
                        CreatePointer();
                        RaycastForDragHandler();
                        break;
                    }
                }
                yield return null;
            }
            else
            {
                break;
            }
        }
    }

    public void CreatePointer ()
    {
        pointer = new PointerEventData(EventSystem.current);
        pointer.position = clickPosition;
    }

    private void RaycastForDragHandler()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);
        if (results.Count > 1)
        {
            GameObject newTarget = results[1].gameObject;
            GameObject p = FindDragHandler(newTarget);
            if (p != null && results.FindIndex(x => x.gameObject == p) >= 0)
                newTarget = p;

            PointerDragBegin(newTarget);
        }
        results.Clear();
    }

    private GameObject FindDragHandler(GameObject go)
    {
        Transform t = go.transform;
        while (t != null)
        {
            if (t.GetComponent<IDragHandler>() != null && t!=_transform && t.GetComponent<ScrollRect>() == null)
            {
                return t.gameObject;
            }
            t = t.parent;
        }
        return null;
    }

    private void PointerDragBegin(GameObject dragTarget)
    {
        if (dragTarget != null)
        {
            ExecuteEvents.Execute(dragTarget, pointer, ExecuteEvents.beginDragHandler);
            dragging = true;
            StartCoroutine(Dragging(dragTarget));
            //Debug.Log("Drag START on "+ dragTarget.name);
        }
    }

    IEnumerator Dragging(GameObject dragTarget)
    {
     
        while (true)
        {
            //"dragging".LogDev();
            if (!Mouse.Btn)
            {               
                OnBtnRelease(dragTarget);
                //Debug.Log("Drag RELEASE");
                break;
            }
            else
            {
                pointer.position = Mouse.Position;
                PointerDrag(dragTarget);
                yield return null;
            }
        }
    }

    public void OnBtnRelease(GameObject dragTarget)
    {
        if (pointer == null)
            return;

        dragging = false;
        pointer.position = Mouse.Position;
        PointerDragEnd(dragTarget);
    }

    public void PointerDrag(GameObject dragTarget)
    {
        if (dragTarget != null)
        {
            ExecuteEvents.Execute(dragTarget, pointer, ExecuteEvents.dragHandler);
        }
    }

    public void PointerDragEnd(GameObject dragTarget)
    {
        //Debug.Log("Pointer drag hanbder");
        if (dragTarget != null)
        {
            ExecuteEvents.Execute(dragTarget, pointer, ExecuteEvents.endDragHandler);
        }
    }

    private void OnDisable()
    {
        OnBtnRelease(scrollrect.gameObject);
    }
}
