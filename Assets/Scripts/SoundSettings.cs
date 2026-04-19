using UnityEngine;

public static class SoundSettings
{
    private const string MUSIC_KEY = "MusicEnabled";
    private const string SFX_KEY = "SFXEnabled";

    public static bool MusicEnabled
    {
        get => PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        set 
        {
            PlayerPrefs.SetInt(MUSIC_KEY, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool SFXEnabled
    {
        get => PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
        set 
        {
            PlayerPrefs.SetInt(SFX_KEY, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
