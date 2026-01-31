#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using BlockPuzzle.UI;
using BlockPuzzle.Core;

namespace BlockPuzzle.Editor
{
    /// <summary>
    /// Editor tools to fix broken scene references and remove missing scripts.
    /// </summary>
    public static class FixGameSceneReferences
    {
        private const string GAME_SCENE_PATH = "Assets/_Game/Scenes/Game.unity";

        [MenuItem("Tools/Setup/Fix Game Scene References")]
        public static void FixReferences()
        {
            // Open the Game scene
            var scene = EditorSceneManager.OpenScene(GAME_SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[FixGameSceneReferences] Failed to open scene: {GAME_SCENE_PATH}");
                return;
            }

            Debug.Log("[FixGameSceneReferences] Scanning scene for broken references...");

            // Find SimpleGridView
            var gridView = Object.FindObjectOfType<SimpleGridView>();
            if (gridView == null)
            {
                Debug.LogError("[FixGameSceneReferences] SimpleGridView not found in scene!");
                return;
            }

            Debug.Log($"[FixGameSceneReferences] Found SimpleGridView on: {gridView.gameObject.name}");

            // Find SimpleGameController
            var gameController = Object.FindObjectOfType<SimpleGameController>();
            if (gameController == null)
            {
                Debug.LogWarning("[FixGameSceneReferences] SimpleGameController not found in scene!");
            }

            // Find piece slots
            PieceTraySlot[] slots = FindPieceSlots();
            if (slots == null || slots.Length == 0)
            {
                Debug.LogError("[FixGameSceneReferences] No PieceTraySlots found in scene!");
                return;
            }

            Debug.Log($"[FixGameSceneReferences] Found {slots.Length} PieceTraySlots");

            // Find grid container (the RectTransform of the GridView itself or a child named GridContainer)
            RectTransform gridContainer = FindGridContainer(gridView);

            // Find cell prefab
            GameObject cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/GridCell.prefab");

            // Wire up SimpleGridView using SerializedObject
            var so = new SerializedObject(gridView);

            // Set _pieceSlots array
            var pieceSlotsProperty = so.FindProperty("_pieceSlots");
            if (pieceSlotsProperty != null)
            {
                pieceSlotsProperty.arraySize = slots.Length;
                for (int i = 0; i < slots.Length; i++)
                {
                    pieceSlotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
                    Debug.Log($"[FixGameSceneReferences] Assigned slot {i}: {slots[i].gameObject.name}");
                }
            }
            else
            {
                Debug.LogError("[FixGameSceneReferences] Could not find _pieceSlots property!");
            }

            // Set _gridContainer
            var gridContainerProperty = so.FindProperty("_gridContainer");
            if (gridContainerProperty != null && gridContainer != null)
            {
                gridContainerProperty.objectReferenceValue = gridContainer;
                Debug.Log($"[FixGameSceneReferences] Assigned _gridContainer: {gridContainer.name}");
            }

            // Set _cellPrefab
            var cellPrefabProperty = so.FindProperty("_cellPrefab");
            if (cellPrefabProperty != null && cellPrefab != null)
            {
                cellPrefabProperty.objectReferenceValue = cellPrefab;
                Debug.Log($"[FixGameSceneReferences] Assigned _cellPrefab: {cellPrefab.name}");
            }

            // Set _gameController
            var gameControllerProperty = so.FindProperty("_gameController");
            if (gameControllerProperty != null && gameController != null)
            {
                gameControllerProperty.objectReferenceValue = gameController;
                Debug.Log($"[FixGameSceneReferences] Assigned _gameController: {gameController.name}");
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            // Also fix the DraggablePiece references on each slot
            FixDraggablePieceReferences(slots);

            // Save scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[FixGameSceneReferences] Scene references fixed and saved!");
            Debug.Log("  - PieceSlots wired to SimpleGridView");
            Debug.Log("  - GridContainer assigned");
            Debug.Log("  - CellPrefab assigned");
            Debug.Log("  - GameController assigned");
        }

        private static PieceTraySlot[] FindPieceSlots()
        {
            PieceTraySlot[] slots = new PieceTraySlot[3];

            // Try to find by path first
            for (int i = 0; i < 3; i++)
            {
                string slotName = $"PieceSlot_{i}";

                // Try path: GridCanvas/PieceTray/PieceSlot_X
                var gridCanvas = GameObject.Find("GridCanvas");
                if (gridCanvas != null)
                {
                    var pieceTray = gridCanvas.transform.Find("PieceTray");
                    if (pieceTray != null)
                    {
                        var slotTransform = pieceTray.Find(slotName);
                        if (slotTransform != null)
                        {
                            var slot = slotTransform.GetComponent<PieceTraySlot>();
                            if (slot != null)
                            {
                                slots[i] = slot;
                                continue;
                            }
                        }
                    }
                }

                // Fallback: search all objects
                var allSlots = Object.FindObjectsOfType<PieceTraySlot>(true);
                foreach (var s in allSlots)
                {
                    if (s.gameObject.name == slotName)
                    {
                        slots[i] = s;
                        break;
                    }
                }

                // Last resort: find by index in any PieceTray
                if (slots[i] == null && gridCanvas != null)
                {
                    var pieceTray = gridCanvas.transform.Find("PieceTray");
                    if (pieceTray != null && pieceTray.childCount > i)
                    {
                        var slot = pieceTray.GetChild(i).GetComponent<PieceTraySlot>();
                        if (slot != null)
                        {
                            slots[i] = slot;
                        }
                    }
                }
            }

            // Verify we found all slots
            int found = 0;
            foreach (var s in slots)
            {
                if (s != null) found++;
            }

            if (found < 3)
            {
                Debug.LogWarning($"[FixGameSceneReferences] Only found {found}/3 PieceTraySlots");
            }

            return slots;
        }

        private static RectTransform FindGridContainer(SimpleGridView gridView)
        {
            // The GridView's own RectTransform is the grid container
            var rect = gridView.GetComponent<RectTransform>();
            if (rect != null)
            {
                return rect;
            }

            // Fallback: look for a child named GridContainer
            var child = gridView.transform.Find("GridContainer");
            if (child != null)
            {
                return child.GetComponent<RectTransform>();
            }

            return null;
        }

        private static void FixDraggablePieceReferences(PieceTraySlot[] slots)
        {
            foreach (var slot in slots)
            {
                if (slot == null) continue;

                var draggable = slot.GetComponent<DraggablePiece>();
                if (draggable == null)
                {
                    Debug.LogWarning($"[FixGameSceneReferences] No DraggablePiece on {slot.name}, adding one");
                    draggable = slot.gameObject.AddComponent<DraggablePiece>();
                }

                // Find the PieceContainer child
                var pieceContainer = slot.transform.Find("PieceContainer");
                if (pieceContainer == null)
                {
                    Debug.LogWarning($"[FixGameSceneReferences] No PieceContainer found under {slot.name}");
                    continue;
                }

                var containerRect = pieceContainer.GetComponent<RectTransform>();

                // Wire DraggablePiece
                var draggableSO = new SerializedObject(draggable);
                var containerProperty = draggableSO.FindProperty("_pieceContainer");
                if (containerProperty != null)
                {
                    containerProperty.objectReferenceValue = containerRect;
                }
                draggableSO.ApplyModifiedPropertiesWithoutUndo();

                // Also wire PieceTraySlot's container reference
                var slotSO = new SerializedObject(slot);
                var slotContainerProperty = slotSO.FindProperty("_container");
                if (slotContainerProperty != null)
                {
                    slotContainerProperty.objectReferenceValue = containerRect;
                }

                // Wire cell prefab for slot
                var miniCellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/MiniCell.prefab");
                var cellPrefabProperty = slotSO.FindProperty("_cellPrefab");
                if (cellPrefabProperty != null && miniCellPrefab != null)
                {
                    cellPrefabProperty.objectReferenceValue = miniCellPrefab;
                }

                slotSO.ApplyModifiedPropertiesWithoutUndo();

                Debug.Log($"[FixGameSceneReferences] Fixed references for {slot.name}");
            }
        }

        [MenuItem("Tools/Setup/Remove Missing Scripts (Scene)")]
        public static void RemoveMissingScripts()
        {
            int totalRemoved = 0;
            var allObjects = Object.FindObjectsOfType<GameObject>(true);

            foreach (var go in allObjects)
            {
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (removed > 0)
                {
                    Debug.Log($"[RemoveMissingScripts] Removed {removed} missing script(s) from: {go.name}");
                    totalRemoved += removed;
                    EditorUtility.SetDirty(go);
                }
            }

            if (totalRemoved > 0)
            {
                var scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[RemoveMissingScripts] Total removed: {totalRemoved}. Scene saved.");
            }
            else
            {
                Debug.Log("[RemoveMissingScripts] No missing scripts found in scene.");
            }
        }

        [MenuItem("Tools/Setup/Validate Game Scene")]
        public static void ValidateGameScene()
        {
            Debug.Log("[ValidateGameScene] Running validation...");

            // Check SimpleGridView
            var gridView = Object.FindObjectOfType<SimpleGridView>();
            if (gridView == null)
            {
                Debug.LogError("[ValidateGameScene] FAIL: SimpleGridView not found");
                return;
            }
            Debug.Log("[ValidateGameScene] OK: SimpleGridView found");

            // Check slots via serialized object
            var so = new SerializedObject(gridView);
            var slotsProperty = so.FindProperty("_pieceSlots");
            if (slotsProperty == null || slotsProperty.arraySize == 0)
            {
                Debug.LogError("[ValidateGameScene] FAIL: _pieceSlots not set");
            }
            else
            {
                int validSlots = 0;
                for (int i = 0; i < slotsProperty.arraySize; i++)
                {
                    if (slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    {
                        validSlots++;
                    }
                }
                if (validSlots == slotsProperty.arraySize)
                {
                    Debug.Log($"[ValidateGameScene] OK: All {validSlots} piece slots assigned");
                }
                else
                {
                    Debug.LogError($"[ValidateGameScene] FAIL: Only {validSlots}/{slotsProperty.arraySize} slots assigned");
                }
            }

            // Check grid container
            var gridContainerProperty = so.FindProperty("_gridContainer");
            if (gridContainerProperty == null || gridContainerProperty.objectReferenceValue == null)
            {
                Debug.LogError("[ValidateGameScene] FAIL: _gridContainer not set");
            }
            else
            {
                Debug.Log("[ValidateGameScene] OK: GridContainer assigned");
            }

            // Check cell prefab
            var cellPrefabProperty = so.FindProperty("_cellPrefab");
            if (cellPrefabProperty == null || cellPrefabProperty.objectReferenceValue == null)
            {
                Debug.LogError("[ValidateGameScene] FAIL: _cellPrefab not set");
            }
            else
            {
                Debug.Log("[ValidateGameScene] OK: CellPrefab assigned");
            }

            // Check game controller
            var gameControllerProperty = so.FindProperty("_gameController");
            if (gameControllerProperty == null || gameControllerProperty.objectReferenceValue == null)
            {
                Debug.LogWarning("[ValidateGameScene] WARN: _gameController not set (will use FindObjectOfType fallback)");
            }
            else
            {
                Debug.Log("[ValidateGameScene] OK: GameController assigned");
            }

            // Check for missing scripts
            int missingScripts = 0;
            var allObjects = Object.FindObjectsOfType<GameObject>(true);
            foreach (var go in allObjects)
            {
                var components = go.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c == null)
                    {
                        missingScripts++;
                        Debug.LogWarning($"[ValidateGameScene] Missing script on: {go.name}");
                    }
                }
            }

            if (missingScripts == 0)
            {
                Debug.Log("[ValidateGameScene] OK: No missing scripts");
            }
            else
            {
                Debug.LogError($"[ValidateGameScene] FAIL: {missingScripts} missing script(s) found. Run 'Remove Missing Scripts (Scene)'");
            }

            Debug.Log("[ValidateGameScene] Validation complete.");
        }
    }
}
#endif
