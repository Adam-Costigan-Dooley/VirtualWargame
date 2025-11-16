using UnityEngine;
using UnityEngine.Profiling;
using System.IO;

public class PerformanceStats : MonoBehaviour
{
    public TMPro.TextMeshProUGUI statsText;

    private float fpsTimer;
    private int frames;
    private float avgFrameTime;

    void Update()
    {
        frames++;
        fpsTimer += Time.unscaledDeltaTime;

        if (fpsTimer >= 1f)
        {
            float fps = frames / fpsTimer;
            avgFrameTime = (fpsTimer / frames) * 1000f;

            long mem = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);

            statsText.text =
                $"FPS: {fps:F1}\n" +
                $"Frame Time: {avgFrameTime:F1} ms\n" +
                $"Memory: {mem} MB\n" +
                $"Enemies: {FindObjectsOfType<Enemy>().Length}";

            fpsTimer = 0f;
            frames = 0;
        }
    }

    public void LogSnapshot(string label)
    {
        string path = Application.persistentDataPath + "/perf_log.txt";
        using (StreamWriter sw = new StreamWriter(path, true))
        {
            sw.WriteLine($"[{label}] FrameTime {avgFrameTime:F1} ms, " +
                         $"Enemies {FindObjectsOfType<Enemy>().Length}");
        }
        Debug.Log($"Performance snapshot saved: {path}");
    }
}
