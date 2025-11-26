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

    // Progress bar UI
    Canvas progressCanvas;
    Image progressBarBackground;
    Image progressBarFill;

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

        // Create progress bar UI
        CreateProgressBar();
    }

    void CreateProgressBar()
    {
        // Create canvas for progress bar
        var canvasObj = new GameObject("ProgressBarCanvas");
        progressCanvas = canvasObj.AddComponent<Canvas>();
        progressCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        progressCanvas.sortingOrder = 200;
        canvasObj.AddComponent<CanvasScaler>();

        // Background (dark border)
        var bgObj = new GameObject("ProgressBarBg");
        bgObj.transform.SetParent(progressCanvas.transform, false);
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = new Vector2(0, -50);
        bgRect.sizeDelta = new Vector2(204, 24);
        progressBarBackground = bgObj.AddComponent<Image>();
        progressBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // Fill bar (white)
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

        // Hide initially
        progressCanvas.gameObject.SetActive(false);
    }

    void UpdateProgressBar(float progress)
    {
        if (progressBarFill != null)
        {
            // Max width is 200 (204 - 4 for padding)
            float width = progress * 200f;
            progressBarFill.rectTransform.sizeDelta = new Vector2(width, 20);
        }
    }

    void ShowProgressBar(bool show)
    {
        if (progressCanvas != null)
        {
            progressCanvas.gameObject.SetActive(show);
        }
    }

    void Update()
    {
        Move();
        Look();
        HandleInteraction();
        if (Input.GetKeyDown(KeyCode.I) && InventoryUIBehaviour.Instance != null)
            InventoryUIBehaviour.Instance.Toggle();
        if (Input.GetKeyDown(KeyCode.F5) && SaveSystem.Instance != null) SaveSystem.Instance.Save();
        if (Input.GetKeyDown(KeyCode.F9) && SaveSystem.Instance != null) SaveSystem.Instance.Load();
    }

    void HandleInteraction()
    {
        PlantingSpot spot = GetLookedAtSpot();

        if (Input.GetKey(KeyCode.E) && spot != null)
        {
            // Check if we can interact (plant or harvest)
            bool canInteract = false;
            if (spot.HasCrop() && spot.CanHarvest())
            {
                canInteract = true; // Can harvest big plant
            }
            else if (!spot.HasCrop() && Inventory.Instance.GetFirstSeed() != null)
            {
                canInteract = true; // Can plant
            }

            if (canInteract)
            {
                if (!isHolding || currentSpot != spot)
                {
                    // Start holding
                    isHolding = true;
                    currentSpot = spot;
                    holdTimer = 0f;

                    // Play audio
                    if (cropsAudio != null && !audioSource.isPlaying)
                    {
                        audioSource.clip = cropsAudio;
                        audioSource.Play();
                    }

                    // Show progress bar
                    ShowProgressBar(true);
                }

                holdTimer += Time.deltaTime;

                // Update progress bar
                float progress = Mathf.Clamp01(holdTimer / holdDuration);
                UpdateProgressBar(progress);

                if (holdTimer >= holdDuration)
                {
                    // Complete interaction
                    PerformInteraction(spot);
                    ResetHold();
                }
            }
            else
            {
                ResetHold();
            }
        }
        else
        {
            ResetHold();
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
            }
        }
        else
        {
            var seed = Inventory.Instance.GetFirstSeed();
            if (seed != null)
            {
                spot.Plant(seed.itemId);
                Inventory.Instance.RemoveItem(seed.itemId, 1);
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
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            // Hide progress bar
            ShowProgressBar(false);
            UpdateProgressBar(0f);
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