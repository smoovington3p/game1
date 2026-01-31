#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using BlockPuzzle.Core;
using BlockPuzzle.UI;

namespace BlockPuzzle.Editor
{
    public static class BuildGameScene
    {
        private const string GAME_SCENE_PATH = "Assets/_Game/Scenes/Game.unity";

        [MenuItem("Tools/Setup/Build Game Scene")]
        public static void BuildScene()
        {
            // Open the Game scene
            var scene = EditorSceneManager.OpenScene(GAME_SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[BuildGameScene] Failed to open scene: {GAME_SCENE_PATH}");
                return;
            }

            // 1. Ensure Camera exists
            SetupGameCamera.EnsureGameCamera();

            // 2. Ensure EventSystem exists for UI input
            EnsureEventSystem();

            // 3. Create GameController
            var gameController = CreateGameController();

            // 4. Create Grid Visual
            var gridView = CreateGridView(gameController);

            // 5. Create UI Canvas
            CreateUICanvas(gameController);

            // 6. Wire up references
            WireReferences(gameController, gridView);

            // Save scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[BuildGameScene] Game scene built successfully!");
            Debug.Log("  - EventSystem for UI input");
            Debug.Log("  - GameController with SimpleGameController");
            Debug.Log("  - GridContainer with SimpleGridView");
            Debug.Log("  - UI Canvas with Score, Back, New Game buttons");
            Debug.Log("  - Cell prefab for grid rendering");
        }

        private static void EnsureEventSystem()
        {
            // Find existing EventSystem
            var existing = Object.FindObjectOfType<EventSystem>();
            if (existing != null)
            {
                Debug.Log("[BuildGameScene] EventSystem already exists");
                return;
            }

            // Create EventSystem
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();

            // Add StandaloneInputModule (works with both old and new input system in compatibility mode)
            eventSystemGO.AddComponent<StandaloneInputModule>();

            Debug.Log("[BuildGameScene] Created EventSystem with StandaloneInputModule");
        }

        private static SimpleGameController CreateGameController()
        {
            // Find or create GameController
            var existing = Object.FindObjectOfType<SimpleGameController>();
            if (existing != null)
            {
                Debug.Log("[BuildGameScene] GameController already exists");
                return existing;
            }

            var go = new GameObject("GameController");
            var controller = go.AddComponent<SimpleGameController>();
            Debug.Log("[BuildGameScene] Created GameController");
            return controller;
        }

        private static SimpleGridView CreateGridView(SimpleGameController gameController)
        {
            // Find or create GridView
            var existing = Object.FindObjectOfType<SimpleGridView>();
            if (existing != null)
            {
                Debug.Log("[BuildGameScene] GridView already exists");
                return existing;
            }

            // Create GridContainer as child of a Canvas for UI-based rendering
            var gridCanvas = new GameObject("GridCanvas");
            var canvas = gridCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            gridCanvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gridCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            gridCanvas.AddComponent<GraphicRaycaster>();

            // Grid container - sized for 9x9 grid with 80px cells + 4px spacing
            // Total: 9 * (80 + 4) - 4 = 752px
            var gridContainer = new GameObject("GridContainer");
            gridContainer.transform.SetParent(gridCanvas.transform, false);
            var gridRect = gridContainer.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.pivot = new Vector2(0.5f, 0.5f);
            gridRect.anchoredPosition = new Vector2(0, 150);
            gridRect.sizeDelta = new Vector2(760, 760);

            // Add SimpleGridView
            var gridView = gridContainer.AddComponent<SimpleGridView>();

            // Create cell prefab
            var cellPrefab = CreateCellPrefab();

            // Create piece tray
            var pieceTray = CreatePieceTray(gridCanvas.transform);

            // Set serialized fields via SerializedObject
            var so = new SerializedObject(gridView);
            so.FindProperty("_gridContainer").objectReferenceValue = gridRect;
            so.FindProperty("_cellPrefab").objectReferenceValue = cellPrefab;
            so.FindProperty("_cellSize").floatValue = 80f;
            so.FindProperty("_cellSpacing").floatValue = 4f;
            so.FindProperty("_gameController").objectReferenceValue = gameController;
            so.FindProperty("_pieceSlots").arraySize = 3;

            // Wire piece slots - find them by name for reliability
            var slotsProperty = so.FindProperty("_pieceSlots");
            slotsProperty.arraySize = 3;

            for (int i = 0; i < 3; i++)
            {
                string slotName = $"PieceSlot_{i}";
                Transform slotTransform = pieceTray.transform.Find(slotName);
                if (slotTransform != null)
                {
                    var slot = slotTransform.GetComponent<PieceTraySlot>();
                    if (slot != null)
                    {
                        slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = slot;
                        Debug.Log($"[BuildGameScene] Wired slot {i}: {slot.gameObject.name}");
                    }
                    else
                    {
                        Debug.LogError($"[BuildGameScene] PieceTraySlot component missing on {slotName}");
                    }
                }
                else
                {
                    Debug.LogError($"[BuildGameScene] Could not find {slotName} under PieceTray");
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            // Verify wiring
            int wiredCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    wiredCount++;
            }
            Debug.Log($"[BuildGameScene] Created GridView - {wiredCount}/3 slots wired");

            return gridView;
        }

        private static GameObject CreateCellPrefab()
        {
            // Check if prefab already exists
            string prefabPath = "Assets/_Game/Prefabs/GridCell.prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            // Create cell prefab
            var cell = new GameObject("GridCell");
            var cellRect = cell.AddComponent<RectTransform>();
            cellRect.anchorMin = new Vector2(0.5f, 0.5f);
            cellRect.anchorMax = new Vector2(0.5f, 0.5f);
            cellRect.pivot = new Vector2(0.5f, 0.5f);
            cellRect.sizeDelta = new Vector2(80, 80);

            var image = cell.AddComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.25f);
            image.raycastTarget = true; // Enable for click detection

            // Save as prefab
            EnsureDirectoryExists("Assets/_Game/Prefabs");
            var prefab = PrefabUtility.SaveAsPrefabAsset(cell, prefabPath);
            Object.DestroyImmediate(cell);

            Debug.Log("[BuildGameScene] Created GridCell prefab");
            return prefab;
        }

        private static GameObject CreatePieceTray(Transform parent)
        {
            var tray = new GameObject("PieceTray");
            tray.transform.SetParent(parent, false);
            var trayRect = tray.AddComponent<RectTransform>();
            trayRect.anchorMin = new Vector2(0.5f, 0f);
            trayRect.anchorMax = new Vector2(0.5f, 0f);
            trayRect.pivot = new Vector2(0.5f, 0.5f);
            trayRect.anchoredPosition = new Vector2(0, 200);
            trayRect.sizeDelta = new Vector2(700, 160);

            var layout = tray.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Create 3 piece slots
            for (int i = 0; i < 3; i++)
            {
                CreatePieceSlot(tray.transform, i);
            }

            Debug.Log("[BuildGameScene] Created PieceTray with 3 slots");
            return tray;
        }

        private static PieceTraySlot CreatePieceSlot(Transform parent, int index)
        {
            var slot = new GameObject($"PieceSlot_{index}");
            slot.transform.SetParent(parent, false);
            var slotRect = slot.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(140, 140);

            // Background
            var bg = slot.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

            // Selection frame
            var frame = new GameObject("SelectionFrame");
            frame.transform.SetParent(slot.transform, false);
            var frameRect = frame.AddComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(-4, -4);
            frameRect.offsetMax = new Vector2(4, 4);
            var frameImage = frame.AddComponent<Image>();
            frameImage.color = new Color(1f, 0.8f, 0.2f);
            frameImage.enabled = false;

            // Make frame outline only
            var outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.8f, 0.2f);
            outline.effectDistance = new Vector2(2, 2);

            // Piece container
            var container = new GameObject("PieceContainer");
            container.transform.SetParent(slot.transform, false);
            var containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(120, 120);

            // Add PieceTraySlot component
            var slotComponent = slot.AddComponent<PieceTraySlot>();

            // Wire references
            var so = new SerializedObject(slotComponent);
            so.FindProperty("_container").objectReferenceValue = containerRect;
            so.FindProperty("_selectionFrame").objectReferenceValue = frameImage;
            so.FindProperty("_cellSize").floatValue = 24f;

            // Create mini cell prefab for piece display
            var miniCellPrefab = CreateMiniCellPrefab();
            so.FindProperty("_cellPrefab").objectReferenceValue = miniCellPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Add DraggablePiece for drag-and-drop support
            var draggable = slot.AddComponent<DraggablePiece>();
            var draggableSO = new SerializedObject(draggable);
            draggableSO.FindProperty("_pieceContainer").objectReferenceValue = containerRect;
            draggableSO.FindProperty("_dragScale").floatValue = 1.5f;
            draggableSO.FindProperty("_dragOffsetY").floatValue = 100f;
            draggableSO.ApplyModifiedPropertiesWithoutUndo();

            // Add button for click detection (backup for non-drag clicks)
            var button = slot.AddComponent<Button>();
            button.onClick.AddListener(() => slotComponent.OnClick());

            return slotComponent;
        }

        private static GameObject CreateMiniCellPrefab()
        {
            string prefabPath = "Assets/_Game/Prefabs/MiniCell.prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            var cell = new GameObject("MiniCell");
            var cellRect = cell.AddComponent<RectTransform>();
            cellRect.anchorMin = new Vector2(0.5f, 0.5f);
            cellRect.anchorMax = new Vector2(0.5f, 0.5f);
            cellRect.pivot = new Vector2(0.5f, 0.5f);
            cellRect.sizeDelta = new Vector2(24, 24);

            var image = cell.AddComponent<Image>();
            image.color = new Color(0.3f, 0.5f, 0.9f);
            image.raycastTarget = false; // Disable so clicks pass through to slot

            EnsureDirectoryExists("Assets/_Game/Prefabs");
            var prefab = PrefabUtility.SaveAsPrefabAsset(cell, prefabPath);
            Object.DestroyImmediate(cell);

            return prefab;
        }

        private static void CreateUICanvas(SimpleGameController gameController)
        {
            // Check if UI Canvas already exists
            var existingUI = Object.FindObjectOfType<SimpleGameUI>();
            if (existingUI != null)
            {
                Debug.Log("[BuildGameScene] UI Canvas already exists");
                return;
            }

            // Create UI Canvas
            var uiCanvas = new GameObject("UICanvas");
            var canvas = uiCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = uiCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            uiCanvas.AddComponent<GraphicRaycaster>();

            // Score Text (top)
            var scoreText = CreateTextElement(uiCanvas.transform, "ScoreText", "Score: 0",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(300, 60));
            scoreText.fontSize = 48;
            scoreText.alignment = TextAlignmentOptions.Center;

            // Combo Text (below score)
            var comboText = CreateTextElement(uiCanvas.transform, "ComboText", "",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -110), new Vector2(200, 40));
            comboText.fontSize = 32;
            comboText.alignment = TextAlignmentOptions.Center;
            comboText.color = new Color(1f, 0.8f, 0.2f);

            // Back Button (top left)
            var backButton = CreateButton(uiCanvas.transform, "BackButton", "← Back",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100, -50), new Vector2(150, 50));

            // New Game Button (top right)
            var newGameButton = CreateButton(uiCanvas.transform, "NewGameButton", "New Game",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-100, -50), new Vector2(180, 50));

            // Add SimpleGameUI and wire references
            var gameUI = uiCanvas.AddComponent<SimpleGameUI>();
            var so = new SerializedObject(gameUI);
            so.FindProperty("_scoreText").objectReferenceValue = scoreText;
            so.FindProperty("_comboText").objectReferenceValue = comboText;
            so.FindProperty("_backButton").objectReferenceValue = backButton;
            so.FindProperty("_newGameButton").objectReferenceValue = newGameButton;
            so.FindProperty("_gameController").objectReferenceValue = gameController;
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("[BuildGameScene] Created UI Canvas with Score, Back, New Game");
        }

        private static TextMeshProUGUI CreateTextElement(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 36;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f);

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
            button.colors = colors;

            // Add text child
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return button;
        }

        private static void WireReferences(SimpleGameController controller, SimpleGridView gridView)
        {
            var so = new SerializedObject(controller);
            so.FindProperty("_gridView").objectReferenceValue = gridView;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[BuildGameScene] Wired controller references");
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = System.IO.Path.GetDirectoryName(path);
                var folderName = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
#endif
