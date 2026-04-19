using UnityEngine;

/// <summary>
/// Gắn script này vào mỗi Enemy prefab để tự động đăng ký với LevelManager
/// Cách này chính xác hơn là dựa vào layer/tag
/// </summary>
[RequireComponent(typeof(Damageable), typeof(UniqueId))]
public class EnemyTracker : MonoBehaviour
{
    private Damageable damageable;
    private UniqueId uniqueId;
    private bool hasRegistered = false;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        uniqueId = GetComponent<UniqueId>();
        // Debug.Log($"[EnemyTracker] Awake on {gameObject.name}");
    }

    private void OnEnable()
    {
        if (damageable != null)
        {
            damageable.damageableDeath.AddListener(OnDeath);
            // Debug.Log($"[EnemyTracker] Subscribed to death event on {gameObject.name}");
        }
        else
        {
            // Debug.LogError($"[EnemyTracker] Damageable is null on {gameObject.name} in OnEnable!");
        }
    }

    private void OnDisable()
    {
        if (damageable != null)
            damageable.damageableDeath.RemoveListener(OnDeath);
    }

    private void Start()
    {
        // Check if this enemy was already killed in this session/save
        if (GameProgressManager.Instance != null && uniqueId != null)
        {
            if (GameProgressManager.Instance.IsEnemyDead(uniqueId.Id))
            {
                // Already dead, remove it immediately
                Destroy(gameObject);
                return;
            }
        }

        // Đăng ký với LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterEnemy(this);
            hasRegistered = true;
        }
    }

    private void OnDeath()
    {
        if (GameProgressManager.Instance != null && uniqueId != null)
        {
            GameProgressManager.Instance.MarkEnemyDead(uniqueId.Id);
        }
    }

    private void OnDestroy()
    {
        // Debug.Log($"[EnemyTracker] OnDestroy called for {gameObject.name}. IsAlive: {IsAlive()}");
        
        // Hủy đăng ký khi bị destroy
        if (hasRegistered && LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterEnemy(this, damageable != null && !damageable.IsAlive);
        }
    }

    public bool IsAlive()
    {
        return damageable != null && damageable.IsAlive;
    }
}
