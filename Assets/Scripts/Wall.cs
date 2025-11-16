using UnityEngine;
using UnityEngine.InputSystem;
public class Wall : MonoBehaviour
{
    [SerializeField] private int baseHealth = 5;
    [SerializeField] private int maxLevel = 2;
    [SerializeField] private int upgradeCost = 3;
    [SerializeField] private float upgradeRadius = 1.5f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite level0Sprite;
    [SerializeField] private Sprite level1Sprite;
    [SerializeField] private Sprite level2Sprite;

    [Header("Health Bar")]
    [SerializeField] private Transform healthBarFill;

    private Transform _player;
    private int _level;
    private int _currentHealth;
    private int _maxHealth;

    private SpriteRenderer _renderer;
    private Collider2D _collider;

    public bool CanUpgrade => _level < maxLevel;
    public int Level => _level;

    private void Start()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
    }

    private void Update()
    {
        HandleUpgradeInput();
    }

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        _level = 0;
        _maxHealth = baseHealth;
        _currentHealth = _maxHealth;

        UpdateVisual();
        UpdateHealthBar();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        switch (_level)
        {
            case 0:
                spriteRenderer.sprite = level0Sprite;
                break;
            case 1:
                spriteRenderer.sprite = level1Sprite;
                break;
            case 2:
                spriteRenderer.sprite = level2Sprite;
                break;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null) return;

        float t = (float)_currentHealth / _maxHealth;
        t = Mathf.Clamp01(t);

        healthBarFill.localScale = new Vector3(t, 1f, 1f);
    }



    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        UpdateHealthBar();
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"event=wall_destroyed level={_level} hp={_currentHealth}");
        gameObject.SetActive(false);
    }

    public void ResetWall()
    {
        _currentHealth = _maxHealth;
        gameObject.SetActive(true);
        UpdateHealthBar();
    }

        private void HandleUpgradeInput()
    {
        if (!CanUpgrade) return;
        if (_player == null) return;
        if (GameManager.Instance == null) return;

        if (!InputHelper.InteractPressedThisFrame())
        return;

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > upgradeRadius) return;

        if (GameManager.Instance.SpendCoins(upgradeCost))
        {
            Upgrade();
        }
        else
        {
            Debug.Log("Not enough coins to upgrade wall!");
        }
    }

    public void Upgrade()
    {
        if (!CanUpgrade) return;

        _level++;
        _maxHealth *= 2;
        _currentHealth = _maxHealth;

        UpdateVisual();
        UpdateHealthBar();

        Debug.Log($"event=wall_upgraded level={_level} maxHP={_maxHealth}");
    }
}
