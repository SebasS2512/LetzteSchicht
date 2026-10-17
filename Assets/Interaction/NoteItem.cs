using UnityEngine;

public class NoteItem : MonoBehaviour, IInteractable
{
    [SerializeField] private string displayName = "Notiz";

    [Header("Text im Lese-Fenster")]
    [TextArea(5, 12)]
    [SerializeField] private string noteContent = "Hier steht die gruselige Nachricht...";

    [Header("Aufgabe (optional)")]
    [Tooltip("LEER lassen bei Notizen, die nicht mitzählen sollen - " +
             "so bestimmst du, welche für die Aufgabe wichtig sind.")]
    [SerializeField] private string objectiveId = "";

    private bool hasReported;

    public string GetDisplayName() => displayName;
    public InteractionType GetInteractionType() => InteractionType.Examine;

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

    public void Interact()
    {
        if (UIController.Instance == null) return;

        UIController.Instance.ShowNote(noteContent);

        ReportToObjective();
    }

    private void ReportToObjective()
    {
        if (hasReported) return;
        if (string.IsNullOrEmpty(objectiveId)) return;
        if (ObjectiveManager.Instance == null) return;

        hasReported = true;
        ObjectiveManager.Instance.ReportProgress(objectiveId);
    }
}