#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 《雾钟》交互系统快捷配置工具。
/// 先选中主角或建筑，再从顶部“雾钟/交互系统”菜单执行。
/// </summary>
public static class WuzhongInteractionSetup
{
    private const string MenuRoot = "雾钟/交互系统/";
    private const string PromptCanvasName = "WuzhongInteractionPromptCanvas";

    [MenuItem(MenuRoot + "设置主角头顶提示（先选中主角）")]
    private static void SetupPlayerInteraction()
    {
        GameObject player = Selection.activeGameObject;
        if (player == null)
        {
            EditorUtility.DisplayDialog("雾钟交互系统", "请先在 Hierarchy 中选中主角。", "确定");
            return;
        }

        PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();
        if (interactor == null)
            interactor = Undo.AddComponent<PlayerInteractor>(player);

        // 在主角下面创建或复用 World Space Canvas，让提示跟随主角。
        Transform existingCanvas = player.transform.Find(PromptCanvasName);
        GameObject canvasObject;

        if (existingCanvas != null)
        {
            canvasObject = existingCanvas.gameObject;
        }
        else
        {
            canvasObject = new GameObject(
                PromptCanvasName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler)
            );

            Undo.RegisterCreatedObjectUndo(canvasObject, "创建雾钟头顶交互提示");
            canvasObject.transform.SetParent(player.transform, false);
        }

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.localPosition = new Vector3(0f, 1.5f, 0f);
        canvasRect.localRotation = Quaternion.identity;
        canvasRect.localScale = Vector3.one * 0.01f;
        canvasRect.sizeDelta = new Vector2(280f, 60f);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        // 创建或复用背景 Image。
        Transform existingBackground = canvasObject.transform.Find("Background");
        GameObject backgroundObject;

        if (existingBackground != null)
        {
            backgroundObject = existingBackground.gameObject;
        }
        else
        {
            backgroundObject = new GameObject(
                "Background",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            Undo.RegisterCreatedObjectUndo(backgroundObject, "创建交互提示背景");
            backgroundObject.transform.SetParent(canvasObject.transform, false);
        }

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        backgroundRect.localScale = Vector3.one;

        Image background = backgroundObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.78f);
        background.raycastTarget = false;

        // 创建或复用提示文字。
        Transform existingText = backgroundObject.transform.Find("PromptText");
        GameObject textObject;

        if (existingText != null)
        {
            textObject = existingText.gameObject;
        }
        else
        {
            textObject = new GameObject(
                "PromptText",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );

            Undo.RegisterCreatedObjectUndo(textObject, "创建交互提示文字");
            textObject.transform.SetParent(backgroundObject.transform, false);
        }

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);
        textRect.localScale = Vector3.one;

        TextMeshProUGUI promptText = textObject.GetComponent<TextMeshProUGUI>();
        promptText.text = "[E] 调查老宅";
        promptText.fontSize = 24f;
        promptText.color = new Color(1f, 0.93f, 0.80f, 1f);
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.enableWordWrapping = false;
        promptText.raycastTarget = false;

        Undo.RecordObject(interactor, "设置雾钟交互提示");
        interactor.ConfigurePrompt(canvas, promptText, background);
        EditorUtility.SetDirty(interactor);

        // 初始隐藏整个 Canvas，交互检测到目标后再显示。
        canvasObject.SetActive(false);

        Selection.activeGameObject = player;
        EditorUtility.DisplayDialog(
            "雾钟交互系统",
            "已为主角创建头顶交互提示。\n\n" +
            "在 PlayerInteractor Inspector 中指定中文像素 TMP 字体，" +
            "并按需调整字号、背景透明度和头顶偏移。",
            "确定"
        );
    }

    [MenuItem(MenuRoot + "设置所选建筑可交互")]
    private static void SetupBuildingInteraction()
    {
        GameObject building = Selection.activeGameObject;
        if (building == null)
        {
            EditorUtility.DisplayDialog("雾钟交互系统", "请先选中建筑或它的 InteractionZone。", "确定");
            return;
        }

        GameObject target = building;
        Transform zone = building.transform.Find("InteractionZone");
        if (zone != null)
            target = zone.gameObject;

        if (target.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(target);
            collider.isTrigger = true;
            collider.size = new Vector2(0.8f, 0.6f);
        }

        if (target.GetComponent<BuildingInteractable>() == null)
            Undo.AddComponent<BuildingInteractable>(target);

        EditorUtility.SetDirty(target);
        EditorUtility.DisplayDialog(
            "雾钟交互系统",
            $"已设置可交互对象：{target.name}\n请检查 Collider 2D 的位置和大小，并设置建筑名称与动作。",
            "确定"
        );
    }
}
#endif
