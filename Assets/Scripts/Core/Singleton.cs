using UnityEngine;

/// <summary>
/// Inherit from this base class to create a singleton.
/// e.g. public class MyClassName : Singleton<MyClassName> {}
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Check to see if we're about to be destroyed.
    private static bool m_ShuttingDown = false;
    private static object m_Lock = new object();
    private static T m_Instance;

    public bool inDebug = false;

    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static T Instance
    {
        get
        {
            // TODO: commented - _TEST

            //if (m_ShuttingDown)
            //{
            //    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
            //        "' already destroyed. Returning null.");
            //    return null;
            //}

            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    // Search for existing instance.
                    //m_Instance = (T)FindObjectOfType(typeof(T));

                    Object[] instances = Resources.FindObjectsOfTypeAll(typeof(T));
                    int instances_count = instances.Length;
                    if (instances_count > 1)
                    {
                        Debug.Log(typeof(T).ToString());
                        instances_count.LogDev("Instances count: ");
                    }

                    m_Instance = Resources.FindObjectsOfTypeAll(typeof(T))[0] as T;

                    // Create new instance if one doesn't already exist.
                    if (m_Instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        m_Instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";

                        // Make instance persistent.
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return m_Instance;
            }
        }
    }

    public static void SetExternalSingleton(T obj)
    {
        m_Instance = obj;
    }


    private void OnApplicationQuit()
    {
        m_ShuttingDown = true;
    }


    private void OnDestroy()
    {
        m_ShuttingDown = true;
    }
}