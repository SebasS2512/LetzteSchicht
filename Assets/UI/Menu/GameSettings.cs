using UnityEngine;

public static class GameSettings
{
    private const string MasterKey = "volume_master";
    private const string MusicKey = "volume_music";
    private const string SfxKey = "volume_sfx";
    private const string SensitivityKey = "mouse_sensitivity";

    public const float MinSensitivity = 0.1f;
    public const float MaxSensitivity = 2f;

    public static float MasterVolume { get; private set; } = 1f;
    public static float MusicVolume { get; private set; } = 0.6f;
    public static float SfxVolume { get; private set; } = 1f;

    public static float MouseSensitivity { get; private set; } = 0.5f;

    [RuntimeInitializeOnLoadMethod]
    private static void LoadOnStartup()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterKey, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MusicKey, 0.6f);
        SfxVolume = PlayerPrefs.GetFloat(SfxKey, 1f);
        MouseSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);

        ApplyMaster();
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        ApplyMaster();
        Save(MasterKey, MasterVolume);
    }

    public static void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        Save(MusicKey, MusicVolume);
    }

    public static void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        Save(SfxKey, SfxVolume);
    }

    public static void SetMouseSensitivity(float value)
    {
        MouseSensitivity = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
        Save(SensitivityKey, MouseSensitivity);
    }

    private static void ApplyMaster()
    {
        AudioListener.volume = MasterVolume;
    }

    private static void Save(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }
}