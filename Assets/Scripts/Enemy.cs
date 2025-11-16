using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 0.3f;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private int wallDamage = 1;
    [SerializeField] private int playerDamage = 1;
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private int baseHealth = 3;


    private Transform _target;
    private int _currentHealth;
    private Rigidbody2D _rb;
    private float _attackTimer;
    private Wall _currentWallTarget;
    private PlayerHealth _playerHealth;
    private BaseHealth _baseHealth;
    private bool _initialized = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        if (!_initialized)
        {
            _currentHealth = baseHealth;
        }

        var baseObj = GameObject.FindWithTag("Base");
        if (baseObj != null)
        {
            _target = baseObj.transform;
            _baseHealth = baseObj.GetComponent<BaseHealth>();
        }

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }



    private void Update()
    {
        if (_target == null) return;

        Wall wall = FindBlockingWall();
        if (wall != null)
        {
            _currentWallTarget = wall;
            AttackWall();
            return;
        }

        if (TryAttackPlayer())
            return;

        if (TryAttackBase())
            return;

        MoveTowardsBase();
    }


    private void MoveTowardsBase()
    {
        Vector3 dir = (_target.position - transform.position).normalized;
        transform.position += dir * (moveSpeed * Time.deltaTime);
    }

    private Wall FindBlockingWall()
    {
        Wall[] walls = FindObjectsOfType<Wall>();
        Wall closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var wall in walls)
        {
            if (!wall.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(transform.position, wall.transform.position);
            float distToBase = Vector2.Distance(transform.position, _target.position);
            float wallToBase = Vector2.Distance(wall.transform.position, _target.position);

            if (wallToBase < distToBase && dist < closestDist && dist <= attackRange * 3f)
            {
                closestDist = dist;
                closest = wall;
            }
        }

        return closest;
    }

    public void Init(int healthBonus)
    {
        _currentHealth = baseHealth + healthBonus;
        _initialized = true;
    }

    private void AttackWall()
    {
        if (_currentWallTarget == null || !_currentWallTarget.gameObject.activeInHierarchy)
        {
            _currentWallTarget = null;
            return;
        }

        float dist = Vector2.Distance(transform.position, _currentWallTarget.transform.position);

        if (dist > attackRange)
        {
            Vector3 dir = (_currentWallTarget.transform.position - transform.position).normalized;
            transform.position += dir * (moveSpeed * Time.deltaTime);
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= attackInterval)
        {
            _attackTimer = 0f;
            _currentWallTarget.TakeDamage(wallDamage);
            Debug.Log($"event=enemy_hit_wall wall={_currentWallTarget.name}");
        }
    }



    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private bool TryAttackPlayer()
    {
        if (_playerHealth == null) return false;

        float distToPlayer = Vector2.Distance(transform.position, _playerHealth.transform.position);
        if (distToPlayer > attackRange) return false;

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= attackInterval)
        {
            _attackTimer = 0f;
            _playerHealth.TakeDamage(playerDamage);
            Debug.Log("event=enemy_hit_player");
        }

        return true;
    }

    private bool TryAttackBase()
    {
        if (_baseHealth == null) return false;

        float distToBase = Vector2.Distance(transform.position, _target.position);
        if (distToBase > attackRange) return false;

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= attackInterval)
        {
            _attackTimer = 0f;
            _baseHealth.TakeDamage(baseDamage);
            Debug.Log("event=enemy_hit_base");
        }

        return true;
    }

    private void Die()
    {
        GameManager.Instance.AddCoins(1);
        WaveManager.Instance?.OnEnemyDied();
        Debug.Log("event=enemy_killed");
        Destroy(gameObject);
    }


}
