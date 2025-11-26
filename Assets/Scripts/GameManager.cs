using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Give player some starter seeds
        var inv = FindFirstObjectByType<Inventory>();
        if (inv != null && inv.items.Count == 0)
        {
            var plantSeed = new Item { itemId = "plant_seed", displayName = "Plant Seed", count = 1000 };
            inv.AddItem(plantSeed);
        }
    }
}