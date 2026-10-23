using System;
using UnityEngine;

/// <summary>
/// 一句对话的数据结构，可在 Unity Inspector 中配置。
/// Serializable 让该类型能够作为序列化字段显示在编辑器中。
/// </summary>
[Serializable]
public class DialogueLine
{
    // 当前发言者的名字。
    public string speakerName;

    // 对话正文；TextArea 让 Inspector 使用多行文本输入框。
    [TextArea(2, 5)] public string text;
}
