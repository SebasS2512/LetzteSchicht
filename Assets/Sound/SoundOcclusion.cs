using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundOcclusion : MonoBehaviour
{
    [Header("Was blockiert den Schall?")]
    [Tooltip("Wände, Böden, Decken. Der Spieler selbst MUSS hier raus, " +
             "sonst blockiert er seinen eigenen Strahl.")]
    [SerializeField] private LayerMask occluderMask = ~0;

    [Header("Dämpfung hinter einer Wand")]
    [Tooltip("Restlautstärke hinter einer Wand. 0 = völlig still. " +
             "0.1 bis 0.2 klingt meist natürlicher als hartes Aus.")]
    [SerializeField, Range(0f, 1f)] private float occludedVolume = 0.12f;

    [Tooltip("Tiefpass hinter der Wand in Hertz. Niedriger = dumpfer. " +
             "500 klingt wie durch Beton, 2000 wie durch eine dünne Tür.")]
    [SerializeField] private float occludedCutoff = 600f;

    [Header("Feinheiten")]
    [Tooltip("Sekunden für den weichen Übergang. Zu klein = es knackt beim Durchgehen.")]
    [SerializeField] private float smoothing = 0.25f;

    [Tooltip("Wie oft der Strahl geprüft wird. 0.1 reicht völlig und " +
             "spart Rechenzeit bei vielen Tonquellen.")]
    [SerializeField] private float checkInterval = 0.1f;

    private AudioSource source;
    private AudioLowPassFilter lowPass;
    private Transform listener;

    private bool isOccluded;
    private float currentFactor = 1f;
    private float checkTimer;

    private float intendedVolume;
    private float lastAppliedVolume = -1f;

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        lowPass = GetComponent<AudioLowPassFilter>();
        if (lowPass == null) lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = 22000f;

        intendedVolume = source.volume;

        checkTimer = Random.Range(0f, checkInterval);
    }

    private void Start()
    {
        AudioListener found = FindAnyObjectByType<AudioListener>();
        if (found != null) listener = found.transform;
    }

    private void LateUpdate()
    {
        if (listener == null) return;

        if (!Mathf.Approximately(source.volume, lastAppliedVolume))
        {
            intendedVolume = source.volume;
        }

        UpdateOcclusionCheck();

        float target = isOccluded ? 0f : 1f;
        float step = smoothing > 0f ? Time.unscaledDeltaTime / smoothing : 1f;
        currentFactor = Mathf.MoveTowards(currentFactor, target, step);

        ApplyOcclusion();
    }

    private void UpdateOcclusionCheck()
    {
        checkTimer -= Time.unscaledDeltaTime;
        if (checkTimer > 0f) return;

        checkTimer = checkInterval;

        isOccluded = Physics.Linecast(
            transform.position,
            listener.position,
            occluderMask,
            QueryTriggerInteraction.Ignore);
    }

    private void ApplyOcclusion()
    {
        float volumeMultiplier = Mathf.Lerp(occludedVolume, 1f, currentFactor);

        lastAppliedVolume = intendedVolume * volumeMultiplier;
        source.volume = lastAppliedVolume;

        lowPass.cutoffFrequency = Mathf.Lerp(occludedCutoff, 22000f, currentFactor);
    }

    private void OnDrawGizmosSelected()
    {
        if (listener == null) return;

        Gizmos.color = isOccluded ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, listener.position);
    }
}