using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    string saveFile => Path.Combine(Application.persistentDataPath, "farm_save.json");

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    [System.Serializable]
    class SaveData
    {
        public List<PlantingSpotState> spots = new List<PlantingSpotState>();
        public List<Item> items = new List<Item>();
    }

    public void Save()
    {
        var sd = new SaveData();
        var spots = FindObjectsByType<PlantingSpot>(FindObjectsSortMode.None);
        foreach (var s in spots) sd.spots.Add(s.GetState());
        var inv = FindFirstObjectByType<Inventory>();
        if (inv != null) sd.items = inv.items;

        var json = JsonUtility.ToJson(sd, true);
        File.WriteAllText(saveFile, json);
        Debug.Log("Saved to: " + saveFile);
    }

    public void Load()
    {
        if (!File.Exists(saveFile)) { Debug.Log("No save file found."); return; }
        var json = File.ReadAllText(saveFile);
        var sd = JsonUtility.FromJson<SaveData>(json);

        var spots = FindObjectsByType<PlantingSpot>(FindObjectsSortMode.None);
        for (int i = 0; i < spots.Length; i++)
        {
            if (i < sd.spots.Count) spots[i].LoadState(sd.spots[i]);
        }

        var inv = FindFirstObjectByType<Inventory>();
        if (inv != null) { inv.items = sd.items; }

        Debug.Log("Loaded save.");
    }
}