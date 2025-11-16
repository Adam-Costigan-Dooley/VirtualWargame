using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private string gameplaySceneName = "Main";

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OnOptionsClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        Time.timeScale = 1f;
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void OnOptionsClicked()
    {
        Debug.Log("Options clicked (not implemented yet).");
    }

    private void OnExitClicked()
    {
        Debug.Log("Exit clicked.");
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
