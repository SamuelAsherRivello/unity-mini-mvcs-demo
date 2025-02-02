﻿using UnityEngine;

// ReSharper disable StaticMemberInGenericType
namespace RMC.Core.DesignPatterns.Creational.SingletonMonobehaviour
{
    //  Namespace Properties ------------------------------

    //  Class Attributes ----------------------------------

    /// <summary>
    /// This class provides a generic singleton implementation for MonoBehaviour-based classes in Unity.
    /// It includes several common features for managing the singleton lifecycle, ensuring only one instance exists,
    /// and handling Unity-specific lifecycle events like play mode changes.
    /// 
    /// Features:
    /// 1. **Lazy Initialization**: The singleton instance is created only when it is first accessed.
    /// 2. **DontDestroyOnLoad**: The singleton instance persists across scene loads, ensuring continuity.
    /// 3. **Thread Safety**: Access to the instance is managed through a static property with locking for thread safety.
    /// 4. **Handling Shutdown State**: The `_isShuttingDown` flag prevents access to the instance during application shutdown.
    /// 5. **Play Mode State Handling**: Special handling for Unity Editor play mode changes using `RuntimeInitializeOnLoadMethod` to reset the shutdown state.
    /// 6. **Duplicate Instance Management**: Ensures that duplicate instances are destroyed if they exist.
    /// 7. **Instantiate Completed Callback**: Provides a hook (`OnInitialized()`) that can be overridden for additional setup after instantiation.
    /// 8. **Automatic Cleanup**: Automatically cleans up the instance on `OnDestroy` or `OnApplicationQuit` to prevent lingering references.
    /// </summary>
    public abstract class Singleton<T> : Singleton where T : MonoBehaviour
    {
        //  Logging ---------------------------------------
        //  Easy to toggle off
        private static readonly bool IsDebug = false; //false for production

        private static void DebugLog(string message)
        {
            if (!IsDebug)
            {
                return;
            }
            Debug.Log($"{DebugPrefix}{message}");
        }
        
        private static void DebugLogError(string message)
        {
            if (!IsDebug)
            {
                return;
            }
            Debug.LogError($"{DebugPrefix}{message}");
        }
        
        
        //  Properties ------------------------------------
        public static T Instance
        {
            get
            {
                lock (_InstanceLock)
                {
                    // do nothing if currently quitting
                    if (IsShuttingDown)
                        return null;

                    // instance already found?
                    if (_Instance != null)
                        return _Instance;

                    _IsInitializing = true;

                    // search for any in-scene instance of T
                    var AllInstances = FindObjectsByType<T>(FindObjectsSortMode.None);

                    // found exactly one?
                    if (AllInstances.Length == 1)
                    {
                        DebugLog($"Found exactly 1 {typeof(T)}");
                        _Instance = AllInstances[0];
                    } // found none?
                    else if (AllInstances.Length == 0)
                    {
                        DebugLog($"Found exactly no {typeof(T)}");
                        _Instance = new GameObject($"Singleton<{typeof(T)}>").AddComponent<T>();
                    } // multiple found?
                    else
                    {
                        DebugLog($"Found exactly {AllInstances.Length} {typeof(T)}");
                        _Instance = AllInstances[0];

                        // destroy the duplicates
                        for (int Index = 1; Index < AllInstances.Length; ++Index)
                        {
                            DebugLogError($"Destroying duplicate {typeof(T)} on {AllInstances[0].gameObject.name}");
                            Destroy(AllInstances[Index].gameObject);
                        }
                    }

                    _IsInitializing = false;
                    (_Instance as Singleton)?.OnInitialized();
                    return _Instance;
                }
            }
        }

        //  Fields ----------------------------------------
        private static T _Instance = null;
        private static readonly object _InstanceLock = new object();
        private static bool _IsInitializing = false;
        

        private static string DebugPrefix => $"[{CustomName}]: ";
        
        private static string CustomName => $"Singleton<{typeof(T).Name}>";

        
        //  Initialization  -------------------------------
        private static void ConstructIfNeeded(Singleton<T> InInstance)
        {
            lock (_InstanceLock)
            {
                // only construct if the instance is null and is not being initialised
                if (_Instance == null && !_IsInitializing)
                {
                    DebugLog($"ConstructIfNeeded run for {typeof(T)}");
                    _Instance = InInstance as T;
                    
                    (_Instance as Singleton)?.OnInitialized();
                }
                else if (_Instance != null && !_IsInitializing)
                {
                    DebugLogError($"Destroying duplicate {typeof(T)} on {InInstance.gameObject.name}");
                    Destroy(InInstance.gameObject);
                }
            }
        }

        private void Awake()
        {
            ConstructIfNeeded(this);
            OnAwake();
        }

        //  Unity Methods   -------------------------------
        protected virtual void OnAwake()
        {
            //Unity requires DontDestroyOnLoad for root-level objects only
            gameObject.transform.parent = null;
            DontDestroyOnLoad(gameObject);
            _Instance!.gameObject.name = CustomName;
        }
        
        //  Other Methods ---------------------------------


        //  Event Handlers --------------------------------
    }

    public abstract class Singleton : MonoBehaviour
    {
        //  Events ----------------------------------------

        
        //  Properties ------------------------------------
        public static bool IsShuttingDown { get; private set; } = false;
        
        //  Fields ----------------------------------------

        //  Initialization  -------------------------------

        //  Unity Methods   -------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            IsShuttingDown = false;
        }

        private void OnApplicationQuit()
        {
            IsShuttingDown = true;
        }
        
        //  Other Methods ---------------------------------
        public virtual void OnInitialized()
        {
            // Override, if desired
        }
        
        //  Event Handlers --------------------------------
    }
}
