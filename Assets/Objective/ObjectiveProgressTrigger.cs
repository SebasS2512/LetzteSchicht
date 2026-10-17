using UnityEngine;

public class ObjectiveProgressTrigger : MonoBehaviour
{
    [Header("Was passiert")]
    [Tooltip("Die ScareEvents, die dann ablaufen. Bei denen sollte " +
             "'Trigger On Player Enter' AUS sein.")]
    [SerializeField] private ScareEvent[] events;

    [Header("Weg A: Aufgaben-Stand")]
    [Tooltip("Feuert, wenn der Spieler etwas in den Eimer geworfen hat.")]
    [SerializeField] private bool useObjectiveProgress = true;

    [SerializeField] private string objectiveId = "collect_trash";

    [Tooltip("Wie viele dürfen noch fehlen? 1 heißt: Es feuert, wenn " +
             "noch genau eines offen ist - bei 6 Stück also bei 5/6.")]
    [SerializeField] private int objectiveRemaining = 1;

    [Header("Weg B: Eingesammelter Müll")]
    [Tooltip("Feuert, wenn der Spieler ein Stück vom Boden aufgehoben hat. " +
             "Nötig für Spieler, die erst alles einsammeln und dann zum Eimer gehen.")]
    [SerializeField] private bool useTrashRemaining = true;

    [SerializeField] private string trashItemId = "trash";

    [Tooltip("Wie viele Stücke dürfen noch in der Welt liegen? " +
             "0 heißt: Es feuert, wenn der Spieler das letzte aufgehoben hat.")]
    [SerializeField] private int trashRemaining = 0;

    [Header("Wann")]
    [Tooltip("Nur in dieser Nacht. 0 = in jeder Nacht.")]
    [SerializeField] private int onlyInNight = 0;

    [SerializeField] private bool oncePerNight = true;

    private bool hasFiredThisNight;

    private void Awake()
    {
        ObjectiveManager.OnObjectiveProgress += HandleObjectiveProgress;
        TrashItem.OnRemainingChanged += HandleTrashRemaining;
        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        ObjectiveManager.OnObjectiveProgress -= HandleObjectiveProgress;
        TrashItem.OnRemainingChanged -= HandleTrashRemaining;
        NightManager.OnNightChanged -= HandleNightChanged;
    }

    private void HandleNightChanged(int night)
    {
        hasFiredThisNight = false;
    }

    private void HandleObjectiveProgress(string id, int current, int target)
    {
        if (!useObjectiveProgress) return;
        if (id != objectiveId) return;

        if ((target - current) == objectiveRemaining) Fire();
    }

    private void HandleTrashRemaining(string id, int remaining)
    {
        if (!useTrashRemaining) return;
        if (id != trashItemId) return;

        if (remaining == trashRemaining) Fire();
    }

    private void Fire()
    {
        if (oncePerNight && hasFiredThisNight) return;
        if (!IsActiveNight()) return;

        hasFiredThisNight = true;

        foreach (ScareEvent scareEvent in events)
        {
            if (scareEvent != null) scareEvent.Play();
        }
    }

    private bool IsActiveNight()
    {
        if (onlyInNight <= 0) return true;
        if (NightManager.Instance == null) return true;

        return NightManager.Instance.CurrentNight == onlyInNight;
    }
}