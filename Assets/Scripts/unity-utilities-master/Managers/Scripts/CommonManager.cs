using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Globalization;

namespace DKK
{
	public class CommonManager : MonoBehaviour
	{
		private List<MonoBehaviourDKK> scripts = new List<MonoBehaviourDKK> ();
		static public CommonManager self;

        static public CultureInfo dotsCulture, commasCulture;

        private Dictionary<string, Dictionary<string, UnityEngine.Coroutine>> coroutinesCollections = new Dictionary<string, Dictionary<string, UnityEngine.Coroutine>> ();

		void Awake ()
		{
			self = this;

            dotsCulture = new CultureInfo("");
            dotsCulture.NumberFormat.NumberDecimalSeparator = ".";
            commasCulture = new CultureInfo("");
            commasCulture.NumberFormat.NumberDecimalSeparator = ",";
        }

		void Start ()
		{
			GameObject[] goes = gameObject.scene.GetRootGameObjects ();
			for (int i = 0; i < goes.Length; i++) {
				scripts.AddRange (goes [i].transform.GetComponentsInChildren<MonoBehaviourDKK> (true).ToList ());
			}

			for (int i = 0; i < scripts.Count; i++) {
				scripts [i].OnAppStart ();
			}

			Coroutine (delegate {
				for (int i = 0; i < scripts.Count; i++) {
					scripts [i].OnLateAppStart ();
				}
			});


//			Coroutine (delegate {
//				Debug.Log ("Korutyn kolekcji " + coroutinesCollections ["kolekcja"].Count);
//				Debug.Log ("Korutyna kolekcji zakończona!");
//			}, 1, "kolekcja");
//
//			Coroutine (delegate {
//				if (coroutinesCollections.ContainsKey ("kolekcja")) {
//					Debug.Log ("Korutyn kolekcji " + coroutinesCollections ["kolekcja"].Count);
//					foreach (KeyValuePair<string, UnityEngine.Coroutine> co in coroutinesCollections["kolekcja"]) {
//						Debug.Log ("Znaleziona korutyna kolekcji");
//					}
//				} else
//					Debug.Log ("Brak korutyn kolekcji");
//			}, 5);
		}

		public void OpenURL (string url)
		{
			Application.OpenURL (url);
		}


		static public void TurnOff ()
		{
			Application.Quit ();
			GameObject[] goes = self.gameObject.scene.GetRootGameObjects ();
			for (int i = 0; i < goes.Length; i++) {
				goes [i].SetActive (false);
			}
		}

		static public bool RemoveCoroutines (string collection)
		{
			return self.RemoveCoroutinesSelf (collection);
		}

		private bool RemoveCoroutinesSelf (string collection)
		{
			if (coroutinesCollections.ContainsKey (collection)) {
				foreach (KeyValuePair<string, UnityEngine.Coroutine> pair in coroutinesCollections[collection]) {
					StopCoroutine (pair.Value);
				}
			}
			return coroutinesCollections.Remove (collection);
		}

		private void AddCoroutineToCollection (string collection, string hashID, UnityEngine.Coroutine uec)
		{
			if (coroutinesCollections.ContainsKey (collection))
				coroutinesCollections [collection].Add (hashID, uec);
			else {
				Dictionary<string, UnityEngine.Coroutine> dic = new Dictionary<string, UnityEngine.Coroutine> ();
				dic.Add (hashID, uec);
				coroutinesCollections.Add (collection, dic);
			}
		}

		private bool RemoveCoroutineFromCollections (string collection, string hashID)
		{
			if (!string.IsNullOrEmpty (collection) && coroutinesCollections.ContainsKey (collection)) {
				if (coroutinesCollections [collection].Remove (hashID)) {
					if (coroutinesCollections [collection].Count == 0) {
						coroutinesCollections.Remove (collection);
					}
					return true;
				}
			}
			return false;
		}

		static public void Coroutine (Coroutine coroutine)
		{
			if (self != null)
				self.StartCoroutine (coroutine ());
		}

		static public void Coroutine (ReturnBoolMethod whileTrue, string collection = null)
		{
			if (self != null) {
				if (string.IsNullOrEmpty (collection))
					self.StartCoroutine (self._Coroutine (whileTrue));
				else {
					string hashID = GetUniqueString ();
					UnityEngine.Coroutine uec = self.StartCoroutine (self._Coroutine (whileTrue, collection, hashID));
					self.AddCoroutineToCollection (collection, hashID, uec);
				}
			}
		}

		static public void Coroutine (UnityAction action, string collection = null)
		{
			if (self != null) {
				if (string.IsNullOrEmpty (collection))
					self.StartCoroutine (self._Coroutine (action));
				else {
					string hashID = GetUniqueString ();
					UnityEngine.Coroutine uec = self.StartCoroutine (self._Coroutine (action, collection, hashID));
					self.AddCoroutineToCollection (collection, hashID, uec);
				}
			}
		}

		static public void Coroutine (UnityAction action, int atFrame, string collection = null)
		{
			if (self != null) {
				if (string.IsNullOrEmpty (collection))
					self.StartCoroutine (self._Coroutine (action, atFrame));
				else {
					string hashID = GetUniqueString ();
					UnityEngine.Coroutine uec = self.StartCoroutine (self._Coroutine (action, atFrame, collection, hashID));
					self.AddCoroutineToCollection (collection, hashID, uec);
				}
			}
		}

		static public void Coroutine (UnityAction action, float afterTime, string collection = null)
		{
			if (self != null) {
				if (string.IsNullOrEmpty (collection))
					self.StartCoroutine (self._CoroutineSeconds (action, afterTime));
				else {
					string hashID = GetUniqueString ();
					UnityEngine.Coroutine uec = self.StartCoroutine (self._CoroutineSeconds (action, afterTime, collection, hashID));
					self.AddCoroutineToCollection (collection, hashID, uec);
				}
			}
		}

		IEnumerator _CoroutineSeconds (UnityAction action, float afterTime, string collection = null, string hashID = null)
		{
			yield return new WaitForSeconds (afterTime);
			action.Invoke ();
			RemoveCoroutineFromCollections (collection, hashID);
		}

		IEnumerator _Coroutine (UnityAction action, int atFrame, string collection = null, string hashID = null)
		{
			int counter = 0;
			while (counter++ < atFrame) {
				yield return null;
			}
			action.Invoke ();
			RemoveCoroutineFromCollections (collection, hashID);
		}

		IEnumerator _Coroutine (UnityAction action, string collection = null, string hashID = null)
		{
			yield return new WaitForEndOfFrame ();
			action.Invoke ();
			RemoveCoroutineFromCollections (collection, hashID);
		}

		IEnumerator _Coroutine (ReturnBoolMethod whileTrue, string collection = null, string hashID = null)
		{
			bool goAhead = true;

			while (goAhead) {
				goAhead = whileTrue ();
				yield return null;
			}
			RemoveCoroutineFromCollections (collection, hashID);
		}

		static public string GetUniqueString ()
		{
			return System.Guid.NewGuid ().ToString ();
		}

		static public string GetCountKey (List<string> list, string key)
		{

			if (key == null)
				Debug.LogError ("Given key is null!");
		
			int highest = -1;
			foreach (string str in list) {
				if (str != null) {
					bool same = true;
					for (int i = 0; i < key.Length; i++) {
						if (key [i] != str [i]) {
							same = false;
							break;
						}
					}
					if (same) {
						if (str.Length > key.Length) {
							string ending = str.Substring (key.Length);
							int nr;
							if (int.TryParse (ending, out nr)) {
								if (nr > highest)
									highest = nr;
							}
						}
					}
				}
			}
		
			return key + (highest + 1).ToString ();
		}
	}

	public class MonoBehaviourDKK : MonoBehaviour
	{
		public virtual void OnAppStart ()
		{
			
		}

		public virtual void OnLateAppStart ()
		{

		}
	}
}
