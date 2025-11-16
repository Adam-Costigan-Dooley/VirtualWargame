using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverTitle;
    [SerializeField] private TMP_Text gameOverReason;
    [SerializeField] private Button retryButton;

    private void Awake()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryPressed);
        }
    }

    public void ShowGameOver(string title, string reason)
    {
        if (gameOverPanel == null) return;

        if (gameOverTitle != null)
            gameOverTitle.text = title;

        if (gameOverReason != null)
            gameOverReason.text = reason;

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log($"event=game_over title={title} reason={reason}");
    }

    private void OnRetryPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
