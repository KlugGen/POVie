using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

class AndroidJavaUti
{
	private static string m_pkgName;
	private static string m_externalStorage;
	private static string m_dataDirectory;
	public static AndroidJavaObject Activity {
		get {
			AndroidJavaClass jcPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			return jcPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
		}
	}


	public static string CurrentPkgName {
		get {
			if (m_pkgName == null)
				m_pkgName = Activity.Call<string> ("getPackageName");
			return m_pkgName;
		}
	}

	public static string DataDirectory {
		get {
			if (m_dataDirectory == null) {
				AndroidJavaClass jc = new AndroidJavaClass ("android.os.Environment");
				IntPtr getDataDirectoryMethod = AndroidJNI.GetStaticMethodID (jc.GetRawClass (), "getDataDirectory", "()Ljava/io/File;");
				IntPtr file = AndroidJNI.CallStaticObjectMethod (jc.GetRawClass (), getDataDirectoryMethod, new jvalue[] { });
				IntPtr getPathMethod = AndroidJNI.GetMethodID (AndroidJNI.GetObjectClass (file), "getPath", "()Ljava/lang/String;");
				IntPtr path = AndroidJNI.CallObjectMethod (file, getPathMethod, new jvalue[] { });
				m_dataDirectory = AndroidJNI.GetStringUTFChars (path);
				AndroidJNI.DeleteLocalRef (file);
				AndroidJNI.DeleteLocalRef (path);
				Debug.Log ("m_dataDirectory = " + m_dataDirectory);
			}
			return m_dataDirectory;
		}
	}

	public static string ExternalStorage {
		get {
			if (m_externalStorage == null) {
				AndroidJavaClass jc = new AndroidJavaClass ("android.os.Environment");
				IntPtr getExternalStorageDirectoryMethod = AndroidJNI.GetStaticMethodID (jc.GetRawClass (), "getExternalStorageDirectory", "()Ljava/io/File;");
				IntPtr file = AndroidJNI.CallStaticObjectMethod (jc.GetRawClass (), getExternalStorageDirectoryMethod, new jvalue[] { });
				IntPtr getPathMethod = AndroidJNI.GetMethodID (AndroidJNI.GetObjectClass (file), "getPath", "()Ljava/lang/String;");
				IntPtr path = AndroidJNI.CallObjectMethod (file, getPathMethod, new jvalue[] { });
				m_externalStorage = AndroidJNI.GetStringUTFChars (path);
				AndroidJNI.DeleteLocalRef (file);
				AndroidJNI.DeleteLocalRef (path);
				Debug.Log ("m_externalStorage = " + m_externalStorage);
			}
			return m_externalStorage;
		}
	}

	public static int GetSDKInt ()
	{
		using (var version = new AndroidJavaClass ("android.os.Build$VERSION")) {
			return version.GetStatic<int> ("SDK_INT");
		}
	}

	public class URL : MonoBehaviour
	{
		static private AndroidJavaClass plugin;

		static private void Init ()
		{
			AndroidJavaClass unityClass;
			AndroidJavaObject unityActivity;

			unityClass = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			unityActivity = unityClass.GetStatic<AndroidJavaObject> ("currentActivity");
			plugin = new AndroidJavaClass ("com.maroplugin.dkk.plugin.OpenURL");
			string str = plugin.CallStatic<string> ("SetActivity", unityActivity);

		}

		static public void Open (string path)
		{
			string type = "";
			switch (Path.GetExtension (path).ToLower ()) {
			case ".apk":
				//type = "application/vnd.android.package-archive";
				OpenAPK (path);
				return;
			case ".pdf":
				type = "application/pdf";
				break;
			case ".jpeg":
			case ".jpg":
			case ".png":
			case ".gif":
			case ".bmp":
				type = "image/*";
				break;
			case ".mp3":
			case ".wav":
			case ".ogg":
				type = "audio/*";
				break;
			case ".mp4":
			case ".avi":
			case ".flv":
			case ".mkv":
				type = "video/*";
				break;
			}

			if (plugin == null) {
				Init ();
			}

			try {
				string str = plugin.CallStatic<string> ("Open", path, type);
			} catch (System.Exception e) {
				Debug.Log (e.ToString ());
			}
		}

		static private void OpenAPK (string path)
		{
			if (AndroidJavaUti.GetSDKInt () >= 24) {
				Debug.Log ("SDK level >= 24");
				try {
					//Get Activity then Context
					AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
					AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
					AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject> ("getApplicationContext");
		
					//Get the package Name
					string packageName = unityContext.Call<string> ("getPackageName");
					string authority = packageName + ".fileprovider";
		
					AndroidJavaClass intentObj = new AndroidJavaClass ("android.content.Intent");
					string ACTION_VIEW = intentObj.GetStatic<string> ("ACTION_VIEW");
					AndroidJavaObject intent = new AndroidJavaObject ("android.content.Intent", ACTION_VIEW);
		
		
					int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int> ("FLAG_ACTIVITY_NEW_TASK");
					int FLAG_GRANT_READ_URI_PERMISSION = intentObj.GetStatic<int> ("FLAG_GRANT_READ_URI_PERMISSION");
		
					//File fileObj = new File(String pathname);
					AndroidJavaObject fileObj = new AndroidJavaObject ("java.io.File", path);
					//FileProvider object that will be used to call it static function
					AndroidJavaClass fileProvider = new AndroidJavaClass ("android.support.v4.content.FileProvider");
					//getUriForFile(Context context, String authority, File file)
					AndroidJavaObject uri = fileProvider.CallStatic<AndroidJavaObject> ("getUriForFile", unityContext, authority, fileObj);
		
					intent.Call<AndroidJavaObject> ("setDataAndType", uri, "application/vnd.android.package-archive");
					intent.Call<AndroidJavaObject> ("addFlags", FLAG_ACTIVITY_NEW_TASK);
					intent.Call<AndroidJavaObject> ("addFlags", FLAG_GRANT_READ_URI_PERMISSION);
					currentActivity.Call ("startActivity", intent);
		
				} catch (System.Exception e) {
					Debug.Log (e.ToString ());
				}
		
			} else {
				Debug.Log ("SDK level < 24");
				try {
					AndroidJavaClass intentObj = new AndroidJavaClass ("android.content.Intent");
					string ACTION_VIEW = intentObj.GetStatic<string> ("ACTION_VIEW");
					int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int> ("FLAG_ACTIVITY_NEW_TASK");
					AndroidJavaObject intent = new AndroidJavaObject ("android.content.Intent", ACTION_VIEW);
		
					AndroidJavaObject fileObj = new AndroidJavaObject ("java.io.File", path);
					AndroidJavaClass uriObj = new AndroidJavaClass ("android.net.Uri");
					AndroidJavaObject uri = uriObj.CallStatic<AndroidJavaObject> ("fromFile", fileObj);
		
					intent.Call<AndroidJavaObject> ("setDataAndType", uri, "application/vnd.android.package-archive");
					intent.Call<AndroidJavaObject> ("addFlags", FLAG_ACTIVITY_NEW_TASK);
					intent.Call<AndroidJavaObject> ("setClassName", "com.android.packageinstaller", "com.android.packageinstaller.PackageInstallerActivity");
		
					AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
					AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
					currentActivity.Call ("startActivity", intent);
		
				} catch (System.Exception e) {
					Debug.Log (e.ToString ());
				}
			}
		}
	}
}
