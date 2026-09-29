// Copyright for DKK Development (c)
// Jacek Krzysztofiński, Marcin Kubasik, Łukasz Działo
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;

//using UnityEditor.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;

namespace DKK
{
    [InitializeOnLoad]
    public class MyAddons : EditorWindow
    {
        private static Transform tTransform = null;
        private const string catName = "DKK";
        private static Component[] tComponents = null;

        /*
	private static bool isComponentChosen = false;
	private static bool isTransformSelected = false;*/

        //		[MenuItem (catName + "/buttonsUpload", false, 1)]
        //		private static void SetBtns ()
        //		{
        //			Button[] buttons = Resources.FindObjectsOfTypeAll (typeof(Button)) as Button[];
        //			AudioController[] ac = Resources.FindObjectsOfTypeAll (typeof(AudioController)) as AudioController[];
        //			foreach (Button btn in buttons) {
        //				UnityEventTools.AddPersistentListener (btn.onClick, ac [0].ClickSound);
        ////				btn.onClick.SetPersistentListenerState(.AddListener (ac [0].ClickSound);
        //			}
        //		}


        //		[MenuItem (catName + "/Set Keyboard", false, 1)]
        //		private static void SetKeyboard ()
        //		{
        //			InputField[] scripts = Resources.FindObjectsOfTypeAll (typeof(InputField)) as InputField[];
        //			foreach (InputField field in scripts) {
        //				if (field.gameObject.GetComponent<TIM.KeyboardField> () == null)
        //					field.gameObject.AddComponent<TIM.KeyboardField> ();
        //			}
        //		}


        //[MenuItem(catName + "/Demo V Full/Set Demo Version", false, 1)]
        //private static void SetDemo()
        //{
        //    DemoLock[] scripts = Resources.FindObjectsOfTypeAll(typeof(DemoLock)) as DemoLock[];
        //    if (scripts.Length > 0)
        //    {
        //        scripts[0].Demo();
        //    }
        //    else
        //        Debug.LogWarning("No DemoLock scrits!");
        //}

        [MenuItem(catName + "/FindObjects --", false, 1)]
        private static void FindObjects()
        {

            //			Image[] scripts = GameObject.FindObjectsOfType <Image> ();
            ////			scripts.Length.Log ();
            //			foreach (Image i in scripts) {
            //				i.name.Log ();
            //				if (i.sprite != null && i.sprite.texture != null) {
            //					if (i.sprite.texture.name == "Rounded Rectangle 1 copy 8") {
            //						i.gameObject.tag = "_btnYellow";
            ////						i.gameObject.name.Log ();
            //					}
            //
            //					if (i.sprite.texture.name == "blue_rect") {
            //						i.gameObject.tag = "_btnBlue";
            ////						i.gameObject.name.Log ();
            //					}
            //
            //					if (i.sprite.texture.name == "Rectangle 2") {
            //						i.gameObject.tag = "_backgroundGradient";
            ////						i.gameObject.name.Log ();
            //					}
            //
            //
            //				}
            //			}
        }





        // GameObject //////////////////////////////////////////////////////////////////
        //[MenuItem (catName + "/GameObject/Change Active State #&a", false, 1)]
        //private static void SetUnactive ()
        //{
        //	if (Selection.gameObjects.Length > 0)
        //		for (int i = 0; i < Selection.gameObjects.Length; i++)
        //			Selection.gameObjects [i].SetActive (!Selection.gameObjects [i].activeSelf);
        //}

        // # ///////////////////////////////////////////////////////////////////////

        // Layout //////////////////////////////////////////////////////////////////
        [MenuItem(catName + "/Layout/Add margins (0.1f) #&d", false, 1)]
        private static void SetMargins()
        {
            if (Selection.gameObjects.Length > 0)
            {
                for (int i = 0; i < Selection.gameObjects.Length; i++)
                {
                    Selection.gameObjects[i].GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.1f);
                    Selection.gameObjects[i].GetComponent<RectTransform>().anchorMax = new Vector2(0.9f, 0.9f);
                    Selection.gameObjects[i].GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
                    Selection.gameObjects[i].GetComponent<RectTransform>().offsetMin = new Vector2(0f, 0f);
                }
            }
            else
                Debug.LogWarning("Select al least one transform!");

        }

        // # //////////////////////////////////////////////////////////////////

        [MenuItem(catName + "/Editor/ChangeResoulutions", false, 1)]
        private static void Resolutions()
        {
            Debug.Log(Handles.GetMainGameViewSize());

            //return;
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object Res = GetMainGameView.Invoke(null, null);

            EditorWindow v2 = (EditorWindow)Res;
            Rect R = v2.position;
            R.width = 100;
            R.height = 500;
            v2.position = R;
        }
        

        [MenuItem(catName + "/Editor/FindAllPopUps", false, 1)]
        private static void PrepareList()
        {
            List<string> translations = new List<string>();
            JSONObject jo = new JSONObject();
            // find all paths to files inside the project that fullfills condition t: Script (are scriptable)
            string[] allAssets = AssetDatabase.FindAssets("t:Script");

            // remove /Assets at the end of path to the project
            string dataPath = Application.dataPath.Remove(Application.dataPath.Length - 6);

            // for all founded paths
            for (int i = 0; i < allAssets.Length; i++)
            {
                // take current to string
                string filePath = UnityEditor.AssetDatabase.GUIDToAssetPath(allAssets[i]);

                // cobine path in the project with path OF the project to get path to the file on hard-disk
                string fullPath = dataPath + filePath;

                // open file from the path
                System.IO.StreamReader sReader = System.IO.File.OpenText(fullPath);


                string condition = "PopUpMicrophone";

                // read all file
                string content = sReader.ReadToEnd();
                // for all string in conditions array

                // if file contains current condition
                if (content.Contains(condition))
                {

                    // split string to seperate lines
                    string[] lines = content.Split('\n');

                    // create id integer variable
                    int id = -1;

                    // find the last slash from the end...
                    for (int z = filePath.Length - 1; z > 0; z--)
                    {
                        if (filePath[z] == '/')
                        {
                            id = z;
                            break;
                        }
                    }

                    // ... end get name of asset
                    filePath = filePath.Substring(id + 1);

                  
                    // for all lines in current file
                    for (int j = 0; j < lines.Length; j++)
                    {

                        // if current lin in current file contains cuurent condition
                        if (lines[j].Contains(condition))
                        {
                            // except THIS file                                   
                            // add this file to list by creating unit with file name, line, full path
                            var reg = new Regex("\".*?\"");
                            var matches = reg.Matches(lines[j].Replace("\t", ""));
                            foreach (var item in matches)
                            {
                                if (item.ToString().Length < 2)
                                    continue;

                                string word = item.ToString();

                                word = word.Substring(1, word.Length-1);
                                word = word.Substring(0, word.Length-1);

                                if (!translations.Contains(word))
                                {                                  
                                    translations.Add(word);
                                    jo.Add(word);
                                }

                                //Debug.Log(item.ToString());
                            }

                           // Debug.Log(lines[j].Replace("\t", ""));
                        }

                    }
                   
                }

            }

            translations.ForEach(o => o.Log());
            jo.ToString().Log();
        }

        // Screenshots //////////////////////////////////////////////////////////////////
        [MenuItem(catName + "/Camera/Take Screenshot #&c", false, 1)]
        private static void TakeScreenshot()
        {
            string sName = Screen.width + "_" + Screen.height + "_";
            string extenstion = ".jpg";

            string date = System.DateTime.Now.ToString("h:mm:ss tt").Replace(":", "");
            string endName = sName + date + extenstion;

            Debug.Log("Image resolution: " + Screen.width + "x" + Screen.height);
            Debug.Log("Name: " + endName);

            ScreenCapture.CaptureScreenshot(endName);
        }

        [MenuItem(catName + "/Camera/Take Super Screenshot", false, 1)]
        private static void TakeSuperScreenshot()
        {
            string sName = Screen.width + "_" + Screen.height + "_";
            string extenstion = ".jpg";

            string date = System.DateTime.Now.ToString("h:mm:ss tt").Replace(":", "");
            string endName = sName + date + extenstion;

            Debug.Log("Image resolution: " + Screen.width + "x" + Screen.height);
            Debug.Log("Name: " + endName);

            ScreenCapture.CaptureScreenshot("super" + endName, 2);
        }

        // Transform	//////////////////////////////////////////////////////////////
        // Shift+Alt+S
        [MenuItem(catName + "/Transform/Revert Scale #&s", false, 1)]

        private static void ScaleInvert()
        {
            if (Selection.transforms.Length > 0)
            {
                for (int i = 0; i < Selection.transforms.Length; i++)
                {

                    if (Selection.transforms[i].localScale == Vector3.zero)
                        Selection.transforms[i].localScale = Vector3.one;
                    else if (Selection.transforms[i].localScale == Vector3.one)
                        Selection.transforms[i].localScale = Vector3.zero;
                }
            }
            else
                Debug.LogWarning("Select al least one transform!");
        }


        [MenuItem(catName + "/Transform/Copy Values", false, 1)]
        private static void CopyCurrentTransform()
        {
            if (Selection.transforms.Length == 1)
            {
                tTransform = Selection.transforms[0];
                //isTransformSelected = true;
                Debug.Log("Done !");
            }
            else
                Debug.LogWarning("Select one transform!");
        }

        [MenuItem(catName + "/Transform/Paste Position", false, 51)]
        private static void PastePosition()
        {
            PasteValues(0);
        }

        [MenuItem(catName + "/Transform/Paste Position", true)]
        private static bool _PastePosition()
        {
            return tTransform != null;
        }


        [MenuItem(catName + "/Transform/Paste Rotation", false, 52)]
        private static void PasteRotation()
        {
            PasteValues(1);
        }

        [MenuItem(catName + "/Transform/Paste Rotation", true)]
        private static bool _PasteRotation()
        {
            return tTransform != null;
        }


        [MenuItem(catName + "/Transform/Paste Scale", false, 53)]
        private static void PasteScale()
        {
            PasteValues(2);
        }

        [MenuItem(catName + "/Transform/Paste Scale", true)]
        private static bool _PasteScale()
        {
            return tTransform != null;
        }

        [MenuItem(catName + "/Transform/Paste All", false, 54)]
        private static void PasteAllValues()
        {
            PasteValues(3);
        }

        [MenuItem(catName + "/Transform/Paste All", true)]
        private static bool _PasteAllValues()
        {
            return tTransform != null;
        }

        private static void PasteValues(int id)
        {
            if (tTransform != null)
            {
                if (Selection.transforms.Length > 0)
                {

                    for (int i = 0; i < Selection.transforms.Length; i++)
                    {

                        switch (id)
                        {
                            case 0:
                                Selection.transforms[i].position = tTransform.position;
                                break;
                            case 1:
                                Selection.transforms[i].rotation = tTransform.rotation;
                                break;
                            case 2:
                                Selection.transforms[i].localScale = tTransform.localScale;
                                break;
                            case 3:
                                Selection.transforms[i].position = tTransform.position;
                                Selection.transforms[i].rotation = tTransform.rotation;
                                Selection.transforms[i].localScale = tTransform.localScale;
                                break;
                        }
                    }

                    tTransform = null;
                    Debug.Log("Done for " + Selection.transforms.Length + " transforms.");

                }
                else
                    Debug.LogWarning("Select at least one transform!");
            }
            else
                Debug.LogWarning("Use 'Copy Values' option first!");

        }

        [MenuItem(catName + "/Transform/Clear Values", false, 101)]
        private static void ClearAll()
        {

            if (Selection.transforms.Length > 0)
            {
                for (int i = 0; i < Selection.transforms.Length; i++)
                {
                    Selection.transforms[i].position = Vector3.zero;
                    Selection.transforms[i].rotation = Quaternion.identity;
                    Selection.transforms[i].localScale = Vector3.one;
                }

                Debug.Log("Done for " + Selection.transforms.Length + " transforms.");

            }
            else
                Debug.LogWarning("Select at least one transform!");
        }

        // Components ////////////////////////////////////////////////////

        [MenuItem(catName + "/Components/Copy all components", false, 1)]
        private static void CopyComponents()
        {
            if (Selection.transforms.Length == 1)
            {
                tComponents = Selection.gameObjects[0].GetComponents<Component>();
                Debug.Log(tComponents[0]);
            }
            else
                Debug.LogWarning("Select one transform!");
        }

        //	[MenuItem(catName+ "/Components/Copy Rigidbody",false, 51)]
        //	private static void GetRigidbody ()
        //	{
        //		GetMyComponent ("Rigidbody");
        //		Debug.Log (Selection.activeGameObject.name);
        //	}
        //
        //
        //
        //	[MenuItem(catName+ "/Components/Copy Rigidbody2D",false, 51)]
        //	private static void GetRigidbody2D ()
        //	{
        //		GetMyComponent ("Rigidbody2D");
        //	}
        //
        //
        //	private static void GetMyComponent (string nm)
        //	{
        //		if (Selection.transforms.Length == 1) {
        //
        //			switch (nm) {
        //			case "Rigidbody":
        //				tComponents = Selection.gameObjects [0].GetComponents <Rigidbody> ();
        //				break;
        //			case "Rigidbody2D":
        //				tComponents = Selection.gameObjects [0].GetComponents  <Rigidbody2D> ();
        //				break;
        //
        //			}
        //
        //			if (tComponents.Length > 0) {
        //
        //				Debug.Log (nm + " component copied!");
        //
        //			} else {
        //				tComponents = null;
        //				Debug.LogWarning ("No" + nm + " component!");
        //			}
        //		} else
        //			Debug.LogWarning ("Select one transform!");
        //	}

        [MenuItem(catName + "/Components/Paste components", false, 101)]
        private static void PasteComponent()
        {
            if (tComponents != null)
            {
                if (Selection.transforms.Length == 1)
                {

                    for (int i = 0; i < tComponents.Length; i++)
                        Selection.gameObjects[0].AddComponent(tComponents[i].GetType());

                    Debug.Log("Copied " + tComponents.Length + " components!");
                    tComponents = null;

                }
                else
                    Debug.LogWarning("Select one transform!");
            }
            else
                Debug.LogWarning("Use 'Copy Components' option first!");
        }

        [MenuItem(catName + "/UI Materials/Set optimized UI materials", false, 1)]
        private static void SetOptimizedUI()
        {
            Material mat0 = Resources.Load("Materials/defaultUI") as Material;
            Material mat1 = Resources.Load("Materials/defaultUI_NoMask") as Material;

            Image[] images = Resources.FindObjectsOfTypeAll(typeof(Image)) as Image[];
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].material == images[i].defaultMaterial)
                {
                    if (IsMaskRequired(images[i].transform))
                        images[i].material = mat0;
                    else
                        images[i].material = mat1;
                }
            }
        }


        [MenuItem(catName + "/FindAllCanvasRenderers", false, 1)]
        private static void GetCR()
        {
            //		GameObject[] gos = Resources.FindObjectsOfTypeAll (typeof(GameObject)) as GameObject[];
            //		List<CanvasRenderer> crs = new List<CanvasRenderer> ();

            //		foreach (GameObject g in gos) {
            //			if (g.GetComponent<CanvasRenderer> () != null && g.GetComponents<Component> ().Length < 3) {
            //				g.name.Log ();
            //			}
            //		}
        }


        [MenuItem(catName + "/UI Materials/Set optimized UI materials (all with masks)", false, 1)]
        private static void SetOptimizedWithMaskUI()
        {
            Material mat0 = Resources.Load("Materials/defaultUI") as Material;

            Image[] images = Resources.FindObjectsOfTypeAll(typeof(Image)) as Image[];
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].material != mat0)
                {
                    images[i].material = mat0;
                }
            }
        }

        [MenuItem(catName + "/UI Materials/Replace null fileds with texture", false, 1)]
        private static void ReplaceNullImages()
        {
            if (a_properties.emptyImage == null)
            {
                Debug.LogError("You have to choose empty image in Configuration window!");
                return;
            }

            Image[] images = Resources.FindObjectsOfTypeAll(typeof(Image)) as Image[];
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].sprite == null)
                {
                    images[i].sprite = a_properties.emptyImage;
                }
            }
        }

        [MenuItem(catName + "/UI Materials/Remove optimized UI materials", false, 1)]
        private static void RemoveOptimizedUI()
        {
            Material mat0 = Resources.Load("Materials/defaultUI") as Material;
            Material mat1 = Resources.Load("Materials/defaultUI_NoMask") as Material;

            Image[] images = Resources.FindObjectsOfTypeAll(typeof(Image)) as Image[];

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].material == mat0 || images[i].material == mat1)
                {
                    images[i].material = null;
                }
            }
        }

        private static bool IsMaskRequired(Transform trans)
        {
            if (EditorUtility.IsPersistent(trans.gameObject))
                return true;
            while (trans.parent != null)
            {
                if (trans.GetComponent<Mask>() != null || trans.GetComponent<RectMask2D>() != null)
                    return true;
                else
                    trans = trans.parent;
            }
            return false;
        }

        [MenuItem(catName + "/Components/Paste components", true)]
        private static bool _PasteComponent()
        {
            return tComponents != null;
        }

        //	[MenuItem ("GameObject/NEW!!", false, 0)]
        //	private static bool _PastesComponent ()
        //	{
        //		return tComponents != null;
        //	}


        // # //////////////////////////////////////////////////////////////////

        float myMargins = 0f;
        bool groupEnabled;
        bool tl = true, tr = true, bl = true, br = true;

        // Add menu item named "My Window" to the Window menu
        [MenuItem(catName + "/Window/My Window")]
        private static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(MyAddons));
        }

        [MenuItem(catName + "/Open persistent data path", false, 1)]
        private static void OpenPDP()
        {
            Application.OpenURL(Application.persistentDataPath);
        }

        void OnGUI()
        {
            GUILayout.Label("Set up margins", EditorStyles.boldLabel);
            myMargins = EditorGUILayout.FloatField("Margins", myMargins);

            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            tl = EditorGUILayout.Toggle("Top-Left", tl);
            tr = EditorGUILayout.Toggle("Top-Right", tr);
            bl = EditorGUILayout.Toggle("Bottom-Left", bl);
            br = EditorGUILayout.Toggle("Bottom-Right", br);
            EditorGUILayout.EndToggleGroup();

            if (GUILayout.Button("Button"))
            {
                Debug.Log("Button!");
            }
        }

    }
}
#endif