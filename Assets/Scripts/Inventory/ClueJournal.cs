using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责保存当前运行期间收集到的线索，并在新增线索时通知其他系统。
/// 当前实现保存在内存中，尚未包含磁盘存档。
/// </summary>
public class ClueJournal : MonoBehaviour
{
    /// <summary>单条线索的数据：唯一 ID、标题和描述。</summary>
    [Serializable]
    public class ClueEntry
    {
        public string id;
        public string title;
        [TextArea(2, 5)] public string description;
    }

    // 序列化列表会显示在 Inspector 中；新线索通过 AddClue 添加。
    [SerializeField] private List<ClueEntry> clues = new List<ClueEntry>();

    // 对外只提供只读列表接口，避免其他脚本直接修改内部集合。
    public IReadOnlyList<ClueEntry> Clues => clues;

    // 成功新增线索后触发，可用于更新日志 UI 或任务状态。
    public event Action<ClueEntry> ClueAdded;

    /// <summary>
    /// 添加一条线索。ID 为空或已存在时返回 false；成功添加时返回 true。
    /// </summary>
    public bool AddClue(string id, string title, string description)
    {
        if (string.IsNullOrWhiteSpace(id) || clues.Exists(c => c.id == id)) return false;

        ClueEntry entry = new ClueEntry { id = id, title = title, description = description };
        clues.Add(entry);
        ClueAdded?.Invoke(entry);
        return true;
    }

    /// <summary>判断指定 ID 的线索是否已经收集。</summary>
    public bool HasClue(string id) => clues.Exists(c => c.id == id);
}
