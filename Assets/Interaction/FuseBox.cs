using UnityEngine;

public class FuseBox : MonoBehaviour, IInteractable
{
    [SerializeField] private string displayName = "Sicherungskasten";
    [SerializeField] private GameObject[] lights;
    [SerializeField] private bool startsOn = false;

    [Header("Aufgabe")]
    [SerializeField] private string objectiveId = "power_on";

    [Header("Sound")]
    [SerializeField] private AudioSource switchSource;
    [SerializeField] private AudioClip switchOnClip;
    [SerializeField] private AudioClip switchOffClip;
    [SerializeField] private AudioSource humSource;

    [Tooltip("Grundlautstärke des Brummens. Der Effekt-Regler kommt oben drauf.")]
    [SerializeField, Range(0f, 1f)] private float humVolume = 0.4f;

    private bool isOn;
    private bool hasReported = false;

    public string GetDisplayName() => displayName;
    public InteractionType GetInteractionType() => InteractionType.Activate;

    private void Awake()
    {
        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;
    }

    private void HandleNightChanged(int night)
    {
        hasReported = false;
        SetPower(startsOn);
    }

    private void Update()
    {
        if (isOn && humSource != null)
        {
            humSource.volume = humVolume * GameSettings.SfxVolume;
        }
    }

    public void Interact()
    {
        SetPower(!isOn);

        SoundUtils.Play(switchSource, isOn ? switchOnClip : switchOffClip, 0.04f);

        if (isOn && !hasReported)
        {
            hasReported = true;
            ObjectiveManager.Instance.ReportProgress(objectiveId);
        }
    }

    public void ForceOff()
    {
        SetPower(false);
    }

    private void SetPower(bool on)
    {
        isOn = on;
        foreach (GameObject light in lights)
        {
            if (light != null) light.SetActive(on);
        }

        if (humSource != null)
        {
            if (on) SoundUtils.PlayLoop(humSource, humVolume);
            else humSource.Stop();
        }
    }
}