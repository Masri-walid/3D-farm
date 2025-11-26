using UnityEngine;
using System.Collections;

public class Crop : MonoBehaviour
{
    public string seedId;
    public float timeToGrow = 10f; // Total growth time: 3s for bush, 7s for plant
    public double plantedAt;

    PlantingSpot owner;
    float progress = 0f;
    GameObject visual;
    bool hasTransitioned = false; // Track if we've switched from bush to plant
    Color plantColor; // Unique color for this plant

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

        StartCoroutine(GrowRoutine());

        // Start with small-plant stage
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
            // Instantiate the prefab model
            visual = Instantiate(prefab, transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            // Remove any colliders from the visual
            foreach (var col in visual.GetComponentsInChildren<Collider>())
            {
                Destroy(col);
            }

            // Apply the plant's unique color to all renderers
            ApplyColorToVisual();
        }
        else
        {
            // Fallback to primitive if no prefab found
            visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(visual.GetComponent<Collider>());
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = new Vector3(0.2f, 0.05f, 0.2f);
            visual.transform.localPosition = new Vector3(0, 0.05f, 0);
        }
    }

    public void ForceSetTime(double plantedAt, float timeToGrow)
    {
        this.plantedAt = plantedAt;
        this.timeToGrow = timeToGrow;

        // Determine which stage we should be in
        double now = System.DateTime.UtcNow.Subtract(new System.DateTime(1970,1,1)).TotalSeconds;
        double elapsed = now - plantedAt;

        if (elapsed >= 3f)
        {
            LoadPlantStage("big-plant");
            hasTransitioned = true;
        }
        else
        {
            LoadPlantStage("small-plant");
            hasTransitioned = false;
        }

        StopAllCoroutines();
        StartCoroutine(GrowRoutine());
    }

    IEnumerator GrowRoutine()
    {
        while (true)
        {
            double now = System.DateTime.UtcNow.Subtract(new System.DateTime(1970,1,1)).TotalSeconds;
            double elapsed = now - plantedAt;
            progress = Mathf.Clamp01((float)(elapsed / timeToGrow));

            // Transition from small-plant to big-plant after 3 seconds
            if (!hasTransitioned && elapsed >= 3f)
            {
                LoadPlantStage("big-plant");
                hasTransitioned = true;
            }

            UpdateVisual(progress);
            if (progress >= 1f) break;
            yield return new WaitForSeconds(0.1f); // Check more frequently for smooth transition
        }
    }

    void UpdateVisual(float p)
    {
        // Only update visual for primitive fallback (when no model is loaded)
        if (visual != null && visual.GetComponent<MeshFilter>() != null)
        {
            // Check if it's a primitive (has a mesh filter from CreatePrimitive)
            if (visual.name.Contains("Cylinder"))
            {
                visual.transform.localScale = new Vector3(0.2f, 0.05f + p * 0.8f, 0.2f);
                visual.transform.localPosition = new Vector3(0, 0.05f + p * 0.4f, 0);
                var r = visual.GetComponent<Renderer>();
                if (r != null)
                {
                    r.material.color = Color.Lerp(Color.yellow, plantColor, p);
                }
            }
        }
    }

    void ApplyColorToVisual()
    {
        if (visual == null) return;

        // Apply color to all renderers in the visual
        foreach (var renderer in visual.GetComponentsInChildren<Renderer>())
        {
            // Create a new material instance to avoid affecting other objects
            foreach (var mat in renderer.materials)
            {
                // Tint the material with plant color
                mat.color = plantColor;
            }
        }
    }

    public Item Harvest()
    {
        if (progress < 1f) return null; // not ready

        // Determine produce name based on seed type
        string produceName = "Plant Produce";
        if (seedId.Contains("seed"))
        {
            produceName = seedId.Replace("_seed", "").Replace("_", " ");
            // Capitalize first letter
            if (produceName.Length > 0)
            {
                produceName = char.ToUpper(produceName[0]) + produceName.Substring(1);
            }
        }

        var item = new Item { itemId = seedId + "_produce", displayName = produceName };
        return item;
    }
}