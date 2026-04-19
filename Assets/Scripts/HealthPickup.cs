using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueId))]
public class HealthPickup : MonoBehaviour
{
    public int healthRestore = 20;
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

        Damageable damageable = collision.GetComponent<Damageable>();

        if (damageable && damageable.Health < damageable.MaxHealth)
        {
            bool wasHealed = damageable.Heal(healthRestore);

            if (wasHealed)
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

    private void Update()
    {
        transform.eulerAngles += spinRotationSpeed * Time.deltaTime;
    }
}
