using UnityEngine;

public class Door : MonoBehaviour, IInteractable, ILockable
{
    public enum DoorMode
    {
        Unlocked,
        NeedsKey,
        Sealed
    }

    [Header("Einstellungen")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 120f;
    [SerializeField] private string displayName = "Tür";

    [Tooltip("Tür steht beim Nachtbeginn schon offen. Für den Gruselfaktor: " +
             "eine Tür, die offen ist, obwohl sie es nicht sein sollte.")]
    [SerializeField] private bool startsOpen = false;

    [Header("Schlüssel")]
    [Tooltip("Unlocked = geht auf. NeedsKey = braucht den Schlüssel unten. " +
             "Sealed = geht diese Nacht gar nicht auf. Sealed ist richtig für " +
             "Räume, hinter denen nichts ist - der Spieler soll gar nicht " +
             "erst suchen.")]
    [SerializeField] private DoorMode mode = DoorMode.Unlocked;

    [Tooltip("Gilt für Öffnen UND Abschließen. Auch eine Unlocked-Tür braucht " +
             "diesen Schlüssel, wenn sie abschließbar ist.")]
    [SerializeField] private string requiredKeyId = "hausmeister";

    [Header("Abschließen (Aufgabe)")]
    [SerializeField] private bool canBeLocked = false;
    [SerializeField] private string objectiveId = "lock_doors";

    [Header("Klinken-Animation (optional)")]
    [SerializeField] private Animator handleAnimator;
    [SerializeField] private float handleDelay = 0.4f;

    [Header("Sound - Bewegung")]
    [Tooltip("Nur für Öffnen und Schließen. Diese beiden schließen sich " +
             "gegenseitig aus und brechen einander ab.")]
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;

    [Header("Sound - Einzelgeräusche")]
    [Tooltip("Für Klinke, Schloss und Rütteln. Eigene Quelle, damit diese " +
             "PARALLEL zum Knarren laufen können, statt es abzuschneiden.")]
    [SerializeField] private AudioSource detailSource;
    [SerializeField] private AudioClip handleClip;
    [SerializeField] private AudioClip unlockClip;
    [SerializeField] private AudioClip lockClip;

    [Tooltip("Wie viele Grad VOR dem Rahmen der Schnapper-Sound startet. " +
             "Höher = früher. So knallt es genau beim Ankommen statt danach.")]
    [SerializeField] private float closeSoundAngle = 8f;

    [Header("Bedienung")]
    [Tooltip("Verhindert Umschalten mitten in der Bewegung. Sorgt dafür, dass " +
             "die Tür sich wie ein schweres Objekt anfühlt statt wie ein Schalter.")]
    [SerializeField] private bool blockWhileMoving = true;

    private bool isOpen;
    private bool isLockedShut = false;

    private bool closeSoundPlayed = true;

    private bool handleRoutineRunning = false;

    private float keyTurnDoneAt;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    public string GetDisplayName() => displayName;

    public InteractionType GetInteractionType()
    {
        if (isLockedShut) return InteractionType.Locked;

        if (mode == DoorMode.Sealed) return InteractionType.Locked;

        if (!isOpen && mode == DoorMode.NeedsKey && !PlayerHasKey())
        {
            return InteractionType.Locked;
        }

        if (isOpen) return InteractionType.Close;

        return InteractionType.Use;
    }

    private void Awake()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);

        ApplyStartState();

        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;
    }

    private void ApplyStartState()
    {
        StopAllCoroutines();
        handleRoutineRunning = false;

        isOpen = startsOpen;
        isLockedShut = false;
        closeSoundPlayed = true;

        if (isOpen)
        {
            transform.localRotation = openRotation;
        }
        else
        {
            transform.localRotation = closedRotation;
        }
    }

    private void HandleNightChanged(int night)
    {
        ApplyStartState();
    }

    private void Update()
    {
        Quaternion target;
        if (isOpen)
        {
            target = openRotation;
        }
        else
        {
            target = closedRotation;
        }

        float maxDegreesThisFrame = openSpeed * Time.deltaTime;

        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, target, maxDegreesThisFrame);

        if (isOpen) return;

        float angleLeft = Quaternion.Angle(transform.localRotation, closedRotation);

        if (!closeSoundPlayed && angleLeft <= closeSoundAngle)
        {
            SoundUtils.PlayExclusive(movementSource, closeClip);
            closeSoundPlayed = true;
        }
    }

    public void Interact()
    {
        if (isLockedShut) return;

        if (handleRoutineRunning) return;

        if (blockWhileMoving && IsMoving()) return;

        if (!isOpen && mode == DoorMode.Sealed) return;

        if (!isOpen && mode == DoorMode.NeedsKey && !PlayerHasKey()) return;

        if (!isOpen && handleAnimator != null)
        {
            StartCoroutine(OpenWithHandle());
            return;
        }

        isOpen = !isOpen;

        if (isOpen)
        {
            PlayOpeningDetail();
            SoundUtils.PlayExclusive(movementSource, openClip);
        }
        else
        {
            closeSoundPlayed = false;
        }
    }

    private void PlayOpeningDetail()
    {
        AudioClip clip;
        if (mode == DoorMode.NeedsKey && unlockClip != null)
        {
            clip = unlockClip;
        }
        else
        {
            clip = handleClip;
        }

        SoundUtils.Play(detailSource, clip);
    }

    private bool IsMoving()
    {
        Quaternion target;
        if (isOpen)
        {
            target = openRotation;
        }
        else
        {
            target = closedRotation;
        }

        float angleLeft = Quaternion.Angle(transform.localRotation, target);

        return angleLeft > 1f;
    }

    private System.Collections.IEnumerator OpenWithHandle()
    {
        handleRoutineRunning = true;

        PlayOpeningDetail();
        handleAnimator.SetTrigger("Open");
        yield return new WaitForSeconds(handleDelay);

        SoundUtils.PlayExclusive(movementSource, openClip);
        isOpen = true;

        handleRoutineRunning = false;
    }

    public void SetMode(DoorMode newMode)
    {
        mode = newMode;
    }

    public void SetRequiredKeyId(string newKeyId)
    {
        requiredKeyId = newKeyId;
    }

    public void SetCanBeLocked(bool value)
    {
        canBeLocked = value;
    }

    public void SetStartsOpen(bool value)
    {
        startsOpen = value;
    }
    public void ResetToStartState()
    {
        ApplyStartState();
    }

    public bool IsLocked()
    {
        return isLockedShut;
    }

    public bool CanLockNow()
    {
        if (!canBeLocked) return false;
        if (!PlayerHasKey()) return false;
        if (IsKeyTurning()) return false;

        if (isLockedShut) return true;

        return !isOpen && !IsMoving();
    }

    public void TryLock()
    {
        if (!canBeLocked) return;

        if (!PlayerHasKey()) return;

        if (IsKeyTurning()) return;

        if (isLockedShut)
        {
            isLockedShut = false;
            SoundUtils.Play(detailSource, unlockClip);
            BlockUntilClipEnds(unlockClip);

            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.ReportRegress(objectiveId);
            }
            return;
        }

        if (isOpen || IsMoving()) return;

        isLockedShut = true;
        SoundUtils.Play(detailSource, lockClip);
        BlockUntilClipEnds(lockClip);

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.ReportProgress(objectiveId);
        }
    }

    private bool IsKeyTurning()
    {
        return Time.unscaledTime < keyTurnDoneAt;
    }

    private void BlockUntilClipEnds(AudioClip clip)
    {
        if (clip == null) return;

        keyTurnDoneAt = Time.unscaledTime + clip.length;
    }

    private bool PlayerHasKey()
    {
        return PlayerInventory.Instance != null
               && PlayerInventory.Instance.HasKey(requiredKeyId);
    }
}