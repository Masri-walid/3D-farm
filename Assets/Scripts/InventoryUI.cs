using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

public class InventoryUIBehaviour : MonoBehaviour
{
    public static InventoryUIBehaviour Instance;
    Canvas canvas;
    GameObject inventoryPanel;
    InventorySlot[] slots = new InventorySlot[9]; // 3x3 grid
    bool isVisible = false; // Start hidden, open with 'I'

    // Loaded icon textures
    Dictionary<string, Sprite> itemSprites = new Dictionary<string, Sprite>();

    // Drag and drop
    InventorySlot dragSourceSlot;
    GameObject dragIcon;
    Image dragIconImage;

    void Awake()
    {
        Instance = this;
        LoadIcons();
        CreateInventoryUI();
    }

    void Start()
    {
        // Hide inventory initially
        inventoryPanel.SetActive(false);
        Invoke("Refresh", 0.5f);
    }

    void LoadIcons()
    {
        // Load icons from Crops folder
        Texture2D seedTex = Resources.Load<Texture2D>("Crops/seeds");
        Texture2D extractionTex = Resources.Load<Texture2D>("Crops/medecin");
        Texture2D powderTex = Resources.Load<Texture2D>("Crops/pouder");

        if (seedTex != null)
            itemSprites["plant_seed"] = Sprite.Create(seedTex, new Rect(0, 0, seedTex.width, seedTex.height), new Vector2(0.5f, 0.5f));
        if (extractionTex != null)
            itemSprites["extraction"] = Sprite.Create(extractionTex, new Rect(0, 0, extractionTex.width, extractionTex.height), new Vector2(0.5f, 0.5f));
        if (powderTex != null)
            itemSprites["powder"] = Sprite.Create(powderTex, new Rect(0, 0, powderTex.width, powderTex.height), new Vector2(0.5f, 0.5f));
    }

    void CreateInventoryUI()
    {
        // Create canvas
        var canvasObj = new GameObject("InventoryCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create inventory panel on left side with 20% padding
        inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvas.transform, false);
        var panelRect = inventoryPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0.5f);
        panelRect.anchorMax = new Vector2(0f, 0.5f);
        panelRect.pivot = new Vector2(0f, 0.5f);
        // 20% padding from left edge
        float paddingX = Screen.width * 0.2f / (Screen.width / 1920f);
        panelRect.anchoredPosition = new Vector2(paddingX, 0);
        panelRect.sizeDelta = new Vector2(300, 340);

        // Panel background
        var panelBg = inventoryPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(inventoryPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(200, 30);
        var titleText = titleObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 24;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.text = "Inventory";

        // Grid container
        var gridObj = new GameObject("Grid");
        gridObj.transform.SetParent(inventoryPanel.transform, false);
        var gridRect = gridObj.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.anchoredPosition = new Vector2(0, -10);
        gridRect.sizeDelta = new Vector2(270, 270);

        var gridLayout = gridObj.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(80, 80);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;

        // Create 9 slots (3x3)
        for (int i = 0; i < 9; i++)
        {
            slots[i] = CreateSlot(gridObj.transform, i);
        }

        // Create drag icon (initially hidden)
        CreateDragIcon();
    }

    InventorySlot CreateSlot(Transform parent, int index)
    {
        var slotObj = new GameObject($"Slot_{index}");
        slotObj.transform.SetParent(parent, false);
        var slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(80, 80);

        // Border background
        var borderImage = slotObj.AddComponent<Image>();
        borderImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Inner background
        var inner = new GameObject("Inner");
        inner.transform.SetParent(slotObj.transform, false);
        var innerRect = inner.AddComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(3, 3);
        innerRect.offsetMax = new Vector2(-3, -3);
        var innerImage = inner.AddComponent<Image>();
        innerImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Icon
        var iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        var iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(60, 60);
        iconRect.anchoredPosition = Vector2.zero;
        var iconImage = iconObj.AddComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.color = new Color(1, 1, 1, 0); // Start invisible

        // Count text
        var countObj = new GameObject("Count");
        countObj.transform.SetParent(slotObj.transform, false);
        var countRect = countObj.AddComponent<RectTransform>();
        countRect.anchorMin = new Vector2(1f, 0f);
        countRect.anchorMax = new Vector2(1f, 0f);
        countRect.pivot = new Vector2(1f, 0f);
        countRect.anchoredPosition = new Vector2(-5, 5);
        countRect.sizeDelta = new Vector2(50, 25);
        var countText = countObj.AddComponent<Text>();
        countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countText.fontSize = 18;
        countText.fontStyle = FontStyle.Bold;
        countText.alignment = TextAnchor.LowerRight;
        countText.color = Color.white;
        countText.text = "";
        var outline = countObj.AddComponent<Outline>();
        outline.effectColor = Color.black;

        // Add event trigger for drag/drop
        var eventTrigger = slotObj.AddComponent<EventTrigger>();
        int slotIndex = index; // Capture for closure

        // Begin drag
        var beginDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        beginDragEntry.callback.AddListener((data) => OnBeginDrag(slotIndex, (PointerEventData)data));
        eventTrigger.triggers.Add(beginDragEntry);

        // Drag
        var dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        dragEntry.callback.AddListener((data) => OnDrag((PointerEventData)data));
        eventTrigger.triggers.Add(dragEntry);

        // End drag
        var endDragEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        endDragEntry.callback.AddListener((data) => OnEndDrag(slotIndex, (PointerEventData)data));
        eventTrigger.triggers.Add(endDragEntry);

        // Drop
        var dropEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drop };
        dropEntry.callback.AddListener((data) => OnDrop(slotIndex, (PointerEventData)data));
        eventTrigger.triggers.Add(dropEntry);

        return new InventorySlot { slotObject = slotObj, iconImage = iconImage, countText = countText, index = index };
    }

    void CreateDragIcon()
    {
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(canvas.transform, false);
        var dragRect = dragIcon.AddComponent<RectTransform>();
        dragRect.sizeDelta = new Vector2(60, 60);
        dragIconImage = dragIcon.AddComponent<Image>();
        dragIconImage.preserveAspect = true;
        dragIconImage.raycastTarget = false;
        dragIcon.SetActive(false);
    }

    void OnBeginDrag(int slotIndex, PointerEventData eventData)
    {
        var slot = slots[slotIndex];
        if (string.IsNullOrEmpty(slot.itemId)) return;

        dragSourceSlot = slot;
        dragIcon.SetActive(true);
        dragIconImage.sprite = slot.iconImage.sprite;
        dragIconImage.color = Color.white;
        slot.iconImage.color = new Color(1, 1, 1, 0.3f);
    }

    void OnDrag(PointerEventData eventData)
    {
        if (dragSourceSlot == null) return;
        dragIcon.transform.position = eventData.position;
    }

    void OnEndDrag(int slotIndex, PointerEventData eventData)
    {
        if (dragSourceSlot != null)
        {
            dragSourceSlot.iconImage.color = Color.white;
        }
        dragSourceSlot = null;
        dragIcon.SetActive(false);
    }

    void OnDrop(int targetSlotIndex, PointerEventData eventData)
    {
        if (dragSourceSlot == null) return;
        if (dragSourceSlot.index == targetSlotIndex) return;

        // Swap items in inventory
        SwapInventorySlots(dragSourceSlot.index, targetSlotIndex);
        Refresh();
    }

    void SwapInventorySlots(int fromIndex, int toIndex)
    {
        if (Inventory.Instance == null) return;
        Inventory.Instance.SwapSlots(fromIndex, toIndex);
    }

    public void Toggle()
    {
        isVisible = !isVisible;
        inventoryPanel.SetActive(isVisible);

        // Unlock cursor when inventory is open
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isVisible;

        if (isVisible) Refresh();
    }

    public void Refresh()
    {
        if (Inventory.Instance == null) return;
        var inv = Inventory.Instance;

        // Clear all slots first
        for (int i = 0; i < 9; i++)
        {
            slots[i].itemId = null;
            slots[i].iconImage.sprite = null;
            slots[i].iconImage.color = new Color(1, 1, 1, 0);
            slots[i].countText.text = "";
        }

        // Get items from inventory and assign to slots
        var slotItems = inv.GetSlotItems();
        for (int i = 0; i < Mathf.Min(slotItems.Count, 9); i++)
        {
            var item = slotItems[i];
            if (item != null && item.count > 0)
            {
                slots[i].itemId = item.itemId;
                if (itemSprites.TryGetValue(item.itemId, out Sprite sprite))
                {
                    slots[i].iconImage.sprite = sprite;
                    slots[i].iconImage.color = Color.white;
                }
                slots[i].countText.text = item.count.ToString();
            }
        }
    }

    void Update()
    {
        if (isVisible) Refresh();
    }
}

public class InventorySlot
{
    public GameObject slotObject;
    public Image iconImage;
    public Text countText;
    public int index;
    public string itemId;
}
