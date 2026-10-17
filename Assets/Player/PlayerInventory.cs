using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private readonly List<string> keys = new List<string>();

    private readonly Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddKey(string keyId)
    {
        if (!keys.Contains(keyId)) keys.Add(keyId);
    }

    public bool HasKey(string keyId)
    {
        return keys.Contains(keyId);
    }

    public void AddItem(string itemId, int amount = 1)
    {
        if (itemCounts.ContainsKey(itemId)) itemCounts[itemId] += amount;
        else itemCounts[itemId] = amount;
    }

    public int GetItemCount(string itemId)
    {
        return itemCounts.TryGetValue(itemId, out int count) ? count : 0;
    }

    public int RemoveAll(string itemId)
    {
        int count = GetItemCount(itemId);
        itemCounts[itemId] = 0;
        return count;
    }
}