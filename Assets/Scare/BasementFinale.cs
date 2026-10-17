using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class BasementFinale : MonoBehaviour
{
    [Header("Auslöser")]
    [SerializeField] private bool onlyOnce = true;

    [Header("Freeze")]
    [Tooltip("Werden beim Betreten abgeschaltet: CharakterController, CameraController, " +
             "Flashlight, PlayerCast ... (dieselben wie beim Geist-Scare).")]
    [SerializeField] private MonoBehaviour[] componentsToFreeze;

    [Header("Schritte")]
    [Tooltip("2D-AudioSource (Spatial Blend 0), z.B. am Spieler.")]
    [SerializeField] private AudioSource sfxSource;
    [Tooltip("Schritt-Clips, z.B. Walking/walk1..walk5.")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private int footstepCount = 6;
    [Tooltip("Pause nach dem Einfrieren, bevor die Schritte beginnen.")]
    [SerializeField] private float firstStepDelay = 0.8f;
    [SerializeField] private float stepInterval = 0.55f;
    [Tooltip("Lautstärke erster -> letzter Schritt (kommt näher).")]
    [SerializeField, Range(0f, 1f)] private float stepVolumeStart = 0.4f;
    [SerializeField, Range(0f, 1f)] private float stepVolumeEnd = 1f;

    [Header("Schlag")]
    [Tooltip("Der harte Schlag am Ende. z.B. Scare/752078...heavy_metal_hit...wav")]
    [SerializeField] private AudioClip slamClip;
    [SerializeField] private float slamDelayAfterSteps = 0.35f;
    [SerializeField, Range(0f, 1f)] private float slamVolume = 1f;

    [Header("Musik")]
    [SerializeField] private bool silenceMusic = true;

    [Header("Fade / Ende")]
    [Tooltip("Dasselbe schwarze UI-Image (CanvasGroup) wie beim Geist-Scare.")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [Tooltip("Der Fade beginnt zusammen mit dem Schlag.")]
    [SerializeField] private float fadeToBlackTime = 0.6f;
    [SerializeField] private float holdBlack = 2.5f;
    [Tooltip("Szene, die danach geladen wird (z.B. 'MainMenu'). Leer = keine - " +
             "dann bleibt es schwarz und nur onEnd feuert.")]
    [SerializeField] private string endSceneName = "";

    [Header("Events (optional)")]
    public UnityEvent onFinaleStart;
    public UnityEvent onEnd;

    private bool fired;

    private void OnTriggerEnter(Collider other)
    {
        if (fired && onlyOnce) return;
        if (!other.CompareTag("Player")) return;

        fired = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        onFinaleStart?.Invoke();

        SetFrozen(true);
        if (silenceMusic && MusicPlayer.Instance != null)
            MusicPlayer.Instance.RequestSilence(this);

        if (firstStepDelay > 0f) yield return new WaitForSeconds(firstStepDelay);

        for (int i = 0; i < footstepCount; i++)
        {
            float t = footstepCount > 1 ? (float)i / (footstepCount - 1) : 1f;
            PlayStep(Mathf.Lerp(stepVolumeStart, stepVolumeEnd, t));
            yield return new WaitForSeconds(stepInterval);
        }

        if (slamDelayAfterSteps > 0f) yield return new WaitForSeconds(slamDelayAfterSteps);

        if (sfxSource != null && slamClip != null)
            sfxSource.PlayOneShot(slamClip, slamVolume * GameSettings.SfxVolume);

        yield return Fade(1f, fadeToBlackTime);

        if (holdBlack > 0f) yield return new WaitForSeconds(holdBlack);

        onEnd?.Invoke();
        if (!string.IsNullOrEmpty(endSceneName))
            SceneManager.LoadScene(endSceneName);
    }

    private void PlayStep(float volume)
    {
        if (sfxSource == null || footstepClips == null || footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        sfxSource.pitch = Random.Range(0.9f, 1.06f);
        sfxSource.PlayOneShot(clip, volume * GameSettings.SfxVolume);
    }

    private void SetFrozen(bool frozen)
    {
        if (componentsToFreeze == null) return;
        foreach (MonoBehaviour mb in componentsToFreeze)
            if (mb != null) mb.enabled = !frozen;
    }

    private IEnumerator Fade(float targetAlpha, float time)
    {
        if (fadeOverlay == null) yield break;

        float start = fadeOverlay.alpha;
        if (targetAlpha > 0.5f) fadeOverlay.blocksRaycasts = true;

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t / time));
            yield return null;
        }
        fadeOverlay.alpha = targetAlpha;
    }
}
