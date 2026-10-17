using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("Gesamt")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_Text masterLabel;

    [Header("Musik")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicLabel;

    [Header("Effekte")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxLabel;

    [Header("Steuerung")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityLabel;

    private void OnEnable()
    {
        ApplyToSlider(masterSlider, masterLabel, GameSettings.MasterVolume);
        ApplyToSlider(musicSlider, musicLabel, GameSettings.MusicVolume);
        ApplyToSlider(sfxSlider, sfxLabel, GameSettings.SfxVolume);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = GameSettings.MinSensitivity;
            sensitivitySlider.maxValue = GameSettings.MaxSensitivity;
            sensitivitySlider.SetValueWithoutNotify(GameSettings.MouseSensitivity);
        }
        UpdateSensitivityLabel(GameSettings.MouseSensitivity);
    }

    public void OnMasterVolumeChanged(float value)
    {
        GameSettings.SetMasterVolume(value);
        UpdateLabel(masterLabel, value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        GameSettings.SetMusicVolume(value);
        UpdateLabel(musicLabel, value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        GameSettings.SetSfxVolume(value);
        UpdateLabel(sfxLabel, value);
    }

    public void OnMouseSensitivityChanged(float value)
    {
        GameSettings.SetMouseSensitivity(value);
        UpdateSensitivityLabel(value);
    }

    private void ApplyToSlider(Slider slider, TMP_Text label, float value)
    {
        if (slider != null) slider.SetValueWithoutNotify(value);
        UpdateLabel(label, value);
    }

    private void UpdateLabel(TMP_Text label, float value)
    {
        if (label != null) label.text = Mathf.RoundToInt(value * 100f) + " %";
    }

    private void UpdateSensitivityLabel(float value)
    {
        if (sensitivityLabel != null) sensitivityLabel.text = value.ToString("0.0") + "x";
    }
}