using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UI;

namespace DKK
{
    public class MacroPlayer : MonoBehaviour
    {
        MacroData.Timeline timeline;
        PointerEventData pointer;
        public bool rayIfNameDifference = false;
        private Vector2 lastPosition;

        private bool up, down, keep;

        public bool IsPlaying
        {
            get
            {
                return playing;
            }
        }

        private bool playing = false;

        public Vector3 MousePosition()
        {
            return pointer.position;
        }

        public bool MouseBtnDown()
        {
            return down;
        }

        public bool MouseBtnUp()
        {
            return up;
        }

        public bool MouseBtn()
        {
            Debug.Log("BB");
            return keep;
        }

        private void SetMouseSources()
        {
            Mouse.custom_down_source = MouseBtnDown;
            Mouse.custom_up_source = MouseBtnUp;
            Mouse.custom_keep_source = MouseBtn;
            Mouse.custom_position_source = MousePosition;
        }

        private void DefaultMouseSources()
        {
            Mouse.custom_down_source = null;
            Mouse.custom_up_source = null;
            Mouse.custom_keep_source = null;
            Mouse.custom_position_source = null;
        }

        public IEnumerator Play(string filename, float speedMultiplier = 1)
        {
            SetMouseSources();

            Debug.Log("PLAYER RUN");

            string macrospath = "Macros";
            macrospath = System.IO.Path.Combine(System.IO.Path.Combine(Application.persistentDataPath, macrospath), filename);

            pointer = new PointerEventData(EventSystem.current);
            timeline = MacroData.Timeline.Load(macrospath);

            Screen.SetResolution(timeline.screenWidth, timeline.screenHeight, false);

            playing = true;

            if (timeline.actions.Count > 0)
            {
                GameObject lastone = null;
                int id = 0;
                while (timeline.actions.Count-1>=id)
                {
                    MacroData.Action action = timeline.actions[id];

                    yield return new WaitForSeconds(action.time / speedMultiplier);

                    if(action.screenAction!=null)
                    {
                        pointer.position = new Vector2(action.screenAction.x * Screen.width, action.screenAction.y * Screen.height);

                        GameObject go;
                        switch (action.screenAction.phase)
                        {
                            case MacroData.Action.ScreenAction.Phase.Down:
                                go = GetGameObject<IPointerDownHandler>(action.screenAction.target);
                                if (go != null)
                                {
                                    Debug.Log("DOWN with any object "+ go.name);
                                    down = true;
                                    keep = true;
                                    ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerDownHandler);

                                    lastone = go;
                                }
                                else
                                    Debug.Log("Cannoct interract DOWN with any object");
                                break;

                            case MacroData.Action.ScreenAction.Phase.Up:
                                go = GetGameObject<IPointerUpHandler>(action.screenAction.target);
                                if (go != null)
                                {
                                    Debug.Log("UP with any object "+ go.name);
                                    up = true;
                                    keep = false;
                                    ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerUpHandler);

                                    lastone = go;
                                }
                                else
                                    Debug.Log("Cannoct interract UP with any object");
                                break;

                            case MacroData.Action.ScreenAction.Phase.Click:
                                go = lastone;//GetGameObject<IPointerClickHandler>(action.screenAction.target);
                                if (go != null)
                                {
                                    ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerClickHandler);
                                    if(go.GetComponent<IPointerUpHandler>()!=null)
                                    {
                                        Debug.Log("CLICK with any object "+ go.name);
                                        up = true;
                                        keep = false;
                                        ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerUpHandler);
                                    }
                                }
                                else
                                    Debug.Log("Cannoct interract CLICK with any object");
                                break;

                            case MacroData.Action.ScreenAction.Phase.DragBegin:
                                go = GetGameObject<IDragHandler>(action.screenAction.target);
                                if (go != null)
                                {
                                    pointer.dragging = true;
                                    Debug.Log("DRAG BEGIN with any object " + go.name);
                                    ExecuteEvents.Execute(go, pointer, ExecuteEvents.beginDragHandler);
                                    lastone = go;
                                }
                                else
                                    Debug.Log("Cannoct interract DRAG BEGIN with any object");
                                break;

                            case MacroData.Action.ScreenAction.Phase.DragEnd:
                                go = lastone;
                                if (go != null)
                                {
                                    pointer.dragging = false;
                                    Debug.Log("END DRAG with any object " + go.name);
                                    ExecuteEvents.Execute(go, pointer, ExecuteEvents.endDragHandler);
                                }
                                else
                                    Debug.Log("Cannoct interract DRAG END with any object");
                                break;

                            case MacroData.Action.ScreenAction.Phase.Drag:
                                go = lastone;
                                Debug.Log("DRAGGGGG");
                                if (go != null)
                                {
                                    pointer.delta = pointer.position - lastPosition;
                                    Debug.Log(pointer.dragging);
                                    Debug.Log("DRAG with any object " + go.name);
                                    ExecuteEvents.Execute(go, pointer, ExecuteEvents.dragHandler);
                                }
                                else
                                    Debug.Log("Cannoct interract DRAG with any object");
                                break;
                        }

                        lastPosition = pointer.position;
                    }
                    else if(action.textfieldaction != null)
                    {
                        GameObject go = GetGameObject<InputField>(action.textfieldaction.target);
                        if (go != null)
                        {
                            Debug.Log("INPUT TEXT with any object " + go.name);
                            go.GetComponent<InputField>().text = action.textfieldaction.value;
                        }
                        else
                            Debug.Log("Cannoct interract DOWN with any object");
                    }

                    id++;


                    yield return null;
                    up = down = false;
                }
            }

            playing = false;

            DefaultMouseSources();
        }

        private GameObject GetGameObject<T>(MacroData.ObjectInfo info)
        {
            GameObject go = null;

            // list hierarchy
            List<string> objs = new List<string>();
            List<int> sids = new List<int>();

            MacroData.ObjectInfo i = info;
            while (i!=null)
            {
                objs.Add(i.name);
                sids.Add(i.ID);
                i = i.parentInfo;
            }

            // find object by hierarchy
            bool diffNameAndRay = false;
            GameObject[] roots = gameObject.scene.GetRootGameObjects();
            Transform t = null;
            if (roots.Length-1>=sids.Last() && roots[sids.Last()].name == objs.Last())
            {
                t = roots[sids.Last()].transform;
                for (int j = objs.Count-2; j >= 0; j--)
                {
                   //Debug.Log(t.name + " " + t.childCount + " " + sids[j] + " " + (t.childCount - 1 >= sids[j]));
                    if (t.childCount - 1 >= sids[j])
                    {
                        Transform child = t.GetChild(sids[j]);
                        //Debug.Log(child.name + " " + objs[j] + " " + (child.name == objs[j]));
                        if (!rayIfNameDifference || child.name == objs[j])
                        {
                            t = child;
                            //Debug.Log("---- new child " + t.name + "----");
                        }
                        else
                        {
                            diffNameAndRay = true;
                            break;
                        }
                    }
                    else
                    {
                        diffNameAndRay = true;
                        break;
                    }
                }
            }

            //Debug.Log("T RES: "+t.name+ " " + rr.Count);

            if(diffNameAndRay)
            {

                if (rayIfNameDifference)
                {
                    List<RaycastResult> rr = Ray();
                    if (rr.Count > 0)
                    {
                        go = rr[0].gameObject;
                    }
                }
                else
                {
                    Debug.Log("DIFFERENT OBJECT NAME");
                }
            }
            //else if(rr.Count>0 && t!=null)
            //{
            //    RaycastResult r = rr.Find(u => u.gameObject.name == t.name);
            //    go = r.gameObject;
            //    Debug.Log(t.name);
            //    Debug.Log(r);
            //    Debug.Log(r.gameObject);
            //    Debug.Log(r.gameObject.name);
            //    Debug.Log(t.name + " " + r.gameObject.name);
            //}
            else if(t!=null)
            {
                go = t.gameObject;
            }

            return go;
        }

        private List<RaycastResult> Ray()
        {
            // raycast
            List<RaycastResult> rr = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, rr);
            //Debug.Log("###" + x + "," + y + "###");
            //foreach (RaycastResult a in rr)
            //{
            //    Debug.Log("----" + a.gameObject + "----");
            //}

            return rr;
        }
    }
}