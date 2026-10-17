using System;
using System.Collections.Generic;
using UnityEngine;

public class TrashItem : MonoBehaviour, IInteractable
{
    public static event Action<string, int> OnRemainingChanged;

    private static readonly Dictionary<string, int> remaining = new Dictionary<string, int>();

    [SerializeField] private string displayName = "Müll";

    [Tooltip("Bei mehreren Sorten (Papier, Flaschen) hier " +
             "unterschiedliche Werte eintragen.")]
    [SerializeField] private string itemId = "trash";

    [Header("Tragelimit")]
    [Tooltip("Wie viele Stücke der Spieler gleichzeitig tragen kann. " +
             "0 = unbegrenzt.")]
    [SerializeField] private int carryLimit = 0;

    [Header("Sound")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private float pickupVolume = 1f;

    private bool isCounted;

    public string GetDisplayName() => displayName;
    public InteractionType GetInteractionType() => InteractionType.Take;

    private void OnEnable()
    {
        if (isCounted) return;

        Add(itemId, 1);
        isCounted = true;
    }

    private void OnDisable()
    {
        if (!isCounted) return;

        Add(itemId, -1);
        isCounted = false;
    }

    public void Interact()
    {
        if (PlayerInventory.Instance == null) return;

        if (carryLimit > 0 && PlayerInventory.Instance.GetItemCount(itemId) >= carryLimit) return;

        PlayerInventory.Instance.AddItem(itemId);

        if (isCounted)
        {
            Add(itemId, -1);
            isCounted = false;
        }

        OnRemainingChanged?.Invoke(itemId, GetRemaining(itemId));

        SoundUtils.PlayAtPosition(pickupClip, transform.position, pickupVolume);

        Destroy(gameObject);
    }

public static int GetRemaining(string itemId)
{
    if (remaining.ContainsKey(itemId))
    {
        return remaining[itemId];
    }

    return 0;
}

    private static void Add(string itemId, int delta)
    {
        if (remaining.ContainsKey(itemId)) remaining[itemId] += delta;
        else remaining[itemId] = delta;

        if (remaining[itemId] < 0) remaining[itemId] = 0;
    }
}