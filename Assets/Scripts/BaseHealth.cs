using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    private int _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        Debug.Log($"event=base_hit hp={_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("event=base_destroyed");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBaseDestroyed();
        }
    }

}
