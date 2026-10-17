using System;
using UnityEngine;

public class NightManager : MonoBehaviour
{
    public static NightManager Instance;

    [Header("Einstellungen")]
    [Tooltip("Mit dieser Nacht startet das Spiel. Zum Testen hochstellen.")]
    [SerializeField] private int startNight = 1;

    [Tooltip("Nach dieser Nacht ist das Spiel vorbei.")]
    [SerializeField] private int lastNight = 3;

    public int CurrentNight { get; private set; }

    public int LastNight => lastNight;

    public bool IsLastNight => CurrentNight >= lastNight;

    public static event Action<int> OnNightChanged;

    public static event Action OnAllNightsCompleted;

    private void Awake()
    {
        Instance = this;
        CurrentNight = startNight;
    }

    private void Start()
    {
        if (OnNightChanged != null)
        {
            OnNightChanged.Invoke(CurrentNight);
        }
    }

    public void AdvanceToNextNight()
    {
        if (IsLastNight)
        {
            if (OnAllNightsCompleted != null)
            {
                OnAllNightsCompleted.Invoke();
            }
            return;
        }

        CurrentNight++;

        if (OnNightChanged != null)
        {
            OnNightChanged.Invoke(CurrentNight);
        }
    }
}