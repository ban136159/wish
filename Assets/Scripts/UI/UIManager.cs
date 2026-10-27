using UnityEngine;
using TMPro;

/// <summary>
/// 《雾钟》统一 UI 管理器。
/// 集中控制交互提示的显示与隐藏，后续可扩展其他 UI 面板。
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("交互提示 UI")]
    [SerializeField] private GameObject interactionPromptPanel;
    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中存在多个 UIManager。", this);
            enabled = false;
            return;
        }

        Instance = this;
        HideInteractionPrompt();
    }

    public void ShowInteractionPrompt(string message)
    {
        if (interactionPromptPanel != null)
            interactionPromptPanel.SetActive(true);

        if (promptText != null)
            promptText.text = message;
    }

    public void HideInteractionPrompt()
    {
        if (interactionPromptPanel != null)
            interactionPromptPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
