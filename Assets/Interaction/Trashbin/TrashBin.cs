using UnityEngine;

public class TrashBin : MonoBehaviour, IInteractable
{
    [SerializeField] private string displayName = "Mülleimer";

    [SerializeField] private string itemId = "trash";

    [Header("Modelle")]
    [Tooltip("Beide Modelle als Kinder unter dieses Objekt hängen.")]
    [SerializeField] private GameObject emptyModel;
    [SerializeField] private GameObject fullModel;

    [Tooltip("Ab wie vielen Stücken der Eimer als voll gilt und das " +
             "Modell wechselt.")]
    [SerializeField] private int fullThreshold = 1;

    [Header("Aufgabe")]
    [Tooltip("Wird pro eingeworfenem STÜCK einmal gemeldet. Setz den " +
             "targetCount der Aufgabe auf die Anzahl Müllstücke im Level.")]
    [SerializeField] private string objectiveId = "collect_trash";

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip throwClip;

    private int contentCount;

    public string GetDisplayName() => displayName;
    public InteractionType GetInteractionType() => InteractionType.Discard;

    private void Awake()
    {
        NightManager.OnNightChanged += HandleNightChanged;
    }

    private void OnDestroy()
    {
        NightManager.OnNightChanged -= HandleNightChanged;
    }

    private void Start()
    {
        UpdateModels();
    }

    private void HandleNightChanged(int night)
    {
        contentCount = 0;
        UpdateModels();
    }

    public void Interact()
    {
        if (PlayerInventory.Instance == null) return;

        int carried = PlayerInventory.Instance.GetItemCount(itemId);

        if (carried <= 0) return;

        int thrown = PlayerInventory.Instance.RemoveAll(itemId);
        contentCount += thrown;

        SoundUtils.Play(audioSource, throwClip);
        UpdateModels();

        if (ObjectiveManager.Instance != null)
        {
            for (int i = 0; i < thrown; i++)
            {
                ObjectiveManager.Instance.ReportProgress(objectiveId);
            }
        }
    }

    private void UpdateModels()
    {
        bool isFull = contentCount >= fullThreshold;

        if (emptyModel != null) emptyModel.SetActive(!isFull);
        if (fullModel != null) fullModel.SetActive(isFull);
    }
}