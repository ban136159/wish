using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
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
        EnsureFolder(Root, "Tiles");
        EnsureFolder("Assets", "Scenes");

        Sprite ground = MakeSprite("Ground", new Color32(45, 65, 58, 255));
        Sprite road = MakeSprite("StoneRoad", new Color32(93, 91, 79, 255));
        Sprite roadAlt = MakeSprite("StoneRoadAlt", new Color32(105, 101, 87, 255));
        Sprite wall = MakeSprite("OldHouseWall", new Color32(65, 51, 47, 255));
        Sprite roof = MakeSprite("OldHouseRoof", new Color32(83, 34, 39, 255));
        Sprite playerSprite = MakeSprite("PlayerPlaceholder", new Color32(202, 185, 151, 255));
        Sprite lantern = MakeSprite("RedLantern", new Color32(166, 37, 43, 255));
        Sprite fog = MakeSprite("FogOverlay", new Color32(92, 137, 129, 70));

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("WuzhongVillage");
        // Tilemap-based ground and paths, editable through Unity Tile Palette.
        Grid grid = new GameObject("Village Grid").AddComponent<Grid>();
        grid.transform.SetParent(root.transform, false);
        grid.cellSize = Vector3.one;
        Tile groundTile = CreateTileAsset("Ground_Tile", ground);
        Tile roadTile = CreateTileAsset("StoneRoad_Tile", road);
        Tile roadAltTile = CreateTileAsset("StoneRoadAlt_Tile", roadAlt);
        Tilemap groundMap = CreateTilemap(grid.transform, "Ground Tilemap", -10);
        Tilemap pathMap = CreateTilemap(grid.transform, "Road Tilemap", -5);

        for (int x = -12; x <= 12; x++)
        for (int y = -9; y <= 9; y++)
            groundMap.SetTile(new Vector3Int(x, y, 0), groundTile);

        for (int x = -12; x <= 12; x++)
        {
            pathMap.SetTile(new Vector3Int(x, -1, 0), (x % 2 == 0) ? roadTile : roadAltTile);
            pathMap.SetTile(new Vector3Int(x, 4, 0), (x % 3 == 0) ? roadAltTile : roadTile);
        }
        for (int y = -8; y <= 8; y++)
            pathMap.SetTile(new Vector3Int(1, y, 0), (y % 4 == 0) ? roadAltTile : roadTile);
        for (int x = -8; x <= -4; x++)
            pathMap.SetTile(new Vector3Int(x, 0, 0), roadTile);
        for (int x = 4; x <= 8; x++)
            pathMap.SetTile(new Vector3Int(x, 2, 0), roadTile);
        for (int y = -5; y <= -3; y++)
            pathMap.SetTile(new Vector3Int(-4, y, 0), roadTile);

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
        player.tag = "Player";
        player.AddComponent<PlayerController>();
        var visual = new GameObject("Visual");
        visual.transform.SetParent(player.transform, false);
        sr.transform.SetParent(visual.transform, false);
        sr.gameObject.AddComponent<WuzhongYSort>();

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
        EditorUtility.DisplayDialog("Wu Zhong", "Village scene created at " + ScenePath + "\nGround and roads are editable Tilemaps. Reusable Tile assets are in Assets/WuzhongGenerated/Tiles. Open Window > 2D > Tile Palette to paint details.", "OK");
    }

    private static void BuildHouse(Transform parent, string name, Vector3 pos, Sprite wall, Sprite roof, float width, float height)
    {
        var house = new GameObject(name);
        house.transform.SetParent(parent);
        house.transform.position = pos;
        // All house parts participate in Y sorting. The roof is higher on screen,
        // so it naturally renders behind the player when the player walks in front.
        GameObject wallObject = Place("Wall", wall, pos, house.transform, width, height, 0);
        wallObject.AddComponent<WuzhongYSort>();

        GameObject roofObject = Place("Roof", roof, pos + new Vector3(0, height * 0.48f, -0.1f),
            house.transform, width + 0.65f, height * 0.42f, 0);
        roofObject.AddComponent<WuzhongYSort>();

        GameObject doorObject = Place("Door", MakeSprite("DoorPlaceholder", new Color32(35, 27, 27, 255)),
            pos + new Vector3(0, -height * 0.2f, -0.2f), house.transform, 0.7f, 1.15f, 0);
        doorObject.AddComponent<WuzhongYSort>();
    }

    private static Tilemap CreateTilemap(Transform grid, string name, int sortingOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(grid, false);
        var tilemap = go.AddComponent<Tilemap>();
        var renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        renderer.mode = TilemapRenderer.Mode.Chunk;
        return tilemap;
    }

    private static Tile CreateTileAsset(string name, Sprite sprite)
    {
        string path = Root + "/Tiles/" + name + ".asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, path);
        }
        else
        {
            tile.sprite = sprite;
            EditorUtility.SetDirty(tile);
        }
        return tile;
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


/// <summary>
/// Simple top-down depth sorting. Objects lower on screen render in front of objects higher on screen.
/// Attach to a SpriteRenderer object; house roofs can keep a fixed higher order if desired.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WuzhongYSort : MonoBehaviour
{
    [SerializeField] private int orderOffset;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f) + orderOffset;
    }
}
