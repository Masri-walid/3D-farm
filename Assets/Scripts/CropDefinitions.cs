using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Farm/Crop Definitions")]
public class CropDefinitions : ScriptableObject
{
    public List<CropInfo> crops;
}

[System.Serializable]
public class CropInfo
{
    public string seedId;
    public float growTime;
    public GameObject cropModel;
    public GameObject seedModel;
    public GameObject produceModel;
}