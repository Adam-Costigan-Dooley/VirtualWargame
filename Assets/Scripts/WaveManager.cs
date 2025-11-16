using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    public enum Phase
    {
        Day,
        Night
    }

    [Header("Durations (seconds)")]
    [SerializeField] private float dayDuration = 20f;
    [SerializeField] private float nightEndDelayAfterClear = 5f;

    [Header("Spawners")]
    [SerializeField] private EnemySpawner[] enemySpawners;

    private Phase _currentPhase = Phase.Day;
    private float _phaseTimer;

    private int _nightIndex = 0;
    private int _enemiesAlive = 0;
    private float _nightClearTimer = -1f;

    public Phase CurrentPhase => _currentPhase;
    public int NightIndex => _nightIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _phaseTimer = dayDuration;
        SetPhase(Phase.Day);
    }

    private void Update()
    {
        if (_currentPhase == Phase.Day)
        {
            _phaseTimer -= Time.deltaTime;
            if (_phaseTimer <= 0f)
            {
                StartNight();
            }
        }
        else if (_currentPhase == Phase.Night)
        {
            if (_nightClearTimer > 0f)
            {
                _nightClearTimer -= Time.deltaTime;
                if (_nightClearTimer <= 0f)
                {
                    StartDay();
                }
            }
        }
    }

    private void StartDay()
    {
        SetPhase(Phase.Day);
        _phaseTimer = dayDuration;
    }

    private void StartNight()
    {
        _nightIndex++;
        _enemiesAlive = 0;
        _nightClearTimer = -1f;

        SetPhase(Phase.Night);

        int enemiesPerSpawner = 3 + Mathf.CeilToInt(_nightIndex / 2f);
        int healthBonus = _nightIndex / 2;

        foreach (var spawner in enemySpawners)
        {
            if (spawner != null)
            {
                spawner.ConfigureForNight(enemiesPerSpawner, healthBonus, _nightIndex);
            }
        }
    }

    private void SetPhase(Phase newPhase)
    {
        _currentPhase = newPhase;
        Debug.Log($"event=phase_changed phase={_currentPhase}");

        bool spawnersActive = (_currentPhase == Phase.Night);
        foreach (var spawner in enemySpawners)
        {
            if (spawner != null)
                spawner.SetActive(spawnersActive);
        }

        if (_currentPhase == Phase.Day)
        {
            ResetWallsForNewDay();
        }
        FindObjectOfType<PerformanceStats>()?.LogSnapshot("Night End");
    }

    private void ResetWallsForNewDay()
    {
        var buildSpots = FindObjectsOfType<BuildSpot>();
        foreach (var spot in buildSpots)
        {
            spot.ResetForNewDay();
        }
    }

    public void OnEnemySpawned()
    {
        if (_currentPhase != Phase.Night) return;

        _enemiesAlive++;

        if (_nightClearTimer > 0f)
        {
            _nightClearTimer = -1f;
        }
    }

    public void OnEnemyDied()
    {
        if (_currentPhase != Phase.Night) return;

        _enemiesAlive--;

        if (_enemiesAlive <= 0)
        {
            _nightClearTimer = nightEndDelayAfterClear;
            Debug.Log("event=night_clear_timer_started");
        }
    }
}
