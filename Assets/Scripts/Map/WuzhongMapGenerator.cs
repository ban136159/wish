using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 《雾钟》程序化地面与道路生成器。
/// 负责生成不规则地面、蜿蜒主路和分支道路。
/// 不生成建筑、人物或装饰物。
/// </summary>
public class WuzhongMapGenerator : MonoBehaviour
{
    [Header("地图 Tilemap")]
    [Tooltip("地面绘制层")]
    public Tilemap groundTilemap;

    [Tooltip("道路绘制层；留空则与地面共用一层")]
    public Tilemap roadTilemap;

    [Header("地面素材")]
    [Tooltip("普通地面的 Tile")]
    public TileBase groundTile;

    [Tooltip("石板路的 Tile")]
    public TileBase roadTile;

    [Header("地图尺寸")]
    [Min(20)] public int mapWidth = 80;
    [Min(20)] public int mapHeight = 60;

    [Tooltip("地图中心偏移")]
    public Vector2Int mapOrigin = new Vector2Int(-40, -30);

    [Header("地面形状")]
    [Range(0f, 0.45f)]
    [Tooltip("边缘不规则程度")]
    public float edgeRoughness = 0.16f;

    [Range(0f, 0.5f)]
    [Tooltip("地面边缘起伏频率")]
    public float edgeFrequency = 0.12f;

    [Header("主路")]
    [Min(1)] public int mainRoadWidth = 4;

    [Tooltip("主路纵向变化幅度")]
    [Min(1)] public int mainRoadWander = 10;

    [Header("支路")]
    [Range(0, 12)] public int branchCount = 5;

    [Min(1)] public int branchMinLength = 8;
    [Min(2)] public int branchMaxLength = 24;
    [Min(1)] public int branchRoadWidth = 2;

    [Header("随机生成")]
    public int randomSeed = 12345;

    [Tooltip("每次生成前清除目标 Tilemap 的旧内容")]
    public bool clearBeforeGenerate = true;

    private System.Random rng;

    private readonly List<Vector2Int> mainRoad =
        new List<Vector2Int>();

    /// <summary>
    /// 在 Inspector 组件右上角菜单中可以直接执行。
    /// </summary>
    [ContextMenu("生成《雾钟》地面与道路")]
    public void GenerateMap()
    {
        if (groundTilemap == null)
        {
            Debug.LogError(
                "《雾钟》地图生成失败：请指定 Ground Tilemap。", this);
            return;
        }

        if (groundTile == null)
        {
            Debug.LogError(
                "《雾钟》地图生成失败：请指定 Ground Tile。", this);
            return;
        }

        if (roadTile == null)
        {
            Debug.LogError(
                "《雾钟》地图生成失败：请指定 Road Tile。", this);
            return;
        }

        if (roadTilemap == null)
            roadTilemap = groundTilemap;

        rng = new System.Random(randomSeed);
        mainRoad.Clear();

        // 清除本次生成涉及的地图层。
        if (clearBeforeGenerate)
        {
            groundTilemap.ClearAllTiles();

            if (roadTilemap != groundTilemap)
                roadTilemap.ClearAllTiles();
        }

        // 第一阶段：绘制不规则地面。
        GenerateGround();

        // 第二阶段：生成并绘制贯穿地图的主路。
        GenerateMainRoad();

        // 第三阶段：从主路生成若干分支。
        GenerateBranches();

        groundTilemap.CompressBounds();

        if (roadTilemap != groundTilemap)
            roadTilemap.CompressBounds();

        Debug.Log(
            $"《雾钟》地图生成完成。尺寸：{mapWidth}×{mapHeight}，" +
            $"种子：{randomSeed}。", this);
    }

    /// <summary>
    /// 生成带有不规则边界的椭圆形地面。
    /// </summary>
    private void GenerateGround()
    {
        float centerX = mapOrigin.x + (mapWidth - 1) * 0.5f;
        float centerY = mapOrigin.y + (mapHeight - 1) * 0.5f;

        float radiusX = mapWidth * 0.5f;
        float radiusY = mapHeight * 0.5f;

        float noiseOffsetX = RandomRange(0f, 1000f);
        float noiseOffsetY = RandomRange(0f, 1000f);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float nx = (x - (mapWidth - 1) * 0.5f) / radiusX;
                float ny = (y - (mapHeight - 1) * 0.5f) / radiusY;

                float distance = Mathf.Sqrt(nx * nx + ny * ny);

                float noise = Mathf.PerlinNoise(
                    noiseOffsetX + x * edgeFrequency,
                    noiseOffsetY + y * edgeFrequency);

                float edgeOffset = (noise - 0.5f) * 2f * edgeRoughness;

                if (distance <= 1f + edgeOffset)
                {
                    Vector3Int cell = new Vector3Int(
                        mapOrigin.x + x,
                        mapOrigin.y + y,
                        0);

                    groundTilemap.SetTile(cell, groundTile);
                }
            }
        }
    }

    /// <summary>
    /// 生成从地图左侧通往右侧的蜿蜒主路。
    /// </summary>
    private void GenerateMainRoad()
    {
        int currentY = mapOrigin.y + mapHeight / 2;
        int lastY = currentY;

        for (int x = 0; x < mapWidth; x++)
        {
            // 每隔一段距离随机调整道路方向，
            // 避免道路变成完全笔直的横线。
            if (x % 4 == 0)
            {
                int drift = rng.Next(-1, 2);
                currentY += drift;

                int limit = Mathf.Max(2, mainRoadWander);
                currentY = Mathf.Clamp(
                    currentY,
                    mapOrigin.y + mapHeight / 2 - limit,
                    mapOrigin.y + mapHeight / 2 + limit);
            }

            // 平滑过渡，减少道路突然折返。
            currentY = Mathf.RoundToInt(
                Mathf.Lerp(lastY, currentY, 0.7f));

            lastY = currentY;

            Vector2Int point = new Vector2Int(
                mapOrigin.x + x, currentY);

            mainRoad.Add(point);

            PaintRoad(point, mainRoadWidth);
        }
    }

    /// <summary>
    /// 从主路随机选择起点，向上或向下延伸支路。
    /// </summary>
    private void GenerateBranches()
    {
        if (mainRoad.Count == 0)
            return;

        for (int i = 0; i < branchCount; i++)
        {
            int minIndex = Mathf.Min(4, mainRoad.Count - 1);
            int maxIndexExclusive = Mathf.Max(
                minIndex + 1, mainRoad.Count - 3);
            maxIndexExclusive = Mathf.Clamp(
                maxIndexExclusive, minIndex + 1, mainRoad.Count);

            int startIndex = rng.Next(minIndex, maxIndexExclusive);

            Vector2Int current = mainRoad[startIndex];

            int direction = rng.Next(0, 2) == 0 ? -1 : 1;
            int length = rng.Next(
                branchMinLength,
                Mathf.Max(branchMinLength + 1, branchMaxLength + 1));

            // 先画起点，保证支路与主路连接。
            PaintRoad(current, branchRoadWidth);

            for (int step = 0; step < length; step++)
            {
                // 大多数时候向前延伸，偶尔横向偏移。
                int dx = rng.Next(0, 100) < 75
                    ? 0
                    : rng.Next(-1, 2);

                current.x += dx;
                current.y += direction;

                // 避免支路跑出地图边界。
                int minX = mapOrigin.x + 1;
                int maxX = mapOrigin.x + mapWidth - 2;
                int minY = mapOrigin.y + 1;
                int maxY = mapOrigin.y + mapHeight - 2;

                current.x = Mathf.Clamp(current.x, minX, maxX);
                current.y = Mathf.Clamp(current.y, minY, maxY);

                PaintRoad(current, branchRoadWidth);

                // 偶尔改变支路方向，形成自然弯曲。
                if (rng.Next(0, 100) < 12)
                    direction *= -1;
            }
        }
    }

    /// <summary>
    /// 在指定位置绘制有宽度的道路。
    /// 道路宽度为单元格数，支持奇数与偶数宽度。
    /// </summary>
    private void PaintRoad(Vector2Int center, int width)
    {
        width = Mathf.Max(1, width);

        int start = -(width - 1) / 2;
        int end = width / 2;

        for (int dx = start; dx <= end; dx++)
        {
            for (int dy = start; dy <= end; dy++)
            {
                // 用圆角形状减少道路边缘的方块感。
                if (dx * dx + dy * dy >
                    (width * width) * 0.8f)
                    continue;

                Vector3Int cell = new Vector3Int(
                    center.x + dx,
                    center.y + dy,
                    0);

                roadTilemap.SetTile(cell, roadTile);
            }
        }
    }

    private float RandomRange(float min, float max)
    {
        return (float)(rng.NextDouble() * (max - min) + min);
    }

    /// <summary>
    /// 清空生成器使用的地面和道路。
    /// 注意：如果地图层还存放手工绘制内容，也会一起清除。
    /// </summary>
    [ContextMenu("清空《雾钟》生成地图")]
    public void ClearGeneratedMap()
    {
        if (groundTilemap != null)
            groundTilemap.ClearAllTiles();

        if (roadTilemap != null && roadTilemap != groundTilemap)
            roadTilemap.ClearAllTiles();

        mainRoad.Clear();

        Debug.Log("《雾钟》地图层已清空。", this);
    }
}