using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }
    public List<Item> items = new List<Item>();

    // Slot-based inventory (9 slots for 3x3 grid)
    public Item[] slots = new Item[9];

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        // Initialize slots
        for (int i = 0; i < 9; i++)
        {
            slots[i] = null;
        }
    }

    public void AddItem(Item item, int count = -1)
    {
        int amountToAdd = (count == -1) ? item.count : count;

        // First, try to stack with existing item in slots
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemId == item.itemId)
            {
                slots[i].count += amountToAdd;
                Debug.Log($"Added {item.displayName} x{amountToAdd} to slot {i}. Total: {slots[i].count}");
                UpdateItemsList();
                return;
            }
        }

        // Find first empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new Item { itemId = item.itemId, displayName = item.displayName, count = amountToAdd };
                Debug.Log($"Added {item.displayName} x{amountToAdd} to empty slot {i}");
                UpdateItemsList();
                return;
            }
        }

        Debug.LogWarning("Inventory full! Could not add item.");
    }

    public void RemoveItem(string itemId, int count = 1)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemId == itemId)
            {
                slots[i].count -= count;
                if (slots[i].count <= 0)
                {
                    slots[i] = null;
                }
                UpdateItemsList();
                return;
            }
        }
    }

    public void SwapSlots(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= 9 || toIndex < 0 || toIndex >= 9) return;

        var temp = slots[fromIndex];
        slots[fromIndex] = slots[toIndex];
        slots[toIndex] = temp;

        UpdateItemsList();
    }

    public List<Item> GetSlotItems()
    {
        return slots.ToList();
    }

    void UpdateItemsList()
    {
        // Keep items list in sync with slots for save/load compatibility
        items.Clear();
        foreach (var slot in slots)
        {
            if (slot != null && slot.count > 0)
            {
                items.Add(slot);
            }
        }
    }

    public InventoryItem GetFirstSeed()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemId.EndsWith("_seed") && slots[i].count > 0)
            {
                return new InventoryItem { itemId = slots[i].itemId, displayName = slots[i].displayName };
            }
        }
        return null;
    }

    public static class InventoryUI
    {
        public static bool visible = false;
        public static void Toggle()
        {
            visible = !visible;
            Debug.Log("Inventory toggled: " + visible);
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