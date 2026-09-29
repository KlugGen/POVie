using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class Marcin_DragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public ScrollRect scrollRect;

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.parent.GetComponent<LayoutGroup>().enabled = false;
        //scrollRect.enabled = false;
        //throw new System.NotImplementedException();
    }

    private Vector2 dragOffset = new Vector2(0f, 0f);

    public void OnDrag(PointerEventData eventData)
    {
       
        eventData = GetPointerEventDataWithOffset(eventData);
        GetComponent<RectTransform>().position = eventData.position;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //scrollRect.enabled = true;
        transform.parent.GetComponent<LayoutGroup>().enabled = true;
        //throw new System.NotImplementedException();
    }

    private PointerEventData GetPointerEventDataWithOffset(PointerEventData data)
    {
        PointerEventData ped = data;
        ped.position = new Vector2(data.position.x + dragOffset.x, data.position.y + dragOffset.y);

        return ped;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
