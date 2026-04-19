using UnityEngine;

/// <summary>
/// Handles item drops for a Boss.
/// Drops 1 item every time the boss loses 25% of its max health.
/// </summary>
[RequireComponent(typeof(Damageable))]
public class BossItemDropper : MonoBehaviour
{
    [Header("Item Prefabs")]
    [Tooltip("Health pickup prefab")]
    public GameObject healthPickupPrefab;

    [Tooltip("Shield pickup prefab")]
    public GameObject shieldPickupPrefab;

    [Header("Drop Behavior")]
    public Vector3 dropOffset = Vector3.zero;

    private Damageable damageable;

    private int maxHealth;
    private int nextHealthThreshold;
    private int dropsDone = 0;

    private const int MAX_DROPS = 4; // 25% × 4

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        maxHealth = damageable.MaxHealth;

        // First drop happens at 75% HP
        nextHealthThreshold = Mathf.CeilToInt(maxHealth * 0.75f);
    }

    private void OnEnable()
    {
        damageable.damageableHit.AddListener(OnDamageTaken);
    }

    private void OnDisable()
    {
        damageable.damageableHit.RemoveListener(OnDamageTaken);
    }

    private void OnDamageTaken(int damage, Vector2 knockback)
    {
        // Prevent extra drops
        if (dropsDone >= MAX_DROPS)
            return;

        // If boss crossed the next threshold
        if (damageable.Health <= nextHealthThreshold)
        {
            DropItem();
            dropsDone++;

            // Calculate next threshold
            float nextPercent = 0.75f - (dropsDone * 0.25f);
            nextHealthThreshold = Mathf.CeilToInt(maxHealth * nextPercent);
        }
    }

    private void DropItem()
    {
        GameObject prefabToDrop =
            Random.Range(0, 2) == 0 ? healthPickupPrefab : shieldPickupPrefab;

        if (prefabToDrop == null)
            return;

        Vector3 dropPosition = transform.position + dropOffset;
        Instantiate(prefabToDrop, dropPosition, Quaternion.identity);
    }
}
