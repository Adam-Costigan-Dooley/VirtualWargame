using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;
using TMPro;
using System.IO;
using System.Text;

public class PerformanceMonitor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F3;

    [Header("Logging")]
    [SerializeField] private bool enableFileLogging = true;
    [SerializeField] private float logInterval = 10f;

    private int _frameCount;
    private float _fpsTimer;
    private float _avgFPS;
    private float _avgFrameTime;
    private float _minFrameTime = float.MaxValue;
    private float _maxFrameTime;

    private long _lastTotalAllocated;
    private long _lastGCAllocated;
    private int _gcCollectionCount;

    private float _logTimer;
    private StringBuilder _logBuilder = new StringBuilder();

    private static readonly ProfilerMarker s_UpdateMarker = new ProfilerMarker("PerfMonitor.Update");
    
    private ProfilerRecorder _gcAllocRecorder;
    private ProfilerRecorder _mainThreadRecorder;

    private void Start()
    {
        if (statsText != null)
        {
            statsText.gameObject.SetActive(showOnStart);
        }

        _gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc");
        _mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);

        _gcCollectionCount = System.GC.CollectionCount(0);

        if (enableFileLogging)
        {
            InitializeLog();
        }
    }

    private void Update()
    {
        using (s_UpdateMarker.Auto())
        {
            UpdateFrameStats();
            UpdateMemoryStats();

            if (statsText != null && statsText.gameObject.activeSelf)
            {
                UpdateUI();
            }

            if (enableFileLogging)
            {
                _logTimer += Time.unscaledDeltaTime;
                if (_logTimer >= logInterval)
                {
                    LogSnapshot("Interval");
                    _logTimer = 0f;
                }
            }

            if (Input.GetKeyDown(toggleKey))
            {
                if (statsText != null)
                {
                    statsText.gameObject.SetActive(!statsText.gameObject.activeSelf);
                }
            }
        }
    }

    private void UpdateFrameStats()
    {
        _frameCount++;
        _fpsTimer += Time.unscaledDeltaTime;

        float currentFrameTime = Time.unscaledDeltaTime * 1000f;
        _minFrameTime = Mathf.Min(_minFrameTime, currentFrameTime);
        _maxFrameTime = Mathf.Max(_maxFrameTime, currentFrameTime);

        if (_fpsTimer >= 1f)
        {
            _avgFPS = _frameCount / _fpsTimer;
            _avgFrameTime = (_fpsTimer / _frameCount) * 1000f;

            _fpsTimer = 0f;
            _frameCount = 0;
            _minFrameTime = float.MaxValue;
            _maxFrameTime = 0f;
        }
    }

    private void UpdateMemoryStats()
    {
        _lastTotalAllocated = Profiler.GetTotalAllocatedMemoryLong();
        
        if (_gcAllocRecorder.Valid)
        {
            _lastGCAllocated = _gcAllocRecorder.LastValue;
        }

        int currentGC = System.GC.CollectionCount(0);
        if (currentGC > _gcCollectionCount)
        {
            Debug.LogWarning($"[Performance] GC Collection detected! Count: {currentGC}");
            _gcCollectionCount = currentGC;
        }
    }

    private void UpdateUI()
    {
        if (statsText == null) return;

        _logBuilder.Clear();
        _logBuilder.AppendLine($"<b><color=yellow>Performance Stats</color></b>");
        _logBuilder.AppendLine($"FPS: {_avgFPS:F1} ({_avgFrameTime:F2}ms)");
        _logBuilder.AppendLine($"Min/Max: {_minFrameTime:F1}ms / {_maxFrameTime:F1}ms");
        _logBuilder.AppendLine();
        
        _logBuilder.AppendLine($"<b>Memory</b>");
        _logBuilder.AppendLine($"Total: {_lastTotalAllocated / (1024 * 1024)}MB");
        _logBuilder.AppendLine($"GC/Frame: {_lastGCAllocated / 1024f:F1}KB");
        _logBuilder.AppendLine($"GC Collections: {_gcCollectionCount}");
        _logBuilder.AppendLine();

        _logBuilder.AppendLine($"<b>Entities</b>");
        if (CombatManager.Instance != null)
        {
            _logBuilder.AppendLine($"Enemies: {CombatManager.Instance.EnemyCount}");
            _logBuilder.AppendLine($"Towers: {CombatManager.Instance.TowerCount}");
            _logBuilder.AppendLine($"Walls: {CombatManager.Instance.WallCount}");
        }

        if (ObjectPool.Instance != null)
        {
            _logBuilder.AppendLine();
            _logBuilder.AppendLine($"<b>Pools</b>");
            _logBuilder.Append(ObjectPool.Instance.GetPoolStats());
        }
        statsText.text = _logBuilder.ToString();
    }

    public void LogSnapshot(string label)
    {
        if (!enableFileLogging) return;

        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logEntry = $"[{timestamp}][{label}] " +
                         $"FPS={_avgFPS:F1}, " +
                         $"FrameTime={_avgFrameTime:F2}ms, " +
                         $"Memory={_lastTotalAllocated / (1024 * 1024)}MB, " +
                         $"GC/Frame={_lastGCAllocated / 1024f:F1}KB, " +
                         $"Enemies={CombatManager.Instance?.EnemyCount ?? 0}\n";

        try
        {
            string path = GetLogPath();
            File.AppendAllText(path, logEntry);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write performance log: {e.Message}");
        }
    }

    private void InitializeLog()
    {
        string path = GetLogPath();
        
        try
        {
            string header = $"=== Performance Log Started ===\n" +
                           $"Date: {System.DateTime.Now}\n" +
                           $"Device: {SystemInfo.deviceModel}\n" +
                           $"OS: {SystemInfo.operatingSystem}\n" +
                           $"Unity: {Application.unityVersion}\n" +
                           $"================================\n\n";
            
            File.WriteAllText(path, header);
            Debug.Log($"[Performance] Log initialized at: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize performance log: {e.Message}");
        }
    }

    private string GetLogPath()
    {
        return Path.Combine(Application.persistentDataPath, "performance_log.txt");
    }

    public string GetCurrentStats()
    {
        return $"FPS: {_avgFPS:F1}, Frame: {_avgFrameTime:F2}ms, Mem: {_lastTotalAllocated / (1024 * 1024)}MB";
    }

    private void OnDestroy()
    {
        _gcAllocRecorder.Dispose();
        _mainThreadRecorder.Dispose();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (statsText == null)
        {
            statsText = GetComponentInChildren<TMP_Text>();
        }
    }
#endif
}