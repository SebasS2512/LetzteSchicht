using UnityEngine;

public class TutorialHintZone : MonoBehaviour
{
    public enum HintMode
    {
        Sticky,
        WhileInside,
        Timed
    }

    [Header("Hinweis")]
    [TextArea(2, 4)]
    [SerializeField] private string hintText = "[E] Benutzen";

    [Tooltip("Sticky: bleibt bis zum Nachtwechsel stehen (Standard für Steuerung). " +
             "WhileInside: nur in der Zone. " +
             "Timed: verschwindet nach kurzer Zeit (gut für den Geh-Hinweis).")]
    [SerializeField] private HintMode mode = HintMode.Sticky;

    [Tooltip("Nur bei 'Timed': Wie lange der Hinweis stehen bleibt.")]
    [SerializeField] private float timedDuration = 10f;

    [Header("Wann")]
    [Tooltip("Bis zu welcher Nacht der Hinweis erscheint. 1 = nur in Nacht 1.")]
    [SerializeField] private int showUntilNight = 1;

    private bool isInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!IsActiveNight()) return;
        if (HintUI.Instance == null) return;

        isInside = true;

        switch (mode)
        {
            case HintMode.Sticky:
                HintUI.Instance.AddSticky(hintText);
                break;
            case HintMode.WhileInside:
                HintUI.Instance.ShowNearby(hintText);
                break;
            case HintMode.Timed:
                HintUI.Instance.ShowTemporary(hintText, timedDuration);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isInside) return;

        isInside = false;

        if (mode == HintMode.WhileInside && HintUI.Instance != null)
        {
            HintUI.Instance.HideNearby(hintText);
        }
    }

    private bool IsActiveNight()
    {
        if (NightManager.Instance == null) return true;
        return NightManager.Instance.CurrentNight <= showUntilNight;
    }
}