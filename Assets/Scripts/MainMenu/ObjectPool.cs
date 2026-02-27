using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.SceneManagement;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 50;
        public Transform parent;
    }

    [SerializeField] private List<Pool> pools = new List<Pool>();

    private Dictionary<string, Queue<GameObject>> _poolDictionary;
    private Dictionary<string, Pool> _poolConfigs;
    private Dictionary<string, int> _activeCount;

    // Profiler markers
    private static readonly ProfilerMarker s_SpawnMarker = new ProfilerMarker("ObjectPool.Spawn");
    private static readonly ProfilerMarker s_ReturnMarker = new ProfilerMarker("ObjectPool.Return");
    private static readonly ProfilerMarker s_WarmupMarker = new ProfilerMarker("ObjectPool.Warmup");

    public static ObjectPool Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePools();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReturnAllActiveObjectsToPool();
    }

    private void InitializePools()
    {
        using (s_WarmupMarker.Auto())
        {
            _poolDictionary = new Dictionary<string, Queue<GameObject>>();
            _poolConfigs = new Dictionary<string, Pool>();
            _activeCount = new Dictionary<string, int>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectQueue = new Queue<GameObject>();

                for (int i = 0; i < pool.initialSize; i++)
                {
                    GameObject obj = CreateNewObject(pool);
                    objectQueue.Enqueue(obj);
                }

                _poolDictionary.Add(pool.tag, objectQueue);
                _poolConfigs.Add(pool.tag, pool);
                _activeCount.Add(pool.tag, 0);
            }
        }
    }

    private GameObject CreateNewObject(Pool pool)
    {
        GameObject obj = Instantiate(pool.prefab, pool.parent != null ? pool.parent : transform);
        obj.SetActive(false);
        
        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled == null)
        {
            pooled = obj.AddComponent<PooledObject>();
        }
        pooled.poolTag = pool.tag;

        return obj;
    }

    public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        using (s_SpawnMarker.Auto())
        {
            if (!_poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist!");
                return null;
            }

            GameObject objectToSpawn;
            Queue<GameObject> pool = _poolDictionary[tag];
            Pool config = _poolConfigs[tag];

            if (pool.Count > 0)
            {
                objectToSpawn = pool.Dequeue();
            }
            else
            {
                if (_activeCount[tag] >= config.maxSize)
                {
                    Debug.LogWarning($"Pool {tag} reached max size ({config.maxSize})!");
                    return null;
                }

                objectToSpawn = CreateNewObject(config);
            }

            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            _activeCount[tag]++;

            return objectToSpawn;
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        using (s_ReturnMarker.Auto())
        {
            PooledObject pooled = obj.GetComponent<PooledObject>();
            if (pooled == null)
            {
                Destroy(obj);
                return;
            }

            string tag = pooled.poolTag;

            if (!_poolDictionary.ContainsKey(tag))
            {
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            _poolDictionary[tag].Enqueue(obj);
            _activeCount[tag]--;
        }
    }

    private void ReturnAllActiveObjectsToPool()
    {
        PooledObject[] activeObjects = FindObjectsOfType<PooledObject>();
        
        foreach (PooledObject pooled in activeObjects)
        {
            if (pooled.gameObject.activeInHierarchy)
            {
                ReturnToPool(pooled.gameObject);
            }
        }

        Debug.Log($"[ObjectPool] Returned {activeObjects.Length} active objects to pool on scene reload");
    }

    public string GetPoolStats()
    {
        string stats = "=== Pool Stats ===\n";
        foreach (var kvp in _poolDictionary)
        {
            string tag = kvp.Key;
            int available = kvp.Value.Count;
            int active = _activeCount[tag];
            int total = available + active;
            stats += $"{tag}: {active} active, {available} pooled, {total} total\n";
        }
        return stats;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

public class PooledObject : MonoBehaviour
{
    [HideInInspector] public string poolTag;

    public void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}