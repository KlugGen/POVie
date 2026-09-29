using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DKK
{
    public class MacroManager : MonoBehaviour
    {
        public GameObject savestatepanel;
        public InputField nameField;
        public MacroRecorder recorder;
        public MacroPlayer player;

        [InspectorName("Opcje odtwarzania")]
        public bool play = false;
        public float speedMultiplier = 1;
        public string playScenarioWithName;
        public List<Scenario> scenarios;

        //public GameObject recorderPrefab, playerPrefab;

        //private bool recording = false;

        void Start()
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }

        void Update()
        {
            if (Input.anyKey)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (Input.GetKeyDown(KeyCode.RightShift))
                    {
                        if(play)
                        {
                            if(!player.IsPlaying)
                            {
                                StartCoroutine(Play());
                            }
                        }
                        else
                        {
                            if (!recorder.enabled)
                            {
                                recorder.Record();
                            }
                            else
                            {
                                savestatepanel.SetActive(true);
                                recorder.Stop();
                            }
                        }
                    }
                }
            }
        }

        IEnumerator Play()
        {
            Scenario scenario = scenarios.Find(x => x.scenarioname == playScenarioWithName);
            if (scenario == null)
            {
                Debug.Log("No scenario with given name!");
                yield break;
            }

            foreach (string macro in scenario.macros)
            {
                yield return StartCoroutine(player.Play(macro, speedMultiplier));
            }
        }

        public void SaveState()
        {
            savestatepanel.SetActive(false);
            Debug.Log("SAVED");
            string path = recorder.SaveTimeline(nameField.text);
            TakeScreenshot(path+"_imgs");
        }

        private static void TakeScreenshot(string directory)
        {
            string sName = Screen.width + "_" + Screen.height + "_";
            string extenstion = ".jpg";

            string date = System.DateTime.Now.ToString("h:mm:ss tt").Replace(":", "");
            string endName = sName + date + extenstion;

            Debug.Log("Image resolution: " + Screen.width + "x" + Screen.height);
            Debug.Log("Name: " + endName);

            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(directory, endName));
        }
    }

    [System.Serializable]
    public class Scenario
    {
        public string scenarioname;
        public List<string> macros;
    }

    //[CustomEditor(typeof(MacroManager))]
    //public class MAcroManagerEditor : Editor
    //{
    //    SerializedProperty boolProperty;
    //    SerializedProperty floatProperty;
    //    SerializedProperty intProperty;


    //    void OnEnable()
    //    {
    //        boolProperty = serializedObject.FindProperty("boolVariable");
    //        floatProperty = serializedObject.FindProperty("floatVariable");
    //        intProperty = serializedObject.FindProperty("intVariable");
    //    }


    //    public override void OnInspectorGUI()
    //    {
    //        serializedObject.Update();

    //        EditorGUILayout.Space();

    //        boolProperty.boolValue = EditorGUILayout.Toggle(boolProperty.displayName, boolProperty.boolValue);

    //        if (boolProperty.boolValue)
    //        {
    //            OnBoolPropertyTrue();
    //        }

    //        floatProperty.floatValue = EditorGUILayout.FloatField(floatProperty.displayName, floatProperty.floatValue);
    //        intProperty.intValue = EditorGUILayout.IntField(intProperty.displayName, intProperty.intValue);

    //        serializedObject.ApplyModifiedProperties();
    //    }


    //    void OnBoolPropertyTrue()
    //    {
    //        EditorGUILayout.LabelField("I AM BELOW THE BOOL PROPERTY NOW");
    //    }
    //}
}