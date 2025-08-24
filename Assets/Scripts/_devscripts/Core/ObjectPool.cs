using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assets.Scripts.Helpers
{
    /// <summary>
    /// Simple object pool for reusing GameObjects to improve performance.
    /// Particularly useful for bullets, particles, UI elements, or any frequently spawned objects.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [BoxGroup("Pool Settings")]
        [Tooltip("Prefab to pool")]
        public GameObject PooledObject;

        [BoxGroup("Pool Settings")]
        [Tooltip("Initial number of objects to create")]
        public int PoolSize = 10;

        [BoxGroup("Pool Settings")]
        [Tooltip("Can the pool grow beyond initial size if needed?")]
        public bool CanGrow = true;

        [BoxGroup("Pool Settings")]
        [Tooltip("Maximum number of objects if CanGrow is true (0 = unlimited)")]
        public int MaxPoolSize = 50;

        [BoxGroup("Debug")]
        [SerializeField, ReadOnly]
        private int _activeCount;

        [BoxGroup("Debug")]
        [SerializeField, ReadOnly]
        private int _totalCount;

        private readonly Queue<GameObject> _availableObjects = new();
        private readonly HashSet<GameObject> _allPooledObjects = new();

        void Start()
        {
            InitializePool();
        }

        /// <summary>
        /// Creates the initial pool of objects.
        /// </summary>
        [Button("Initialize Pool")]
        void InitializePool()
        {
            if (PooledObject == null)
            {
                Debug.LogError("ObjectPool: No pooled object assigned!", this);
                return;
            }

            // Clear existing pool
            ClearPool();

            // Create initial objects
            for (int i = 0; i < PoolSize; i++)
            {
                CreateNewObject();
            }

            Debug.Log($"ObjectPool: Initialized with {PoolSize} objects of type {PooledObject.name}");
        }

        /// <summary>
        /// Gets an object from the pool. Returns null if none available and can't grow.
        /// </summary>
        /// <returns>A pooled GameObject, or null if none available</returns>
        public GameObject GetPooledObject()
        {
            if (PooledObject == null)
            {
                Debug.LogError("ObjectPool: No pooled object assigned!", this);
                return null;
            }

            GameObject obj = null;

            // Try to get from available objects
            if (_availableObjects.Count > 0)
            {
                obj = _availableObjects.Dequeue();
            }
            // If none available and we can grow
            else if (CanGrow && (MaxPoolSize == 0 || _totalCount < MaxPoolSize))
            {
                obj = CreateNewObject();
                Debug.Log($"ObjectPool: Grew pool to {_totalCount} objects");
            }

            if (obj != null)
            {
                obj.SetActive(true);
                _activeCount++;
            }

            return obj;
        }

        /// <summary>
        /// Returns an object to the pool for reuse.
        /// </summary>
        /// <param name="obj">The object to return to the pool</param>
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            // Verify this object belongs to our pool
            if (!_allPooledObjects.Contains(obj))
            {
                Debug.LogWarning($"ObjectPool: Attempted to return object {obj.name} that doesn't belong to this pool", this);
                return;
            }

            // Reset object state
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            
            // Reset common components
            var rigidbody = obj.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            var rigidbody2D = obj.GetComponent<Rigidbody2D>();
            if (rigidbody2D != null)
            {
                rigidbody2D.linearVelocity = Vector2.zero;
                rigidbody2D.angularVelocity = 0f;
            }

            _availableObjects.Enqueue(obj);
            _activeCount--;
        }

        /// <summary>
        /// Gets multiple objects from the pool.
        /// </summary>
        /// <param name="count">Number of objects to get</param>
        /// <returns>List of pooled objects (may be fewer than requested if pool exhausted)</returns>
        public List<GameObject> GetPooledObjects(int count)
        {
            var objects = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                var obj = GetPooledObject();
                if (obj != null)
                {
                    objects.Add(obj);
                }
                else
                {
                    break; // Pool exhausted
                }
            }
            return objects;
        }

        /// <summary>
        /// Returns all currently active objects to the pool.
        /// </summary>
        [Button("Return All Active Objects")]
        public void ReturnAllActiveObjects()
        {
            var activeObjects = new List<GameObject>();
            
            foreach (var obj in _allPooledObjects)
            {
                if (obj != null && obj.activeInHierarchy)
                {
                    activeObjects.Add(obj);
                }
            }

            foreach (var obj in activeObjects)
            {
                ReturnToPool(obj);
            }

            Debug.Log($"ObjectPool: Returned {activeObjects.Count} active objects to pool");
        }

        /// <summary>
        /// Creates a new object and adds it to the pool.
        /// </summary>
        GameObject CreateNewObject()
        {
            var obj = Instantiate(PooledObject, transform);
            obj.SetActive(false);
            
            _allPooledObjects.Add(obj);
            _availableObjects.Enqueue(obj);
            _totalCount++;

            // Add a component to help with automatic return
            var pooledObjectComponent = obj.GetComponent<PooledObject>();
            if (pooledObjectComponent == null)
            {
                pooledObjectComponent = obj.AddComponent<PooledObject>();
            }
            pooledObjectComponent.SetPool(this);

            return obj;
        }

        /// <summary>
        /// Clears the entire pool, destroying all objects.
        /// </summary>
        [Button("Clear Pool")]
        void ClearPool()
        {
            foreach (var obj in _allPooledObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }

            _allPooledObjects.Clear();
            _availableObjects.Clear();
            _activeCount = 0;
            _totalCount = 0;
        }

        // Public properties for monitoring
        public int ActiveCount => _activeCount;
        public int AvailableCount => _availableObjects.Count;
        public int TotalCount => _totalCount;
        public bool HasAvailable => _availableObjects.Count > 0;
    }

    /// <summary>
    /// Component automatically added to pooled objects to help with pool management.
    /// Provides easy access to return the object to its pool.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        private ObjectPool _parentPool;

        public void SetPool(ObjectPool pool)
        {
            _parentPool = pool;
        }

        /// <summary>
        /// Returns this object to its pool.
        /// </summary>
        public void ReturnToPool()
        {
            if (_parentPool != null)
            {
                _parentPool.ReturnToPool(gameObject);
            }
            else
            {
                Debug.LogWarning($"PooledObject: No parent pool set for {gameObject.name}", this);
            }
        }

        /// <summary>
        /// Returns this object to its pool after a delay.
        /// </summary>
        /// <param name="delay">Delay in seconds before returning to pool</param>
        public void ReturnToPoolAfterDelay(float delay)
        {
            if (_parentPool != null)
            {
                StartCoroutine(ReturnToPoolCoroutine(delay));
            }
        }

        private System.Collections.IEnumerator ReturnToPoolCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool();
        }

        public ObjectPool ParentPool => _parentPool;
    }
}
