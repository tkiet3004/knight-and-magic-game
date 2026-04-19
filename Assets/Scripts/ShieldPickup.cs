using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shield pickup that grants one-time damage protection.
/// Based on HealthPickup but provides shield instead of health.
/// </summary>
[RequireComponent(typeof(UniqueId))]
public class ShieldPickup : MonoBehaviour
{
    public Vector3 spinRotationSpeed = new Vector3(0, 180, 0);
    public GameObject pickupParticlesPrefab;   // assign your PickupParticles prefab in the inspector

    AudioSource pickupSource;
    private bool consumed = false;             // ensure one-time trigger
    private UniqueId uniqueId;

    private void Awake()
    {
        pickupSource = GetComponent<AudioSource>();
        uniqueId = GetComponent<UniqueId>();

        // Prevent child particles from playing at scene start
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Check if this item was already picked up
        if (GameProgressManager.Instance != null && uniqueId != null)
        {
            if (GameProgressManager.Instance.IsItemPicked(uniqueId.Id))
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (consumed) return; // already taken

        // Check if the colliding object can receive a shield
        Damageable damageable = collision.GetComponent<Damageable>();

        if (damageable && damageable.IsAlive)
        {
            // Grant shield by making the character temporarily invincible
            // We'll use the existing invincibility system
            bool shieldGranted = GrantShield(damageable);

            if (shieldGranted)
            {
                consumed = true;

                // Mark as picked
                if (GameProgressManager.Instance != null && uniqueId != null)
                {
                    GameProgressManager.Instance.MarkItemPicked(uniqueId.Id);
                }

                if (pickupParticlesPrefab)
                {
                    var particles = Instantiate(pickupParticlesPrefab, transform.position, pickupParticlesPrefab.transform.rotation);
                    var ps = particles.GetComponent<ParticleSystem>();
                    if (ps)
                    {
                        ps.Play();
                        Destroy(particles, ps.main.duration + ps.main.startLifetime.constantMax);
                    }
                    else
                    {
                        Destroy(particles, 0.5f);
                    }
                }

                if (pickupSource && SoundSettings.SFXEnabled)
                    AudioSource.PlayClipAtPoint(pickupSource.clip, gameObject.transform.position, pickupSource.volume);

                Destroy(gameObject);
            }
        }
    }

    private bool GrantShield(Damageable damageable)
    {
        // Add a ShieldEffect component to the character if it doesn't exist
        ShieldEffect shieldEffect = damageable.GetComponent<ShieldEffect>();
        if (shieldEffect == null)
        {
            shieldEffect = damageable.gameObject.AddComponent<ShieldEffect>();
        }

        // Activate the shield
        shieldEffect.ActivateShield();
        return true;
    }

    private void Update()
    {
        transform.eulerAngles += spinRotationSpeed * Time.deltaTime;
    }
}