using UnityEngine;

public class ObjectiveZone : MonoBehaviour
{
    [Header("Aufgabe")]
    [Tooltip("Braucht einen Collider mit 'Is Trigger' auf diesem Objekt.")]
    [SerializeField] private string objectiveId = "visit_room";

    private bool hasReported = false;

    private void Awake()
    {
        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;
    }

    private void HandleNightChanged(int night)
    {
        hasReported = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasReported) return;
        if (!other.CompareTag("Player")) return;

        hasReported = true;
        ObjectiveManager.Instance.ReportProgress(objectiveId);
    }
}