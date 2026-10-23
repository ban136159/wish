using UnityEngine;

/// <summary>
/// 简单的全局游戏管理器。
/// Instance 提供全局访问入口；对象在切换场景时不会被销毁。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // 如果场景中已经存在另一个实例，销毁重复对象。
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        // 保存唯一实例，并使其在场景切换时继续存在。
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
