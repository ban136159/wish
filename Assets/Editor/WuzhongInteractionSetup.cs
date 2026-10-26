#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor helpers for wiring up Wu Zhong's interaction prompt and buildings.
/// Select a player or building in the Hierarchy, then use the Wu Zhong menu.
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
            EditorUtility.DisplayDialog("雾钟交互设置", "请先在 Hierarchy 中选中主角。", "知道了");
            return;
        }

        PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();
        if (interactor == null)
        {
            interactor = Undo.AddComponent<PlayerInteractor>(player);
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(
                "InteractionCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create interaction canvas");

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        Transform existingPrompt = canvas.transform.Find("InteractionPrompt");
        TextMeshProUGUI prompt = existingPrompt != null
            ? existingPrompt.GetComponent<TextMeshProUGUI>()
            : null;

        if (prompt == null)
        {
            GameObject promptObject = new GameObject(
                "InteractionPrompt",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI)
            );
            Undo.RegisterCreatedObjectUndo(promptObject, "Create interaction prompt");
            promptObject.transform.SetParent(canvas.transform, false);

            RectTransform rect = promptObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 80f);
            rect.sizeDelta = new Vector2(700f, 80f);

            prompt = promptObject.GetComponent<TextMeshProUGUI>();
            prompt.text = "[E] 交互";
            prompt.fontSize = 32f;
            prompt.alignment = TextAlignmentOptions.Center;
            prompt.color = new Color(1f, 0.9f, 0.72f, 1f);
            prompt.raycastTarget = false;
        }

        prompt.gameObject.SetActive(false);

        SerializedObject serializedInteractor = new SerializedObject(interactor);
        SerializedProperty promptProperty = serializedInteractor.FindProperty("promptText");
        if (promptProperty != null)
        {
            promptProperty.objectReferenceValue = prompt;
            serializedInteractor.ApplyModifiedProperties();
        }

        // Detect IInteractable objects on any layer by default.
        // Users can narrow this mask later in the Inspector if desired.
        SerializedProperty layersProperty = serializedInteractor.FindProperty("interactableLayers");
        if (layersProperty != null)
        {
            layersProperty.intValue = ~0;
            serializedInteractor.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(interactor);
        Selection.activeGameObject = player;
        EditorUtility.DisplayDialog(
            "雾钟交互设置",
            "主角交互提示已创建并绑定。请保存场景，然后给建筑设置交互组件和 Collider2D。",
            "完成"
        );
    }

    [MenuItem(MenuRoot + "设置所选建筑可交互")]
    private static void SetupSelectedBuilding()
    {
        GameObject building = Selection.activeGameObject;
        if (building == null)
        {
            EditorUtility.DisplayDialog("雾钟交互设置", "请先在 Hierarchy 中选中一栋建筑。", "知道了");
            return;
        }

        BuildingInteractable interactable = building.GetComponent<BuildingInteractable>();
        if (interactable == null)
        {
            interactable = Undo.AddComponent<BuildingInteractable>(building);
        }

        Collider2D collider = building.GetComponent<Collider2D>();
        if (collider == null)
        {
            // For a child-sprite building, a BoxCollider2D can be resized in the Inspector.
            collider = Undo.AddComponent<BoxCollider2D>(building);
        }

        // PlayerInteractor uses overlap-radius detection, not trigger callbacks.
        // The collider therefore only needs to be queryable by Physics2D.
        if (collider.isTrigger)
        {
            Undo.RecordObject(collider, "Disable building trigger collider");
            collider.isTrigger = false;
        }

        EditorUtility.SetDirty(interactable);
        EditorUtility.SetDirty(collider);
        EditorUtility.DisplayDialog(
            "雾钟交互设置",
            "已为所选建筑添加 BuildingInteractable 和 Collider2D。请在 Inspector 中设置建筑名称，并调整碰撞体覆盖的范围。",
            "完成"
        );
    }
}
#endif
