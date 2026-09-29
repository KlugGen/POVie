using DKK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AssetBundleDownloader : Singleton<AssetBundleDownloader>
{
    private List<AssetBundleUnitInfo> assetsToDownload = new List<AssetBundleUnitInfo>();
    private bool isDownloading = false;

    private string currentAsset = "";

    private UnityEvent OnDownloadStart = new UnityEvent();
    private UnityEvent OnDownloadCompleted = new UnityEvent();
    private UnityEvent OnRetriesFail = new UnityEvent();


    public int downloadRetriesAmount = 5;

    public void RegisterOnDownloadEvent(UnityAction action) => OnDownloadStart.AddListener(action);

    public void RegisterOnDownloadCompletedEvent(UnityAction action) => OnDownloadCompleted.AddListener(action);   

    public void RegisterOnRetriesFailEvent(UnityAction action) => OnRetriesFail.AddListener(action);

    public void ClearAllEvents()
    {
        OnDownloadStart.RemoveAllListeners();
        OnDownloadCompleted.RemoveAllListeners();
        OnRetriesFail.RemoveAllListeners();
    }

    public bool IsDownloading() => isDownloading;
    
    public void AddAssetToQueue(AssetBundleUnitInfo bundle)
    {
        if (bundle != null && !string.IsNullOrEmpty(bundle.name) && !assetsToDownload.Contains(bundle))
        {
            assetsToDownload.Add(bundle);
        }
        else
        {
            bundle.name.Log("Cannot add bundle to download queue: ");
        }
    }

    public void ClearAssetsQueue()
    {
        if(!IsDownloading())
            assetsToDownload.Clear();
    }

    public void StartDownloading()
    {
        isDownloading = true;
        DownloadAsset();
    } 

    private void DownloadAsset(string assetToRetry = "", int counter = 0)
    {        
        OnDownloadStart?.Invoke();  

        if (AssetsCountToDownload() == 0) 
        {          
            isDownloading = false;
            OnDownloadCompleted?.Invoke();
            return;
        }
         
        if (assetToRetry != "")
        {
            currentAsset = assetToRetry;
            counter++;

            if (counter >= downloadRetriesAmount)
            {
                YesNoPopupController.Instance.Enable(delegate () {
                    DownloadAsset(currentAsset, 0);
                }, "Please connect to the Internet. Retry now?", false, delegate() {
                    OnRetriesFail?.Invoke();
                });
                return;
            }
        }
        else
        {
            currentAsset = assetsToDownload[0].name;
            assetsToDownload.RemoveAt(0);
        }

        AssetsBundleManager.Instance.Download(currentAsset,
                (ne, c, b) =>
                {
                    bool result = ne && APIManager.IsSuccess(c);
                    if (result)
                        DownloadAsset();
                    else
                        DownloadAsset(currentAsset, counter);
                },
                progress =>
                {

                }
            );
    }

    public int AssetsCountToDownload() => assetsToDownload.Count;
   
    public float GetSizeOfAllBundles() => assetsToDownload.Sum(o=> o.size);   

    public bool HasAnythingToDownload() => AssetsCountToDownload() > 0;

    /// <summary>
    /// Is downloading or has something in download queue
    /// </summary>
    /// <returns></returns>
    public bool IsBusy() => (isDownloading || AssetsCountToDownload() > 0);
   
}

public class AssetBundleUnitInfo
{
    public string name;
    public float size;

    public AssetBundleUnitInfo(string name, float size)
    {
        this.name = name;
        this.size = size;
    }
}