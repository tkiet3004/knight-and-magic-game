using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides a shield that blocks one incoming damage hit.
/// This component is added dynamically to characters when they pick up a shield.
/// Shows a HUD icon while the shield is active.
/// </summary>
[RequireComponent(typeof(Damageable))]
public class ShieldEffect : MonoBehaviour
{
    private Damageable damageable;
    private bool shieldActive = false;

    // Optional: you can still assign a HUD in the Inspector if you place the character in the scene.
    // If left null, it will auto-find a GameObject tagged "ShieldHUD".
    [Header("HUD")]
    [Tooltip("UI element (e.g., Image) that indicates the shield is active. If empty, will find an object tagged 'ShieldHUD'.")]
    [SerializeField] private GameObject shieldHud;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();

        // Auto-find by tag if not assigned
        if (shieldHud == null)
        {
            var tagged = GameObject.FindWithTag("ShieldHUD");
            if (tagged != null) shieldHud = tagged;
        }

        UpdateHud();
    }

    private void OnEnable()
    {
        if (damageable != null)
        {
            damageable.damageableHit.AddListener(OnDamageableHit);
        }

        UpdateHud();
    }

    private void OnDisable()
    {
        if (damageable != null)
        {
            damageable.damageableHit.RemoveListener(OnDamageableHit);
        }

        // Ensure HUD doesn't linger if this component is disabled
        SetHudVisible(false);
    }

    /// <summary>
    /// Activates the shield, protecting from the next damage hit
    /// </summary>
    public void ActivateShield()
    {
        shieldActive = true;
        UpdateHud();
    }

    /// <summary>
    /// Called when the Damageable receives damage
    /// </summary>
    private void OnDamageableHit(int damage, Vector2 knockback)
    {
        if (shieldActive)
        {
            // Restore the health that was lost by this hit (clamped to max)
            damageable.Health = Mathf.Min(damageable.Health + damage, damageable.MaxHealth);

            // Deactivate the shield after blocking one hit
            shieldActive = false;

            UpdateHud();
        }
    }

    /// <summary>
    /// Check if the shield is currently active
    /// </summary>
    public bool IsShieldActive()
    {
        return shieldActive;
    }

    /// <summary>
    /// Update the HUD visibility to reflect current shield state.
    /// </summary>
    private void UpdateHud()
    {
        SetHudVisible(shieldActive);
    }

    private void SetHudVisible(bool visible)
    {
        if (shieldHud != null)
        {
            shieldHud.SetActive(visible);
        }
    }
}