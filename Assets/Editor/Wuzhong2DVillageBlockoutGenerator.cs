#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 《雾钟》2D 第一版灰盒场景生成器。
/// 菜单：Tools > Wuzhong > Generate 2D Village Blockout
/// 在当前 XY 平面创建背景、石板路、老宅/祠堂灰盒与灯笼标记。
/// 不修改摄像机、不删除主角，只替换本工具生成的根对象。
/// </summary>
public static class Wuzhong2DVillageBlockoutGenerator
{
    private const string RootName = "WuzhongVillage_Blockout";

    [MenuItem("Tools/Wuzhong/Generate 2D Village Blockout")]
    public static void Generate()
    {
        if (!EditorUtility.DisplayDialog(
            "生成《雾钟》村落灰盒",
            "将在当前场景创建 2D XY 平面灰盒：地面、石板路、老宅、祠堂、院墙、树木和红灯笼标记。\n\n不会删除主角、摄像机或其他场景物体；只替换上次生成的 " + RootName + "。继续吗？",
            "生成", "取消"))
            return;

        GameObject previous = GameObject.Find(RootName);
        if (previous != null) Undo.DestroyObjectImmediate(previous);

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Generate Wuzhong 2D Village Blockout");

        // Sorting layer 使用默认 Default，具体层级通过 Order in Layer 控制。
        MakeRect(root.transform, "Ground_Background", new Vector2(0, 0), new Vector2(36, 24),
            new Color(0.13f, 0.16f, 0.19f), -0.2f, -100);

        // 村中石板路：分块铺设，保持可读的路径方向。
        for (int i = 0; i < 11; i++)
        {
            float y = -9.5f + i * 1.15f;
            float x = (i % 2 == 0) ? -0.15f : 0.15f;
            MakeRect(root.transform, "StonePath_" + (i + 1).ToString("00"),
                new Vector2(x, y), new Vector2(3.8f, 1.0f),
                i % 2 == 0 ? new Color(0.35f, 0.37f, 0.38f) : new Color(0.30f, 0.33f, 0.35f),
                0f, -20);
        }

        // 路边石块和草丛色块。
        for (int i = 0; i < 26; i++)
        {
            float y = -10f + i * 0.78f;
            float side = i % 2 == 0 ? -1f : 1f;
            float x = side * (2.7f + (i % 3) * 0.35f);
            MakeRect(root.transform, "RoadEdgeStone_" + i.ToString("00"),
                new Vector2(x, y), new Vector2(0.35f, 0.22f),
                new Color(0.24f, 0.28f, 0.27f), 0.02f, -10);
        }

        // 左右两侧老宅：墙体与屋顶分层的 2D 色块。
        BuildHouse(root.transform, "OldHouse_Left", new Vector2(-7.3f, 2.0f), false);
        BuildHouse(root.transform, "OldHouse_Right", new Vector2(7.0f, 4.4f), true);

        // 远处祠堂体量更大，位于道路尽头。
        BuildShrine(root.transform, "AncestralHall", new Vector2(0.0f, 9.0f));

        // 院墙色块与木门。
        MakeRect(root.transform, "Wall_Left", new Vector2(-10.0f, 0.2f), new Vector2(0.35f, 16.5f),
            new Color(0.25f, 0.27f, 0.27f), 0.1f, -5);
        MakeRect(root.transform, "Wall_Right", new Vector2(10.0f, 0.2f), new Vector2(0.35f, 16.5f),
            new Color(0.25f, 0.27f, 0.27f), 0.1f, -5);

        // 简化古树：树干 + 圆形树冠。
        BuildTree(root.transform, "OldTree_Left", new Vector2(-5.4f, 6.4f));
        BuildTree(root.transform, "OldTree_Right", new Vector2(5.1f, -3.8f));
        BuildTree(root.transform, "OldTree_FarLeft", new Vector2(-8.2f, -5.6f));

        // 灯笼标记：用红色方块表示，后续可替换为精灵和点光源。
        MakeLantern(root.transform, "Lantern_Left", new Vector2(-3.0f, 1.8f));
        MakeLantern(root.transform, "Lantern_Right", new Vector2(3.0f, 4.6f));
        MakeLantern(root.transform, "Lantern_Hall_Left", new Vector2(-2.4f, 8.0f));
        MakeLantern(root.transform, "Lantern_Hall_Right", new Vector2(2.4f, 8.0f));

        // 地面上放一个不带物理的主角站位参考标记，方便调整地图布局。
        MakeRect(root.transform, "Player_Start_Marker", new Vector2(0, -7.0f), new Vector2(0.7f, 0.12f),
            new Color(0.8f, 0.75f, 0.45f, 0.8f), 0.03f, 5);

        Selection.activeGameObject = root;
        EditorSceneManagerMarkDirty();
        Debug.Log("《雾钟》2D 村落灰盒已生成。主角与摄像机未被修改。");
    }

    private static void BuildHouse(Transform root, string name, Vector2 p, bool flip)
    {
        float roofAngle = flip ? 0f : 0f;
        MakeRect(root, name + "_Shadow", p + new Vector2(0.25f, -0.2f), new Vector2(5.5f, 3.2f),
            new Color(0.07f, 0.09f, 0.11f, 0.7f), 0.05f, -2);
        MakeRect(root, name + "_Wall", p, new Vector2(4.8f, 2.7f),
            new Color(0.34f, 0.29f, 0.23f), 0.1f, 0);
        MakeRect(root, name + "_Roof", p + new Vector2(0, 1.65f), new Vector2(5.6f, 1.15f),
            new Color(0.19f, 0.20f, 0.21f), 0.15f, 2);
        MakeRect(root, name + "_Door", p + new Vector2(flip ? -0.7f : 0.7f, -0.45f), new Vector2(0.85f, 1.35f),
            new Color(0.12f, 0.08f, 0.07f), 0.2f, 4);
        MakeRect(root, name + "_Window", p + new Vector2(flip ? 1.1f : -1.1f, 0.1f), new Vector2(0.7f, 0.65f),
            new Color(0.12f, 0.16f, 0.17f), 0.2f, 4);
    }

    private static void BuildShrine(Transform root, string name, Vector2 p)
    {
        MakeRect(root, name + "_Foundation", p + new Vector2(0, -0.25f), new Vector2(8.5f, 1.0f),
            new Color(0.29f, 0.30f, 0.30f), 0.1f, -1);
        MakeRect(root, name + "_Wall", p + new Vector2(0, 1.0f), new Vector2(7.3f, 3.0f),
            new Color(0.38f, 0.31f, 0.25f), 0.15f, 0);
        MakeRect(root, name + "_Roof", p + new Vector2(0, 2.85f), new Vector2(8.7f, 1.2f),
            new Color(0.16f, 0.17f, 0.19f), 0.2f, 2);
        MakeRect(root, name + "_MainDoor", p + new Vector2(0, 0.45f), new Vector2(1.5f, 2.1f),
            new Color(0.10f, 0.06f, 0.05f), 0.25f, 4);
        MakeRect(root, name + "_Plaque", p + new Vector2(0, 1.75f), new Vector2(1.9f, 0.42f),
            new Color(0.48f, 0.12f, 0.10f), 0.3f, 5);
    }

    private static void BuildTree(Transform root, string name, Vector2 p)
    {
        MakeRect(root, name + "_Trunk", p, new Vector2(0.55f, 1.8f),
            new Color(0.23f, 0.17f, 0.13f), 0.12f, 1);
        GameObject canopy = new GameObject(name + "_Canopy");
        canopy.transform.SetParent(root, false);
        canopy.transform.position = new Vector3(p.x, p.y + 1.3f, 0.13f);
        canopy.transform.localScale = new Vector3(2.6f, 2.2f, 1f);
        SpriteRenderer sr = canopy.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = new Color(0.12f, 0.20f, 0.18f);
        sr.sortingOrder = 3;
    }

    private static void MakeLantern(Transform root, string name, Vector2 p)
    {
        MakeRect(root, name + "_GlowHint", p, new Vector2(0.85f, 0.85f),
            new Color(0.55f, 0.16f, 0.10f, 0.18f), 0.18f, 6);
        MakeRect(root, name, p, new Vector2(0.38f, 0.58f),
            new Color(0.75f, 0.12f, 0.08f), 0.2f, 7);
        MakeRect(root, name + "_Cap", p + new Vector2(0, 0.34f), new Vector2(0.44f, 0.08f),
            new Color(0.72f, 0.55f, 0.25f), 0.21f, 8);
    }

    private static GameObject MakeRect(Transform parent, string name, Vector2 position, Vector2 size,
        Color color, float z, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(position.x, position.y, z);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSquareSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    private static Sprite squareSprite;
    private static Sprite circleSprite;

    private static Sprite GetSquareSprite()
    {
        if (squareSprite != null) return squareSprite;
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        squareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return squareSprite;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null) return circleSprite;
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f) / size * 2f - 1f;
            float dy = (y + 0.5f) / size * 2f - 1f;
            tex.SetPixel(x, y, dx * dx + dy * dy <= 1f ? Color.white : Color.clear);
        }
        tex.Apply();
        circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }

    private static void EditorSceneManagerMarkDirty()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
#endif
