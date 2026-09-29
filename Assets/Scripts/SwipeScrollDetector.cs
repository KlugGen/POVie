using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeScrollDetector : MonoBehaviour
{

    public ScrollRect scrollRect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //private Vector2 startPosition;
    //private bool canDo = false;
    //private bool canScroll = false;
    // Update is called once per frame
    //void FixedUpdate()
    //{
    //    if (Input.touches.Length > 0)
    //    {
    //        Touch t = Input.GetTouch(0);
    //        switch (t.phase)
    //        {
    //            case TouchPhase.Began:
    //                // begin = 
    //                startPosition = t.position;
    //                GamePlayController.Instance.GetElementReplacementList().ForEach(o => o.motionAllowed = true);
    //               // GamePlayController.Instance.GetElementReplacementList().ForEach(o => o.enabled = true);
    //                canDo = true;
    //                canScroll = false;
    //                break;

    //            case TouchPhase.Moved:
    //                // moving

    //                // detect moving in vectro and decide
    //                Vector2 v2Start = startPosition - t.position;
    //                Vector2 v2 = new Vector2(Mathf.Abs(v2Start.x), Mathf.Abs(v2Start.y));


    //                if (canDo)
    //                {
                       
    //                    int dragTreshold = UnityEngine.EventSystems.EventSystem.current.pixelDragThreshold;
    //                    //Debug.Log(dragTreshold);

    //                    if (Vector2.Distance(Vector2.zero, v2)>dragTreshold/2 ) {
    //                        // decide
    //                        if (v2.x > v2.y)
    //                        {
    //                            // disable elements dragging

    //                            // allow scrolling
    //                            Debug.Log("Allowing scrolling");
    //                            canScroll = true;
    //                            GamePlayController.Instance.GetElementReplacementList().ForEach(o => o.motionAllowed = false);
                              
    //                            ////GamePlayController.Instance.GetElementReplacementList().ForEach(o => o.enabled = false);
    //                        }
    //                        else {
    //                            // allow dragging
    //                            Debug.Log("Allowing dragging");

    //                           // GamePlayController.Instance.GetElementReplacementList().ForEach(o => o.enabled = true);
    //                        }

    //                        canDo = false;
    //                    }

                      
                        


    //                }

    //                if (canScroll)
    //                    scrollRect.horizontalNormalizedPosition += (v2Start.x / Screen.width)*Time.fixedDeltaTime;

    //                break;

    //            case TouchPhase.Ended:
    //            case TouchPhase.Canceled:
    //                // end
    //                break;
    //        }
    //    }
           


    //}

    // detect that 
}
