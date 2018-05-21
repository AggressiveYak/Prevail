using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;
    protected bool _enabled;


    /// <summary>
    /// Singleton design pattern
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (_instance == null)
        {
            //if this is the first instance, make it the Singleton
            _instance = this as T;
            DontDestroyOnLoad(transform.gameObject);
            _enabled = true;
        }
        else
        {
            //If a singleton already exists and you find
            //another reference in the scene, destroy it
            if (this != _instance)
            {
                Destroy(this.gameObject);
            }
        }
    }

}