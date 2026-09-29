using UnityEngine;
using System.IO;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;


#pragma warning disable

namespace DKK
{
	public delegate void FilesProgressMethod (float progress, int counter, int all);

	public class ServerManager : MonoBehaviour
	{
		public string apiKey = "";
		private Queue donwloadsQueue = new Queue ();
		private Dictionary<string, List<Downloads>> downloadsCollection = new Dictionary<string, List<Downloads>> ();
		private Downloads currentDownloads;
		private FilesProgressMethod defaultProgressLog = null;
		private string defaultSource = "", defaultDestination = "";
		private bool downloading = false;
		private bool gettingData = false;
		private List<string> filesToRemove = new List<string> ();
		private Dictionary<string, List<WWW>> collections = new Dictionary<string, List<WWW>> ();

		//		public UnityEvent SynchEvent;
		//		public float actualizationInterval = 10;

		public float cacheSize = 10485760;
		public string cacheCatalog = "Cache";
		private Cache cache;

		//		private float lastActualizationRealTime;
		//		private bool started = false;

		private List<SMStream> streams = new List<SMStream> ();

		public UnityAction<float> defaultProgSMStream;

		private UnityAction<float> logDownload, logUpload;

		public UnityAction<float> LogDownload {
			get {
				return logDownload;
			}
		}

		public UnityAction<float> LogUpload {
			get {
				return logUpload;
			}
		}

		static public ServerManager self;
		static public string success = "success";
		static public float timeout = 10;

		void Awake ()
		{
			self = this;
			cache = new Cache (cacheCatalog, cacheSize);
		}

		//		static public void StartSynchIntervals ()
		//		{
		//			self.started = true;
		//			self.lastActualizationRealTime = Time.realtimeSinceStartup;
		//			self.StartSynchCo ();
		//		}
		//
		//		static public void AddSynchAction (UnityAction action)
		//		{
		//			self.SynchEvent.AddListener (action);
		//		}
		//
		//		void OnApplicationPause (bool pause)
		//		{
		//			if (!pause && started) {
		//				if (Time.realtimeSinceStartup - lastActualizationRealTime > actualizationInterval) {
		//					//				Debug.Log (Time.realtimeSinceStartup - lastActualizationRealTime);
		//					StopSynchCo ();
		//					Synch ();
		//					StartSynchCo ();
		//				}
		//			}
		//		}

		static public void SetStatusMethods (UnityAction<float> logDownload, UnityAction<float> logUpload)
		{
			self.logDownload = logDownload;
			self.logUpload = logUpload;
		}

		public void LogStatus (WWW www, UnityAction<float> myLogDownload = null, UnityAction<float> myLogUpload = null)
		{
			try {
				if (www.uploadProgress != 1) {
					if (logUpload != null)
						logUpload (www.uploadProgress);
					if (myLogUpload != null)
						myLogUpload (www.uploadProgress);
				} else {
					if (logDownload != null)
						logDownload (www.progress);
					if (myLogDownload != null)
						myLogDownload (www.progress);
				}
			} catch (System.Exception e) {
				Debug.Log (e.ToString ());
			}
		}

		//		private void StartSynchCo ()
		//		{
		//			StartCoroutine ("SynchCo");
		//		}
		//
		//		private void StopSynchCo ()
		//		{
		//			StopCoroutine ("SynchCo");
		//		}
		//
		//		IEnumerator SynchCo ()
		//		{
		//
		//			while (true) {
		//				yield return new WaitForSeconds (actualizationInterval);
		//				//			Debug.Log (System.DateTime.Now);
		//				Synch ();
		//			}
		//		}
		//
		//		private void Synch ()
		//		{
		//			lastActualizationRealTime = Time.realtimeSinceStartup;
		//			if (Application.internetReachability != NetworkReachability.NotReachable) {
		//				SynchEvent.Invoke ();
		//			}
		//		}

		static public bool IsGettingData ()
		{
			return self.gettingData;
		}

		/// <summary>
		/// Initialize default parameters.
		/// </summary>
		/// <param name="popup">Enable yes/no popup method.</param>
		/// <param name="defaultSource">Default source will be combined before all download sources.</param>
		/// <param name="defaultDestination">Default destination will be combined before all download destinations.</param>
		/// <param name="defaultProgressLog">Default progressLog method will be used with downloads where this method is not set.</param>
		static public void Initialize (string defaultSource = "", string defaultDestination = "", FilesProgressMethod defaultProgressLog = null)
		{
			self.defaultDestination = defaultDestination;
			self.defaultSource = defaultSource;
			self.defaultProgressLog = defaultProgressLog;
		}

		static public void Download (List<string> downloadsList, string source, string destination, BoolMethod callbackFun = null, FilesProgressMethod progressLog = null, bool retryMode = false, string collection = null)
		{
			List<Downloads.Download> list = new List<Downloads.Download> (downloadsList.Count);
			foreach (string s in downloadsList) {
				list.Add (new Downloads.Download (s, s));
			}
			Download (list, source, destination, callbackFun, progressLog, retryMode, collection);
		}

		/// <summary>
		/// Starts downloading a list of files.
		/// </summary>
		/// <param name="downloadsList">List of files you want to download (names).</param>
		/// <param name="source">Source URL. Default source will be combined before.</param>
		/// <param name="destination">Destination location. Default destination will be combined before.</param>
		/// <param name="callbackFun">Method executed when downloading is finished.</param>
		/// <param name="progressLog">Method with string parameter for logging downloading progress. Default will be used if null.</param>
		static public void Download (List<Downloads.Download> downloadsList, string source, string destination, BoolMethod callbackFun = null, FilesProgressMethod progressLog = null, bool retryMode = false, string collection = null)
		{
			if (downloadsList.Count > 0) {
				if (string.IsNullOrEmpty (source) && string.IsNullOrEmpty (self.defaultSource))
					Debug.LogError ("Source is empty. You need to define default source in Initialize method or set as parameter in Download method.");
				
				if (string.IsNullOrEmpty (destination) && string.IsNullOrEmpty (self.defaultDestination)) {
					Debug.LogError ("Destination is empty. You need to define default destination in Initialize method or set as parameter in Download method.");
				}

				if (progressLog == null)
					progressLog = self.defaultProgressLog;
				Downloads newDownloads = new Downloads (downloadsList, Path.Combine (self.defaultSource, source), Path.Combine (self.defaultDestination, destination), callbackFun, progressLog, retryMode, collection);
				self.donwloadsQueue.Enqueue (newDownloads);
				self.CollectDownloads (collection, newDownloads);
				if (!self.downloading) {
					self.downloading = true;
					self.KeepDownload ();
				}
			} else {
				if (callbackFun != null)
					callbackFun (true);
			}
		}

		private void CollectDownloads (string collectionName, Downloads d)
		{
			if (!string.IsNullOrEmpty (collectionName)) {
				if (!downloadsCollection.ContainsKey (collectionName))
					downloadsCollection.Add (collectionName, new List<Downloads> ());

				downloadsCollection [collectionName].Add (d);
			}
		}

		static public void DisposeDownloadsCollection (string collectionName)
		{
			if (!string.IsNullOrEmpty (collectionName) && self.downloadsCollection.ContainsKey (collectionName)) {
				foreach (Downloads d in self.downloadsCollection[collectionName]) {
					d.Cancel ();
				}
				self.downloadsCollection.Remove (collectionName);
			}
		}

		public void RemoveFromDownloadsCollection (Downloads d)
		{
			if (!string.IsNullOrEmpty (d.CollectionName) && downloadsCollection.ContainsKey (d.CollectionName)) {
				List<Downloads> list = downloadsCollection [d.CollectionName];
				for (int i = 0; i < list.Count; i++) {
					if (list [i] == d) {
						list.RemoveAt (i);
						break;
					}
				}
				if (downloadsCollection [d.CollectionName].Count == 0)
					downloadsCollection.Remove (d.CollectionName);
			}
		}

		static public WWWForm PreparedForm (WWWForm form = null)
		{
			if (form == null)
				form = new WWWForm ();

			form.AddField ("apikey", self.apiKey);

			return form;
		}

		public void CollectWWW (string collectionName, WWW www)
		{
			if (!string.IsNullOrEmpty (collectionName)) {
				if (!collections.ContainsKey (collectionName))
					collections.Add (collectionName, new List<WWW> ());

				collections [collectionName].Add (www);
			}
		}

		static public void DisposeCollection (string collectionName)
		{
			if (!string.IsNullOrEmpty (collectionName) && self.collections.ContainsKey (collectionName)) {
				foreach (WWW www in self.collections[collectionName]) {
					www.Dispose ();
				}
				self.collections.Remove (collectionName);
			}
		}

		public void RemoveFromCollection (string collectionName, WWW www)
		{
			if (!string.IsNullOrEmpty (collectionName) && collections.ContainsKey (collectionName)) {
				List<WWW> list = collections [collectionName];
				for (int i = 0; i < list.Count; i++) {
					if (list [i] == www) {
						list.RemoveAt (i);
						break;
					}
				}
				if (collections [collectionName].Count == 0)
					collections.Remove (collectionName);
			}
		}

		static public WWW GetTextureFromServer (string source, BoolTexture2DMethod callbackFun, FloatMethod progressLog = null, string collection = null)
		{
			return GetBytesFromServer (
				source, 
				(b, o) => {
					if (callbackFun != null) {
						if (b && o != null) {
							Texture2D tex = new Texture2D (1, 1, TextureFormat.RGBA32, false);
							tex.LoadImage ((byte[])o);
							tex.Apply ();
							callbackFun (b, tex);
						} else {
							callbackFun (b, null);
						}
					}
				},
				progressLog, collection
			);


		}

		static public WWW GetBytesFromServer (string source, BoolBytesMethod callbackFun, FloatMethod progressLog = null, string collection = null, bool disposedCallback = false)
		{
			byte[] bytes = self.cache.Get (source);
			if (bytes != null)
				callbackFun (true, bytes);
			else {
				WWW www = new WWW (source);
				self.StartCoroutine (self.GetBytesFromServerCo (www, callbackFun, progressLog, collection, disposedCallback));
				return www;
			}
			return null;
		}

		IEnumerator GetBytesFromServerCo (WWW www, BoolBytesMethod callbackFun, FloatMethod progressLog, string collection, bool disposedCallback)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable) {
				if (callbackFun != null) {
					callbackFun (false, null);
				}
			}

			CollectWWW (collection, www);

			bool success = true;
			bool timeout = false;
			bool done = false;
			bool disposed = false;

			float idleTime = 0;
			float progress = 0;

			while (!done) {

				timeout = idleTime > ServerManager.timeout;

				try {

					done = timeout || www.isDone;

				} catch (System.Exception e) {
					Debug.Log ("Disposed!" + e.ToString ());
					success = false;
					disposed = true;
					break;
				}

				if (done) {
					if (!string.IsNullOrEmpty (www.error)) {
						Debug.Log (www.error + " " + www.url);
						success = false;
					} else if (timeout) {
						Debug.Log ("timeout: " + www.url);
						www.Dispose ();
						success = false;
					}
				} else {
					if (progress == www.progress)
						idleTime += Time.deltaTime;
					else
						idleTime = 0;
				}

				if (progressLog != null) {
					progressLog (www.progress);
				}

				LogStatus (www);
				
				yield return null;
			}
				
			try {
				done = www.isDone;
			} catch (System.Exception e) {
				Debug.Log ("Disposed!" + e.ToString ());
				success = false;
				disposed = true;
			}

			if (success) {
				cache.Set (www.bytes, www.url); 
			}

			if (disposed) {
				if (callbackFun != null && disposedCallback) {
					callbackFun (false, null);
				}
			} else {
				if (callbackFun != null) {
					callbackFun (success, www.bytes);
				}
			}

			RemoveFromCollection (collection, www);
		}

		/// <summary>
		/// Starts downloading a files.
		/// </summary>
		/// <param name="requiredFiles">Files required.</param>
		/// <param name="callbackFun">Method executed when downloading is finished.</param>
		/// <param name="sourceURL">File source URL. Default source will be combined before.</param>
		/// <param name="removeNotRequired">If set to <c>true</c> remove not required. Else you can use FlushFiles to remove them later.</param>
		/// <param name="progressLog">Method with string parameter for logging downloading progress. Default will be used if null.</param>
		static public void Synchronize (HashSet<string> requiredFiles, BoolMethod callbackFun = null, string sourceURL = null, string destinationPath = "", bool removeNow = true, FilesProgressMethod progressLog = null, bool retryMode = false, string collection = null, string hiddenExtension = "")
		{
			HashSet<string> existingFiles = new HashSet<string> ();

			// reading files
			DirectoryInfo dinf = new DirectoryInfo (destinationPath);
			foreach (FileInfo finf in dinf.GetFiles("*", SearchOption.AllDirectories)) {
				existingFiles.Add (finf.FullName.Substring (destinationPath.Length + 1));
				//				Debug.Log ("Existing " + finf.FullName.Substring (dst.Length + 1));
			}

			// removing
			if (removeNow) {
				foreach (string existingFile in existingFiles) {
					if (!requiredFiles.Contains (existingFile))
						File.Delete (Path.Combine (destinationPath, existingFile));
				}
			} else {
				self.filesToRemove.Clear ();
				foreach (string existingFile in existingFiles) {
					if (!requiredFiles.Contains (existingFile))
						self.filesToRemove.Add (Path.Combine (destinationPath, existingFile));
				}
			}

			// downloading
			List<Downloads.Download> downloadFilesList = new List<Downloads.Download> ();
			HashSet<string> downloadFilesHashSet = new HashSet<string> ();
			foreach (string requiredFile in requiredFiles) {
				if (!existingFiles.Contains (requiredFile) && !downloadFilesHashSet.Contains (requiredFile)) {
//					Debug.Log ("Required " + requiredFile);
					string str = requiredFile.Replace ('\\', '/');
					downloadFilesList.Add (new Downloads.Download (str + hiddenExtension, str));
					downloadFilesHashSet.Add (requiredFile);
				}
			}
			Download (downloadFilesList, sourceURL, destinationPath, callbackFun, progressLog, retryMode, collection);
		}

		/// <summary>
		/// Flushs the files.
		/// </summary>
		static public void FlushFiles ()
		{
			foreach (string file in self.filesToRemove) {
				File.Delete (file);
			}
			self.filesToRemove.Clear ();
		}

		private void KeepDownload ()
		{
			if (donwloadsQueue.Count != 0) {
				
				currentDownloads = (Downloads)donwloadsQueue.Dequeue ();
				StartCoroutine ("DownloadFiles");
			} else {
				downloading = false;
			}
		}

		private IEnumerator DownloadFiles ()
		{	
			currentDownloads.Run ();
			yield return null;
			while (!currentDownloads.CheckProgress ())
				yield return null;
				
			currentDownloads.Callback ();
			RemoveFromDownloadsCollection (currentDownloads);
			KeepDownload ();
		}

		//		/// <summary>
		//		/// Gets the json from server.
		//		/// </summary>
		//		/// <param name="location">URL from where the data will be downloaded.</param>
		//		/// <param name="callback">Callback will be executed when operation is finished. Parameter is a downloaded json object or null if there was a problem with getting data.</param>
		//		/// <param name="progressMethod">Method for logging progress of downloading process.</param>
		//		static public void GetJsonFromServer (string location, JsonMethod callback, FloatMethod progressMethod = null)
		//		{
		//			self.StartCoroutine (self.GetJsonFromServerCoroutine (location, callback, progressMethod));
		//		}
		//
		//		private IEnumerator GetJsonFromServerCoroutine (string location, JsonMethod callback, FloatMethod progressMethod)
		//		{
		//			gettingData = true;
		//			bool disposed = false;
		//			WWW www = new WWW (location/*, PreparedForm ()*/);
		//			if (progressMethod != null) {
		//
		//				while (true) {
		//
		//					try {
		//						if (www.isDone)
		//							break;
		//					} catch (System.Exception e) {
		//						Debug.Log ("Exception catched: " + e.ToString ());
		//						disposed = true;
		//						break;
		//					}
		//
		//					progressMethod (www.progress);
		//
		//					LogStatus (www);
		//
		//					yield return null;
		//				}
		//			} else
		//				yield return www;
		//
		//			if (!disposed && string.IsNullOrEmpty (www.error)) {
		//				if (progressMethod != null)
		//					progressMethod (www.progress);
		//
		//
		//				LogStatus (www);
		//
		//				string jsonData = www.text;
		//				jsonData = UTF8.ConvertToUTF8 (jsonData);
		//				JSONObject json = new JSONObject (jsonData);
		//				callback (json);
		//
		//			} else {
		//
		////				Debug.Log ("Data download problem.");
		//				callback (null);
		//			}
		//			gettingData = false;
		//		}

		static public WWW SendField (string name, string message, string destination, bool useDefSource = false, ResponseMethod callback = null, string collection = null, int retryTimes = 0, bool disposedCallback = false, UnityAction<float> uploadProgress = null, UnityAction<float> downloadProgress = null)
		{
			//destination.Log("WWW:");
			if (useDefSource)
				destination = Path.Combine (self.defaultSource, destination);

			WWWForm form = PreparedForm ();
			form.AddField (name, message);

			WWW www = new WWW (destination, form);
			self.StartCoroutine (self.SendFormCo (www, callback, collection, retryTimes, disposedCallback, uploadProgress, downloadProgress));
			return www;
		}

		static public WWW SendForm (WWWForm form, string destination, bool useDefSource = false, ResponseMethod callback = null, string collection = null, int retryTimes = 0, bool disposedCallback = false, UnityAction<float> uploadProgress = null, UnityAction<float> downloadProgress = null)
		{
			// destination.Log("WWW:");

			if (useDefSource)
				destination = Path.Combine (self.defaultSource, destination);

			WWW www = new WWW (destination, PreparedForm (form));
			self.StartCoroutine (self.SendFormCo (www, callback, collection, retryTimes, disposedCallback, uploadProgress, downloadProgress));
			return www;
		}

		//		static public void SendJSON (JSONObject json, string destination, bool useDefSource = false, ResponseMethod callback = null)
		//		{
		//			json.AddField ("apikey", self.apiKey);
		//
		//			if (useDefSource)
		//				destination = Path.Combine (self.defaultSource, destination);
		//
		//			Dictionary<string, string> headers = new Dictionary<string, string> ();
		//			headers.Add ("Content-Type", "application/json");
		//			self.StartCoroutine (self.SendJsonCo (System.Text.Encoding.UTF8.GetBytes (json.ToString ()), destination, callback, headers));
		//		}

		IEnumerator SendFormCo (WWW www, ResponseMethod callback, string collection, int retryTimes, bool disposedCallback, UnityAction<float> uploadProgress, UnityAction<float> downloadProgress)
		{
			CollectWWW (collection, www);

			bool disposed = false;
			while (!disposed) {
				
				try {
					if (www.isDone)
						break;
					else {
						LogStatus (www, downloadProgress, uploadProgress);
					}
				} catch (System.Exception e) {
//					Debug.Log ("Exception catched: " + e.ToString ());
					disposed = true;
					break;
				}

				yield return null;
			}

			if (disposed) {
				if (callback != null && disposedCallback)
					callback (false, "Disposed!");
			} else {
				if (string.IsNullOrEmpty (www.error)) {
					if (callback != null)
						callback (true, www.text);

				} else {
					
					Debug.LogWarning ("Sending error: " + www.error + " for address " + www.url);

					if (retryTimes > 0) {
						RemoveFromCollection (collection, www);
//						Debug.Log ("Retry " + www.url);
						StartCoroutine (self.SendFormCo (www, callback, collection, --retryTimes, disposedCallback, uploadProgress, downloadProgress));
						yield break;
					} else if (callback != null)
						callback (false, www.error);
				}
			}

			RemoveFromCollection (collection, www);
		}

		//		IEnumerator SendJsonCo (byte[] bytes, string destination, ResponseMethod callback, Dictionary<string, string> headers)
		//		{
		//			WWW www = new WWW (destination, bytes, headers);
		//
		//			yield return www;
		//
		//			// failed
		//			if (string.IsNullOrEmpty (www.error)) {
		//				if (callback != null)
		//					callback (true, www.text);
		//			} else {
		//				Debug.LogWarning ("Sending error: " + www.error);
		//				if (callback != null)
		//					callback (false, www.error);
		//			}
		//		}

		static private void RemoveSMStream (SMStream stream)
		{
			self.streams.Remove (stream);
		}

		static public SMStream CreateNetworkStream (string host, string uri, string savePath, ResponseMethod callback, UnityAction<float> progress = null)
		{
			SMStream stream = new SMStream (host, uri, savePath, callback, progress != null ? progress : ServerManager.self.defaultProgSMStream);
			self.streams.Add (stream);
			return stream;
		}

		public class SMStream
		{
			private bool canceled = false;
			private bool disposed = false;
			private uint contentLength;
			private NetworkStream networkStream;
			private FileStream fileStream;
			private Socket client;
			private ResponseMethod Callback;
			private string Path;

			public SMStream (string host, string uri, string savePath, ResponseMethod callback, UnityAction<float> progress)
			{
				try {
					this.Path = savePath;
					this.Callback = callback;
					string query = "GET " + uri.Replace (" ", "%20") + " HTTP/1.1\r\n" +
					               "Host: " + host + "\r\n" +
					               "User-Agent: undefined\r\n" +
					               "Connection: close\r\n" +
					               "\r\n";

					Debug.Log (query);

					client = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
					client.Connect (host, 80);   

					networkStream = new NetworkStream (client);

					var bytes = Encoding.Default.GetBytes (query);
					networkStream.Write (bytes, 0, bytes.Length);

					var bReader = new BinaryReader (networkStream, Encoding.Default);

					string response = "";
					string line;
					char c;

					do {
						line = "";
						c = '\u0000';
						while (true) {
							c = bReader.ReadChar ();
							if (c == '\r')
								break;
							line += c;
						}
						c = bReader.ReadChar ();
						response += line + "\r\n";
					} while (line.Length > 0);  

					Debug.Log (response);

					Regex reContentLength = new Regex (@"(?<=Content-Length:\s)\d+", RegexOptions.IgnoreCase);
					contentLength = uint.Parse (reContentLength.Match (response).Value);

					fileStream = new FileStream (savePath + ".temp", FileMode.Create);

				} catch (System.Exception e) {
					
					Debug.Log (e.ToString ());
					callback (false, null);
					Callback = null;
					return;
				}

				int n = 0;
				int read = 0;
				byte[] buffer = new byte[1024 * 1024];

				CommonManager.Coroutine (delegate {
					if (!canceled && n < contentLength) {
						try {
							if (networkStream.DataAvailable) {
								read = networkStream.Read (buffer, 0, buffer.Length);
								n += read;
								fileStream.Write (buffer, 0, read);
								float progr = (float)n / (float)contentLength;
								if (progress != null)
									progress (progr);
								if (ServerManager.self.logDownload != null)
									ServerManager.self.logDownload (progr);
							}
						} catch (System.Exception e) {
							Debug.Log (e.ToString ());
							disposed = true;
							EndJob ();
							return false;
						}
						//				Debug.Log ("Downloaded: " + n + " of " + contentLength + " bytes ...");
						return true;
					}
					EndJob ();
					return false;
				});
			}

			private void EndJob ()
			{
				if (!canceled) {
					fileStream.Flush ();
					fileStream.Close ();
					client.Close ();

					if (disposed) {
						if (Callback != null) {
							Callback (false, null);
							Callback = null;
						}
					} else {
						try {
							if (File.Exists (Path))
								File.Delete (Path);
							File.Move (Path + ".temp", Path);
							if (Callback != null) {
								Callback (true, Path);
								Callback = null;
							}
						} catch (System.Exception e) {
							Debug.Log (e.ToString ());
							if (Callback != null) {
								Callback (false, null);
								Callback = null;
							}
						}
					}
				}

				ServerManager.RemoveSMStream (this);
			}

			public void Cancel (bool doCallback = true)
			{
				if (fileStream != null) {
					fileStream.Flush ();
					fileStream.Close ();
					client.Close ();
					canceled = true;
					try {
						if (File.Exists (Path + ".temp"))
							File.Delete (Path + ".temp");
					} catch (System.Exception e) {
						Debug.Log (e.ToString ());
					}
					if (doCallback && Callback != null) {
						Callback (false, null);
						Callback = null;
					}
				}
			}
		}

		void OnDestroy ()
		{
			for (int i = 0; i < streams.Count; i++) {
				streams [i].Cancel (false);
			}
		}
	}

	public class Downloads
	{
		public class Download
		{
			public string Source, Destination;

			public Download (string s, string d)
			{
				Source = s;
				Destination = d;
			}
		}

		private int _count;

		public int Count {
			get {
				return downloads.Count;
			}
		}

		private List<Download> downloads;
		private string source;
		private string destination;
		private List<WWW> wwwList = new List<WWW> ();
		private bool[] downloaded;
		private float idleTime;
		private float[] lastProgresses;
		private bool success = true;
		private int downloadsAtOnce = 100;
		private int last = -1;

		private BoolMethod callbackFun = null;
		private FilesProgressMethod progressLog = null;

		private int counter;
		private string collectionName = null;

		public string CollectionName {
			get {
				return collectionName;
			}
		}

		private bool retryMode = false;

		public Downloads (List<Download> downloads, string source, string destination, BoolMethod callbackFun = null, FilesProgressMethod progressLog = null, bool retryMode = false, string collection = null)
		{
			this.collectionName = collection;
			this.retryMode = retryMode;
			this.idleTime = 0;
			this.downloads = downloads;
			this.downloaded = new bool[downloads.Count];
			this.lastProgresses = new float[downloads.Count];

			this.source = source;
			this.destination = destination;

			this.callbackFun = callbackFun;
			this.progressLog = progressLog;

			counter = 0;
		}

		public Downloads (string source, string destination, BoolMethod callbackFun = null, FilesProgressMethod progressLog = null, bool retryMode = false, string collection = null)
		{
			this.collectionName = collection;
			this.retryMode = retryMode;
			this.idleTime = 0;
			downloads = new List<Download> ();
			downloads.Add (new Download ("", ""));
			this.downloaded = new bool[downloads.Count];
			this.lastProgresses = new float[downloads.Count];

			this.source = source;
			this.destination = destination;

			this.callbackFun = callbackFun;
			this.progressLog = progressLog;

			counter = 0;
		}

		public bool AnyLeft ()
		{
			return counter < downloads.Count;
		}

		public void Callback ()
		{
			if (callbackFun != null)
				callbackFun (success);
		}

		//		public string GetName (int ID)
		//		{
		//			if (downloads [ID] != "")
		//				return downloads [ID];
		//			else
		//				return Path.GetFileName (source);
		//		}

		public void Run ()
		{
//			Debug.Log ("Start download");
			last = (downloads.Count >= downloadsAtOnce) ? downloadsAtOnce - 1 : downloads.Count - 1;

			wwwList.Clear ();
			for (int i = 0; i <= last; i++) {
				wwwList.Add (new WWW (Path.Combine (this.source, downloads [i].Source)/*, ServerManager.PreparedForm ()*/));
			}
			if (progressLog != null) {
				progressLog (0, counter, downloads.Count);
			}
		}

		private bool noConnInfDisplayed = false;

		private void NoConnectionInfo ()
		{
			if (!noConnInfDisplayed) {
				noConnInfDisplayed = true;
				//DKK.UI.Dialogs.Information.Create ("attention".LangGet (), "noInternet".LangGet (),
				//	delegate {
				//		noConnInfDisplayed = false;
				//	}
				//);
			}
		}

		public bool CheckProgress ()
		{
//			bool done = true;
			float progress = 0;

			if (Application.internetReachability == NetworkReachability.NotReachable) {
				if (retryMode) {
					NoConnectionInfo ();
					idleTime = 0;
					return false;
				} else {
					success = false;
					return true;
				}
			}

			bool noProgress = true;
			bool timeout = idleTime > ServerManager.timeout;

			for (int i = 0; i < wwwList.Count; i++) {
				bool isDone;
				try {
					isDone = wwwList [i].isDone || timeout;
				} catch (System.Exception e) {
					Debug.Log ("Exception catched: " + e.ToString ());
					success = false;
					counter++;
					downloaded [i] = true;
					continue;
				}

//				done = done && isDone;
				progress += wwwList [i].progress;

				ServerManager.self.LogStatus (wwwList [i]);

				if (!downloaded [i]) {
					if (isDone) {
						if (!string.IsNullOrEmpty (wwwList [i].error)) {
							Debug.Log (wwwList [i].error + " " + wwwList [i].url);
							if (retryMode)
								RetryWWW (i, wwwList [i].url);
							else {
								success = false;
								counter++;
								downloaded [i] = true;
							}
						} else if (timeout) {
							Debug.Log ("timeout: " + wwwList [i].url);
							string url = wwwList [i].url;
							wwwList [i].Dispose ();
							if (retryMode)
								RetryWWW (i, url);
							else {
								success = false;
								counter++;
								downloaded [i] = true;
							}
						} else {
							IO.SaveToFile (GetDestination (i), wwwList [i].bytes);
							counter++;
							downloaded [i] = true;
						}
					} else {
						noProgress = noProgress && lastProgresses [i] == wwwList [i].progress;
					}
				}

				try {
					lastProgresses [i] = wwwList [i].progress;
				} catch (System.Exception e) {
					Debug.Log ("Exception catched: " + e.ToString ());
				}
			}
			if (noProgress)
				idleTime += Time.deltaTime;
			else
				idleTime = 0;
			
			CompleteTheList ();

			float prog = progress / (float)downloads.Count;

			if (progressLog != null) {
				progressLog (prog, counter, downloads.Count);
			}
				
//			if (done && wwwList.Count == downloads.Count) {
			if (counter == downloads.Count) {
				Clear ();
				return true;
			}
			return false;
		}

		private void RetryWWW (int i, string url)
		{
			wwwList [i] = new WWW (url);
			lastProgresses [i] = 0;
			idleTime = 0;
		}

		public void Cancel ()
		{
			foreach (WWW www in wwwList) {
				www.Dispose ();
			}
			Clear ();
		}

		private void Clear ()
		{
			this.downloads = null;
			this.downloaded = null;
			this.source = null;
			this.destination = null;
		}

		private void CompleteTheList ()
		{
			if (last < downloads.Count - 1) {
				int end = last + downloadsAtOnce - (wwwList.Count - counter);
				for (int j = last + 1; j <= end; j++) {
					if (j >= downloads.Count)
						break;
					wwwList.Add (new WWW (Path.Combine (this.source, downloads [j].Source)/*, ServerManager.PreparedForm ()*/));
					idleTime = 0;
				}
				last = end;
			}
		}

		public List<string> GetSources ()
		{
			List<string> sources = new List<string> ();
			foreach (Download d in downloads)
				sources.Add (Path.Combine (this.source, d.Source));
			return sources;
		}

		public string GetSource (int ID)
		{
			return Path.Combine (this.source, downloads [ID].Source);
		}

		public string GetDestination (int ID)
		{
			return Path.Combine (this.destination, downloads [ID].Destination);
		}
	}
}

#pragma warning restore