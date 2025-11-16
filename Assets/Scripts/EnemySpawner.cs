using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [Header("Spawning")]
    [SerializeField] private float baseSpawnInterval = 3f;
    [SerializeField] private float minSpawnInterval = 1.5f;
    [SerializeField] private float spawnIntervalStepMin = 0.2f;
    [SerializeField] private float spawnIntervalStepMax = 0.4f;
    private float _currentSpawnInterval;
    private float _timer;
    private bool _isActive = false;

    private int _remainingToSpawn;
    private int _enemyHealthBonus;

    private void Update()
    {
        if (!_isActive) return;
        if (_remainingToSpawn <= 0) return;

        _timer += Time.deltaTime;
        if (_timer >= _currentSpawnInterval)
        {
            _timer = 0f;
            SpawnEnemy();
        }
    }


    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner has no enemyPrefab assigned.");
            return;
        }

        GameObject obj = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Init(_enemyHealthBonus);
        }

        WaveManager.Instance?.OnEnemySpawned();
        _remainingToSpawn--;

        Debug.Log("event=enemy_spawned");
    }

    public void SetActive(bool active)
    {
        _isActive = active;
        if (!active)
        {
            _timer = 0f;
        }
    }

public void ConfigureForNight(int enemiesToSpawn, int healthBonus, int nightIndex)
{
    _remainingToSpawn = enemiesToSpawn;
    _enemyHealthBonus = healthBonus;
    _timer = 0f;

    float step = Random.Range(spawnIntervalStepMin, spawnIntervalStepMax);
    float target = baseSpawnInterval - (nightIndex - 1) * step;

    _currentSpawnInterval = Mathf.Max(minSpawnInterval, target);

    Debug.Log($"Spawner {name} night={nightIndex} enemiesToSpawn={enemiesToSpawn} interval={_currentSpawnInterval:F2}");
}

}
