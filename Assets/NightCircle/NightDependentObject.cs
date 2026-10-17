using UnityEngine;

public class NightDependentObject : MonoBehaviour
{
    [SerializeField] private int minNight = 1;

    [Tooltip("Bis zu welcher Nacht. Min 1 / Max 1 = nur in Nacht 1.")]
    [SerializeField] private int maxNight = 3;

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
        bool shouldBeActive = night >= minNight && night <= maxNight;
        gameObject.SetActive(shouldBeActive);
    }
}