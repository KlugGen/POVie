using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DKK
{
    public class Mouse
    {
        public delegate Vector3 Vector3Method();
        public delegate bool BoolMethod();

        static public Vector3Method custom_position_source = null;
        static public BoolMethod custom_down_source = null;
        static public BoolMethod custom_up_source = null;
        static public BoolMethod custom_keep_source = null;

        static public Vector3 Position
        {
            get
            {
#if UNITY_EDITOR
                if (custom_position_source == null)
                {
                    return Input.mousePosition;
                }
                else
                {
                    return custom_position_source.Invoke();
                }
#else
                return Input.mousePosition;
#endif
            }
        }

        static public bool BtnDown
        {
            get
            {
#if UNITY_EDITOR
                if (custom_down_source == null)
                {
                    return Input.GetMouseButtonDown(0);
                }
                else
                {
                    return custom_down_source.Invoke();
                }
#else
                return Input.GetMouseButtonDown(0);
#endif
            }
        }

        static public bool BtnUp
        {
            get
            {
#if UNITY_EDITOR
                if (custom_up_source == null)
                {
                    return Input.GetMouseButtonUp(0);
                }
                else
                {
                    return custom_up_source.Invoke();
                }
#else
                return Input.GetMouseButtonUp(0);
#endif
            }
        }

        static public bool Btn
        {
            get
            {
#if UNITY_EDITOR
                if (custom_keep_source == null)
                {
                    return Input.GetMouseButton(0);
                }
                else
                {
                    return custom_keep_source.Invoke();
                }
#else
                return Input.GetMouseButton(0);
#endif
            }
        }
    }
}