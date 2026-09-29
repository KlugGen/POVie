using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ElementReplacement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public GameObject container;
    public GameObject parent;
    //	private int shiftID;
    private RectTransform _rect;
    //	private List<CanvasGroup> canvasList = new List<CanvasGroup> ();
    //	private CanvasGroup myCanvasGroup = null;
    private List<IMovable> currentList = new List<IMovable>();
    //	private bool interactOnDrop = false;

    public bool motionAllowed = true;

    private IMovable _IMovable;

    private bool raycastAllDuringDraging = false, raycastAllDuringDroping = false;

    public bool duringDragging = false;

    public List<ImovablePair> imovables = new List<ImovablePair>();

    //private int lastChildId = 0;

    void Start() { }

    public void Initialize(GameObject container, GameObject parent, int shiftID, bool interactOnDrop = false, bool raycastAllDuringDraging = false, bool raycastAllDuringDroping = false)
    {
        this.raycastAllDuringDraging = raycastAllDuringDraging;
        this.raycastAllDuringDroping = raycastAllDuringDroping;
        _rect = parent.GetComponent<RectTransform>();
        _IMovable = parent.GetComponent<IMovable>();
        //this.container = container;
        this.parent = parent;
        //		this.shiftID = shiftID;
        //		this.interactOnDrop = interactOnDrop;

        //enabled = false;
    }

    public void SetDragOffset(Vector2 newOffset)
    {
        dragOffset = newOffset;
    }

    private Vector2 dragOffset = new Vector2(0f, 0f);

    public void OnDrag(PointerEventData data)
    {
        
        if (!duringDragging)
            return;

        if (!motionAllowed)
            return;

        data = GetPointerEventDataWithOffset(data);
        _rect.position = data.position;

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(data, results);

        if (_IMovable != null) {
            _IMovable.OnDrag();
        }

        List<IMovable> list = new List<IMovable>();

        List<ImovablePair> movables = new List<ImovablePair>();

        //imovables.Clear();

        foreach (RaycastResult rr in results)
        {
            IMovable im = rr.gameObject.GetComponent<IMovable>();
            if (im != null)
            {
                ImovablePair imp = new ImovablePair(im, rr);
                imp.obj = rr.gameObject;
                movables.Add(imp);
            }
        }

        IEnumerable<ImovablePair> movablesOrdered = movables.OrderByDescending(o => o.iMovable.GetOrderID());

        foreach (ImovablePair im in movablesOrdered.ToList<ImovablePair>())
        {            
            if (im != null && im.iMovable != null)
            {
                if (!im.iMovable.ShouldInterractWithObject(parent))
                    continue;
                
             
                if (im.rResult.gameObject != parent)
                    {
                        list.Add(im.iMovable);
                        im.iMovable.OnHover(parent);
                        //imovables.Add(im);
                    }

                    if (!raycastAllDuringDraging)
                        break;
            }
        }

        CompareLists(list);
    }

    private void CompareLists(List<IMovable> toCompare)
    {
        foreach (IMovable element in currentList)
        {            
            if (!toCompare.Contains(element))
            {
                if (element.ShouldInterractWithObject(parent))
                    element.OnHoverEnd(parent);
            }
        }

        foreach (IMovable element in toCompare)
        {
            if (!currentList.Contains(element))
            {
                if(element.ShouldInterractWithObject(parent))
                    element.OnHoverStart(parent);
            }
        }

        currentList = new List<IMovable>(toCompare);
    }

    public PointerEventData beginDragData;



    public void OnBeginDrag(PointerEventData data)
    {       
        duringDragging = true;

        if (!motionAllowed)
            return;
               
        beginDragData = data;
       
        if (parent.GetComponent<LayoutElement>() != null)
            parent.GetComponent<LayoutElement>().ignoreLayout = true;           

        if (parent.GetComponent<IMovable>() != null)
            parent.GetComponent<IMovable>().OnStart();


        Canvas c = parent.AddComponent<Canvas>();

        if (c != null)
        {
            c.overrideSorting = true;
            c.sortingOrder = 99;
        }
    }

    private PointerEventData GetPointerEventDataWithOffset(PointerEventData data)
    {
        PointerEventData ped = data;
        ped.position = new Vector2(data.position.x + dragOffset.x, data.position.y + dragOffset.y);

        return ped;
    }

    public void OnEndDrag(PointerEventData data)
    {
        //"End drag".Log();

        if (!duringDragging)
            return;

        duringDragging = false;

        if (!motionAllowed)
            return;

        PointerEventData ped = GetPointerEventDataWithOffset(data);

        List<RaycastResult> results = new List<RaycastResult>();
        //ped.position.Log("End drag pos: ");
        EventSystem.current.RaycastAll(ped, results);

        //results.ForEach(delegate(RaycastResult rr) {
        //    IMovable im = rr.gameObject.GetComponent<IMovable>();
        //    if(im != null)
        //    {
        //        Debug.Log("Bef id: " + im.GetOrderID() + im.ToString());
        //        //im.GetOrderID().Log("Bef: ");
        //    }
        //});


        

        List<ImovablePair> movables = new List<ImovablePair>();

        foreach (RaycastResult rr in results)
        {
           
            IMovable im = rr.gameObject.GetComponent<IMovable>();
            if (im != null)
            {
                //rr.gameObject.name.Log();
                movables.Add(new ImovablePair(im, rr));
            }
        }

        IEnumerable<ImovablePair> movablesOrdered = movables.OrderByDescending(o => o.iMovable.GetOrderID());
        int movablesDetected = movablesOrdered.ToList<ImovablePair>().Count;

        foreach (ImovablePair im in movablesOrdered.ToList<ImovablePair>())
        {
            if (im != null && im.iMovable != null)
            {
                if (!im.iMovable.ShouldInterractWithObject(parent))
                    continue;

                if (im.rResult.gameObject != parent)
                {
                    //"drop".Log();
                    im.iMovable.OnDrop(parent);
                }

                if (!raycastAllDuringDroping)
                    break;
            }
        }        

        if (parent.GetComponent<IMovable>() != null)
            parent.GetComponent<IMovable>().OnEnd(movablesDetected);

        if (parent.GetComponent<LayoutElement>() != null)
            parent.GetComponent<LayoutElement>().ignoreLayout = false;

        if (parent.GetComponent<Canvas>() != null)
            Destroy(parent.GetComponent<Canvas>());

        currentList.Clear();
    }    
}

public interface IMovable
{
    int GetOrderID();

    void OnDrag();

    void OnDrop(GameObject target);

    void OnStart();

    void OnEnd(int movablesDetected = -1);

    int GetShiftID();

    bool IsInteractable();

    void OnHover(GameObject target);

    void OnHoverStart(GameObject target);

    void OnHoverEnd(GameObject target);

    bool ShouldInterractWithObject(GameObject target);
}

[System.Serializable]
public class ImovablePair
{
    public IMovable iMovable;
    public RaycastResult rResult;
    public GameObject obj;

    public ImovablePair(IMovable iMovable, RaycastResult rResult)
    {
        this.iMovable = iMovable;
        this.rResult = rResult;
    }
}