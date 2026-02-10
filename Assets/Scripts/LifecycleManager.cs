using UnityEngine;
using Unity.Profiling;

public class LifecycleManager : MonoBehaviour
{
    public static LifecycleManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private bool autoPauseOnFocusLoss = true;
    [SerializeField] private bool logLifecycleEvents = true;

    private bool _wasPausedByFocusLoss;
    private float _lastPauseTime;
    private float _lastResumeTime;

    private static readonly ProfilerMarker s_PauseMarker = new ProfilerMarker("Lifecycle.OnPause");
    private static readonly ProfilerMarker s_ResumeMarker = new ProfilerMarker("Lifecycle.OnResume");

    public delegate void LifecycleEvent();
    public static event LifecycleEvent OnApplicationPauseEvent;
    public static event LifecycleEvent OnApplicationResumeEvent;
    public static event LifecycleEvent OnApplicationFocusLostEvent;
    public static event LifecycleEvent OnApplicationFocusGainedEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (logLifecycleEvents)
        {
            Debug.Log("[Lifecycle] Manager initialized");
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            HandleApplicationPaused();
        }
        else
        {
            HandleApplicationResumed();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            HandleFocusLost();
        }
        else
        {
            HandleFocusGained();
        }
    }

    private void HandleApplicationPaused()
    {
        using (s_PauseMarker.Auto())
        {
            _lastPauseTime = Time.realtimeSinceStartup;

            if (logLifecycleEvents)
            {
                Debug.Log($"[Lifecycle] Application paused at {_lastPauseTime:F2}s");
            }

            if (autoPauseOnFocusLoss && !PauseManager.IsPaused)
            {
                PauseManager.Instance?.PauseGame();
                _wasPausedByFocusLoss = true;
            }

            SaveGameState();

            OnApplicationPauseEvent?.Invoke();

            AudioListener.pause = true;
        }
    }

    private void HandleApplicationResumed()
    {
        using (s_ResumeMarker.Auto())
        {
            _lastResumeTime = Time.realtimeSinceStartup;
            float pauseDuration = _lastResumeTime - _lastPauseTime;

            if (logLifecycleEvents)
            {
                Debug.Log($"[Lifecycle] Application resumed after {pauseDuration:F2}s");
            }

            AudioListener.pause = false;
            OnApplicationResumeEvent?.Invoke();

            LogTelemetryEvent("app_resumed", pauseDuration);
        }
    }

    private void HandleFocusLost()
    {
        if (logLifecycleEvents)
        {
            Debug.Log("[Lifecycle] Focus lost");
        }

        if (autoPauseOnFocusLoss && !PauseManager.IsPaused)
        {
            PauseManager.Instance?.PauseGame();
            _wasPausedByFocusLoss = true;
        }

        OnApplicationFocusLostEvent?.Invoke();
    }

    private void HandleFocusGained()
    {
        if (logLifecycleEvents)
        {
            Debug.Log("[Lifecycle] Focus gained");
        }

        OnApplicationFocusGainedEvent?.Invoke();
    }

    private void SaveGameState()
    {
        if (WaveManager.Instance != null)
        {
            PlayerPrefs.SetInt("LastNightIndex", WaveManager.Instance.NightIndex);
        }

        if (GameManager.Instance != null)
        {
            PlayerPrefs.SetInt("Coins", GameManager.Instance.Coins);
        }

        PlayerPrefs.SetString("LastPauseTime", System.DateTime.Now.ToString());
        PlayerPrefs.Save();

        if (logLifecycleEvents)
        {
            Debug.Log("[Lifecycle] Game state saved");
        }
    }

    private void LogTelemetryEvent(string eventName, float value)
    {
        Debug.Log($"event={eventName} value={value:F2}");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}