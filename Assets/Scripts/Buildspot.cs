using UnityEngine;
using UnityEngine.InputSystem;

public class BuildSpot : MonoBehaviour
{
    [Header("Tower Setup")]
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private Transform towerAnchor;

    [Header("Wall Setup")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform wallAnchor;

    [Header("Costs")]
    [SerializeField] private int buildCost = 3;

    private bool _playerInRange;
    private GameObject _currentTower;
    private GameObject _currentWall;
    private SpriteRenderer _markerRenderer;

    private void Awake()
    {
        _markerRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            Debug.Log($"Player entered build spot {name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            Debug.Log($"Player left build spot {name}");
        }
    }

    private void Update()
    {
        if (!_playerInRange)
            return;

        if (InputHelper.InteractPressedThisFrame())
        {
            if (_currentTower == null)
            {
                BuildTower();
            }
            else
            {
                Debug.Log("Tower already built here.");
            }
        }
    }

    private void BuildTower()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("No GameManager instance when trying to build.");
            return;
        }

        if (!GameManager.Instance.SpendCoins(buildCost))
        {
            Debug.Log("Not enough coins to build a tower!");
            return;
        }

        if (towerPrefab == null || towerAnchor == null)
        {
            Debug.LogWarning($"BuildSpot {name} missing towerPrefab or towerAnchor");
            return;
        }

        _currentTower = Instantiate(towerPrefab, towerAnchor.position, Quaternion.identity);
        Debug.Log($"event=tower_built spot={name}");

        if (wallPrefab != null && wallAnchor != null)
        {
            _currentWall = Instantiate(wallPrefab, wallAnchor.position, Quaternion.identity);
            Debug.Log($"event=wall_built spot={name}");
        }

        if (_markerRenderer != null)
        {
            _markerRenderer.enabled = false;
        }
    }


    public void ResetForNewDay()
    {
        if (_currentWall != null)
        {
            Wall wall = _currentWall.GetComponent<Wall>();
            if (wall != null)
            {
                wall.ResetWall();
                Debug.Log($"event=wall_reset spot={name}");
            }
        }
    }
}
