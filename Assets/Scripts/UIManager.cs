using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TMP_Text coinText;

    private void Awake()
    {
        Instance = this;
    }

        private void Start()
    {

        UpdateCoinUI(GameManager.Instance.Coins);
    }

    public void UpdateCoinUI(int newAmount)
    {
        coinText.text = $"Coins: {newAmount}";
    }
}
