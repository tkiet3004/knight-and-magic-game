using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinAmount = 1;
    public Vector3 spinRotationSpeed = new Vector3(0, 180, 0);
    public GameObject pickupParticlesPrefab;
    
    AudioSource pickupSource;
    private bool consumed = false;

    private void Awake()
    {
        pickupSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (consumed) return;

        // Only player collects coins
        if (collision.CompareTag("Player"))
        {
            consumed = true;

            // Save to PlayerPrefs
            int currentCoins = PlayerPrefs.GetInt("Coins", 0);
            PlayerPrefs.SetInt("Coins", currentCoins + coinAmount);
            PlayerPrefs.Save();

            // Notify UI/events
            CharacterEvents.characterCoinCollected?.Invoke(collision.gameObject, coinAmount);

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

    private void Update()
    {
        transform.eulerAngles += spinRotationSpeed * Time.deltaTime;
    }
}
