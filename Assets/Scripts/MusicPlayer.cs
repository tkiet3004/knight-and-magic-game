using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Plays intro followed by loop when intro is done
public class MusicPlayer : MonoBehaviour
{
    public AudioSource introSource, loopSource;

    // Start is called before the first frame update
    void Start()
    {
        introSource.Play();
        loopSource.PlayScheduled(AudioSettings.dspTime + introSource.clip.length);
        UpdateVolume();
    }

    private void Update()
    {
        UpdateVolume();
    }

    private void UpdateVolume()
    {
        // Reduced music volume to 20% so it doesn't overpower SFX
        float vol = SoundSettings.MusicEnabled ? 0.2f : 0f;
        if (introSource != null) introSource.volume = vol;
        if (loopSource != null) loopSource.volume = vol;
    }
}
