using System.Collections.Generic;
using UnityEngine;

namespace JungleRun
{
    // Allows pooled objects to reset themselves whenever reused.
    public interface IPoolable
    {
        void OnSpawnedFromPool();
        void OnReturnedToPool();
    }

    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size = 10;
        }

        [Header("Pool Configurations")]
        [SerializeField] private List<Pool> pools =
            new List<Pool>();

        private Dictionary<string, List<GameObject>>
            poolDictionary =
                new Dictionary<string, List<GameObject>>();

        private Dictionary<string, Transform>
            poolFolders =
                new Dictionary<string, Transform>();

        // Inactive parent used so new objects do NOT run Awake/OnEnable
        // until they are positioned and activated by SpawnFromPool.
        private Transform staging;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            GameObject stagingObj = new GameObject("[Pool Staging]");
            stagingObj.transform.SetParent(transform, false);
            stagingObj.SetActive(false);
            staging = stagingObj.transform;

            BuildPools();
        }

        private GameObject CreateInstance(GameObject prefab, Transform parent)
        {
            // Created under an inactive parent so Awake/OnEnable do NOT run yet
            GameObject obj = Instantiate(prefab, staging);
            obj.SetActive(false);
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private void BuildPools()
        {
            poolDictionary.Clear();
            poolFolders.Clear();

            foreach (Pool pool in pools)
            {
                if (pool == null ||
                    string.IsNullOrWhiteSpace(pool.tag) ||
                    pool.prefab == null)
                {
                    Debug.LogWarning(
                        "ObjectPooler: Invalid pool configuration."
                    );
                    continue;
                }

                if (poolDictionary.ContainsKey(pool.tag))
                {
                    Debug.LogWarning(
                        "ObjectPooler: Duplicate pool tag: " +
                        pool.tag
                    );
                    continue;
                }

                List<GameObject> objects =
                    new List<GameObject>();

                GameObject folder = new GameObject(
                    "[Pool] " + pool.tag
                );

                folder.transform.SetParent(transform, false);

                poolFolders.Add(
                    pool.tag,
                    folder.transform
                );

                poolDictionary.Add(
                    pool.tag,
                    objects
                );

                int size = Mathf.Max(1, pool.size);

                for (int i = 0; i < size; i++)
                {
                    GameObject obj = CreateInstance(pool.prefab, folder.transform);

                    objects.Add(obj);
                }
            }

            Debug.Log(
                "ObjectPooler: All pools initialized successfully."
            );
        }

        public GameObject SpawnFromPool(
            string poolTag,
            Vector3 position,
            Quaternion rotation)
        {
            if (string.IsNullOrWhiteSpace(poolTag))
            {
                Debug.LogError(
                    "ObjectPooler: Empty pool tag."
                );
                return null;
            }

            if (!poolDictionary.TryGetValue(
                poolTag, out List<GameObject> objects))
            {
                Debug.LogError(
                    "ObjectPooler: Pool not found: " + poolTag
                );
                return null;
            }

            // First reuse an inactive object.
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject obj = objects[i];

                if (obj == null)
                    continue;

                if (!obj.activeSelf)
                {
                    obj.transform.SetPositionAndRotation(
                        position,
                        rotation
                    );

                    obj.SetActive(true);

                    NotifySpawned(obj);

                    return obj;
                }
            }

            // Expand the pool if every object is active.
            Pool definition = pools.Find(
                p => p != null && p.tag == poolTag
            );

            if (definition == null ||
                definition.prefab == null)
            {
                Debug.LogError(
                    "ObjectPooler: Invalid pool definition: " +
                    poolTag
                );
                return null;
            }

            Transform parent = poolFolders.TryGetValue(
                poolTag, out Transform folder)
                    ? folder
                    : transform;

            GameObject newObject = CreateInstance(definition.prefab, parent);

            newObject.transform.SetPositionAndRotation(
                position,
                rotation
            );

            objects.Add(newObject);

            newObject.SetActive(true);

            NotifySpawned(newObject);

            return newObject;
        }

        private void NotifySpawned(GameObject obj)
        {
            MonoBehaviour[] components =
                obj.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                if (component is IPoolable poolable)
                    poolable.OnSpawnedFromPool();
            }
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null || !obj.activeSelf)
                return;

            // Notify components before deactivation.
            MonoBehaviour[] components =
                obj.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                if (component is IPoolable poolable)
                    poolable.OnReturnedToPool();
            }

            obj.SetActive(false);
        }

        public void ReturnObjectsInRange(
            float startZ,
            float endZ)
        {
            foreach (var pool in poolDictionary)
            {
                List<GameObject> objects = pool.Value;

                for (int i = 0; i < objects.Count; i++)
                {
                    GameObject obj = objects[i];

                    if (obj == null || !obj.activeSelf)
                        continue;

                    float z = obj.transform.position.z;

                    if (z >= startZ - 0.5f &&
                        z < endZ + 0.5f)
                    {
                        ReturnToPool(obj);
                    }
                }
            }
        }

        public void ReturnAllObjects()
        {
            foreach (var pool in poolDictionary)
            {
                foreach (GameObject obj in pool.Value)
                {
                    if (obj != null && obj.activeSelf)
                        ReturnToPool(obj);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}