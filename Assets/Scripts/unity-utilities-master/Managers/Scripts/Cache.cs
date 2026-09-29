using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

namespace DKK
{
	public class Cache
	{
		private string dataPath;
		private string storagePath;
		private float maxSize;
		private FileAccessList fa;

		public Cache (string path, float maxSize)
		{
			if (Directory.Exists (Path.Combine (Application.persistentDataPath, "Cache")))
				Directory.Delete (Path.Combine (Application.persistentDataPath, "Cache"), true);

			string p = Path.Combine (Application.persistentDataPath, path);

			if (!Directory.Exists (p))
				Directory.CreateDirectory (p);
			this.storagePath = Path.Combine (p, "Storage");

			if (!Directory.Exists (this.storagePath))
				Directory.CreateDirectory (this.storagePath);
			this.dataPath = Path.Combine (p, "data");

			DirectoryInfo di = new DirectoryInfo (this.storagePath);
			this.maxSize = maxSize;

			if (File.Exists (this.dataPath)) {
				fa = new FileAccessList (this.dataPath);
			} else
				fa = new FileAccessList (IO.CountFiles (di));

			IO.ForeachFile (di,
				(fi) => {
					if (!fa.dct.Contains (fi.FullName)) {
						fa.dct.Add (fi.FullName, fi.FullName);
						fa.currentSize += fi.Length;
					} 
//					else
//						Debug.Log ("OK");
				});
		}

		//		private void AddFileToList

		private string CutHTTP (string str)
		{
			if (!string.IsNullOrEmpty (str) && str.Length > 8 && str [0] == 'h' && str [1] == 't' && str [2] == 't' && str [3] == 'p') {
				if (str [4] == ':' && str [5] == '/' && str [6] == '/') {
					return str.Substring (7, str.Length - 7);
				} else if (str [4] == 's' && str [5] == ':' && str [6] == '/' && str [7] == '/') {
					return str.Substring (8, str.Length - 8);
				}
			}
			return str;
		}

		public byte[] Get (string filePath)
		{
			filePath = CutHTTP (filePath);

			string fullName = Path.Combine (storagePath, filePath);
			if (File.Exists (fullName)) {
//				int id = fa.list.FindIndex (x => x == fullName);
//				if (id >= 0) {
//					fa.list.RemoveAt (id);
//					fa.list.Add (fullName);
//				} else
//					fa.list.Add (fullName);
				if (fa.dct.Contains (fullName)) {
					fa.dct.Remove (fullName);
					fa.dct.Add (fullName, fullName);
//					Debug.Log ("GOT " + fullName);
				} else {
					long length = new FileInfo (fullName).Length;
					fa.dct.Add (fullName, fullName);
					fa.currentSize += length;
				}
				Save ();

				return IO.BytesFromFile (fullName);
			}
			return null;
		}

		public void Set (byte[] bytes, string filePath)
		{
			filePath = CutHTTP (filePath);

			if (bytes != null && bytes.Length > 0) {
				if (bytes.Length > maxSize) {
					Debug.LogWarning ("File " + filePath + " is too big! File size: " + bytes.Length + " max space:" + maxSize);
				} else {
					
					string fp = Path.Combine (storagePath, filePath);
					IO.SaveToFile (fp, bytes);
					FileInfo fi = new FileInfo (fp);
					fa.currentSize += fi.Length;
//					Debug.Log ("SET " + filePath);
					if (fa.currentSize > maxSize)
						MakeSpace ();
				}
				Save ();
			}
		}

		private void MakeSpace ()
		{
			try {
				while (fa.currentSize > maxSize && fa.dct.Count > 0) {
					FileInfo fi = new FileInfo ((string)fa.dct [0]);
					fa.currentSize -= fi.Length;
					IO.RemoveFileWithEmptyDirectories ((string)fa.dct [0]);
					fa.dct.RemoveAt (0);
				}
			} catch (System.Exception e) {
				Debug.Log (e.ToString ());
			}
		}

		private bool saving = false;

		public void Save ()
		{
			if (!saving) {
				saving = true;
				CommonManager.Coroutine (
					delegate {
						fa.Save (dataPath);
						saving = false;
					}, 1);
			}
		}

		[System.Serializable]
		public class FileAccessList
		{
			public long currentSize = 0;
			public OrderedDictionary dct;

			public FileAccessList (int capacity)
			{
				dct = new OrderedDictionary (capacity);
			}

			public FileAccessList (string path)
			{
				byte[] bytes = IO.BytesFromFile (path);
				if (bytes != null) {
					FileAccessList fal = bytes.ToObjectDeserialize () as FileAccessList;
					currentSize = fal.currentSize;
					dct = fal.dct;
				}
			}

			public void Save (string path)
			{
				IO.SaveToFile (path, this.ToBytesSerialize ());
			}
		}
	}
}