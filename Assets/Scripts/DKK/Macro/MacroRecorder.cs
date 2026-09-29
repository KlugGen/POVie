using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DKK
{
    public class MacroRecorder : MonoBehaviour
    {
        public float dragDiff = 3;

        MacroData.Timeline timeline;
        Vector2 pointerposition;
        List<RaycastResult> raycastresult;
        PointerEventData pointer;

        private bool thekeyisdown;
        private bool isdragging;
        //private bool isOnField;
        private GameObject selectedGo;
        private float lastActionTime;
        private Vector3 lastActionPosition;
        private InputField inputfield;
        //private string inputfieldtext;

        public void Record()
        {
            pointer = new PointerEventData(EventSystem.current);
            Debug.Log("Recording started");
            timeline = new MacroData.Timeline();
            lastActionTime = Time.time;
            enabled = true;
        }

        public void Stop()
        {
            enabled = false;
        }

        void Update()
        {

            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
            {
                if (Input.anyKeyDown)
                {
                    inputfield = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
                    //isOnField = true;
                    //inputfieldtext = inputfield.text;

                    Debug.Log("INPUT");
                    //SetPointer();
                    selectedGo = EventSystem.current.currentSelectedGameObject;
                    timeline.AddAction(new MacroData.Action.TextFieldAction(selectedGo, inputfield.text), Time.time - lastActionTime);
                    lastActionTime = Time.time;
                    lastActionPosition = Input.mousePosition;
                }

            }
            else
            {
                //if(isOnField)
                //{
                //    SetPointer();

                //    isOnField = false;
                //    if(inputfield!=null)
                //    {

                //    }
                //}

                if (Input.GetMouseButtonDown(0))
                {
                    SetPointer();

                    thekeyisdown = true;

                    Debug.Log("DOWN");
                    //SetPointer();
                    selectedGo = CurrentSelected<IPointerDownHandler>();
                    if (selectedGo == null)
                    {
                        selectedGo = CurrentSelected<IDragHandler>();
                    }
                    timeline.AddAction(new MacroData.Action.ScreenAction(selectedGo, pointerposition, MacroData.Action.ScreenAction.Phase.Down, 0), Time.time - lastActionTime);
                    lastActionTime = Time.time;
                    lastActionPosition = Input.mousePosition;
                }
                else if (thekeyisdown)
                {
                    SetPointer();

                    Debug.Log("KEY WAS DOWN");
                    if (Input.GetMouseButton(0))
                    {
                        Debug.Log("BTN STILL IS DOWN");
                        if (isdragging)
                        {
                            if (selectedGo == CurrentSelected<IDragHandler>())
                            {
                                Debug.Log("DRAG");
                                //SetPointer();
                                timeline.AddAction(new MacroData.Action.ScreenAction(selectedGo, pointerposition, MacroData.Action.ScreenAction.Phase.Drag, 0), Time.time - lastActionTime);
                                lastActionTime = Time.time;
                                lastActionPosition = Input.mousePosition;
                            }
                        }
                        else
                        {
                            Vector3 diff = lastActionPosition - Input.mousePosition;
                            if (selectedGo == CurrentSelected<IDragHandler>() && (Mathf.Abs(diff.x) > EventSystem.current.pixelDragThreshold || Mathf.Abs(diff.y) > EventSystem.current.pixelDragThreshold))
                            {
                                Debug.Log(selectedGo.name+"------------------------------------------");
                                Debug.Log("DRAG_BEGIN");
                                //SetPointer();
                                timeline.AddAction(new MacroData.Action.ScreenAction(selectedGo, pointerposition, MacroData.Action.ScreenAction.Phase.DragBegin, 0), Time.time - lastActionTime);
                                lastActionTime = Time.time;
                                lastActionPosition = Input.mousePosition;

                                isdragging = true;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("BTN UP");
                        if (isdragging)
                        {
                            Debug.Log("DRAG_END");
                            //SetPointer();
                            timeline.AddAction(new MacroData.Action.ScreenAction(selectedGo, pointerposition, MacroData.Action.ScreenAction.Phase.DragEnd, 0), Time.time - lastActionTime);
                            lastActionTime = Time.time;
                            lastActionPosition = Input.mousePosition;
                        }

                        if (!isdragging && selectedGo == EventSystem.current.currentSelectedGameObject)
                        {
                            Debug.Log("CLICK");
                            //SetPointer();
                            timeline.AddAction(new MacroData.Action.ScreenAction(selectedGo, pointerposition, MacroData.Action.ScreenAction.Phase.Click, 0), Time.time - lastActionTime);
                            lastActionTime = Time.time;
                            lastActionPosition = Input.mousePosition;
                        }
                        else
                        {
                            Debug.Log("UP");
                            //SetPointer();
                            selectedGo = CurrentSelected<IPointerUpHandler>();
                            timeline.AddAction(new MacroData.Action.ScreenAction(selectedGo, pointerposition, MacroData.Action.ScreenAction.Phase.Up, 0), Time.time - lastActionTime);
                            lastActionTime = Time.time;
                            lastActionPosition = Input.mousePosition;
                        }

                        isdragging = false;
                        thekeyisdown = false;
                    }
                }
            }
        }

        private GameObject CurrentSelected<T>()
        {
            GameObject go = EventSystem.current.currentSelectedGameObject;
            if(go==null)
            {
                return Ray<T>();
            }
            return go;
        }

        private GameObject Ray<T>()
        {
            // raycast
            List<RaycastResult> raycastresult = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastresult);
            Debug.Log(raycastresult.Count);
            if(raycastresult.Count>0)
            {
                //foreach (RaycastResult result in raycastresult)
                //{
                //    T mytype = result.gameObject.GetComponent<T>();
                //    Debug.Log((mytype != null) + " " + result.gameObject.name);
                //    if (mytype != null)
                //    {
                //        return result.gameObject;
                //    }
                //}
                GameObject scrollrect = ScrollParent(raycastresult[0].gameObject);
                if (scrollrect != null)
                    return scrollrect;
                else
                    return raycastresult[0].gameObject;
            }
            return null;
        }

        private GameObject ScrollParent(GameObject go)
        {
            Transform t = go.transform;
            bool found = false;
            while(t!=null)
            {
                if(t.GetComponent<ScrollRect>()!=null)
                {
                    found = true;
                    break;
                }
                t = t.parent;
            }

            if(found)
            {
                return t.gameObject;
            }
            return null;
        }

        public string SaveTimeline(string name)
        {
            string macrospath = "Macros";
            macrospath = System.IO.Path.Combine(Application.persistentDataPath, macrospath);
            if(!System.IO.Directory.Exists(macrospath))
            {
                System.IO.Directory.CreateDirectory(macrospath);
            }
            macrospath = System.IO.Path.Combine(macrospath, name);
            timeline.Save(macrospath);

            return macrospath;
        }

        public void SetPointer()
        {
            pointer.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            pointerposition = new Vector2(Input.mousePosition.x/Screen.width, Input.mousePosition.y/Screen.height);
        }
    }
}