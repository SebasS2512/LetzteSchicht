using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    [Header("Musik")]
    [SerializeField] private AudioClip musicClip;

    [Tooltip("Musik im Pausenmenü. Leer lassen = Musik läuft einfach weiter " +
             "(z.B. im Hauptmenü, wo es keine Pause gibt).")]
    [SerializeField] private AudioClip pauseMusicClip;

    [Tooltip("Grundlautstärke dieser Szene. Der Musik-Regler aus den " +
             "Einstellungen wird zusätzlich draufgerechnet.")]
    [SerializeField, Range(0f, 1f)] private float baseVolume = 0.7f;

    [Header("Übergang")]
    [Tooltip("Sekunden für Einblenden und für den Wechsel der Clips.")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Tooltip("Restlautstärke beim Absenken (Ducking). 0.15 = auf 15 Prozent.")]
    [SerializeField, Range(0f, 1f)] private float duckedVolume = 0.15f;

    private AudioSource activeSource;
    private AudioSource fadingSource;

    private AudioClip currentClip;
    private AudioClip eventClip;

    private readonly HashSet<Object> silenceRequests = new HashSet<Object>();
    private readonly HashSet<Object> duckRequests = new HashSet<Object>();

    private void Awake()
    {
        Instance = this;

        activeSource = CreateSource();
        fadingSource = CreateSource();
    }

    public void RequestSilence(Object requester)
    {
        if (requester != null) silenceRequests.Add(requester);
    }

    public void ReleaseSilence(Object requester)
    {
        if (requester != null) silenceRequests.Remove(requester);
    }

    public void RequestDuck(Object requester)
    {
        if (requester != null) duckRequests.Add(requester);
    }

    public void ReleaseDuck(Object requester)
    {
        if (requester != null) duckRequests.Remove(requester);
    }

    public void SetEventMusic(AudioClip clip)
    {
        eventClip = clip;
    }

    public void ClearAll()
    {
        silenceRequests.Clear();
        duckRequests.Clear();
        eventClip = null;
    }

    private void Update()
    {
        AudioClip wanted;

        if (PauseMenu.IsPaused && pauseMusicClip != null) wanted = pauseMusicClip;
        else if (eventClip != null) wanted = eventClip;
        else wanted = musicClip;

        if (wanted != currentClip)
        {
            SwitchTo(wanted);
        }

        activeSource.volume = Mathf.MoveTowards(activeSource.volume, GetTargetVolume(), GetStep());
        fadingSource.volume = Mathf.MoveTowards(fadingSource.volume, 0f, GetStep());

        if (fadingSource.volume <= 0.001f && fadingSource.isPlaying)
        {
            fadingSource.Stop();
        }
    }

    private float GetTargetVolume()
    {
        if (silenceRequests.Count > 0) return 0f;

        float volume = baseVolume * GameSettings.MusicVolume;

        if (duckRequests.Count > 0) volume *= duckedVolume;

        return volume;
    }

    private float GetStep()
    {
        float speed = fadeDuration > 0f ? 1f / fadeDuration : 999f;

        return speed * Time.unscaledDeltaTime;
    }

    private void SwitchTo(AudioClip clip)
    {
        currentClip = clip;

        AudioSource previous = activeSource;
        activeSource = fadingSource;
        fadingSource = previous;

        activeSource.clip = clip;
        activeSource.volume = 0f;

        if (clip != null) activeSource.Play();
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0f;
        return source;
    }
}