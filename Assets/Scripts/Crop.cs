using UnityEngine;
using System.Collections;

public class Crop : MonoBehaviour
{
    public string seedId;
    public float timeToGrow = 10f;
    public double plantedAt;

    PlantingSpot owner;
    GameObject visual;
    bool isWatered = false; // Track if plant has been watered (small -> big)
    Color plantColor;

    // Plant color palette
    static readonly Color[] plantColors = new Color[]
    {
        new Color(0.2f, 0.7f, 0.3f),   // Green
        new Color(0.8f, 0.2f, 0.3f),   // Red
        new Color(0.9f, 0.6f, 0.1f),   // Orange
        new Color(0.6f, 0.2f, 0.7f),   // Purple
        new Color(0.9f, 0.8f, 0.2f),   // Yellow
        new Color(0.3f, 0.5f, 0.9f),   // Blue
        new Color(0.9f, 0.4f, 0.6f),   // Pink
        new Color(0.1f, 0.8f, 0.7f),   // Cyan/Teal
    };

    public void Initialize(string seedId, PlantingSpot owner)
    {
        this.seedId = seedId;
        this.owner = owner;
        plantedAt = System.DateTime.UtcNow.Subtract(new System.DateTime(1970,1,1)).TotalSeconds;

        // Assign a random color to this plant
        plantColor = plantColors[Random.Range(0, plantColors.Length)];

        // Start with small-plant stage (no automatic growth)
        LoadPlantStage("small-plant");
    }

    void LoadPlantStage(string stageName)
    {
        // Destroy old visual if it exists
        if (visual != null)
        {
            Destroy(visual);
        }

        // Try to load the stage prefab from prefabs folder
        GameObject prefab = Resources.Load<GameObject>($"prefabs/{stageName}");

        if (prefab != null)
        {
            visual = Instantiate(prefab, transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            foreach (var col in visual.GetComponentsInChildren<Collider>())
            {
                Destroy(col);
            }

            ApplyColorToVisual();
        }
        else
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(visual.GetComponent<Collider>());
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = new Vector3(0.2f, isWatered ? 0.4f : 0.15f, 0.2f);
            visual.transform.localPosition = new Vector3(0, isWatered ? 0.2f : 0.075f, 0);
            var r = visual.GetComponent<Renderer>();
            if (r != null) r.material.color = plantColor;
        }
    }

    public void ForceSetTime(double plantedAt, float timeToGrow)
    {
        this.plantedAt = plantedAt;
        this.timeToGrow = timeToGrow;
    }

    public void ForceSetWatered(bool watered)
    {
        isWatered = watered;
        LoadPlantStage(isWatered ? "big-plant" : "small-plant");
    }

    // Called when watering is complete
    public void Water()
    {
        if (isWatered) return; // Already watered

        isWatered = true;
        LoadPlantStage("big-plant");
        Debug.Log("Plant has been watered and grown to big-plant!");
    }

    void ApplyColorToVisual()
    {
        if (visual == null) return;

        foreach (var renderer in visual.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                mat.color = plantColor;
            }
        }
    }

    public bool IsSmallPlant()
    {
        return !isWatered;
    }

    public bool IsBigPlant()
    {
        return isWatered;
    }

    public bool IsFullyGrown()
    {
        return isWatered;
    }

    // Harvest small plant - returns powder
    public Item[] HarvestSmall()
    {
        if (isWatered)
        {
            Debug.Log("This is a big plant, use HarvestBig instead!");
            return null;
        }

        var powder = new Item { itemId = "powder", displayName = "Powder", count = 1 };
        return new Item[] { powder };
    }

    // Harvest big plant - returns extraction and seeds
    public Item[] HarvestBig()
    {
        if (!isWatered)
        {
            Debug.Log("This is a small plant, use HarvestSmall instead!");
            return null;
        }

        var extraction = new Item { itemId = "extraction", displayName = "Extraction", count = 1 };
        var seeds = new Item { itemId = "plant_seed", displayName = "Plant Seed", count = 2 };
        return new Item[] { extraction, seeds };
    }

    // Legacy method for compatibility
    public Item[] Harvest()
    {
        if (isWatered)
            return HarvestBig();
        else
            return HarvestSmall();
    }
}