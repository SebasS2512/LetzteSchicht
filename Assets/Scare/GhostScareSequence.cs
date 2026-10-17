using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GhostScareSequence : MonoBehaviour
{
    [Header("Ablauf")]
    [Tooltip("Kurze Pause nach 'alles außer Müll fertig', BEVOR der Scare startet - damit er " +
             "nicht im selben Moment aufpoppt, in dem der Spieler die Aufgabe abschließt. " +
             "Der Spieler ist in dieser Zeit noch normal beweglich.")]
    [SerializeField] private float startDelay = 2f;

    [Header("Geist")]
    [Tooltip("Das Geist-GameObject (wird zu Beginn ein-, am Ende ausgeblendet).")]
    [SerializeField] private GameObject ghostObject;
    [SerializeField] private Animator ghostAnimator;
    [Tooltip("Transform, das bewegt wird. Meist dasselbe wie ghostObject.")]
    [SerializeField] private Transform ghostTransform;
    [SerializeField] private string floatState = "GHOST_C_Float";
    [SerializeField] private string lungeState = "GHOST_C_Lunge";
    [SerializeField] private float crossFade = 0.1f;

    [Header("Spieler")]
    [SerializeField] private Transform player;
    [Tooltip("Leer = Camera.main.")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("Diese Komponenten werden während des Scares abgeschaltet: " +
             "CharakterController, CameraController, Flashlight, PlayerCast ...")]
    [SerializeField] private MonoBehaviour[] componentsToFreeze;

    [Header("Anflug")]
    [SerializeField] private bool spawnInFrontOfPlayer = true;
    [Tooltip("Aus wie vielen Metern vor dem Spieler er heranschwebt (nur bei 'Spawn In Front Of Player').")]
    [SerializeField] private float spawnDistance = 8f;
    [Tooltip("Fester Startpunkt - NUR benutzt, wenn 'Spawn In Front Of Player' AUS ist. " +
             "Leer = aktuelle Position von ghostTransform.")]
    [SerializeField] private Transform ghostStartPoint;
    [Tooltip("Wie nah der Geist beim Anflug an die Kamera heranfloatet (Meter, waagerecht).")]
    [SerializeField] private float approachStopDistance = 2.2f;
    [SerializeField] private float approachDuration = 3.5f;
    [Tooltip("Wie nah er im Lunge zusätzlich rankommt (kleiner = dichter vorm Gesicht).")]
    [SerializeField] private float lungeStopDistance = 0.8f;
    [Tooltip("Sollte ungefähr zur Lunge-Cliplänge passen (~0.9s).")]
    [SerializeField] private float lungeDuration = 0.9f;
    [Tooltip("Y-Höhe des Geist-Fußpunkts relativ zur Kamera. Negativ = tiefer, " +
             "damit Kopf/Fratze grob auf Augenhöhe schweben.")]
    [SerializeField] private float ghostYOffsetFromCamera = -0.9f;

    [Header("Sound")]
    [Tooltip("2D-AudioSource (Spatial Blend 0) für den Schreck-Sound. Am Spieler oder auf diesem Objekt.")]
    [SerializeField] private AudioSource sfxSource;
    [Tooltip("Sting im Moment des Zugreifens (Lunge). z.B. Scare/847619...horror-metallic-stinger.wav")]
    [SerializeField] private AudioClip scareStinger;
    [Tooltip("Musik/Ambient NACH dem Scare - ersetzt die normale Musik bis zum nächsten " +
             "Nachtwechsel. z.B. etwas aus 'Background Noise'. Leer = normale Musik kommt zurück.")]
    [SerializeField] private AudioClip afterMusic;

    [Header("Fade / Aufwachen")]
    [Tooltip("Schwarzes UI-Image als CanvasGroup (Alpha 0, Vollbild).")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [Tooltip("Ab welcher Sekunde IM Lunge das Bild schwarz wird.")]
    [SerializeField] private float fadeStartInLunge = 0.45f;
    [SerializeField] private float fadeToBlackTime = 0.35f;
    [SerializeField] private float blackHold = 2.0f;
    [Tooltip("AUS (Standard): der Spieler wacht genau dort auf, wo er stand. " +
             "AN: er wird beim Aufwachen an den WakeUpPoint versetzt (z.B. in ein Bett).")]
    [SerializeField] private bool teleportOnWake = false;
    [Tooltip("Wohin der Spieler beim Aufwachen gesetzt wird (Position + Blickrichtung). " +
             "NUR benutzt, wenn 'Teleport On Wake' an ist.")]
    [SerializeField] private Transform wakeUpPoint;
    [SerializeField] private float fadeInTime = 2.5f;

    [Header("Danach")]
    [Tooltip("Gedanke direkt nach dem Aufwachen. Erklärt, warum es dunkel " +
            "ist und wohin es weitergeht.")]
    [TextArea(2, 3)]
    [SerializeField] private string wakeThought = "Der Strom ist weg. Ich muss runter in den Keller.";

    [SerializeField] private float wakeThoughtDuration = 10f;
    [SerializeField] private GameObject basementFinale;
    [Tooltip("Flashlight des Spielers (wird beim Aufwachen hart ausgeschaltet). Optional.")]
    [SerializeField] private Flashlight flashlight;
    [Tooltip("Sicherungskasten, der nach dem Aufwachen wieder ausgeschaltet wird " +
             "(Strom + Licht aus -> der Spieler muss in den Keller). Optional.")]
    [SerializeField] private FuseBox fuseBoxToDisable;

    [Header("Events (optional)")]
    public UnityEvent onSequenceStart;
    public UnityEvent onBlack;
    public UnityEvent onWakeComplete;

    private bool isRunning;

    public void Play()
    {
        if (isRunning) return;
        isRunning = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        onSequenceStart?.Invoke();

        SetFrozen(true);
        if (MusicPlayer.Instance != null) MusicPlayer.Instance.RequestSilence(this);

        if (ghostTransform != null)
        {
            if (spawnInFrontOfPlayer)
                ghostTransform.position = TargetPoint(spawnDistance);
            else if (ghostStartPoint != null)
                ghostTransform.position = ghostStartPoint.position;
            FaceCameraYaw();
        }

        if (ghostObject != null) ghostObject.SetActive(true);
        PlayState(floatState);

        yield return MoveGhost(approachStopDistance, approachDuration);

        PlayState(lungeState);
        if (sfxSource != null && scareStinger != null)
            SoundUtils.Play(sfxSource, scareStinger, 0f);

        Vector3 from = ghostTransform != null ? ghostTransform.position : Vector3.zero;
        float t = 0f;
        bool startedFade = false;
        while (t < lungeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / lungeDuration);
            k = k * k;
            if (ghostTransform != null)
            {
                ghostTransform.position = Vector3.Lerp(from, TargetPoint(lungeStopDistance), k);
                FaceCameraYaw();
            }
            if (!startedFade && t >= fadeStartInLunge)
            {
                startedFade = true;
                StartCoroutine(Fade(1f, fadeToBlackTime));
            }
            yield return null;
        }

        yield return Fade(1f, fadeToBlackTime * 0.5f);
        onBlack?.Invoke();

        if (ghostObject != null) ghostObject.SetActive(false);

        yield return new WaitForSeconds(blackHold);

        if (teleportOnWake && wakeUpPoint != null && player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.SetPositionAndRotation(wakeUpPoint.position, wakeUpPoint.rotation);
            if (cc != null) cc.enabled = true;
        }
        if (flashlight != null) flashlight.ForceOff();
        if (fuseBoxToDisable != null) fuseBoxToDisable.ForceOff();

        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.ReleaseSilence(this);
            if (afterMusic != null) MusicPlayer.Instance.SetEventMusic(afterMusic);
        }

        SetFrozen(false);
        yield return Fade(0f, fadeInTime);
        onWakeComplete?.Invoke();

        if (HintUI.Instance != null && !string.IsNullOrEmpty(wakeThought))
        {
            HintUI.Instance.ShowTemporary(wakeThought, wakeThoughtDuration);
        }

        if (basementFinale != null) basementFinale.SetActive(true);

        isRunning = false;
    }

    private void SetFrozen(bool frozen)
    {
        if (componentsToFreeze == null) return;
        foreach (MonoBehaviour mb in componentsToFreeze)
            if (mb != null) mb.enabled = !frozen;
    }

    private void PlayState(string state)
    {
        if (ghostAnimator != null && !string.IsNullOrEmpty(state))
            ghostAnimator.CrossFadeInFixedTime(state, crossFade);
    }

    private Vector3 TargetPoint(float distance)
    {
        Transform c = playerCamera != null ? playerCamera.transform : player;
        Vector3 fwd = c.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 p = c.position + fwd * distance;
        p.y = c.position.y + ghostYOffsetFromCamera;
        return p;
    }

    private void FaceCameraYaw()
    {
        if (ghostTransform == null) return;
        Transform c = playerCamera != null ? playerCamera.transform : player;
        Vector3 look = c.position - ghostTransform.position; look.y = 0f;
        if (look.sqrMagnitude > 0.01f)
            ghostTransform.rotation = Quaternion.LookRotation(look);
    }

    private IEnumerator MoveGhost(float stopDistance, float duration)
    {
        if (ghostTransform == null) { yield return new WaitForSeconds(duration); yield break; }
        Vector3 from = ghostTransform.position;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
            ghostTransform.position = Vector3.Lerp(from, TargetPoint(stopDistance), k);
            FaceCameraYaw();
            yield return null;
        }
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
        if (targetAlpha < 0.5f) fadeOverlay.blocksRaycasts = false;
    }
}
