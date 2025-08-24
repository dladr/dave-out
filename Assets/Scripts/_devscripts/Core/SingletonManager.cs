using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Helpers
{
    /// <summary>
    /// Modern Service Locator pattern for managing singletons and tagged GameObjects.
    /// Provides caching with automatic cleanup of destroyed objects.
    /// </summary>
    public static class SingletonManager
    {
        private static readonly Dictionary<Type, WeakReference> _singletons = new();
        private static readonly Dictionary<string, WeakReference> _taggedObjects = new();

        /// <summary>
        /// Clears all cached references. Useful for scene transitions.
        /// </summary>
        public static void ClearCache()
        {
            _singletons.Clear();
            _taggedObjects.Clear();
        }

        /// <summary>
        /// Gets a GameObject by tag with caching. Returns null if not found instead of throwing.
        /// </summary>
        public static GameObject GetTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                Debug.LogWarning("SingletonManager.GetTag called with null or empty tag name");
                return null;
            }

            // Check cache first
            if (_taggedObjects.TryGetValue(tagName, out var weakRef) && 
                weakRef.IsAlive && weakRef.Target is GameObject cachedObject && cachedObject != null)
            {
                return cachedObject;
            }

            // Find and cache the object
            var foundObject = GameObject.FindGameObjectWithTag(tagName);
            if (foundObject != null)
            {
                _taggedObjects[tagName] = new WeakReference(foundObject);
            }

            return foundObject;
        }

        /// <summary>
        /// Gets a GameObject by tag, throwing an exception if not found.
        /// </summary>
        public static GameObject RequireTag(string tagName)
        {
            var result = GetTag(tagName);
            if (result == null)
            {
                throw new InvalidOperationException($"Required GameObject with tag '{tagName}' not found in scene");
            }
            return result;
        }

        /// <summary>
        /// Gets a singleton instance of type T. Returns null if not found.
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            
            // Check cache first
            if (_singletons.TryGetValue(type, out var weakRef) && 
                weakRef.IsAlive && weakRef.Target is T cachedInstance && 
                IsValidUnityObject(cachedInstance))
            {
                return cachedInstance;
            }

            // Find the instance
            T foundInstance = null;
            
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                foundInstance = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<T>().FirstOrDefault();
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                foundInstance = Resources.FindObjectsOfTypeAll<ScriptableObject>().OfType<T>().FirstOrDefault();
            }
            else
            {
                // For non-Unity objects, just check if we have any cached reference
                foundInstance = _singletons.Values
                    .Where(wr => wr.IsAlive && wr.Target is T)
                    .Select(wr => (T)wr.Target)
                    .FirstOrDefault();
            }

            // Cache the result if found
            if (foundInstance != null)
            {
                _singletons[type] = new WeakReference(foundInstance);
            }

            return foundInstance;
        }

        /// <summary>
        /// Gets a singleton instance of type T, throwing an exception if not found.
        /// </summary>
        public static T Require<T>() where T : class
        {
            var result = Get<T>();
            if (result == null)
            {
                throw new InvalidOperationException($"Required singleton of type '{typeof(T).Name}' not found");
            }
            return result;
        }

        /// <summary>
        /// Manually registers a singleton instance.
        /// </summary>
        public static void Register<T>(T instance) where T : class
        {
            if (instance == null)
            {
                Debug.LogWarning($"Attempted to register null instance for type {typeof(T).Name}");
                return;
            }

            _singletons[typeof(T)] = new WeakReference(instance);
        }

        /// <summary>
        /// Unregisters a singleton instance.
        /// </summary>
        public static void Unregister<T>()
        {
            _singletons.Remove(typeof(T));
        }

        /// <summary>
        /// Checks if a singleton of type T is currently registered and valid.
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return Get<T>() != null;
        }

        private static bool IsValidUnityObject(object obj)
        {
            if (obj is Object unityObj)
            {
                return unityObj != null; // Unity's null check handles destroyed objects
            }
            return obj != null; // For non-Unity objects, standard null check
        }

        // Convenience methods for common patterns
        public static T GetComponent<T>(string tagName) where T : Component
        {
            var gameObject = GetTag(tagName);
            return gameObject != null ? gameObject.GetComponent<T>() : null;
        }

        public static T RequireComponent<T>(string tagName) where T : Component
        {
            var component = GetComponent<T>(tagName);
            if (component == null)
            {
                throw new InvalidOperationException($"Required component '{typeof(T).Name}' not found on GameObject with tag '{tagName}'");
            }
            return component;
        }
    }
}