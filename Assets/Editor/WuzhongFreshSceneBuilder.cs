using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Creates a fresh playable placeholder village scene for Wu Zhong.
/// Run from Tools > Wu Zhong > Build Fresh Village Scene.
/// Generated art is deliberately simple placeholder art that can later be replaced.
/// </summary>
public static class WuzhongFreshSceneBuilder
{
    private const string Root = "Assets/WuzhongGenerated";
    private const string ScenePath = "Assets/Scenes/WuzhongVillage_New.unity";

    [MenuItem("Tools/Wu Zhong/Build Fresh Village Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureFolder("Assets", "WuzhongGenerated");
        EnsureFolder(Root, "Textures");
        EnsureFolder("Assets", "Scenes");

        Sprite ground = MakeSprite("Ground", new Color32(45, 65, 58, 255));
        Sprite grass = MakeSprite("Grass", new Color32(55, 78, 66, 255));
        Sprite road = MakeSprite("StoneRoad", new Color32(93, 91, 79, 255));
        Sprite roadAlt = MakeSprite("StoneRoadAlt", new Color32(105, 101, 87, 255));
        Sprite wall = MakeSprite("OldHouseWall", new Color32(65, 51, 47, 255));
        Sprite roof = MakeSprite("OldHouseRoof", new Color32(83, 34, 39, 255));
        Sprite playerSprite = MakeSprite("PlayerPlaceholder", new Color32(202, 185, 151, 255));
        Sprite lantern = MakeSprite("RedLantern", new Color32(166, 37, 43, 255));
        Sprite fog = MakeSprite("FogOverlay", new Color32(92, 137, 129, 70));

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("WuzhongVillage");
        var groundRoot = new GameObject("Ground - Base");
        groundRoot.transform.SetParent(root.transform);
        for (int x = -12; x <= 12; x++)
        for (int y = -9; y <= 9; y++)
            Place("Ground", ground, new Vector3(x, y, 5), groundRoot.transform, 0.98f, 0.98f, -10);

        var roadRoot = new GameObject("Stone Paths");
        roadRoot.transform.SetParent(root.transform);
        for (int x = -12; x <= 12; x++)
        {
            Place("Main Road", ((x % 2) == 0 ? road : roadAlt), new Vector3(x, -1, 3), roadRoot.transform, 0.98f, 0.98f, -5);
            Place("North Path", road, new Vector3(x, 4, 3), roadRoot.transform, 0.98f, 0.98f, -5);
        }
        for (int y = -8; y <= 8; y++)
            Place("Cross Path", road, new Vector3(1, y, 3), roadRoot.transform, 0.98f, 0.98f, -5);

        BuildHouse(root.transform, "Abandoned Old House", new Vector3(-7, 3, 0), wall, roof, 4.2f, 3.0f);
        BuildHouse(root.transform, "Ancestral Hall", new Vector3(6, 5, 0), wall, roof, 5.2f, 3.5f);
        BuildHouse(root.transform, "Riverside House", new Vector3(7, -4, 0), wall, roof, 3.5f, 2.8f);
        BuildHouse(root.transform, "Collapsed Storehouse", new Vector3(-7, -5, 0), wall, roof, 3.8f, 2.6f);

        var decor = new GameObject("Red Lanterns");
        decor.transform.SetParent(root.transform);
        foreach (Vector2 p in new[] { new Vector2(-3, -0.1f), new Vector2(4, -0.1f), new Vector2(-4, 4.8f), new Vector2(3, 4.8f) })
            Place("Lantern", lantern, new Vector3(p.x, p.y, -1), decor.transform, 0.35f, 0.48f, 3);

        var player = new GameObject("Player");
        player.transform.position = new Vector3(0, -2, 0);
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite;
        sr.sortingOrder = 20;
        player.AddComponent<Rigidbody2D>().gravityScale = 0;
        var collider = player.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(0.55f, 0.7f);
        player.tag = "Untagged";
        var visual = new GameObject("Visual");
        visual.transform.SetParent(player.transform, false);
        sr.transform.SetParent(visual.transform, false);

        var fogRoot = new GameObject("Atmosphere - Fog");
        fogRoot.transform.SetParent(root.transform);
        Place("Fog", fog, new Vector3(0, 0, -3), fogRoot.transform, 25, 19, -1);

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 0, -10);
        var cam = cameraObject.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8;
        cam.backgroundColor = new Color(0.055f, 0.09f, 0.085f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<WuzhongSimpleFollow>().target = player.transform;

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeGameObject = player;
        EditorUtility.DisplayDialog("Wu Zhong", "Fresh village scene created at " + ScenePath + "\nOpen it and press Play. This is a new scene; your previous files were not deleted.", "OK");
    }

    private static void BuildHouse(Transform parent, string name, Vector3 pos, Sprite wall, Sprite roof, float width, float height)
    {
        var house = new GameObject(name);
        house.transform.SetParent(parent);
        house.transform.position = pos;
        Place("Wall", wall, pos + new Vector3(0, 0, 0), house.transform, width, height, 5);
        Place("Roof", roof, pos + new Vector3(0, height * 0.48f, -0.1f), house.transform, width + 0.65f, height * 0.42f, 8);
        Place("Door", MakeSprite("DoorPlaceholder", new Color32(35, 27, 27, 255)), pos + new Vector3(0, -height * 0.2f, -0.2f), house.transform, 0.7f, 1.15f, 10);
    }

    private static GameObject Place(string name, Sprite sprite, Vector3 position, Transform parent, float sx, float sy, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = new Vector3(sx, sy, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return go;
    }

    private static Sprite MakeSprite(string name, Color color)
    {
        string path = Root + "/Textures/" + name + ".png";
        if (!File.Exists(path))
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % 16, y = i / 16;
                float variation = ((x * 17 + y * 31) % 7) * 0.012f;
                pixels[i] = new Color(Mathf.Clamp01(color.r + variation), Mathf.Clamp01(color.g + variation), Mathf.Clamp01(color.b + variation), color.a);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }
}

public class WuzhongSimpleFollow : MonoBehaviour
{
    public Transform target;
    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = new Vector3(target.position.x, target.position.y, -10);
        transform.position = Vector3.Lerp(transform.position, desired, 8f * Time.deltaTime);
    }
}
