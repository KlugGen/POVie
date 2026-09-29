using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;

public class View : MonoBehaviour
{
    public string viewName = "";
    public View previousView;
                
    public GameObject landscape, portrait;

    [System.Serializable]
    public class Multitouch
    {
        public bool overideMultitouch = false;
        public bool multitouch = false;
    }

    public Multitouch multitouch;

    public List<View> subViews = new List<View>();
    public List<View> toCloseViews = new List<View>();
    private List<View> tempSubViews = new List<View>();   
    
    public bool confirmBackWithPopup = false;
    public string confirmationMessage = "";
    public bool excludeFromViewQueue = false;

    
    public List<LanguageSet> languageObjects = new List<LanguageSet>();
    public bool disableLanguageObjects = true;

    private UnityAction onDisableAction = null;
    private UnityAction onEnableAction = null;

    //[System.Serializable]
    //public class ViewSerializedFields
    //{
    //    //[UnityEngine.Serialization.FormerlySerializedAs("portrait")]
    //    public GameObject portraitF;
    //}

    //public ViewSerializedFields parentFields;
    
    //public  portraitFields, landscapeFields;

    //public View()
    //{
    //    //Debug.Log("Constructor for " + gameObject.name);
    //}

    //void Awake()
    //{
    //    Debug.Log("Awake");
    //}

    //void Start()
    //{
    //    Debug.Log("Start");
    //}

    //void OnLevelWasLoaded(int level)
    //{
    //    Debug.Log("OnLevelWasLoaded");
    //}

    //[RuntimeInitializeOnLoadMethod]
    //public void Initialize()
    //{
    //    //"Init".Log();
    //}

    /// <summary>
    /// Enable view in some context. Will add this view to context tempSubView's.
    /// </summary>
    /// <param name="context"></param>
    public virtual void Enable(View context)
    {
        context.AddViewToClose(this);
        Enable ();
    }

    [NaughtyAttributes.Button("Enable")]
    public virtual void Enable()
    {     
        if (multitouch.overideMultitouch)
            Input.multiTouchEnabled = multitouch.multitouch;

        if (!Application.isPlaying)
        {
            Debug.LogError("Enter playmode first!");
            return;
        }            

        if (disableLanguageObjects)
        {
            languageObjects.ForEach(delegate(LanguageSet l) {
                if(l.g != null)
                {
                    l.g.ForEach(delegate(GameObject g) {
                        if (g != null)
                        {
                            g.SetActive(false);
                        }
                    });
                }               
                }
            );
        }
              
        LanguageSet ls = languageObjects.Find(o => o.language.ToLower() == UserController.Instance.GetLanguage().ToLower());

        if(ls != null)
        {
            ls.g.ForEach(delegate(GameObject g) {
                if (g != null)
                {
                    g.SetActive(true);
                }
            });
        }

        gameObject.SetActive(true);

        if (!excludeFromViewQueue)
        {
            //this.name.LogDev("Added to queue: ");
            ViewsController.Instance.EnabledView(this);
        }

        ResetPanels();
        EnableWithOrientation(ViewsController.Instance.GetCurrentOrientation());

        if (!string.IsNullOrEmpty(viewName))
        {
#if !UNITY_EDITOR
    
            Dictionary<string, object> customParams = new Dictionary<string, object>();
            customParams.Add("time_since_launch", Time.realtimeSinceStartup);
            AnalyticsEvent.ScreenVisit(viewName, customParams);              
#endif
        }

        if (onEnableAction != null)
        {
            onEnableAction.Invoke();
            onEnableAction = null;
        }
    }

    public virtual bool ReadyToSkipConfirm()
    {
        return false;
    }

    public virtual void EnablePrevious()
    {
        //gameObject.name.LogDev("Enabling previous for: ");
        List<View> views = subViews.FindAll(o => o.IsEnabled());

        if (views.Count > 0)
        {
            views.ForEach(o => o.Disable());
            return;
        }

        List<View> viewsToClose = toCloseViews.FindAll(o => o.IsEnabled());
        viewsToClose.ForEach(o => o.Disable());

        if (previousView != null)
        {
            if (confirmBackWithPopup && !ReadyToSkipConfirm())
            {
                YesNoPopupController.Instance.Enable(delegate ()
                {
                    previousView.Enable();
                    Disable();
                }, confirmationMessage, true);
            }
            else
            {
                previousView.Enable();
                Disable();
            }
        }
    }

    [NaughtyAttributes.Button("Disable")]
    public virtual void Disable()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("Enter playmode first!");
            return;
        }
    

        gameObject.SetActive(false);     

        if(!excludeFromViewQueue)
            ViewsController.Instance.DisabledView(this);

        ResetPanels();
        subViews.ForEach(delegate(View v) {
            if (v != null)
            {
                v.Disable();
            }
        });

        tempSubViews.ForEach(delegate (View v) {
            if (v != null)
            {
                v.Disable();
            }
        });

        tempSubViews.Clear();

        if (onDisableAction != null)
        {
            onDisableAction.Invoke();
            onDisableAction = null;
        }
    }

    public void RegisterOnDisableAction(UnityAction action)
    {
        onDisableAction = action;
    }

    public void RegisterOnEnableAction(UnityAction action)
    {
        onEnableAction = action;
    }

    public virtual void SelfDisable()
    {
        Disable();
    }

    public void AddViewToClose(View v, bool tempView = true)
    {
        if (tempView)
        {
            if(!tempSubViews.Contains(v))
                tempSubViews.Add(v);
        }
        else
        {
            if (!toCloseViews.Contains(v))
                toCloseViews.Add(v);
        }
    }

    public bool IsEnabled()
    {
        return gameObject.activeSelf;
    }

    public virtual void EnableLandscape()
    {

    }

    public virtual void EnablePortrait()
    {

    }

    public virtual void RefreshAfterOrientationChange()
    {
        Debug.Log("Refresh orientation change");
    }

    // for UNITY_EDITOR ONLY
    public virtual void ChangeResolution()
    {

    }
    public virtual void ChangeOrientation(ScreenOrientation orientation)
    {
        Debug.Log("Changing orientation for: " + gameObject.name + " to orientation: " + orientation);
        RefreshAfterOrientationChange();

        ResetPanels();
        EnableWithOrientation(orientation);

    }
    protected void EnableWithOrientation(ScreenOrientation orientation)
    {
        if (orientation == ScreenOrientation.Landscape || orientation == ScreenOrientation.LandscapeLeft || orientation == ScreenOrientation.LandscapeRight)
        {
            landscape.SetActive(true);
            EnableLandscape();
        }
        else
        {
            portrait.SetActive(true);
            EnablePortrait();
        }

    }

    // public virtual void ChangeOrientation(DeviceOrientation orientation)
    // {

    //     RefreshAfterOrientationChange();
    //     ResetPanels();

    //     if (orientation == DeviceOrientation.LandscapeLeft || orientation == DeviceOrientation.LandscapeRight )
    //     {
    //         landscape.SetActive(true);
    //         EnableLandscape();
    //     }
    //     else
    //     {
    //         portrait.SetActive(true);
    //         EnablePortrait();
    //     }
    //}

    private void ResetPanels()
    {
        landscape.SetActive(false);
        portrait.SetActive(false);
    }

    [System.Serializable]
    public class LanguageSet
    {
        public string language;

        [NaughtyAttributes.ReorderableList]
        public List<GameObject> g = new List<GameObject>();
    }
}