using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DKK {
    public class MacroData
    {
        public class Timeline
        {
            public int screenWidth = 1920;
            public int screenHeight = 1080;

            public List<Action> actions = new List<Action>();

            public Timeline()
            {
                screenWidth = Screen.width;
                screenHeight = Screen.height;
            }

            public Timeline(JSONObject json)
            {
                screenWidth = (int)json["sw"].i;
                screenHeight = (int)json["sh"].i;

                JSONObject jsonlist = json["actions"];
                if(jsonlist.list!=null)
                {
                    foreach(JSONObject j in jsonlist.list)
                    {
                        actions.Add(new Action(j));
                    }
                }
                else
                {
                    Debug.Log("Timeline is not a list!");
                }
            }

            public JSONObject GetJson()
            {
                JSONObject json = new JSONObject();
                json.AddField("sw", screenWidth);
                json.AddField("sh", screenHeight);

                JSONObject jsonList = new JSONObject();
                foreach(Action action in actions)
                {
                    jsonList.Add(action.GetJson());
                }
                json.AddField("actions", jsonList);
                return json;
            }

            public void Save()
            {
                File.WriteAllText(Path.Combine(Application.persistentDataPath, "macroRecord"), GetJson().ToString());
            }

            public void Save(string path)
            {
                File.WriteAllText(path, GetJson().ToString());
            }

            static public Timeline Load()
            {
                Timeline timeline = null;
                string str = File.ReadAllText(Path.Combine(Application.persistentDataPath, "macroRecord"));
                if (string.IsNullOrEmpty(str))
                {
                    Debug.Log("Błąd odczytu pliku!");
                }
                else
                {
                    timeline = new Timeline(new JSONObject(str));
                }

                return timeline;
            }

            static public Timeline Load(string filepath)
            {
                Timeline timeline = null;
                string str = File.ReadAllText(filepath);
                if (string.IsNullOrEmpty(str))
                {
                    Debug.Log("Błąd odczytu pliku!");
                }
                else
                {
                    timeline = new Timeline(new JSONObject(str));
                }

                return timeline;
            }

            public void AddAction(Action.ScreenAction screenaction, float timefromstartup)
            {
                actions.Add(new Action(screenaction, timefromstartup));
            }

            public void AddAction(Action.KeyAction keyaction, float timefromstartup)
            {
                actions.Add(new Action(keyaction, timefromstartup));
            }

            public void AddAction(Action.TextFieldAction textfieldaction, float timefromstartup)
            {
                actions.Add(new Action(textfieldaction, timefromstartup));
            }
        }

        public class Action
        {
            public Action(ScreenAction screenaction, float timefromstartup)
            {
                //type = Type.Screen;
                this.textfieldaction = null;
                this.screenAction = screenaction;
                this.keyAction = null;
                this.time = timefromstartup;
            }

            public Action(KeyAction keyaction, float timefromstartup)
            {
                //type = Type.Key;
                this.textfieldaction = null;
                this.screenAction = null;
                this.keyAction = keyaction;
                this.time = timefromstartup;
            }

            public Action(TextFieldAction textfieldaction, float timefromstartup)
            {
                //type = Type.Key;
                this.textfieldaction = textfieldaction;
                this.screenAction = null;
                this.keyAction = null;
                this.time = timefromstartup;
            }

            public Action(JSONObject json)
            {
                if(json.HasField("keyAction"))
                {
                    //type = Type.Key;
                    this.textfieldaction = null;
                    this.screenAction = null;
                    this.keyAction = new KeyAction(json["keyAction"]);
                    this.time = json["time"].f;
                }
                else if(json.HasField("screenAction"))
                {
                    //type = Type.Screen;
                    this.textfieldaction = null;
                    this.screenAction = new ScreenAction(json["screenAction"]);
                    this.keyAction = null;
                    this.time = json["time"].f;
                }
                else if (json.HasField("textfieldaction"))
                {
                    //type = Type.Screen;
                    this.textfieldaction = new TextFieldAction(json["textfieldaction"]);
                    this.screenAction = null;
                    this.keyAction = null;
                    this.time = json["time"].f;
                }
            }

            public JSONObject GetJson()
            {
                JSONObject json = new JSONObject();
                if(keyAction!=null)
                {
                    json.AddField("keyAction", keyAction.GetJson());
                }
                
                if(screenAction!=null)
                {
                    json.AddField("screenAction", screenAction.GetJson());
                }

                if (textfieldaction != null)
                {
                    json.AddField("textfieldaction", textfieldaction.GetJson());
                }

                json.AddField("time", time);
                return json;
            }

            public ScreenAction screenAction;
            public KeyAction keyAction;
            public TextFieldAction textfieldaction;
            //public Type type;

            public float time;

            //public enum Type
            //{
            //    Screen = 0,
            //    Key = 1
            //}

            public class TextFieldAction
            {
                public ObjectInfo target;
                public string value;

                public TextFieldAction(GameObject target, string text)
                {
                    this.target = new ObjectInfo(target);
                    value = text;
                }

                public TextFieldAction(JSONObject json)
                {
                    target = new ObjectInfo(json["target"]);
                    value = json["value"].str;
                }

                public JSONObject GetJson()
                {
                    JSONObject json = new JSONObject();
                    json.AddField("target", target.GetJson());
                    json.AddField("value", value);
                    return json;
                }
            }

            public class ScreenAction
            {
                public ObjectInfo target;
                public float x, y;
                public Phase phase;
                public int btnID;

                public ScreenAction(GameObject target, Vector2 pos, Phase phase, int btnID)
                {
                    this.target = new ObjectInfo(target);
                    x = pos.x;
                    y = pos.y;
                    this.phase = phase;
                    this.btnID = btnID;
                }

                public ScreenAction(JSONObject json)
                {
                    target = new ObjectInfo(json["target"]);
                    x = json["x"].f;
                    y = json["y"].f;
                    phase = (Phase)((int)json["phase"].i);
                    btnID = (int)json["btnID"].i;
                }

                public JSONObject GetJson()
                {
                    JSONObject json = new JSONObject();
                    json.AddField("target", target.GetJson());
                    json.AddField("x", x);
                    json.AddField("y", y);
                    json.AddField("phase", (int)phase);
                    json.AddField("btnID", btnID);
                    return json;
                }

                public enum Phase
                {
                    Down = 0,
                    Up = 1,
                    Click = 2,
                    DragBegin = 3,
                    DragEnd = 4,
                    Drag = 5
                }
            }

            public class KeyAction
            {

                public KeyCode keycode;
                public Phase phase;

                public KeyAction(KeyCode keycode, Phase phase)
                {
                    this.keycode = keycode;
                    this.phase = phase;
                }

                public KeyAction(JSONObject json)
                {
                    keycode = (KeyCode)((int)json["keycode"].i);
                    phase = (Phase)((int)json["phase"].i);
                }

                public JSONObject GetJson()
                {
                    JSONObject json = new JSONObject();
                    json.AddField("keycode", (int)keycode);
                    json.AddField("phase", (int)phase);
                    return json;
                }

                public enum Phase
                {
                    Down = 0,
                    Up = 1
                }
            }
        }

        public class ObjectInfo
        {
            public string name;
            public int ID;

            public ObjectInfo parentInfo;

            public ObjectInfo(GameObject go)
            {
                if (go != null)
                {
                    name = go.name;
                    Transform parent = go.transform.parent;
                    ID = go.transform.GetSiblingIndex();

                    if (parent != null)
                    {
                        parentInfo = new ObjectInfo(parent.gameObject);
                    }
                }
            }

            public ObjectInfo(JSONObject json)
            {
                name = json["name"].str;
                ID = (int)json["ID"].i;
                if(json.HasField("parentInfo"))
                {
                    parentInfo = new ObjectInfo(json["parentInfo"]);
                }
            }

            public JSONObject GetJson()
            {
                JSONObject json = new JSONObject();
                json.AddField("name", name);
                json.AddField("ID", ID);
                if(parentInfo!=null)
                {
                    json.AddField("parentInfo", parentInfo.GetJson());
                }
                return json;
            }
        }
    }
}