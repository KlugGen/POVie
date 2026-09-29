using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

namespace DKK
{
	//	#if UNITY_EDITOR
	//	[ExecuteInEditMode]
	//	#endif
	public class ConsoleManager : MonoBehaviour
	{
		public bool logWhenDisabled = true;
		public int maxLogSize = 5000;

		[Tooltip ("Colors for types: \n\t\tError,\n\t\tAssert,\n\t\tWarning,\n\t\tLog,\n\t\tException")]

		public Color[] logsColors = new Color[5];

		public int maxLogsCount = 50;
		//		public int removeIfOver = 10;
		private long counter = 0;

		//		public Text txt;
		public InputField commandField;
		public GameObject console;

		public RectTransform container;
		//		private ScrollRect scrollRect;
		private float lastYPlus, lastYMinus;

		public List<ConsoleLog> logs = new List<ConsoleLog> ();
		private LinkedList<RectTransform> texts = new LinkedList<RectTransform> ();
		public RectTransform paddingTop, paddingBottom;
		public GameObject prefab;
		private int firstID, lastID = -1;
		private int textsObjLimit = 40;

		private Dictionary<string,ConsoleCommand> ConsoleCommands = new Dictionary<string, ConsoleCommand> ();
		static private ConsoleManager self;

		static public bool Blocking = false;

		//		private bool running = true;

		public List<ConsoleCommand> commands = new List<ConsoleCommand> ();

		private StrUnityEvent lastEvent = null;
		private string lastargs = null;

		void Awake ()
		{
			self = this;
//			scrollRect = container.parent.GetComponent<ScrollRect> ();
//			scrollRect = container.transform.parent.GetComponent<RectTransform> ();

			textsObjLimit = (int)UICalc.FindMyCanvas (console.transform).GetComponent<RectTransform> ().rect.height / 26 + 10;

			for (int i = 0; i < commands.Count; i++) {
				ConsoleCommands [commands [i].key] = commands [i];
			}
				
			StrUnityEvent pd = new StrUnityEvent ();
			pd.AddListener (PageDown);
			ConsoleCommands ["pagedown"] = new ConsoleCommand ("pagedown", pd);

			StrUnityEvent pu = new StrUnityEvent ();
			pu.AddListener (PageUp);
			ConsoleCommands ["pageup"] = new ConsoleCommand ("pageup", pu);

//			if (Application.isPlaying)
			//Debug.Log (Application.isPlaying);

			Application.logMessageReceived += CaptureLog;
		}

		static public void RemoveFromListener ()
		{
			Application.logMessageReceived -= self.CaptureLog;
		}

		public void Clear ()
		{
			logs.Clear ();
			paddingTop.sizeDelta = new Vector2 (paddingTop.sizeDelta.x, 0);
			paddingTop.transform.SetAsFirstSibling ();
			paddingBottom.sizeDelta = new Vector2 (paddingTop.sizeDelta.x, 0);
			paddingBottom.transform.SetAsLastSibling ();
			while (texts.Count > 0) {
				Destroy (texts.Last.Value.gameObject);
				texts.RemoveLast ();
			}
			lastID = firstID = -1;
			container.parent.GetComponent<ScrollRect> ().normalizedPosition = Vector2.zero;
		}

		void CaptureLog (string condition, string stacktrace, LogType type)
		{
			if (!Blocking) {
				//			console.name.Log ();
				if (Application.isPlaying && console != null) {
					if (console.activeSelf || logWhenDisabled) {
						if (logs.Count >= maxLogsCount) {
//							IO.SaveToFile (Path.Combine (Application.persistentDataPath, "logs.txt"), string.Join ("\n", logs.ToArray ()));

						}
						string t = (++counter).ToString () + ": " + condition + "\n";
						if (type == LogType.Error || type == LogType.Exception) {
							if (string.IsNullOrEmpty (stacktrace)) {
								stacktrace = new System.Diagnostics.StackTrace ().ToString ();
							}
							t += stacktrace + " (" + type.ToString () + ")" + "\n" + "\n";
						}
						if (t.Length > maxLogSize) {
							t = t.Substring (t.Length - maxLogSize, maxLogSize);
						}

						logs.Add (new ConsoleLog (t, type));
			
						if (lastID == logs.Count - 2)
							Forward ();
					}
				} 
			}
		}

		private void PageDown (string parameter)
		{
			while (lastID < logs.Count - 1) {
				Forward ();
			}
		}

		private void PageUp (string parameter)
		{
			while (firstID > -1) {
				Backward ();
			}
		}

		public void Forward ()
		{
			if (lastID < logs.Count - 1) {
				if (texts.Count < textsObjLimit) {
					GameObject go = Instantiate (prefab);
					go.SetActive (true);
					go.transform.SetParent (container);
					go.transform.SetAsLastSibling ();
					go.transform.localScale = Vector3.one;
					go.transform.localPosition = Vector3.zero;
					Text txt = go.GetComponent<Text> ();
					txt.text = logs [++lastID].log;
					txt.color = logsColors [(int)logs [lastID].type];
					texts.AddLast (txt.GetComponent<RectTransform> ());
					Canvas.ForceUpdateCanvases ();
					go.GetComponent<ContentSizeFitter> ().enabled = false;
					Canvas.ForceUpdateCanvases ();
					go.GetComponent<ContentSizeFitter> ().enabled = true;
					Canvas.ForceUpdateCanvases ();
				} else {
					firstID++;
					RectTransform txt = texts.First.Value;
					float topH = txt.rect.height;
					texts.RemoveFirst ();
					texts.AddLast (txt);
					txt.GetComponent<Text> ().text = logs [++lastID].log;
					txt.GetComponent<Text> ().color = logsColors [(int)logs [lastID].type];
					Canvas.ForceUpdateCanvases ();
					float botH = -txt.rect.height;
					txt.transform.SetAsLastSibling ();
					Padding (topH, botH);


//					if (lastID > maxLogsCount) {
//						lastID -= removeIfOver;
//						firstID -= removeIfOver;
//						logs.RemoveRange (0, removeIfOver);
//					}
				}
				LastLoacalPosition ();
			}
		}

		public void Backward ()
		{
			if (firstID > 0) {
				lastID--;
				RectTransform txt = texts.Last.Value;
				float botH = txt.rect.height;
				texts.RemoveLast ();
				texts.AddFirst (txt);
				txt.GetComponent<Text> ().text = logs [--firstID].log;
				txt.GetComponent<Text> ().color = logsColors [(int)logs [firstID].type];
				txt.transform.SetAsFirstSibling ();
				Canvas.ForceUpdateCanvases ();
				float topH = -txt.rect.height;
				Padding (topH, botH);
				LastLoacalPosition ();
			}
		}

		private void Padding (float addTop, float addBottom)
		{
			float topH = paddingTop.sizeDelta.y + addTop;
			if (topH < 0)
				topH = 0;

			paddingTop.sizeDelta = new Vector2 (paddingTop.sizeDelta.x, topH);
			paddingTop.transform.SetAsFirstSibling ();

			float botH = paddingBottom.sizeDelta.y + addBottom;
			if (botH < 0)
				botH = 0;

			paddingBottom.sizeDelta = new Vector2 (paddingTop.sizeDelta.x, botH);
			paddingBottom.transform.SetAsLastSibling ();
		}

		private void LastLoacalPosition ()
		{
			if (texts.Count > 1) {
				Canvas.ForceUpdateCanvases ();

                if (texts.Last.Previous.Value != null)
                {
                    float hf = texts.Last.Previous.Value.localPosition.y - texts.Last.Value.localPosition.y;
                    lastYPlus = hf;
                    lastYMinus = -hf;
                }
			}
		}

		public void OnDrag (Vector2 value)
		{
			Canvas.ForceUpdateCanvases ();
//			Debug.Log ("DD " + texts.Count);
			if (texts.Count >= textsObjLimit) {
//				Debug.Log ("L " + texts.Last.Value.localPosition.y + " " + container.localPosition.y + " " + lastYPlus + " " + lastYMinus);
				if (texts.Last.Value.localPosition.y + container.localPosition.y > lastYPlus) {

//					Debug.Log ("F");
					Forward ();

				} else if (texts.Last.Value.localPosition.y + container.localPosition.y < lastYMinus) {

//					Debug.Log ("B");
					Backward ();

				}
			}
		}

		static public void InsertTextToConsole (string str)
		{
			self.commandField.text = str;
		}

		public void Command ()
		{
			string cmd = "";
			string parameters = "";
			for (int i = 0; i < commandField.text.Length; i++) {
				if (commandField.text [i] == ' ') {
					for (int j = i + 1; j < commandField.text.Length; j++) {
						parameters += commandField.text [j];
					}
					break;
				}
				cmd += commandField.text [i];
			}

			if (ConsoleCommands.ContainsKey (cmd)) {
				Debug.Log (commandField.text);
				Debug.Log ("EXECUTED");
				ConsoleCommands [cmd].unityevent.Invoke (parameters);
				lastEvent = ConsoleCommands[cmd].unityevent;
				lastargs = parameters;
			} else {
				Debug.Log (commandField.text);
				Debug.Log ("UNKNOWN COMMAND");
			}
			commandField.text = "";
		}

		public void ExecuteLastCommand()
        {
			if(lastEvent==null)
            {
				Debug.Log("There is no command that was executed.");
				return;
			}

			lastEvent.Invoke(lastargs);
        }

		static public void AddCommandListener (string command, UnityAction<string> action)
		{
			if (!self.ConsoleCommands.ContainsKey (command)) {
				StrUnityEvent e = new StrUnityEvent ();
				e.AddListener (action);
				self.ConsoleCommands.Add (command, new ConsoleCommand (command, e));
			} else
				self.ConsoleCommands [command].unityevent.AddListener (action);
		}

		#if UNITY_EDITOR || UNITY_STANDALONE
		void Update ()
		{
			if (Input.GetKeyDown (KeyCode.BackQuote))
				CloseOpen ();
		}
		#else
		private int state = 0;

		void Update ()
		{
			switch (state) {

			case 0:
				if (Input.touchCount == 3) {
					state++;
				}
				break;

			case 1:
				if (Input.touchCount != 3) {
					if (Input.touchCount == 2)
						state++;
					else
						state = 0;
				}
				break;

			case 2:
				if (Input.touchCount != 2) {
					if (Input.touchCount == 3)
						state++;
					else
						state = 0;
				}
				break;

			case 3:
				if (Input.touchCount != 3) {
					if (Input.touchCount == 2)
						state++;
					else
						state = 0;
				}
				break;
			case 4:
				if (Input.touchCount != 2) {
					if (Input.touchCount == 1)
						state++;
					else
						state = 0;
				}
				break;

			case 5:
				if (Input.touchCount != 1) {
					if (Input.touchCount == 2)
						state++;
					else
						state = 0;
				}
				break;
			case 6:
				if (Input.touchCount != 2) {
					if (Input.touchCount == 3)
						CloseOpen ();
					state = 0;
				}
				break;

			}
		}
		#endif

		public void CloseOpen ()
		{
			console.SetActive (!console.activeSelf);
		}

		static public string SaveAllLogsToFile()
        {
			string path = Path.Combine(Application.persistentDataPath, "consolelogs.txt");

			if(File.Exists(path))
            {
				File.Delete(path);
			}

			foreach (ConsoleLog log in self.logs)
			{
				System.IO.File.AppendAllText(path, log.log);
			}

			return path;
        }

		public class ConsoleLog
		{
			public string log;
			public LogType type;

			public ConsoleLog (string log, LogType type)
			{
				this.log = log;
				this.type = type;
			}
		}
	}

	[System.Serializable]
	public class ConsoleCommand
	{
		public string key;
		public StrUnityEvent unityevent;

		public ConsoleCommand (string key, StrUnityEvent unityevent)
		{
			this.key = key;
			this.unityevent = unityevent;
		}
	}

	[System.Serializable]
	public class StrUnityEvent : UnityEvent<string>
	{

	}

	//	#if UNITY_EDITOR
	//	[CustomEditor (typeof(ConsoleManager))]
	//	public class ConsoleManagerInspector : Editor
	//	{
	//		public override void OnInspectorGUI ()
	//		{
	//			DrawDefaultInspector ();
	//			(target as ConsoleManager).SaveCommands ();
	//		}
	//	}
	//	#endif
}