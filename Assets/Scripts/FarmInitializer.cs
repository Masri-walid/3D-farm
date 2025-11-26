// === FarmInitializer.cs ===
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class FarmInitializer : MonoBehaviour
{
    public int gridWidth = 6;
    public int gridHeight = 4;
    public float spacing = 2f;

    void Awake()
    {
        Debug.Log("FarmInitializer: Starting initialization...");

        // Disable the Main Camera in the scene (we'll use PlayerCamera instead)
        var mainCam = GameObject.Find("Main Camera");
        if (mainCam != null)
        {
            Debug.Log("FarmInitializer: Disabling Main Camera");
            mainCam.SetActive(false);
        }

        // ensure a GameManager exists
        if (FindFirstObjectByType<GameManager>() == null)
        {
            Debug.Log("FarmInitializer: Creating GameManager");
            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();
        }

        // Create a ground plane
        if (GameObject.Find("Ground") == null)
        {
            Debug.Log("FarmInitializer: Creating Ground");
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(4, 1, 3);
            ground.transform.position = Vector3.zero;

            // Create grass material
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.6f, 0.2f); // Darker, more realistic grass green

            // Add grass texture tiling for visual detail
            mat.mainTextureScale = new Vector2(10, 10); // Tile the texture

            // Optional: Add some roughness for more realistic grass
            mat.SetFloat("_Glossiness", 0.1f); // Less shiny

            ground.GetComponent<Renderer>().material = mat;
        }

        // Create grass patches
        CreateGrass();

        // Create directional light if none exists
        if (FindFirstObjectByType<Light>() == null)
        {
            Debug.Log("FarmInitializer: Creating Directional Light");
            var lightGo = new GameObject("Directional Light");
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // Create Player if none
        if (FindFirstObjectByType<PlayerController>() == null)
        {
            Debug.Log("FarmInitializer: Creating Player");
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0, 1.2f, -6f);

            // Add CharacterController BEFORE PlayerController (required component)
            var charController = player.AddComponent<CharacterController>();
            charController.height = 1.8f;
            charController.center = new Vector3(0, 0.9f, 0);

            // Now add PlayerController
            var pc = player.AddComponent<PlayerController>();

            // Add a camera
            var cam = new GameObject("PlayerCamera");
            cam.transform.SetParent(player.transform);
            cam.transform.localPosition = new Vector3(0, 0.9f, 0);
            var camera = cam.AddComponent<Camera>();
            camera.tag = "MainCamera";
            cam.AddComponent<AudioListener>();

            Debug.Log($"FarmInitializer: Player created at {player.transform.position}");
            Debug.Log($"FarmInitializer: Camera created at world position {cam.transform.position}");
        }

        // Create planting grid
        Debug.Log("FarmInitializer: Creating planting grid");
        var container = new GameObject("PlantingSpots");
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                var spot = new GameObject($"Spot_{x}_{z}");
                spot.transform.SetParent(container.transform);
                spot.transform.position = new Vector3((x - gridWidth/2f) * spacing, 0.01f, (z - gridHeight/2f) * spacing + 2f);
                var ps = spot.AddComponent<PlantingSpot>();
            }
        }
        Debug.Log($"FarmInitializer: Created {gridWidth * gridHeight} planting spots");

        // Ensure SaveSystem exists
        if (FindFirstObjectByType<SaveSystem>() == null)
        {
            var ss = new GameObject("SaveSystem");
            ss.AddComponent<SaveSystem>();
        }

        // Inventory UI placeholder (optional)
        if (FindFirstObjectByType<Inventory>() == null)
        {
            var inv = new GameObject("Inventory");
            inv.AddComponent<Inventory>();
        }

        Debug.Log("FarmInitializer: Initialization complete!");
    }

    void CreateGrass()
    {
        var grassContainer = new GameObject("Grass");
        int grassCount = 800;

        // Load the small-plant prefab to extract leaf1
        GameObject smallPlantPrefab = Resources.Load<GameObject>("prefabs/small-plant");

        // Define colors for grass variety
        Color[] grassColors = new Color[]
        {
            new Color(0.2f, 0.5f, 0.1f),   // Dark green
            new Color(0.3f, 0.6f, 0.2f),   // Medium green
            new Color(0.4f, 0.7f, 0.2f),   // Light green
            new Color(0.35f, 0.55f, 0.15f) // Yellow-green
        };

        int created = 0;
        for (int i = 0; i < grassCount; i++)
        {
            // Random position on the ground
            float x = Random.Range(-18f, 18f);
            float z = Random.Range(-12f, 12f);

            // Skip areas where planting spots are
            bool inPlantingArea = (x > -7f && x < 7f && z > -3f && z < 7f);
            if (inPlantingArea) continue;

            GameObject grass = null;

            // Try to use leaf1 from small-plant prefab
            if (smallPlantPrefab != null)
            {
                // Find leaf1 in the prefab
                Transform leaf1Transform = smallPlantPrefab.transform.Find("leaf1");
                if (leaf1Transform == null)
                {
                    // Try to find it recursively
                    foreach (Transform child in smallPlantPrefab.GetComponentsInChildren<Transform>())
                    {
                        if (child.name == "leaf1")
                        {
                            leaf1Transform = child;
                            break;
                        }
                    }
                }

                if (leaf1Transform != null)
                {
                    grass = Instantiate(leaf1Transform.gameObject);
                }
            }

            // Fallback to cube if leaf not found
            if (grass == null)
            {
                grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                float height = Random.Range(0.1f, 0.4f);
                float width = Random.Range(0.05f, 0.15f);
                grass.transform.localScale = new Vector3(width, height, width);
            }

            grass.name = $"Grass_{i}";
            grass.transform.SetParent(grassContainer.transform);
            grass.transform.position = new Vector3(x, 0f, z);

            // Random rotation and fixed 0.2 scale
            grass.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            grass.transform.localScale = Vector3.one * 0.2f;

            // Remove colliders so player can walk through
            foreach (var col in grass.GetComponentsInChildren<Collider>())
            {
                Destroy(col);
            }

            // Apply random grass color to all renderers
            Color grassColor = grassColors[Random.Range(0, grassColors.Length)];
            foreach (var renderer in grass.GetComponentsInChildren<Renderer>())
            {
                Material mat;
                if (renderer.sharedMaterial != null)
                {
                    mat = new Material(renderer.sharedMaterial);
                }
                else
                {
                    mat = new Material(Shader.Find("Standard"));
                }
                mat.color = grassColor;
                mat.SetFloat("_Glossiness", 0.1f);
                renderer.material = mat;
            }

            created++;
        }

        Debug.Log($"FarmInitializer: Created {created} grass patches using leaf1");
    }
}
