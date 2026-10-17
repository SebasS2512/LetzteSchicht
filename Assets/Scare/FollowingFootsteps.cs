using System.Collections;
using UnityEngine;

public class FollowingFootsteps : MonoBehaviour
{
    public enum TriggerMode
    {
        PlayerEnterZone,
        Manual,
        Always
    }

    public enum MusicBehaviour
    {
        Nothing,
        Duck,
        Silence
    }

    [Header("Sound")]
    [Tooltip("Muss auf einem KIND-Objekt sitzen - das Skript bewegt es.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Am besten ANDERE Clips als die des Spielers - anderer Boden, " +
             "andere Sohle. Zu ähnlich klingt es wie ein Echo, zu anders " +
             "wie eine zweite Person. Genau dazwischen ist richtig.")]
    [SerializeField] private AudioClip[] footstepClips;

    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    [Header("Musik")]
    [Tooltip("Schritte sind leise. Über laufender Musik hört man sie kaum - " +
             "Silence ist deshalb fast immer die richtige Wahl.")]
    [SerializeField] private MusicBehaviour musicBehaviour = MusicBehaviour.Silence;

    [Tooltip("Wie lange die Musik NACH den Schritten noch weg bleibt. " +
             "Diese Stille ist der eigentliche Moment: Der Spieler geht " +
             "nach oben und findet niemanden - und es bleibt still.")]
    [SerializeField] private float musicHoldAfter = 5f;

    [Header("Wann")]
    [SerializeField] private TriggerMode triggerMode = TriggerMode.PlayerEnterZone;

    [Tooltip("Verzögerung nach dem Betreten der Zone. Die Musik verstummt " +
             "SOFORT, die Schritte kommen erst danach - so hat der Spieler " +
             "einen Moment Ruhe, in dem er sie überhaupt bemerken kann.")]
    [SerializeField] private float startDelay = 2f;

    [Tooltip("Wie lange die Schritte zu hören sind. 8 bis 15 Sekunden " +
             "reichen - lang genug zum Bemerken, kurz genug, dass es " +
             "vorbei ist, bevor man es gewohnt wird.")]
    [SerializeField] private float activeDuration = 12f;

    [Tooltip("Nur in dieser Nacht. 0 = in jeder Nacht.")]
    [SerializeField] private int onlyInNight = 2;

    [Tooltip("Nur einmal pro Nacht. Für diesen Effekt fast immer richtig.")]
    [SerializeField] private bool oncePerNight = true;

    [Header("Position")]
    [Tooltip("Versatz zum Spieler. Y = 4 heißt: eine Etage höher.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 4f, 0f);

    [Tooltip("Wie träge die Quelle folgt. Etwas Trägheit ist wichtig - " +
             "exakt mitzulaufen wirkt künstlich.")]
    [SerializeField] private float followLag = 0.35f;

    [Header("Rhythmus")]
    [Tooltip("Alle wie viele Meter Spielerbewegung ein Schritt fällt. " +
             "Sollte etwa zum Wert im CharakterController passen.")]
    [SerializeField] private float stepDistance = 2.2f;

    [Tooltip("Wie viele Schritte noch kommen, nachdem der Spieler " +
             "stehen geblieben ist. DAS ist der eigentliche Effekt.")]
    [SerializeField] private int extraStepsAfterStop = 2;

    [SerializeField] private float extraStepInterval = 0.55f;

    private Transform player;
    private Transform speaker;

    private Vector3 lastPlayerPosition;
    private Vector3 followVelocity;

    private float distanceWalked;
    private bool wasMoving;

    private int pendingExtraSteps;
    private float extraStepTimer;

    private bool isActive;
    private bool isRunning;
    private bool hasFiredThisNight;

    private void Awake()
    {
        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;

        ReleaseMusic();
    }

    private void Start()
    {
        if (audioSource != null) speaker = audioSource.transform;

        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null)
        {
            player = found.transform;
            lastPlayerPosition = player.position;
        }
        else
        {
            Debug.LogWarning("FollowingFootsteps: Kein Objekt mit Tag 'Player' gefunden.", this);
        }

        if (triggerMode == TriggerMode.Always) Play();
    }

    private void HandleNightChanged(int night)
    {
        hasFiredThisNight = false;

        StopAllCoroutines();
        isActive = false;
        isRunning = false;
        ReleaseMusic();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerMode != TriggerMode.PlayerEnterZone) return;
        if (!other.CompareTag("Player")) return;

        Play();
    }

    public void Play()
    {
        if (isRunning) return;
        if (oncePerNight && hasFiredThisNight) return;
        if (!IsActiveNight()) return;
        if (player == null || speaker == null) return;

        hasFiredThisNight = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        isRunning = true;

        ApplyMusic();

        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        speaker.position = player.position + offset;

        lastPlayerPosition = player.position;
        distanceWalked = stepDistance * 0.5f;
        pendingExtraSteps = 0;
        wasMoving = false;

        isActive = true;
        yield return new WaitForSeconds(activeDuration);
        isActive = false;

        if (musicHoldAfter > 0f) yield return new WaitForSeconds(musicHoldAfter);

        ReleaseMusic();
        isRunning = false;
    }

    private void Update()
    {
        if (!isActive) return;
        if (player == null || speaker == null) return;

        FollowPlayer();
        UpdateSteps();
    }

    private void ApplyMusic()
    {
        if (MusicPlayer.Instance == null) return;

        switch (musicBehaviour)
        {
            case MusicBehaviour.Duck:
                MusicPlayer.Instance.RequestDuck(this);
                break;
            case MusicBehaviour.Silence:
                MusicPlayer.Instance.RequestSilence(this);
                break;
        }
    }

    private void ReleaseMusic()
    {
        if (MusicPlayer.Instance == null) return;

        MusicPlayer.Instance.ReleaseDuck(this);
        MusicPlayer.Instance.ReleaseSilence(this);
    }

    private bool IsActiveNight()
    {
        if (onlyInNight <= 0) return true;
        if (NightManager.Instance == null) return true;

        return NightManager.Instance.CurrentNight == onlyInNight;
    }

    private void FollowPlayer()
    {
        Vector3 target = player.position + offset;

        speaker.position = Vector3.SmoothDamp(
            speaker.position, target, ref followVelocity, followLag);
    }

    private void UpdateSteps()
    {
        Vector3 delta = player.position - lastPlayerPosition;
        delta.y = 0f;

        float moved = delta.magnitude;
        lastPlayerPosition = player.position;

        bool isMoving = moved > 0.001f;

        if (isMoving)
        {
            pendingExtraSteps = 0;

            distanceWalked += moved;
            if (distanceWalked >= stepDistance)
            {
                distanceWalked = 0f;
                PlayStep();
            }
        }
        else if (wasMoving)
        {
            pendingExtraSteps = extraStepsAfterStop;
            extraStepTimer = extraStepInterval;
            distanceWalked = 0f;
        }
        else if (pendingExtraSteps > 0)
        {
            extraStepTimer -= Time.deltaTime;
            if (extraStepTimer <= 0f)
            {
                pendingExtraSteps--;
                extraStepTimer = extraStepInterval;
                PlayStep();
            }
        }

        wasMoving = isMoving;
    }

    private void PlayStep()
    {
        if (audioSource == null || footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

        audioSource.pitch = Random.Range(0.92f, 1.08f);
        audioSource.PlayOneShot(clip, volume * GameSettings.SfxVolume);
    }
}