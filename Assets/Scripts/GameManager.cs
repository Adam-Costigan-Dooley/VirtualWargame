using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int coins = 3;
    [SerializeField] private GameOverUI gameOverUI;

    public int Coins => coins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

         if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinUI(coins);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UIManager.Instance.UpdateCoinUI(coins);
        Debug.Log($"event=coins_updated value={coins}");
    }

    public bool SpendCoins(int cost)
    {
        if (coins >= cost)
        {
            coins -= cost;
            UIManager.Instance.UpdateCoinUI(coins);
            Debug.Log($"event=coins_spent value={coins}");
            return true;
        }
        Debug.Log("not enough coins");
        return false;
    }

    public void OnPlayerDeath()
{
    if (gameOverUI != null)
    {
        gameOverUI.ShowGameOver("You Died", "Enemies overwhelmed you.");
    }
    else
    {
        Debug.LogWarning("GameOverUI not assigned in GameManager (OnPlayerDeath).");
    }
}

public void OnBaseDestroyed()
{
    if (gameOverUI != null)
    {
        gameOverUI.ShowGameOver("Keep Fallen", "Your final keep has been destroyed.");
    }
    else
    {
        Debug.LogWarning("GameOverUI not assigned in GameManager (OnBaseDestroyed).");
    }
}

}
