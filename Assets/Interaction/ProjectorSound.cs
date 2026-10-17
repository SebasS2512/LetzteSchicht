using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProjectorSound : MonoBehaviour
{
    [Header("Lüfter")]
    [Tooltip("Leer lassen = die AudioSource auf diesem Objekt. " +
             "Loop AN, Play On Awake AUS, Spatial Blend 1 (3D).")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Dauerndes Lüfter- oder Brummgeräusch. Muss nahtlos loopen.")]
    [SerializeField] private AudioClip humClip;

    [Tooltip("Grundlautstärke. Der Effekt-Regler kommt oben drauf.")]
    [SerializeField, Range(0f, 1f)] private float humVolume = 0.35f;

    [Header("Anlaufen")]
    [Tooltip("Einmaliges Geräusch beim Einschalten, z.B. Lampe zündet. " +
             "Leer lassen = der Lüfter läuft sofort.")]
    [SerializeField] private AudioClip startupClip;

    [SerializeField, Range(0f, 1f)] private float startupVolume = 0.7f;

    private float lastSfxVolume = -1f;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (audioSource == null || humClip == null) return;

        audioSource.clip = humClip;
        audioSource.loop = true;

        lastSfxVolume = GameSettings.SfxVolume;
        audioSource.volume = humVolume * lastSfxVolume;

        audioSource.Play();

        if (startupClip != null)
        {
            audioSource.PlayOneShot(startupClip, startupVolume * lastSfxVolume);
        }
    }

    private void OnDisable()
    {
        if (audioSource != null) audioSource.Stop();
    }

    private void Update()
    {
        if (audioSource == null) return;
        if (!audioSource.isPlaying) return;

        if (Mathf.Approximately(lastSfxVolume, GameSettings.SfxVolume)) return;

        lastSfxVolume = GameSettings.SfxVolume;
        audioSource.volume = humVolume * lastSfxVolume;
    }
}