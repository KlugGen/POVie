using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseArea : DropArea
{
    public SpotController spotController;


    //[UnityEditor.Callbacks.DidReloadScripts]
    //private static void OnScriptsReloaded()
    //{
    //    // do something
    //    Debug.Log("close"); 
    //}

    public override void OnHoverStart(GameObject target)
    {
        
        DragableElement dragable = target.GetComponent<DragableElement>();
        if (dragable != null)
        {
            if (spotController != null )
            {
                if (spotController.spot.degrees == dragable.GetDegrees())
                {                  
                    dragable.OnHoverCloseAreaStartFeedback(gameObject);
                }
            }
            else
            {
                dragable.OnHoverCloseAreaStartFeedback(gameObject);
            }
        }
    }

    public override void OnHoverEnd(GameObject target)
    {
        DragableElement dragable = target.GetComponent<DragableElement>();
        if (dragable != null)
        {
            if (spotController != null)
            {
                if (spotController.spot.degrees == dragable.GetDegrees())
                {
                    dragable.OnHoverCloseAreaEndFeedback(gameObject);
                }
            }
            else
            {
                dragable.OnHoverCloseAreaEndFeedback(gameObject);
            }
        }       
    }

    public override void OnDrop(GameObject obj)
    {
        DragableElement de = obj.GetComponent<DragableElement>();
        if (de != null)
        {
            if (spotController != null)
            {
                if (spotController.spot.degrees == de.GetDegrees())
                {
                    if (GamePlayController.Instance.bonuses.autoRotationActive)                    
                        spotController.spot.OnDrop(obj);                    
                    else                    
                        de.OnCloseAreaDrop(gameObject);
                }
                else
                {
                    de.OnTemporaryAreaDrop();
                }
            }
            else
            {
                if (GamePlayController.Instance.bonuses.autoRotationActive)
                    spotController.spot.OnDrop(obj);
                else
                    de.OnCloseAreaDrop(gameObject);
            }
        }
    }

    private float timer = 5f;

    private void Update()
    {
        if (flashEnabled)
        {
            timer -= Time.deltaTime;
            if(timer < 0f)
            {
                FlashDisable();
            }
        }
    }

    public bool flashEnabled = false;

    public void FlashEnable()
    {
        timer = 5f;
        flashEnabled = true;
        Image i = GetComponent<Image>();
        i.sprite = SpritesDatabase.Instance.flashRectangle;
        i.pixelsPerUnitMultiplier = 3;
        i.type = Image.Type.Simple;
        Color c = Color.white;
        c.a = 1f;
        i.color = c;

        Button b = GetComponent<Button>();
        if (b == null)
            b = gameObject.AddComponent<Button>();
        else
        {
            b.onClick.RemoveAllListeners();
        }

        //b.onClick.AddListener(delegate() {
           
        //    GamePlayController.Instance.HighlightedElementsInfo();
        //});

        // TODO: uncomment if needed

        //Image image = spotController.spot.GetComponent<Image>();

        //if (image!=null)
        //    image.raycastTarget = false;

    }

    public void FlashDisable()
    {
        timer = 5f;
        flashEnabled = false;
        //"Flash dis".Log();
        Image i = GetComponent<Image>();
        i.sprite = SpritesDatabase.Instance.defaultBackgroud;
        Color c = GetComponent<Image>().color;
        c.a = 0;
        i.color = c;


        Button b = GetComponent<Button>();
        if (b != null)
            Destroy(b);

        Image image = spotController.spot.GetComponent<Image>();

        if (image != null)
            image.raycastTarget = true;
    }

    public void Reset()
    {
        FlashDisable();
    }

    public override bool ShouldInterractWithObject(GameObject target)
    {
        DragableElement de = target.GetComponent<DragableElement>();
        if (de != null)
        {
            if (spotController != null)
            {
                if (spotController.spot.degrees == de.GetDegrees())
                {
                    return true;
                }

            }
        }

        return false;
    }
}
