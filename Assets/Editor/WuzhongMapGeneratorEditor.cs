using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 《雾钟》地图生成器的 Inspector 快捷面板。
/// 仅在 Unity 编辑器中运行，不会打包进游戏。
/// </summary>
[CustomEditor(typeof(WuzhongMapGenerator))]
public class WuzhongMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WuzhongMapGenerator generator = (WuzhongMapGenerator)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("《雾钟》快捷操作", EditorStyles.boldLabel);

        if (GUILayout.Button("自动查找 / 创建地图层", GUILayout.Height(30)))
        {
            SetupTilemaps(generator);
        }

        using (new EditorGUI.DisabledScope(
                   generator.groundTile == null || generator.roadTile == null))
        {
            if (GUILayout.Button("一键生成村落地面与道路", GUILayout.Height(40)))
            {
                Undo.RegisterFullObjectHierarchyUndo(
                    generator.gameObject.scene, "生成《雾钟》地图");

                generator.GenerateMap();

                EditorUtility.SetDirty(generator);
                if (generator.groundTilemap != null)
                    EditorUtility.SetDirty(generator.groundTilemap);
                if (generator.roadTilemap != null)
                    EditorUtility.SetDirty(generator.roadTilemap);

                UnityEditor.SceneManagement.EditorSceneManager
                    .MarkSceneDirty(generator.gameObject.scene);
            }
        }

        if (GUILayout.Button("清空生成地图", GUILayout.Height(26)))
        {
            bool confirm = EditorUtility.DisplayDialog(
                "清空《雾钟》地图",
                "这会清空生成器指定的地面和道路 Tilemap 上的全部格子，包括手工绘制的内容。确定继续吗？",
                "清空地图", "取消");

            if (confirm)
            {
                generator.ClearGeneratedMap();
                UnityEditor.SceneManagement.EditorSceneManager
                    .MarkSceneDirty(generator.gameObject.scene);
            }
        }

        if (generator.groundTile == null || generator.roadTile == null)
        {
            EditorGUILayout.HelpBox(
                "请先在上方分别指定 Ground Tile（普通地面）和 Road Tile（石板路）。" +
                "这里需要拖入 Tile 资源，不是 PNG 图片。",
                MessageType.Warning);
        }
    }

    private static void SetupTilemaps(WuzhongMapGenerator generator)
    {
        Tilemap[] tilemaps = Object.FindObjectsByType<Tilemap>(
            FindObjectsSortMode.None);

        foreach (Tilemap tilemap in tilemaps)
        {
            if (generator.groundTilemap == null &&
                tilemap.name == "Ground_Tilemap")
            {
                generator.groundTilemap = tilemap;
            }

            if (generator.roadTilemap == null &&
                tilemap.name == "Road_Tilemap")
            {
                generator.roadTilemap = tilemap;
            }
        }

        if (generator.groundTilemap == null)
        {
            EditorUtility.DisplayDialog(
                "找不到 Ground_Tilemap",
                "请先在 Hierarchy 的 Grid 下创建 Tilemap，并命名为 Ground_Tilemap。",
                "知道了");
            return;
        }

        if (generator.roadTilemap == null)
        {
            Transform grid = generator.groundTilemap.transform.parent;
            GameObject roadObject = new GameObject(
                "Road_Tilemap",
                typeof(GridLayout),
                typeof(Tilemap),
                typeof(TilemapRenderer));

            Undo.RegisterCreatedObjectUndo(roadObject, "创建 Road_Tilemap");

            if (grid != null)
                roadObject.transform.SetParent(grid, false);

            roadObject.transform.localPosition = Vector3.zero;
            roadObject.transform.localRotation = Quaternion.identity;
            roadObject.transform.localScale = Vector3.one;

            generator.roadTilemap = roadObject.GetComponent<Tilemap>();
        }

        EditorUtility.SetDirty(generator);
        EditorUtility.SetDirty(generator.groundTilemap);
        EditorUtility.SetDirty(generator.roadTilemap);

        UnityEditor.SceneManagement.EditorSceneManager
            .MarkSceneDirty(generator.gameObject.scene);

        Debug.Log("《雾钟》地图层已自动绑定。现在指定地面和道路 Tile，然后点击“一键生成村落地面与道路”。", generator);
    }
}
