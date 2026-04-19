using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles item dropping when enemy dies.
/// Add this component to enemy prefabs to enable drops on death.
/// </summary>
[RequireComponent(typeof(Damageable))]
public class ItemDropper : MonoBehaviour
{
    [Header("Drop Settings")]
    [Tooltip("Chance of dropping an item (0-100%)")]
    [Range(0f, 100f)]
    public float dropChance = 50f;

    [Header("Item Prefabs")]
    [Tooltip("Health pickup prefab to drop")]
    public GameObject healthPickupPrefab;

    [Tooltip("Shield pickup prefab to drop")]
    public GameObject shieldPickupPrefab;

    [Tooltip("Coin pickup prefab to drop")]
    public GameObject coinPickupPrefab;
    
    [Tooltip("Chance of dropping a coin (0-100%)")]
    [Range(0f, 100f)]
    public float coinDropChance = 50f;

    [Header("Drop Behavior")]
    [Tooltip("Offset from enemy position where item spawns")]
    public Vector3 dropOffset = new Vector3(0, 0, 0);

    private Damageable damageable;
    private bool hasDropped = false;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
    }

    private void OnEnable()
    {
        if (damageable != null)
        {
            damageable.damageableDeath.AddListener(OnDeath);
        }
    }

    private void OnDisable()
    {
        if (damageable != null)
        {
            damageable.damageableDeath.RemoveListener(OnDeath);
        }
    }

    private void OnDeath()
    {
        // Ensure we only drop once
        if (hasDropped) return;
        hasDropped = true;

        // Initialize random rolls
        float roll = Random.Range(0f, 100f);
        float coinRoll = Random.Range(0f, 100f);

        bool dropItem = roll <= dropChance;
        
        // Calculate distinct positions if both drop
        Vector3 basePos = transform.position + dropOffset;
        Vector3 coinPos = basePos;
        Vector3 itemPos = basePos;

        if (dropItem && coinRoll <= coinDropChance && coinPickupPrefab != null)
        {
            // Both are dropping, separate them
            coinPos += new Vector3(0.5f, 0.5f, 0); // Right and Up
            itemPos += new Vector3(-0.5f, 0.5f, 0); // Left and Up
        }
        else
        {
            // Single drop logic
            coinPos += new Vector3(0, 0.5f, 0);
            itemPos += new Vector3(0, 0.5f, 0);
        }

        // Drop Coin
        if (coinRoll <= coinDropChance && coinPickupPrefab != null)
        {
             GameObject coin = Instantiate(coinPickupPrefab, coinPos, Quaternion.identity);
             // Ensure it's not huge
             coin.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        }

        // Drop Item (Health/Shield)
        if (dropItem)
        {
            GameObject prefabToDrop = (Random.Range(0, 2) == 0) ? healthPickupPrefab : shieldPickupPrefab;
            if (prefabToDrop != null)
            {
                Instantiate(prefabToDrop, itemPos, Quaternion.identity);
            }
        }
    }
}