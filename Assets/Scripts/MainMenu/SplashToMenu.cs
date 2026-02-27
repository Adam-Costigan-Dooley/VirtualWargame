using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashToMenu : MonoBehaviour
{
    public string mainMenuScene = "MainMenu";
    public float delay = 3f;

    private void Start()
    {
        Invoke(nameof(LoadMenu), delay);
    }

    private void LoadMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}
