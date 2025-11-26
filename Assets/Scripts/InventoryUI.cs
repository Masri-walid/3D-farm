using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryUIBehaviour : MonoBehaviour
{
    public static InventoryUIBehaviour Instance;
    Canvas canvas;
    GameObject hotbar;
    GameObject[] slots = new GameObject[2];
    Image[] slotIcons = new Image[2];
    Text[] slotCounts = new Text[2];
    bool isVisible = true;

    // Loaded icon textures
    Sprite seedSprite;
    Sprite extractionSprite;

    void Awake()
    {
        Instance = this;
        LoadIcons();
        CreateHotbarUI();
    }

    void Start()
    {
        // Refresh UI after inventory is initialized
        Invoke("Refresh", 0.5f);
    }

    void LoadIcons()
    {
        // Load icons from Crops folder
        Texture2D seedTex = Resources.Load<Texture2D>("Crops/seeds");
        Texture2D extractionTex = Resources.Load<Texture2D>("Crops/medecin");

        if (seedTex != null)
        {
            seedSprite = Sprite.Create(seedTex, new Rect(0, 0, seedTex.width, seedTex.height), new Vector2(0.5f, 0.5f));
        }
        if (extractionTex != null)
        {
            extractionSprite = Sprite.Create(extractionTex, new Rect(0, 0, extractionTex.width, extractionTex.height), new Vector2(0.5f, 0.5f));
        }
    }

    void CreateHotbarUI()
    {
        // Create canvas
        var canvasObj = new GameObject("InventoryCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create hotbar container at bottom center
        hotbar = new GameObject("Hotbar");
        hotbar.transform.SetParent(canvas.transform, false);
        var hotbarRect = hotbar.AddComponent<RectTransform>();
        hotbarRect.anchorMin = new Vector2(0.5f, 0f);
        hotbarRect.anchorMax = new Vector2(0.5f, 0f);
        hotbarRect.pivot = new Vector2(0.5f, 0f);
        hotbarRect.anchoredPosition = new Vector2(0, 20);
        hotbarRect.sizeDelta = new Vector2(180, 80);

        // Create horizontal layout
        var layout = hotbar.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        // Create slots
        string[] slotNames = { "Seeds", "Extraction" };
        Sprite[] sprites = { seedSprite, extractionSprite };
        for (int i = 0; i < 2; i++)
        {
            slots[i] = CreateSlot(hotbar.transform, slotNames[i], i, sprites[i]);
        }
    }

    GameObject CreateSlot(Transform parent, string name, int index, Sprite iconSprite)
    {
        // Slot container
        var slot = new GameObject($"Slot_{name}");
        slot.transform.SetParent(parent, false);
        var slotRect = slot.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(80, 80);

        // Border only (no filled background)
        var border = new GameObject("Border");
        border.transform.SetParent(slot.transform, false);
        var borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        var borderImage = border.AddComponent<Image>();
        borderImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

        // Create inner transparent area (border effect)
        var inner = new GameObject("Inner");
        inner.transform.SetParent(border.transform, false);
        var innerRect = inner.AddComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(3, 3); // Border thickness
        innerRect.offsetMax = new Vector2(-3, -3);
        var innerImage = inner.AddComponent<Image>();
        innerImage.color = new Color(0, 0, 0, 0.3f); // Slightly transparent dark

        // Icon (use actual sprite from Crops folder)
        var icon = new GameObject("Icon");
        icon.transform.SetParent(slot.transform, false);
        var iconRect = icon.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(50, 50);
        iconRect.anchoredPosition = new Vector2(0, 5);
        slotIcons[index] = icon.AddComponent<Image>();
        slotIcons[index].preserveAspect = true;

        if (iconSprite != null)
        {
            slotIcons[index].sprite = iconSprite;
            slotIcons[index].color = Color.white;
        }
        else
        {
            // Fallback colored square if icon not found
            slotIcons[index].color = (index == 0) ? new Color(0.6f, 0.4f, 0.2f) : new Color(0.2f, 0.8f, 0.4f);
        }

        // Count text
        var countObj = new GameObject("Count");
        countObj.transform.SetParent(slot.transform, false);
        var countRect = countObj.AddComponent<RectTransform>();
        countRect.anchorMin = new Vector2(1f, 0f);
        countRect.anchorMax = new Vector2(1f, 0f);
        countRect.pivot = new Vector2(1f, 0f);
        countRect.anchoredPosition = new Vector2(-5, 5);
        countRect.sizeDelta = new Vector2(50, 25);
        slotCounts[index] = countObj.AddComponent<Text>();
        slotCounts[index].font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        slotCounts[index].fontSize = 20;
        slotCounts[index].fontStyle = FontStyle.Bold;
        slotCounts[index].alignment = TextAnchor.LowerRight;
        slotCounts[index].color = Color.white;
        slotCounts[index].text = "0";

        // Add outline for better readability
        var outline = countObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        // Label above slot
        var label = new GameObject("Label");
        label.transform.SetParent(slot.transform, false);
        var labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 1f);
        labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.anchoredPosition = new Vector2(0, 20);
        labelRect.sizeDelta = new Vector2(100, 20);
        var labelText = label.AddComponent<Text>();
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 14;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        labelText.text = name;
        var labelOutline = label.AddComponent<Outline>();
        labelOutline.effectColor = Color.black;

        return slot;
    }

    public void Toggle()
    {
        isVisible = !isVisible;
        hotbar.SetActive(isVisible);
        if (isVisible) Refresh();
    }

    public void Refresh()
    {
        if (Inventory.Instance == null) return;

        var inv = Inventory.Instance;

        // Get seed count
        var seeds = inv.items.FirstOrDefault(i => i.itemId.EndsWith("_seed"));
        int seedCount = seeds?.count ?? 0;
        slotCounts[0].text = seedCount.ToString();
        // Dim icon when count is 0
        slotIcons[0].color = seedCount > 0 ? Color.white : new Color(1f, 1f, 1f, 0.3f);

        // Get extraction count
        var extractions = inv.items.FirstOrDefault(i => i.itemId == "extraction");
        int extractionCount = extractions?.count ?? 0;
        slotCounts[1].text = extractionCount.ToString();
        slotIcons[1].color = extractionCount > 0 ? Color.white : new Color(1f, 1f, 1f, 0.3f);
    }

    void Update()
    {
        // Auto-refresh every frame to keep counts updated
        if (isVisible)
        {
            Refresh();
        }
    }
}
