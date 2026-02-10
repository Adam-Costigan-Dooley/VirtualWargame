using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int startingCoins = 3;
    [SerializeField] private GameOverUI gameOverUI;

    private int coins;

    public int Coins => coins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        
        Instance = this;
        coins = startingCoins;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinUI(coins);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinUI(coins);
        }
    }

    public bool SpendCoins(int cost)
    {
        if (coins >= cost)
        {
            coins -= cost;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateCoinUI(coins);
            }
            return true;
        }
        return false;
    }

    public void OnPlayerDeath()
    {
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver("You Died", "Enemies overwhelmed you.");
        }
    }

    public void OnBaseDestroyed()
    {
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver("Keep Fallen", "Your final keep has been destroyed.");
        }
    }
}