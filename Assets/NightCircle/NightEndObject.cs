using System.Collections;
using UnityEngine;

public class NightEndObject : MonoBehaviour, IInteractable
{
    [SerializeField] private string displayName = "Tür";

    [SerializeField] private float blackScreenHold = 2f;

    [Header("Sound - Übergang")]
    [Tooltip("2D-Quelle (Spatial Blend 0). Der Übergangssound soll überall " +
             "gleich laut sein, nicht aus einer Richtung kommen.")]
    [SerializeField] private AudioSource transitionSource;

    [Tooltip("Startet zusammen mit dem Abblenden. Ein tiefer, langer Ton " +
             "wirkt hier besser als ein kurzer Effekt.")]
    [SerializeField] private AudioClip nightEndClip;

    [Tooltip("Kommt im Schwarzbild, kurz vor dem Aufblenden. Markiert den " +
             "Beginn der neuen Nacht - z.B. eine ferne Glocke oder ein Atemzug.")]
    [SerializeField] private AudioClip nightStartClip;

    [Tooltip("Sekunden zwischen Schwarzbild-Beginn und dem Start-Sound. " +
             "Sollte kleiner als 'Black Screen Hold' sein.")]
    [SerializeField] private float nightStartDelay = 0.8f;

    [Header("Sound - Letzte Nacht")]
    [Tooltip("Ersetzt den Start-Sound, wenn dies die letzte Nacht war.")]
    [SerializeField] private AudioClip gameCompleteClip;

    [Header("Sound - Aufgaben offen")]
    [Tooltip("Darf 3D sein, sitzt am Objekt.")]
    [SerializeField] private AudioSource feedbackSource;

    [Tooltip("Türklinke rüttelt oder Seufzer.")]
    [SerializeField] private AudioClip blockedClip;

    [Header("Abschluss-Effekt")]
    [Tooltip("Läuft NACH dem Aufblenden der neuen Nacht - für Momente, " +
             "die der Spieler noch mitbekommen soll.")]
    [SerializeField] private ScareEvent afterTransitionEvent;

    [Tooltip("Läuft VOR dem Abblenden, während der Spieler noch sieht.")]
    [SerializeField] private ScareEvent beforeTransitionEvent;

    [Tooltip("Wartezeit nach dem Vor-Effekt, bevor abgeblendet wird.")]
    [SerializeField] private float beforeEventDelay = 1.5f;

    private bool transitionRunning = false;

    public string GetDisplayName() => displayName;
    public InteractionType GetInteractionType() => InteractionType.Sleep;

    public void Interact()
    {
        if (transitionRunning) return;

        if (!ObjectiveManager.Instance.AllObjectivesDone())
        {
            SoundUtils.Play(feedbackSource, blockedClip);

            IdleHintSystem.ShowHintNow();
            return;
        }

        StartCoroutine(NightTransition());
    }

    private IEnumerator NightTransition()
    {
        transitionRunning = true;

        bool wasLastNight = NightManager.Instance != null
                            && NightManager.Instance.IsLastNight;

        if (beforeTransitionEvent != null)
        {
            beforeTransitionEvent.Play();
            yield return new WaitForSeconds(beforeEventDelay);
        }

        SoundUtils.Play(transitionSource, nightEndClip, 0f);

        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.RequestSilence(this);
        }

        yield return ScreenFader.Instance.FadeOut();

        yield return new WaitForSecondsRealtime(nightStartDelay);

        AudioClip wakeClip;
        if (wasLastNight && gameCompleteClip != null)
        {
            wakeClip = gameCompleteClip;
        }
        else
        {
            wakeClip = nightStartClip;
        }

        SoundUtils.Play(transitionSource, wakeClip, 0f);

        float remaining = Mathf.Max(0f, blackScreenHold - nightStartDelay);
        yield return new WaitForSecondsRealtime(remaining);

        NightManager.Instance.AdvanceToNextNight();

        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.ClearAll();
        }

        yield return ScreenFader.Instance.FadeIn();

        if (afterTransitionEvent != null)
        {
            afterTransitionEvent.Play();
        }

        transitionRunning = false;
    }
}