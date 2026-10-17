using UnityEngine;

public class PhantomSound : MonoBehaviour
{
    public enum Placement
    {
        BehindPlayer,
        FixedPosition
    }

    public enum TriggerMode
    {
        PlayerEnterZone,
        RandomInterval,
        Manual
    }

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Bei mehreren wird zufällig gewählt. Für den Tür-Klick " +
             "genau denselben Clip nehmen wie an den echten Türen!")]
    [SerializeField] private AudioClip[] clips;

    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    [Header("Wo")]
    [SerializeField] private Placement placement = Placement.BehindPlayer;

    [Tooltip("Nur bei 'BehindPlayer': Abstand hinter dem Spieler. " +
             "3 bis 5 Meter ist gut - nah genug für Schreck, weit genug, " +
             "dass es nicht direkt am Ohr klebt.")]
    [SerializeField] private float distanceBehind = 4f;

    [Tooltip("Höhe über dem Boden. Türklinken sitzen auf etwa 1 Meter, " +
             "Schritte auf 0.")]
    [SerializeField] private float heightOffset = 1f;

    [Header("Wann")]
    [SerializeField] private TriggerMode triggerMode = TriggerMode.PlayerEnterZone;

    [Tooltip("Nur in dieser Nacht. 0 = in jeder Nacht.")]
    [SerializeField] private int onlyInNight = 2;

    [Tooltip("Nur einmal pro Nacht - bei Zonen-Auslösung fast immer richtig.")]
    [SerializeField] private bool oncePerNight = true;

    [Tooltip("Wartezeit nach dem Auslösen, bevor irgendetwas passiert. " +
             "Bis dahin läuft alles normal weiter - nichts kündigt sich an. " +
             "2 bis 4 Sekunden: Der Spieler ist dann schon ein paar Schritte " +
             "weiter und der Sound kommt wirklich von hinten.")]
    [SerializeField] private float delay = 2.5f;

    [Header("Nur bei 'Random Interval'")]
    [SerializeField] private Vector2 intervalRange = new Vector2(45f, 120f);

    [Header("Musik")]
    [Tooltip("Blendet die Musik aus, wenn der Sound kommt.")]
    [SerializeField] private bool silenceMusic = true;

    [Tooltip("Vorlauf in Sekunden: Wie lange VOR dem Sound die Musik " +
             "verstummt. 0 = gleichzeitig, also Überraschung. " +
             "1 bis 2 = angekündigte Stille, der Spieler spürt es kommen. " +
             "Beides hat seinen Platz - für den Tür-Klick ist 0 richtig.")]
    [SerializeField] private float silenceLeadIn = 0f;

    [Tooltip("Wie lange die Musik nach dem Sound noch weg bleibt. " +
             "Diese Stille ist der eigentliche Moment: Der Spieler dreht " +
             "sich um und hört nichts.")]
    [SerializeField] private float silenceHold = 4f;

    private Transform player;
    private Camera playerCamera;
    private bool hasFiredThisNight;
    private bool isRunning;
    private float intervalTimer;

    private void Awake()
    {
        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;

        if (MusicPlayer.Instance != null) MusicPlayer.Instance.ReleaseSilence(this);
    }

    private void Start()
    {
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null) player = found.transform;

        playerCamera = Camera.main;

        intervalTimer = Random.Range(intervalRange.x, intervalRange.y);
    }

    private void HandleNightChanged(int night)
    {
        hasFiredThisNight = false;
        intervalTimer = Random.Range(intervalRange.x, intervalRange.y);
    }

    private void Update()
    {
        if (triggerMode != TriggerMode.RandomInterval) return;
        if (!IsActiveNight()) return;

        intervalTimer -= Time.deltaTime;
        if (intervalTimer > 0f) return;

        intervalTimer = Random.Range(intervalRange.x, intervalRange.y);
        Play();
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

        hasFiredThisNight = true;
        StartCoroutine(Sequence());
    }

    private System.Collections.IEnumerator Sequence()
    {
        isRunning = true;

        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (silenceMusic && MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.RequestSilence(this);

            if (silenceLeadIn > 0f) yield return new WaitForSeconds(silenceLeadIn);
        }

        PlaceSource();
        PlayClip();

        if (silenceMusic && MusicPlayer.Instance != null)
        {
            yield return new WaitForSeconds(silenceHold);
            MusicPlayer.Instance.ReleaseSilence(this);
        }

        isRunning = false;
    }

    private void PlaceSource()
    {
        if (placement != Placement.BehindPlayer) return;
        if (player == null) return;

        if (playerCamera == null) playerCamera = Camera.main;

        Vector3 forward = playerCamera != null ? playerCamera.transform.forward : player.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 position = player.position - forward * distanceBehind;
        position.y = player.position.y + heightOffset;

        transform.position = position;
    }

    private void PlayClip()
    {
        if (audioSource == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(clip, volume * GameSettings.SfxVolume);
    }

    private bool IsActiveNight()
    {
        if (onlyInNight <= 0) return true;
        if (NightManager.Instance == null) return true;

        return NightManager.Instance.CurrentNight == onlyInNight;
    }
}