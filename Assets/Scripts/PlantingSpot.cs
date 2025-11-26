using UnityEngine;
using System.Collections;

public class PlantingSpot : MonoBehaviour
{
    Crop plantedCrop;

    void Start()
    {
        // add a simple collider for clicks
        var col = gameObject.AddComponent<BoxCollider>();
        col.size = new Vector3(1f, 0.2f, 1f);
        col.center = new Vector3(0f, 0.1f, 0f);

        // visual marker
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(cube.GetComponent<Collider>());
        cube.transform.SetParent(transform, false);
        cube.transform.localScale = new Vector3(1.2f, 0.05f, 1.2f);
        cube.transform.localPosition = new Vector3(0, 0.02f, 0);
        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.3f, 0.2f, 0.05f);
        cube.GetComponent<Renderer>().material = mat;
    }

    public bool HasCrop() => plantedCrop != null;

    public void Plant(string seedId)
    {
        if (plantedCrop != null) return;
        var go = new GameObject("Crop_");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0, 0.05f, 0);
        plantedCrop = go.AddComponent<Crop>();
        plantedCrop.Initialize(seedId, this);
    }

    public Item Harvest()
    {
        if (plantedCrop == null) return null;
        var item = plantedCrop.Harvest();
        Destroy(plantedCrop.gameObject);
        plantedCrop = null;
        return item;
    }

    // For save/load
    public PlantingSpotState GetState()
    {
        if (plantedCrop == null) return new PlantingSpotState { planted = false };
        return new PlantingSpotState { planted = true, seedId = plantedCrop.seedId, plantedAt = plantedCrop.plantedAt, timeToGrow = plantedCrop.timeToGrow };
    }

    public void LoadState(PlantingSpotState s)
    {
        if (s == null || !s.planted) return;
        Plant(s.seedId);
        plantedCrop.ForceSetTime(s.plantedAt, s.timeToGrow);
    }
}

[System.Serializable]
public class PlantingSpotState
{
    public bool planted;
    public string seedId;
    public double plantedAt;
    public float timeToGrow;
}