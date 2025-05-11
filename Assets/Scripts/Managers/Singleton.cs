using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    static T m_instance = null;

    static public bool IsNull { get { return m_instance == null; } }

    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                // search if it already exists
                if ((m_instance = FindObjectOfType<T>()) == null)
                {
                    // create a new one
                    GameObject gO = new($"{typeof(T).Name} (singleton)");
                    m_instance = gO.AddComponent<T>();
                    m_instance.Initialize();
                }
            }
            return m_instance;
        }
        private set { }
    }

    protected virtual void Initialize() { }

    protected virtual void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this as T;
            Initialize();
        }
        else if (m_instance != this) { Destroy(this); }
    }

    private void OnApplicationQuit() { Destroy(this); }
}