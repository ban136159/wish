using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// WuzhongMapGenerator 的 Inspector 按钮和 Sprite -> Tile 自动转换。
/// 此文件必须放在名为 Editor 的文件夹中。
/// </summary>
[CustomEditor(typeof(WuzhongMapGenerator))]
public class WuzhongMapGeneratorEditor : Editor
{
    private const string GeneratedTileFolder = "Assets/Generated/WuzhongTiles";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("雾钟地图工具", EditorStyles.boldLabel);

        WuzhongMapGenerator generator = (WuzhongMapGenerator)target;

        if (GUILayout.Button("生成地图", GUILayout.Height(32)))
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("请先退出运行模式", "请停止 Play 模式后再生成并保存地图。", "知道了");
                return;
            }

            if (!ValidateGenerator(generator)) return;

            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Generate Wuzhong Map");
            TileBase[] ground = CreateTiles(generator.groundSprites);
            TileBase[] roads = CreateTiles(generator.roadSprites);
            TileBase[] borders = CreateTiles(generator.borderSprites);

            generator.GenerateMap(ground, roads, borders);
            EditorUtility.SetDirty(generator);
            if (generator.groundTilemap != null)
            {
                EditorUtility.SetDirty(generator.groundTilemap);
                TilemapRenderer renderer = generator.groundTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null) EditorUtility.SetDirty(renderer);
            }
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("清除生成的建筑和装饰"))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Clear Generated Objects");
            generator.ClearGeneratedObjects();
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("选中 Ground Tilemap"))
        {
            if (generator.groundTilemap != null)
                Selection.activeGameObject = generator.groundTilemap.gameObject;
            else
                EditorUtility.DisplayDialog("尚未设置", "请先把专用 Tilemap 拖到 Ground Tilemap 字段。", "知道了");
        }

        EditorGUILayout.HelpBox(
            "安全提示：生成按钮会清空 Ground Tilemap 上的所有地块，因此请使用专用 Tilemap，不要放手工绘制的地块或其他内容。清理按钮只清除 Generated Objects 父物体下的对象。",
            MessageType.Warning);
    }

    private bool ValidateGenerator(WuzhongMapGenerator g)
    {
        if (g.groundTilemap == null)
        {
            EditorUtility.DisplayDialog("缺少 Tilemap", "请创建一个专用 Ground Tilemap，并拖入 Ground Tilemap 字段。", "知道了");
            return false;
        }
        if (g.groundSprites == null || g.groundSprites.Length == 0)
        {
            EditorUtility.DisplayDialog("缺少地面资源", "请至少在 Ground Sprites 中添加一张地面 Sprite。", "知道了");
            return false;
        }
        return true;
    }

    private TileBase[] CreateTiles(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return new TileBase[0];

        EnsureFolder(GeneratedTileFolder);
        List<TileBase> result = new List<TileBase>();

        foreach (Sprite sprite in sprites)
        {
            if (sprite == null) continue;

            string safeName = MakeSafeFileName(sprite.name);
            string path = $"{GeneratedTileFolder}/{safeName}.asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);

            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = safeName;
                tile.sprite = sprite;
                AssetDatabase.CreateAsset(tile, path);
            }
            else if (tile.sprite != sprite)
            {
                tile.sprite = sprite;
                EditorUtility.SetDirty(tile);
            }

            result.Add(tile);
        }

        AssetDatabase.SaveAssets();
        return result.ToArray();
    }

    private void EnsureFolder(string path)
    {
        string current = "Assets";
        string[] parts = path.Split('/');
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private string MakeSafeFileName(string value)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');
        return value.Replace('/', '_').Replace('\\', '_');
    }
}
