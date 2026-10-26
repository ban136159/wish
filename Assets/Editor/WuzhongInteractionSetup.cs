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

    [MenuItem(MenuRoot + "设置主角交互提示（先选中主角）")]
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

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(
                "InteractionCanvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Interaction Canvas");
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        TMP_Text prompt = canvas.GetComponentInChildren<TMP_Text>(true);
        if (prompt == null)
        {
            GameObject textObject = new GameObject("InteractionPrompt", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(textObject, "Create Interaction Prompt");
            textObject.transform.SetParent(canvas.transform, false);

            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = "[E] 调查老宅";
            text.fontSize = 32;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.08f);
            rect.anchorMax = new Vector2(0.5f, 0.08f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(700, 70);
            prompt = text;
        }

        prompt.gameObject.SetActive(false);
        SerializedObject serialized = new SerializedObject(interactor);
        serialized.FindProperty("promptText").objectReferenceValue = prompt;
        serialized.FindProperty("interactableLayers").intValue = ~0;
        serialized.ApplyModifiedProperties();

        EditorUtility.SetDirty(player);
        EditorUtility.DisplayDialog(
            "雾钟交互系统",
            "主角交互脚本和屏幕提示已设置。接下来选中建筑，运行“设置所选建筑可交互”。",
            "确定");
    }

    [MenuItem(MenuRoot + "设置所选建筑可交互")]
    private static void SetupBuildingInteraction()
    {
        GameObject building = Selection.activeGameObject;
        if (building == null)
        {
            EditorUtility.DisplayDialog("雾钟交互系统", "请先选中一栋建筑。", "确定");
            return;
        }

        if (building.GetComponentInChildren<Collider2D>() == null)
            Undo.AddComponent<BoxCollider2D>(building);

        Collider2D collider = building.GetComponentInChildren<Collider2D>();
        if (collider != null && collider.isTrigger)
            collider.isTrigger = false;

        if (building.GetComponent<BuildingInteractable>() == null)
            Undo.AddComponent<BuildingInteractable>(building);

        EditorUtility.SetDirty(building);
        EditorUtility.DisplayDialog(
            "雾钟交互系统",
            "建筑已添加交互组件。请确认 Collider 2D 覆盖建筑底部附近，再点击 Play 测试。",
            "确定");
    }
}
#endif
