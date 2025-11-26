using UnityEngine;
using UnityEngine.UI;

public class InventoryUIBehaviour : MonoBehaviour
{
    public static InventoryUIBehaviour Instance;
    Canvas canvas;
    Text text;

    void Awake()
    {
        Instance = this;
        canvas = new GameObject("InventoryCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var txtObj = new GameObject("InventoryText");
        txtObj.transform.SetParent(canvas.transform);
        text = txtObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.UpperLeft;
        text.rectTransform.sizeDelta = new Vector2(600, 400);
        text.rectTransform.anchoredPosition = new Vector2(20, -20);

        canvas.enabled = false;
    }

    public void Toggle()
    {
        canvas.enabled = !canvas.enabled;
        Refresh();
    }

    public void Refresh()
    {
        if (!canvas.enabled) return;
        var inv = Inventory.Instance;
        text.text = "Inventory:";
        foreach (var item in inv.items)
            text.text += $"- {item.displayName} x{item.count}";
    }
}

