using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Organizes screen-space UI objects under a single UI root in the currently open scene.
/// World-space/player-attached prompts are intentionally left where they are.
/// </summary>
public static class WuzhongUIOrganizer
{
    [MenuItem("Tools/Wu Zhong/Organize UI in Current Scene")]
    public static void OrganizeUI()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Wu Zhong UI", "Please open your game scene first.", "OK");
            return;
        }

        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UI");
            Undo.RegisterCreatedObjectUndo(uiRoot, "Create UI Root");
            SceneManager.MoveGameObjectToScene(uiRoot, scene);
        }

        int moved = 0;
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            if (rootObject == uiRoot)
                continue;

            bool isCanvas = rootObject.GetComponent<Canvas>() != null;
            bool isDialogueManager = rootObject.GetComponent<DialogueManager>() != null;

            // Only move screen-space Canvas and DialogueManager. Keep the player-attached
            // interaction prompt and world-space UI in place so their transforms do not break.
            if (isCanvas || isDialogueManager)
            {
                Undo.SetTransformParent(rootObject.transform, uiRoot.transform, "Organize UI");
                moved++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = uiRoot;
        EditorUtility.DisplayDialog(
            "Wu Zhong UI",
            "Created/found the UI root and moved " + moved +
            " screen UI object(s) under it. Check the Hierarchy, then save the scene (Ctrl+S).",
            "OK");
    }
}
