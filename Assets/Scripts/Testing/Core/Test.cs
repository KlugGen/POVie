using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace DKK.Testing
{
    public class Test : MonoBehaviour
    {
        public string testName;
        public float timeout = -1f;
        public bool readyToGo = true;
        public int iterations = 1;
        //private float timer = 0f;
        //private bool timeoutCheck = false;

        public virtual void BeforeAction()
        {
            //if (timeout > 0f)
            //{
            //    timeoutCheck = true;
            //    timer = timeout;
            //}
        }

        public virtual void AfterAction()
        {

        }

        void Update()
        {

        }

        [NaughtyAttributes.ReorderableList]
        public List<TestUnit> actions = new List<TestUnit>();

        public virtual IEnumerator CoroutineTest(bool random = false)
        {
            foreach (TestUnit tu in actions)
            {
                bool result = true;

                tu.unitName.Log("=== Starting action: ");
                if (tu.waitFrame)
                    yield return new WaitForEndOfFrame();
                else
                    yield return new WaitForSeconds(tu.timeBefore);

                if (tu.type == TestUnit.TestUnitType.BUTTON || tu.type == TestUnit.TestUnitType.LIST)
                {
                    Button b = tu.GetButton(random);
                    if (!b.gameObject.activeInHierarchy)
                    {
                        result = false;
                        Debug.LogWarning(string.Format("Button '{0}' not active", b.gameObject.name));
                        yield break;
                    }
                    b.onClick.Invoke();
                }
                else if (tu.type == TestUnit.TestUnitType.ACTION)
                {
                    if (tu.action == null)
                    {
                        result = false;
                        Debug.LogWarning(string.Format("Actions is null"));
                        yield break;
                    }

                    try
                    {
                        tu.action.Invoke();
                    }catch(System.Exception e)
                    {
                        result = false;
                        Debug.LogError(string.Format("Exception: {0}", e.ToString()));
                        yield break;
                    }
                }
                else if (tu.type == TestUnit.TestUnitType.WAIT_FOR_ACTIVE_OBJECT)
                {
                    while (!tu.go.activeInHierarchy)
                    {
                        yield return null;
                    }                   
                }

                if (tu.extraCoroutine != null && tu.extraCoroutine != "")
                {
                    tu.extraCoroutine.LogDev("Running: ");
                    UnityEngine.Coroutine c = null;
                    MethodInfo mi = this.GetType().GetMethod(tu.extraCoroutine);
                    if (mi != null && mi.ReturnType.Name == "IEnumerator")
                    {
                        c = StartCoroutine(tu.extraCoroutine);
                        yield return c;
                    }
                }

                if (tu.waitFrame)
                    yield return new WaitForEndOfFrame();
                else
                    yield return new WaitForSeconds(tu.timeAfter);

                if(result)
                    Debug.Log("=== [OK]");
                else
                    Debug.Log("=== [ERROR]");
            }
        }
    }

    [System.Serializable]
    public class TestUnit
    {
        public enum TestUnitType
        {
            BUTTON,
            LIST,
            CLICK,
            ACTION,
            WAIT_FOR_ACTIVE_OBJECT
        }
        public string unitName = "Test Unit";

        public TestUnitType type = TestUnitType.BUTTON;

        public GameObject go;
        public bool waitFrame = false;
        public float timeBefore = 0f;
        public float timeAfter = 1f;
        public string arg;
        public UnityEngine.Events.UnityEvent action = null;

        public string extraCoroutine = null;

        public Button GetButton(bool random)
        {
            switch (type)
            {
                case TestUnitType.LIST:
                    Button b = null;
                    if (!random)
                        b = TestUnit.GetFirstButtonFromParent(go);
                    else
                        b = TestUnit.GetRandomButtonFromParent(go);

                    return b;
            }

            return go.GetComponent<Button>();
        }

        public static Button GetFirstButtonFromParent(GameObject g)
        {
            Button b = g.transform.GetComponentInChildren<Button>();
            return b;
        }
        public static Button GetRandomButtonFromParent(GameObject g)
        {
            List<Button> b = g.transform.GetComponentsInChildren<Button>().ToList().FindAll(o => o.gameObject.activeInHierarchy == true);
            if (b.Count > 0)
            {
                Button marker = b[Random.Range(0, b.Count)];
                return marker;
            }
            return null;
        }
    }





    //[CustomEditor(typeof(TestUnit))]
    //public class TestUnitEditor : Editor
    //{
    //    public override void OnInspectorGUI()
    //    {
    //        EditorGUILayout.PropertyField(serializedObject.FindProperty("go"));
    //        serializedObject.ApplyModifiedProperties();
    //    }

    //}
}

