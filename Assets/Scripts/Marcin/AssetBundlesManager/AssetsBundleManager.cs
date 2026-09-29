using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace DKK {
	public class AssetsBundleManager : Singleton<AssetsBundleManager>
	{
		public string bundleDirectory = "AssetBundles";
		public string serverUrl = "https://dkk-development.cloud";

        private Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();
		private Dictionary<string, Dictionary<string, Object>> assets = new Dictionary<string, Dictionary<string, Object>>();

        private void Awake()
        {
			bundleDirectory = Path.Combine(Application.persistentDataPath, bundleDirectory);

			if (!Directory.Exists(bundleDirectory))
            {
				Directory.CreateDirectory(bundleDirectory);
            }
        }

   //     private void Start()
   //     {
   //         DownloadExample();

   //         //yield return new WaitForSeconds(20);

   //         //DownloadExample();
   //     }

   //     private void DownloadExample()
   //     {
   //         if (!Exists("citiestextures"))
   //         {
   //             Download("citiestextures",
   //                 (c, b) =>
   //                 {
			//			if (APIManager.IsSuccess(c))
			//				LoadExample();
			//			else
			//				Debug.Log("Download unsucceed");
   //                 },
   //                 progress => progress.Log("Download progress: ")
   //             );
   //         }
   //         else
   //         {
   //             LoadExample();
   //         }

			//Debug.Log("Abort");
			//AbortDownload("citiestextures");
   //     }

   //     private void LoadExample()
   //     {
   //         Load("citiestextures", "PicsArt_10-20-10.51.56",
   //             o =>
   //             {
   //                 ((Texture2D)o).width.Log("TEX WIDTH: ");
   //             },
   //             progress => progress.Log("File load progress: "),
   //             progress => progress.Log("Asset reading progress: ")
   //         );
   //     }

        public bool Exists(string bundleName)
        {
			string path = Path.Combine(bundleDirectory, bundleName);
			return File.Exists(path);
		}

		public void Download(string bundleName, UnityAction<bool, long, byte[]> Callback, UnityAction<float> progressLogger)
		{
			string path = Path.Combine(bundleDirectory, bundleName);

			string url;

#if UNITY_IOS

			url = string.Format("{0}/ios", serverUrl);

#else

			url = serverUrl;

#endif

			APIManager.Instance.GetBytes(APIManager.Combine(url, bundleName), bundleName,
				(noError, code, bytes) =>
				{
                    if (noError && APIManager.IsSuccess(code) && bytes != null && bytes.Length > 0)
						File.WriteAllBytes(path, bytes);
					else
					{
						Debug.Log("Data from server is empty!");
					}

					Callback?.Invoke(noError, code, bytes);

				}, progressLogger);
		}

		public void AbortDownload(string bundleName)
        {
			APIManager.Instance.Abort(bundleName);
        }
		/// <summary>
		/// Loads asset from bundle. Callback returns asset or null if any problem with loading.
		/// </summary>
		/// <param name="bundleName"></param>
		/// <param name="assetName"></param>
		/// <param name="Callback"></param>
		/// <param name="fileLoadProgressLogger"></param>
		/// <param name="assetLoadProgressLogger"></param>
		public void Load(string bundleName, string assetName, UnityAction<Object> Callback, UnityAction<float> fileLoadProgressLogger, UnityAction<float> assetLoadProgressLogger)
        {
			StartCoroutine(LoadCo(bundleName, assetName, Callback, fileLoadProgressLogger, assetLoadProgressLogger));
        }

		private IEnumerator LoadCo(string bundleName, string assetName, UnityAction<Object> Callback, UnityAction<float> fileLoadProgressLogger, UnityAction<float> assetLoadProgressLogger)
		{
			AssetBundle localAssetBundle = null;

			if (assetBundles.ContainsKey(bundleName))
			{
				localAssetBundle = assetBundles[bundleName];
			}
			else
			{
				AssetBundleCreateRequest asyncBundleRequest = AssetBundle.LoadFromFileAsync(Path.Combine(bundleDirectory, bundleName));

				while (!asyncBundleRequest.isDone)
				{
					yield return null;
					fileLoadProgressLogger?.Invoke(asyncBundleRequest.progress);
				}

				localAssetBundle = asyncBundleRequest.assetBundle;

				// add bundle to dictionary
				AddBundleToDict(bundleName, localAssetBundle);
			}

			if (localAssetBundle == null)
			{
				Callback(null);
				Debug.LogError("Failed to load AssetBundle!");
				yield break;
			}

			Object asset = GetAsset(bundleName, assetName);
			if (asset == null)
			{

				AssetBundleRequest assetRequest = localAssetBundle.LoadAssetAsync<Object>(assetName);
				while (!assetRequest.isDone)
				{
					yield return null;
					assetLoadProgressLogger?.Invoke(assetRequest.progress);
				}

				asset = assetRequest.asset;

				// add asset to dictionary
				AddAssetToDict(bundleName, assetName, asset);
			}

			if (asset == null)
			{
				Callback(null);
				Debug.LogError("Failed to load Asset!");
				yield break;
			}

			Callback(asset);
		}

        public void UnloadBundle(string bundleName)
        {
			RemoveBundleFromDict(bundleName);
		}

		private void AddBundleToDict(string bundleName, AssetBundle bundle)
        {
			if (!assetBundles.ContainsKey(bundleName))
			{
				assetBundles.Add(bundleName, bundle);
			}
		}

		public void RemoveBundleFromDict(string bundleName)
        {
			if (assetBundles.ContainsKey(bundleName))
			{
				AssetBundle localAssetBundle = assetBundles[bundleName];
				localAssetBundle.Unload(false);
				assetBundles.Remove(bundleName);
			}

			System.GC.Collect();
			Resources.UnloadUnusedAssets();
		}

		private Object GetAsset(string bundleName, string assetName)
        {
			if (assets.ContainsKey(bundleName))
			{
				if (assets[bundleName].ContainsKey(assetName))
				{
					return assets[bundleName][assetName];
				}
			}

			return null;
		}

		private void AddAssetToDict(string bundleName, string assetName, Object asset)
        {
			if(assets.ContainsKey(bundleName))
            {
				if(!assets[bundleName].ContainsKey(assetName))
                {
					assets[bundleName].Add(assetName, asset);
				}
            }
			else
            {
				Dictionary<string, Object> d = new Dictionary<string, Object>();
				d.Add(assetName, asset);
				assets.Add(bundleName, d);
			}
        }

		public void RemoveAssetFromDict(string bundleName, string assetName)
        {
			if (assets.ContainsKey(bundleName))
			{
				if (assets[bundleName].ContainsKey(assetName))
				{
					assets[bundleName].Remove(assetName);

					if(assets[bundleName].Count==0)
                    {
						assets.Remove(bundleName);
                    }
				}
			}

			System.GC.Collect();
			Resources.UnloadUnusedAssets();
		}
    }
}