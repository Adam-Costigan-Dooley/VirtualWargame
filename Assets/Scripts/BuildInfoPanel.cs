using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private string buildDate = "2025-11-29";

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;
        
        panelRoot.SetActive(false);
    }

    public void Show()
    {
        string version = Application.version;
        string unityVersion = Application.unityVersion;
        string device = SystemInfo.deviceModel;
        string os = SystemInfo.operatingSystem;
        string cpu = SystemInfo.processorType;
        string gpu = SystemInfo.graphicsDeviceName;
        int ram = SystemInfo.systemMemorySize;
        int vram = SystemInfo.graphicsMemorySize;
        string res = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
        

        infoText.text =
            $"Final Keep\n\n" +
            $"Version: {version}\n" +
            $"Unity: {unityVersion}\n" +
            $"Build Date: {buildDate}\n\n" +
            $"Device: {device}\n" +
            $"OS: {os}\n" +
            $"CPU: {cpu}\n" +
            $"GPU: {gpu}\n" +
            $"RAM: {ram}MB\n" +
            $"VRAM: {vram}MB\n" +
            $"Resolution: {res}";

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }
}
