using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    public List<Item> items = new List<Item>();

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(Item item, int count = -1)
    {
        // If count is not specified, use the item's existing count
        int amountToAdd = (count == -1) ? item.count : count;

        var existing = items.FirstOrDefault(i => i.itemId == item.itemId);
        if (existing != null)
        {
            existing.count += amountToAdd;
        }
        else
        {
            item.count = amountToAdd;
            items.Add(item);
        }
        Debug.Log($"Added {item.displayName} x{amountToAdd}. Total: {items.FirstOrDefault(i => i.itemId == item.itemId)?.count}");
    }

    public void RemoveItem(string itemId, int count = 1)
    {
        var existing = items.FirstOrDefault(i => i.itemId == itemId);
        if (existing == null) return;
        existing.count -= count;
        if (existing.count <= 0) items.Remove(existing);
    }

    public InventoryItem GetFirstSeed()
    {
        // seeds are items whose id ends with "_seed"
        var seed = items.FirstOrDefault(i => i.itemId.EndsWith("_seed"));
        if (seed == null) return null;
        return new InventoryItem { itemId = seed.itemId, displayName = seed.displayName };
    }

    public static class InventoryUI
    {
        // Minimal placeholder - you can expand with Unity UI if you want
        public static bool visible = false;
        public static void Toggle()
        {
            visible = !visible;
            Debug.Log("Inventory toggled: " + visible);
            // Implement UI later
        }
    }
}

[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public string displayName;
}

[System.Serializable]
public class Item
{
    public string itemId;
    public string displayName;
    public int count = 1;
}