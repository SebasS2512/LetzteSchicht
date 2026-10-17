using System.Collections.Generic;
using UnityEngine;

public class GhostApparition : MonoBehaviour
{
    public enum AppearTrigger
    {
        WhenPlayerLooksAway,
        WhenPlayerLooksAt
    }

    [Header("Geist")]
    [Tooltip("Das Modell. Darf im Editor sichtbar sein - das Skript " +
             "schaltet es beim Spielstart aus.")]
    [SerializeField] private GameObject ghostObject;

    [Tooltip("Dreht die Gestalt beim Erscheinen zum Spieler. Aus lassen, " +
             "wenn sie bewusst woandershin schauen soll - das ist oft " +
             "unheimlicher, weil sie dann eine eigene Absicht hat.")]
    [SerializeField] private bool faceThePlayer = true;

    [Header("Wo der Spieler stehen muss")]
    [Tooltip("Box Collider mit 'Is Trigger', der den Flur abdeckt. " +
             "NUR innerhalb dieser Zone kann die Gestalt auslösen. " +
             "Leer lassen = überall (nicht empfohlen).")]
    [SerializeField] private Collider requiredZone;

    [Header("Auslöser")]
    [Tooltip("Näher als das erscheint sie nicht - sonst steht sie " +
             "plötzlich direkt vor dem Spieler.")]
    [SerializeField] private float minDistance = 8f;

    [Tooltip("Weiter weg als das passiert nichts. Am besten so wählen, " +
             "dass sie im Nebel gerade noch erkennbar ist.")]
    [SerializeField] private float maxDistance = 22f;

    [SerializeField] private AppearTrigger appearTrigger = AppearTrigger.WhenPlayerLooksAt;

    [Tooltip("Wartezeit, nachdem alle Bedingungen erfüllt sind. Verhindert, " +
             "dass sie im selben Moment aufpoppt, in dem man hinschaut - " +
             "das würde sich wie eine Reaktion auf den Blick anfühlen.")]
    [SerializeField] private float appearDelay = 0.6f;

    [Tooltip("Prüft, ob wirklich freie Sicht besteht. Verhindert, dass " +
             "sie hinter einer Wand erscheint und der Moment verpufft.")]
    [SerializeField] private bool requireLineOfSight = true;

    [Tooltip("Was die Sicht blockiert. Der Player-Layer muss hier RAUS.")]
    [SerializeField] private LayerMask sightBlockers = ~0;

    [Header("Verschwinden")]
    [Tooltip("Nach so vielen Sekunden im Blick ist sie weg - auch wenn " +
             "der Spieler weiter hinschaut. 2 bis 4 Sekunden reichen.")]
    [SerializeField] private float maxVisibleTime = 3f;

    [Tooltip("Verschwindet, sobald der Spieler wegschaut. Dann ist beim " +
             "Zurückschauen niemand mehr da - der stärkste Moment.")]
    [SerializeField] private bool disappearWhenLookedAway = true;

    [Tooltip("Verschwindet, wenn der Spieler näher kommt als das. " +
             "Verhindert, dass er sie aus der Nähe betrachten kann.")]
    [SerializeField] private float disappearWhenCloserThan = 6f;

    [Tooltip("Notbremse: Falls der Spieler nie hinschaut, verschwindet " +
             "sie nach dieser Zeit wieder - darf es dann aber erneut " +
             "versuchen, wenn er wiederkommt.")]
    [SerializeField] private float unseenTimeout = 20f;

    [Header("Wann")]
    [Tooltip("Nur in dieser Nacht. 0 = in jeder Nacht.")]
    [SerializeField] private int onlyInNight = 3;

    [Tooltip("Gestalten mit derselben Gruppe erscheinen nur EINMAL pro " +
             "Nacht - egal welche zuerst dran ist. Für gespiegelte Paare " +
             "an beiden Flurenden denselben Namen eintragen.")]
    [SerializeField] private string groupId = "flur_gestalt";

    [Header("Sound (sparsam einsetzen)")]
    [Tooltip("Meist besser LEER lassen. Ein Geräusch erklärt den Moment - " +
             "die Stille lässt ihn offen.")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip appearClip;

    [Tooltip("Musik verstummt, solange sie da ist. Der Spieler merkt es " +
             "erst hinterher - und genau das macht es unangenehm.")]
    [SerializeField] private bool silenceMusic = true;

    private static readonly HashSet<string> firedGroups = new HashSet<string>();

    private Transform player;
    private Camera playerCamera;

    private bool isShowing;
    private bool hasBeenSeen;
    private float visibleTimer;
    private float aliveTimer;
    private float armedTimer;
    private bool isDoneThisNight;

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

        if (ghostObject != null) ghostObject.SetActive(false);
    }

    private void HandleNightChanged(int night)
    {
        firedGroups.Clear();
        isDoneThisNight = false;
        armedTimer = 0f;
        Hide();
    }

    private void Update()
    {
        if (isDoneThisNight || player == null || ghostObject == null) return;
        if (!IsActiveNight()) return;

        if (isShowing) UpdateWhileVisible();
        else UpdateWhileHidden();
    }

    private void UpdateWhileHidden()
    {
        if (firedGroups.Contains(groupId)) return;

        if (!ConditionsMet())
        {
            armedTimer = 0f;
            return;
        }

        armedTimer += Time.deltaTime;
        if (armedTimer >= appearDelay) Show();
    }

    private bool ConditionsMet()
    {
        if (!IsPlayerInZone()) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < minDistance || distance > maxDistance) return false;

        if (requireLineOfSight && !HasClearLineOfSight()) return false;

        bool looking = IsInPlayerView();

        return appearTrigger == AppearTrigger.WhenPlayerLooksAway ? !looking : looking;
    }

    private void UpdateWhileVisible()
    {
        aliveTimer += Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);
        bool looking = IsInPlayerView();

        if (looking)
        {
            hasBeenSeen = true;
            visibleTimer += Time.deltaTime;
        }

        if (!IsPlayerInZone())
        {
            Hide();
            if (hasBeenSeen) MarkDone();
            return;
        }

        if (distance < disappearWhenCloserThan)
        {
            Hide();
            if (hasBeenSeen) MarkDone();
            return;
        }

        if (disappearWhenLookedAway && hasBeenSeen && !looking)
        {
            Hide();
            MarkDone();
            return;
        }

        if (visibleTimer >= maxVisibleTime)
        {
            Hide();
            MarkDone();
            return;
        }

        if (aliveTimer >= unseenTimeout) Hide();
    }

    private void Show()
    {
        isShowing = true;
        hasBeenSeen = false;
        visibleTimer = 0f;
        aliveTimer = 0f;
        armedTimer = 0f;

        if (faceThePlayer)
        {
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;

            if (toPlayer.sqrMagnitude > 0.01f)
            {
                ghostObject.transform.rotation = Quaternion.LookRotation(toPlayer);
            }
        }

        ghostObject.SetActive(true);

        SoundUtils.Play(audioSource, appearClip, 0f);

        if (silenceMusic && MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.RequestSilence(this);
        }
    }

    private void Hide()
    {
        isShowing = false;

        if (ghostObject != null) ghostObject.SetActive(false);

        if (MusicPlayer.Instance != null) MusicPlayer.Instance.ReleaseSilence(this);
    }

    private void MarkDone()
    {
        isDoneThisNight = true;

        if (!string.IsNullOrEmpty(groupId)) firedGroups.Add(groupId);
    }

    private bool IsActiveNight()
    {
        if (onlyInNight <= 0) return true;
        if (NightManager.Instance == null) return true;

        return NightManager.Instance.CurrentNight == onlyInNight;
    }

    private bool IsPlayerInZone()
    {
        if (requiredZone == null) return true;

        Vector3 closest = requiredZone.ClosestPoint(player.position);
        return (closest - player.position).sqrMagnitude < 0.01f;
    }

    private bool IsInPlayerView()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return false;

        Vector3 view = playerCamera.WorldToViewportPoint(transform.position);

        return view.z > 0f
               && view.x > 0.15f && view.x < 0.85f
               && view.y > 0.15f && view.y < 0.85f;
    }

    private bool HasClearLineOfSight()
    {
        Vector3 from = transform.position + Vector3.up * 1.5f;
        Vector3 to = player.position + Vector3.up * 1f;

        return !Physics.Linecast(from, to, sightBlockers, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);

        Gizmos.color = new Color(1f, 0.4f, 0f);
        Gizmos.DrawWireSphere(transform.position, disappearWhenCloserThan);
    }
}