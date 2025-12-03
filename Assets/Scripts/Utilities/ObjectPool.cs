using UnityEngine;
using System.Collections.Generic;

namespace PixelGame
{
    /// <summary>
    /// Generic object pooling system for performance optimization
    /// Reduces garbage collection by reusing GameObjects
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int initialSize;
        }

        [Header("Pools Configuration")]
        [SerializeField] private List<Pool> pools = new List<Pool>();

        [Header("Settings")]
        [SerializeField] private bool expandPoolsAutomatically = true;
        [SerializeField] private int expandAmount = 5;

        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, GameObject> prefabDictionary;
        private Dictionary<GameObject, string> instanceToPoolTag;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializePools();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            prefabDictionary = new Dictionary<string, GameObject>();
            instanceToPoolTag = new Dictionary<GameObject, string>();

            foreach (var pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.initialSize; i++)
                {
                    GameObject obj = CreatePooledObject(pool.prefab, pool.tag);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
                prefabDictionary.Add(pool.tag, pool.prefab);
            }
        }

        private GameObject CreatePooledObject(GameObject prefab, string tag)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            instanceToPoolTag[obj] = tag;
            return obj;
        }

        /// <summary>
        /// Spawn object from pool by tag
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
                return null;
            }

            GameObject objectToSpawn;

            // If pool is empty, expand it or return null
            if (poolDictionary[tag].Count == 0)
            {
                if (expandPoolsAutomatically)
                {
                    ExpandPool(tag, expandAmount);
                }
                else
                {
                    Debug.LogWarning($"Pool {tag} is empty and auto-expand is disabled.");
                    return null;
                }
            }

            objectToSpawn = poolDictionary[tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.transform.SetParent(null); // Unparent from pool

            // Call interface method if exists
            IPoolable poolable = objectToSpawn.GetComponent<IPoolable>();
            if (poolable != null)
            {
                poolable.OnSpawnedFromPool();
            }

            return objectToSpawn;
        }

        /// <summary>
        /// Return object to pool
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            if (!instanceToPoolTag.ContainsKey(obj))
            {
                Debug.LogWarning($"Object {obj.name} is not from any pool. Destroying instead.");
                Destroy(obj);
                return;
            }

            string tag = instanceToPoolTag[obj];

            // Call interface method if exists
            IPoolable poolable = obj.GetComponent<IPoolable>();
            if (poolable != null)
            {
                poolable.OnReturnedToPool();
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform);
            poolDictionary[tag].Enqueue(obj);
        }

        /// <summary>
        /// Expand a pool by adding more objects
        /// </summary>
        private void ExpandPool(string tag, int amount)
        {
            if (!prefabDictionary.ContainsKey(tag))
            {
                Debug.LogError($"Cannot expand pool {tag}: prefab not found.");
                return;
            }

            GameObject prefab = prefabDictionary[tag];

            for (int i = 0; i < amount; i++)
            {
                GameObject obj = CreatePooledObject(prefab, tag);
                poolDictionary[tag].Enqueue(obj);
            }
        }

        /// <summary>
        /// Clear and destroy all pooled objects
        /// </summary>
        public void ClearPool(string tag)
        {
            if (!poolDictionary.ContainsKey(tag)) return;

            while (poolDictionary[tag].Count > 0)
            {
                GameObject obj = poolDictionary[tag].Dequeue();
                Destroy(obj);
            }
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var poolTag in poolDictionary.Keys)
            {
                ClearPool(poolTag);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Get the number of available objects in a pool
        /// </summary>
        public int GetAvailableCount(string tag)
        {
            if (!poolDictionary.ContainsKey(tag)) return 0;
            return poolDictionary[tag].Count;
        }

        /// <summary>
        /// Pre-warm a pool by spawning and returning objects
        /// </summary>
        public void PreWarmPool(string tag, int count)
        {
            List<GameObject> tempObjects = new List<GameObject>();

            for (int i = 0; i < count; i++)
            {
                GameObject obj = SpawnFromPool(tag, Vector3.zero, Quaternion.identity);
                if (obj != null)
                {
                    tempObjects.Add(obj);
                }
            }

            foreach (var obj in tempObjects)
            {
                ReturnToPool(obj);
            }
        }

        #endregion
    }

    /// <summary>
    /// Interface for pooled objects to respond to pooling events
    /// </summary>
    public interface IPoolable
    {
        void OnSpawnedFromPool();
        void OnReturnedToPool();
    }
}
