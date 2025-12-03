using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 2f;
    public float interactDistance = 3f;
    public float holdDuration = 1f; // Time to hold E for interaction
    public float waterDuration = 2f; // Time to hold R for watering

    CharacterController controller;
    float verticalVelocity = 0f;
    Transform cam;
    float pitch = 0f;

    // Hold E interaction
    float holdTimer = 0f;
    bool isHolding = false;
    PlantingSpot currentSpot = null;
    AudioSource audioSource;
    AudioClip cropsAudio;
    AudioClip waterAudio;

    // Watering interaction (R key)
    float waterTimer = 0f;
    bool isWatering = false;
    PlantingSpot wateringSpot = null;

    // Progress bar UI
    Canvas progressCanvas;
    Image progressBarBackground;
    Image progressBarFill;
    Text progressLabel;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = transform.Find("PlayerCamera");
        Cursor.lockState = CursorLockMode.Locked;

        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        cropsAudio = Resources.Load<AudioClip>("Crops/crops audio");
        waterAudio = Resources.Load<AudioClip>("Crops/water");

        // Create progress bar UI
        CreateProgressBar();
    }

    void CreateProgressBar()
    {
        var canvasObj = new GameObject("ProgressBarCanvas");
        progressCanvas = canvasObj.AddComponent<Canvas>();
        progressCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        progressCanvas.sortingOrder = 200;
        canvasObj.AddComponent<CanvasScaler>();

        var bgObj = new GameObject("ProgressBarBg");
        bgObj.transform.SetParent(progressCanvas.transform, false);
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = new Vector2(0, -50);
        bgRect.sizeDelta = new Vector2(204, 24);
        progressBarBackground = bgObj.AddComponent<Image>();
        progressBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        var fillObj = new GameObject("ProgressBarFill");
        fillObj.transform.SetParent(bgObj.transform, false);
        var fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0.5f);
        fillRect.anchorMax = new Vector2(0, 0.5f);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchoredPosition = new Vector2(2, 0);
        fillRect.sizeDelta = new Vector2(0, 20);
        progressBarFill = fillObj.AddComponent<Image>();
        progressBarFill.color = Color.white;

        // Label for action type
        var labelObj = new GameObject("ProgressLabel");
        labelObj.transform.SetParent(progressCanvas.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0, -80);
        labelRect.sizeDelta = new Vector2(200, 30);
        progressLabel = labelObj.AddComponent<Text>();
        progressLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        progressLabel.fontSize = 18;
        progressLabel.alignment = TextAnchor.MiddleCenter;
        progressLabel.color = Color.white;
        var outline = labelObj.AddComponent<Outline>();
        outline.effectColor = Color.black;

        progressCanvas.gameObject.SetActive(false);
    }

    void UpdateProgressBar(float progress, Color? fillColor = null)
    {
        if (progressBarFill != null)
        {
            float width = progress * 200f;
            progressBarFill.rectTransform.sizeDelta = new Vector2(width, 20);
            if (fillColor.HasValue)
                progressBarFill.color = fillColor.Value;
        }
    }

    void ShowProgressBar(bool show, string labelText = "")
    {
        if (progressCanvas != null)
        {
            progressCanvas.gameObject.SetActive(show);
            if (progressLabel != null)
                progressLabel.text = labelText;
        }
    }

    void Update()
    {
        // Only allow movement/look when inventory is closed
        if (!IsInventoryOpen())
        {
            Move();
            Look();
        }

        HandleInteraction();
        HandleWatering();

        if (Input.GetKeyDown(KeyCode.I) && InventoryUIBehaviour.Instance != null)
            InventoryUIBehaviour.Instance.Toggle();
        if (Input.GetKeyDown(KeyCode.F5) && SaveSystem.Instance != null) SaveSystem.Instance.Save();
        if (Input.GetKeyDown(KeyCode.F9) && SaveSystem.Instance != null) SaveSystem.Instance.Load();
    }

    bool IsInventoryOpen()
    {
        return InventoryUIBehaviour.Instance != null &&
               InventoryUIBehaviour.Instance.gameObject.activeInHierarchy &&
               Cursor.lockState == CursorLockMode.None;
    }

    void HandleInteraction()
    {
        if (IsInventoryOpen()) return;

        PlantingSpot spot = GetLookedAtSpot();

        if (Input.GetKey(KeyCode.E) && spot != null && !isWatering)
        {
            bool canInteract = false;
            if (spot.HasCrop())
            {
                canInteract = true; // Can harvest any plant
            }
            else if (!spot.HasCrop() && Inventory.Instance.GetFirstSeed() != null)
            {
                canInteract = true; // Can plant
            }

            if (canInteract)
            {
                if (!isHolding || currentSpot != spot)
                {
                    isHolding = true;
                    currentSpot = spot;
                    holdTimer = 0f;

                    if (cropsAudio != null && !audioSource.isPlaying)
                    {
                        audioSource.clip = cropsAudio;
                        audioSource.Play();
                    }

                    string actionLabel = spot.HasCrop() ? "Harvesting..." : "Planting...";
                    ShowProgressBar(true, actionLabel);
                }

                holdTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(holdTimer / holdDuration);
                UpdateProgressBar(progress, Color.white);

                if (holdTimer >= holdDuration)
                {
                    PerformInteraction(spot);
                    ResetHold();
                }
            }
            else
            {
                ResetHold();
            }
        }
        else if (!Input.GetKey(KeyCode.E))
        {
            ResetHold();
        }
    }

    void HandleWatering()
    {
        if (IsInventoryOpen()) return;

        PlantingSpot spot = GetLookedAtSpot();

        if (Input.GetKey(KeyCode.R) && spot != null && spot.CanWater() && !isHolding)
        {
            if (!isWatering || wateringSpot != spot)
            {
                isWatering = true;
                wateringSpot = spot;
                waterTimer = 0f;

                if (waterAudio != null && !audioSource.isPlaying)
                {
                    audioSource.clip = waterAudio;
                    audioSource.Play();
                }

                ShowProgressBar(true, "Watering...");
            }

            waterTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(waterTimer / waterDuration);
            UpdateProgressBar(progress, new Color(0.3f, 0.7f, 1f)); // Blue for water

            if (waterTimer >= waterDuration)
            {
                spot.WaterPlant();
                ResetWater();

                if (InventoryUIBehaviour.Instance != null)
                    InventoryUIBehaviour.Instance.Refresh();
            }
        }
        else if (!Input.GetKey(KeyCode.R))
        {
            ResetWater();
        }
    }

    PlantingSpot GetLookedAtSpot()
    {
        if (cam == null) return null;
        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            return hit.collider.GetComponentInParent<PlantingSpot>();
        }
        return null;
    }

    void PerformInteraction(PlantingSpot spot)
    {
        if (spot.HasCrop())
        {
            var results = spot.Harvest();
            if (results != null)
            {
                foreach (var item in results)
                {
                    Inventory.Instance.AddItem(item);
                }

                if (InventoryUIBehaviour.Instance != null)
                    InventoryUIBehaviour.Instance.Refresh();
            }
        }
        else
        {
            var seed = Inventory.Instance.GetFirstSeed();
            if (seed != null)
            {
                spot.Plant(seed.itemId);
                Inventory.Instance.RemoveItem(seed.itemId, 1);

                if (InventoryUIBehaviour.Instance != null)
                    InventoryUIBehaviour.Instance.Refresh();
            }
        }
    }

    void ResetHold()
    {
        if (isHolding)
        {
            isHolding = false;
            holdTimer = 0f;
            currentSpot = null;
            if (audioSource.isPlaying && audioSource.clip == cropsAudio)
            {
                audioSource.Stop();
            }
            if (!isWatering)
            {
                ShowProgressBar(false);
                UpdateProgressBar(0f);
            }
        }
    }

    void ResetWater()
    {
        if (isWatering)
        {
            isWatering = false;
            waterTimer = 0f;
            wateringSpot = null;
            if (audioSource.isPlaying && audioSource.clip == waterAudio)
            {
                audioSource.Stop();
            }
            if (!isHolding)
            {
                ShowProgressBar(false);
                UpdateProgressBar(0f);
            }
        }
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        move *= speed;

        if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -1f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void Look()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mx);
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -85f, 85f);
        if (cam) cam.localEulerAngles = new Vector3(pitch, 0, 0);
    }
}