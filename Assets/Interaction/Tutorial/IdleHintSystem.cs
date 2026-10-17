using UnityEngine;

public class IdleHintSystem : MonoBehaviour
{
    [Header("Zeiten")]
    [Tooltip("Sekunden ohne Fortschritt, bis der erste Tipp kommt.")]
    [SerializeField] private float firstHintDelay = 270f;

    [Tooltip("Abstand zwischen weiteren Tipps, falls der Spieler " +
             "immer noch nicht weiterkommt.")]
    [SerializeField] private float repeatDelay = 120f;

    [SerializeField] private float hintDuration = 10f;

    [Header("Text")]
    [Tooltip("Wird vor den Aufgaben-Tipp gesetzt. Die Tipps müssen als " +
             "Satzteil dahinterpassen, z.B. 'den Strom einschalten'. " +
             "Leer lassen für keinen Vorspann.")]
    [SerializeField] private string prefix = "Vielleicht sollte ich ";

    [Tooltip("Fällt zurück, wenn die Aufgabe keinen eigenen Tipp hat.")]
    [SerializeField] private string fallbackHint = "Ein Blick aufs Aufgabenbrett könnte helfen.";

    private float idleTimer;
    private bool firstHintShown;

    private static IdleHintSystem instance;

    private void Awake()
    {
        instance = this;
        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;
    }

    private void HandleNightChanged(int night)
    {
        ResetTimer();
    }

    public static void NotifyActivity()
    {
        if (instance != null) instance.ResetTimer();
    }

    public static void ShowHintNow()
    {
        if (instance == null) return;

        instance.ShowHint();

        instance.idleTimer = 0f;
        instance.firstHintShown = true;
    }

    private void ResetTimer()
    {
        idleTimer = 0f;
        firstHintShown = false;
    }

    private void Update()
    {
        if (PauseMenu.IsPaused) return;
        if (UIController.Instance != null && UIController.Instance.IsNoteOpen()) return;

        idleTimer += Time.deltaTime;

        float threshold;
        if (firstHintShown)
        {
            threshold = repeatDelay;
        }
        else
        {
            threshold = firstHintDelay;
        }

        if (idleTimer < threshold) return;

        ShowHint();

        idleTimer = 0f;
        firstHintShown = true;
    }

    private void ShowHint()
    {
        if (HintUI.Instance == null) return;

        string hint = null;
        if (ObjectiveManager.Instance != null)
        {
            hint = ObjectiveManager.Instance.GetCurrentHint();
        }

        string text;
        if (string.IsNullOrEmpty(hint))
        {
            text = fallbackHint;
        }
        else
        {
            text = prefix + hint;
        }

        HintUI.Instance.ShowTemporary(text, hintDuration);
    }
}