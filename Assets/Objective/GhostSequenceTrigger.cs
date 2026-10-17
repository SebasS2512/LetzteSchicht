using UnityEngine;
using UnityEngine.Events;

public class GhostSequenceTrigger : MonoBehaviour
{
    [SerializeField] private string trashObjectiveId = "collect_trash";

    [Tooltip("Nur in dieser Nacht auslösen. 0 = in jeder Nacht.")]
    [SerializeField] private int onlyInNight = 3;

    [Tooltip("Wird ausgelöst, wenn alles AUSSER dem Müll erledigt ist. " +
             "Hier GhostScareSequence.Play() einhängen.")]
    [SerializeField] private UnityEvent onAllButTrashDone;

    private bool fired;

    private void Awake()
    {
        ObjectiveManager.OnObjectiveProgress += HandleProgress;
        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        ObjectiveManager.OnObjectiveProgress -= HandleProgress;
        NightManager.OnNightChanged -= HandleNightChanged;
    }

    private void HandleNightChanged(int night) => fired = false;

    private void HandleProgress(string id, int current, int target)
    {
        if (fired) return;

        if (onlyInNight > 0 && NightManager.Instance != null
            && NightManager.Instance.CurrentNight != onlyInNight) return;

        if (ObjectiveManager.Instance == null) return;

        if (ObjectiveManager.Instance.AllObjectivesDoneExcept(trashObjectiveId))
        {
            fired = true;
            onAllButTrashDone.Invoke();
        }
    }
}
