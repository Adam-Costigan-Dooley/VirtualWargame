using UnityEngine;
using UnityEngine.InputSystem;

public class Tower : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int damage = 1;

    [Header("Upgrade Settings")]
    [SerializeField] private int maxLevel = 3;
    [SerializeField] private int upgradeCost = 4;
    [SerializeField] private float upgradeRadius = 1.5f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite level1Sprite;
    [SerializeField] private Sprite level2Sprite;
    [SerializeField] private Sprite level3Sprite;

    private Transform _player;

    private int _level = 1;
    private float _fireTimer;

    public int Level => _level;
    public bool CanUpgrade => _level < maxLevel;

    private void Start()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        UpdateVisual();
    }


    private void Update()
    {
        _fireTimer += Time.deltaTime;
        if (_fireTimer >= fireRate)
        {
            Enemy target = FindClosestEnemy();
            if (target != null)
            {
                Shoot(target);
                _fireTimer = 0f;
            }
        }
         HandleUpgradeInput();
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        switch (_level)
        {
            case 1:
                spriteRenderer.sprite = level1Sprite;
                break;
            case 2:
                spriteRenderer.sprite = level2Sprite;
                break;
            case 3:
                spriteRenderer.sprite = level3Sprite;
                break;
        }
    }


    private Enemy FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist && dist <= attackRange)
            {
                closestDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    private void Shoot(Enemy target)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning($"Tower {name} missing projectilePrefab or firePoint");
            return;
        }

        GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile proj = projObj.GetComponent<Projectile>();
        if (proj == null)
        {
            Debug.LogWarning($"Projectile prefab on Tower {name} has no Projectile script");
            return;
        }

        proj.Launch(target.transform, damage);
        Debug.Log($"event=tower_fired towerLevel={_level}");
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
        Debug.Log("Not enough coins to upgrade tower!");
    }
}


    public void Upgrade()
    {
        if (!CanUpgrade) return;

        _level++;

        switch (_level)
        {
            case 2:
                damage += 1;
                fireRate = Mathf.Max(0.2f, fireRate - 0.25f);
                break;

            case 3:
                damage += 2;
                break;
        }
        UpdateVisual();   

        Debug.Log($"event=tower_upgraded level={_level} damage={damage} fireRate={fireRate}");
    }
}
