using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 雾钟地图生成器。
/// 地面使用 Tilemap；大型建筑、树木和装饰使用独立 SpriteRenderer。
/// 请给本组件分配专用的 Ground Tilemap，避免清理时影响手工绘制的地块。
/// </summary>
[DisallowMultipleComponent]
public class WuzhongMapGenerator : MonoBehaviour
{
    [Serializable]
    public class SpawnEntry
    {
        [Tooltip("要生成的 Sprite。")]
        public Sprite sprite;

        [Min(1)]
        [Tooltip("占用宽度（格）。大型建筑可设置为 2、3 等。")]
        public int widthInCells = 1;

        [Min(1)]
        [Tooltip("占用高度（格）。大型建筑可设置为 2、3 等。")]
        public int heightInCells = 1;

        [Min(0)]
        [Tooltip("随机权重。设为 0 表示不参与随机生成。")]
        public int weight = 1;

        [Tooltip("生成位置微调，单位为 Unity 世界单位。")]
        public Vector2 positionOffset = Vector2.zero;

        [Tooltip("生成时的 Z 值。2D 游戏通常保持 0。")]
        public float zOffset = 0f;

        [Tooltip("SpriteRenderer 的 Sorting Order。建筑一般比地面大。")]
        public int sortingOrder = 10;

        [Tooltip("是否允许此类物件靠近地图边缘生成。")]
        public bool allowNearEdge = false;
    }

    [Header("地图范围")]
    [Min(8)] public int mapWidth = 48;
    [Min(8)] public int mapHeight = 48;
    [Min(0.1f)] public float cellSize = 1f;
    public Vector2 worldOrigin = Vector2.zero;
    public int randomSeed = 12345;
    public bool randomizeSeedEachGeneration = false;

    [Header("地图组件")]
    [Tooltip("专门用于自动生成地面的 Tilemap。建议不要放手工绘制的内容。")]
    public Tilemap groundTilemap;
    [Tooltip("生成的建筑和装饰会放在这个父物体下；留空时自动创建。")]
    public Transform generatedObjectsRoot;

    [Header("地面资源接口")]
    [Tooltip("普通地面 Sprite。生成前在 Inspector 中拖入已切片的 Sprite。")]
    public Sprite[] groundSprites;
    [Tooltip("道路 Sprite。可放石板路、泥路等；没有道路资源时可留空。")]
    public Sprite[] roadSprites;
    [Tooltip("边缘/边界地面 Sprite，可选；不填则使用普通地面。")]
    public Sprite[] borderSprites;

    [Header("道路布局")]
    public bool generateMainRoad = true;
    [Min(1)] public int roadWidth = 3;
    [Range(0f, 1f)] public float secondaryPathChance = 0.15f;
    [Tooltip("主街水平位置，-1 表示自动放在地图中央。")]
    public int mainRoadCoordinate = -1;

    [Header("建筑资源接口")]
    [Tooltip("房屋、祠堂、戏台等大型建筑。每项可设置占格大小和生成权重。")]
    public List<SpawnEntry> buildings = new List<SpawnEntry>();

    [Header("装饰资源接口")]
    [Tooltip("树木、石碑、古井、灯笼、木桶等装饰。")]
    public List<SpawnEntry> props = new List<SpawnEntry>();

    [Header("生成数量")]
    [Min(0)] public int buildingCount = 8;
    [Min(0)] public int propCount = 35;
    [Range(0, 20)] public int minimumObjectSpacing = 1;

    [Header("生成后选项")]
    public bool createCollidersForObjects = false;
    public bool setSortingLayerToDefault = true;

    public void GenerateMap(TileBase[] groundTiles, TileBase[] roadTiles, TileBase[] borderTiles)
    {
        if (groundTilemap == null)
        {
            Debug.LogError("请先在 Inspector 中给 Ground Tilemap 分配专用 Tilemap。", this);
            return;
        }

        if (groundTiles == null || groundTiles.Length == 0)
        {
            Debug.LogError("请至少给 Ground Sprites 添加一张地面 Sprite。", this);
            return;
        }

        ClearGeneratedObjects();
        groundTilemap.ClearAllTiles();

        int seed = randomizeSeedEachGeneration ? Environment.TickCount : randomSeed;
        System.Random rng = new System.Random(seed);

        int roadCoord = mainRoadCoordinate < 0
            ? (generateMainRoad ? mapHeight / 2 : -1000)
            : Mathf.Clamp(mainRoadCoordinate, 0, mapHeight - 1);

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                TileBase chosen;
                bool isBorder = x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1;
                bool isMainRoad = generateMainRoad &&
                    Mathf.Abs(y - roadCoord) < Mathf.Max(1, roadWidth) / 2f;

                bool isSecondaryPath = !isMainRoad &&
                    rng.NextDouble() < secondaryPathChance &&
                    (x % 7 == 0 || y % 9 == 0);

                if (isBorder && borderTiles != null && borderTiles.Length > 0)
                    chosen = borderTiles[rng.Next(borderTiles.Length)];
                else if ((isMainRoad || isSecondaryPath) && roadTiles != null && roadTiles.Length > 0)
                    chosen = roadTiles[rng.Next(roadTiles.Length)];
                else
                    chosen = groundTiles[rng.Next(groundTiles.Length)];

                groundTilemap.SetTile(new Vector3Int(x, y, 0), chosen);
            }
        }

        SpawnObjects(buildings, buildingCount, rng, "Buildings");
        SpawnObjects(props, propCount, rng, "Props");

        Debug.Log($"雾钟地图生成完成：{mapWidth}×{mapHeight} 格，随机种子 {seed}。", this);
    }

    public void ClearGeneratedObjects()
    {
        if (generatedObjectsRoot == null) return;

        for (int i = generatedObjectsRoot.childCount - 1; i >= 0; i--)
        {
            GameObject child = generatedObjectsRoot.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }

    private void SpawnObjects(List<SpawnEntry> entries, int count, System.Random rng, string groupName)
    {
        if (entries == null || entries.Count == 0 || count <= 0) return;

        if (generatedObjectsRoot == null)
        {
            Transform existing = transform.Find("GeneratedObjects");
            if (existing != null) generatedObjectsRoot = existing;
            else
            {
                GameObject root = new GameObject("GeneratedObjects");
                root.transform.SetParent(transform, false);
                generatedObjectsRoot = root.transform;
            }
        }

        Transform group = generatedObjectsRoot.Find(groupName);
        if (group == null)
        {
            GameObject groupObject = new GameObject(groupName);
            groupObject.transform.SetParent(generatedObjectsRoot, false);
            group = groupObject.transform;
        }

        for (int i = group.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying) Destroy(group.GetChild(i).gameObject);
            else DestroyImmediate(group.GetChild(i).gameObject);
        }

        bool[,] occupied = new bool[mapWidth, mapHeight];
        int placed = 0;
        int attempts = Mathf.Max(100, count * 30);

        while (placed < count && attempts-- > 0)
        {
            SpawnEntry entry = PickWeightedEntry(entries, rng);
            if (entry == null || entry.sprite == null) continue;

            int w = Mathf.Max(1, entry.widthInCells);
            int h = Mathf.Max(1, entry.heightInCells);
            if (w > mapWidth || h > mapHeight) continue;

            int x = rng.Next(0, mapWidth - w + 1);
            int y = rng.Next(0, mapHeight - h + 1);

            if (!entry.allowNearEdge &&
                (x < 2 || y < 2 || x + w >= mapWidth - 2 || y + h >= mapHeight - 2))
                continue;

            bool overlaps = false;
            int spacing = minimumObjectSpacing;
            for (int ox = Mathf.Max(0, x - spacing); ox < Mathf.Min(mapWidth, x + w + spacing); ox++)
            {
                for (int oy = Mathf.Max(0, y - spacing); oy < Mathf.Min(mapHeight, y + h + spacing); oy++)
                {
                    if (occupied[ox, oy]) { overlaps = true; break; }
                }
                if (overlaps) break;
            }
            if (overlaps) continue;

            for (int ox = x; ox < x + w; ox++)
                for (int oy = y; oy < y + h; oy++)
                    occupied[ox, oy] = true;

            Vector3 world = worldOrigin + new Vector2((x + w * 0.5f) * cellSize, (y + h * 0.5f) * cellSize);
            world.z = entry.zOffset;

            GameObject obj = new GameObject(entry.sprite.name);
            obj.transform.SetParent(group, true);
            obj.transform.position = world + (Vector3)entry.positionOffset;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = entry.sprite;
            sr.sortingOrder = entry.sortingOrder;
            if (setSortingLayerToDefault) sr.sortingLayerName = "Default";

            if (createCollidersForObjects)
            {
                BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
                collider.isTrigger = false;
            }

            placed++;
        }

        if (placed < count)
            Debug.LogWarning($"{groupName} 只放置了 {placed}/{count} 个物件。可以减少数量或减小最小间距。", this);
    }

    private SpawnEntry PickWeightedEntry(List<SpawnEntry> entries, System.Random rng)
    {
        int total = 0;
        foreach (SpawnEntry e in entries)
            if (e != null && e.sprite != null && e.weight > 0) total += e.weight;

        if (total <= 0) return null;

        int roll = rng.Next(0, total);
        foreach (SpawnEntry e in entries)
        {
            if (e == null || e.sprite == null || e.weight <= 0) continue;
            roll -= e.weight;
            if (roll < 0) return e;
        }
        return null;
    }
}
