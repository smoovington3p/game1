#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BlockPuzzle.Editor
{
    public static class SetupGameCamera
    {
        private const string GAME_SCENE_PATH = "Assets/_Game/Scenes/Game.unity";
        private const string CAMERA_NAME = "Main Camera";

        [MenuItem("Tools/Setup/Ensure Game Camera")]
        public static void EnsureGameCamera()
        {
            // Open the Game scene
            var scene = EditorSceneManager.OpenScene(GAME_SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[SetupGameCamera] Failed to open scene: {GAME_SCENE_PATH}");
                return;
            }

            // Find existing camera or create new one
            Camera mainCamera = null;
            Camera[] allCameras = Object.FindObjectsOfType<Camera>(true); // include inactive

            // Look for existing MainCamera
            foreach (var cam in allCameras)
            {
                if (cam.CompareTag("MainCamera") || cam.gameObject.name == CAMERA_NAME)
                {
                    mainCamera = cam;
                    break;
                }
            }

            // If no camera found, check for any camera
            if (mainCamera == null && allCameras.Length > 0)
            {
                mainCamera = allCameras[0];
            }

            // Create camera if none exists
            if (mainCamera == null)
            {
                var cameraObj = new GameObject(CAMERA_NAME);
                mainCamera = cameraObj.AddComponent<Camera>();
                mainCamera.gameObject.AddComponent<AudioListener>();
                Debug.Log("[SetupGameCamera] Created new Main Camera");
            }

            // Configure camera
            mainCamera.gameObject.name = CAMERA_NAME;
            mainCamera.gameObject.tag = "MainCamera";
            mainCamera.gameObject.SetActive(true);
            mainCamera.enabled = true;

            // Camera settings
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
            mainCamera.nearClipPlane = 0.3f;
            mainCamera.farClipPlane = 1000f;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            mainCamera.depth = -1;

            // Position
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            mainCamera.transform.rotation = Quaternion.identity;
            mainCamera.transform.localScale = Vector3.one;

            // Ensure only one AudioListener
            AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>(true);
            bool hasListener = false;
            foreach (var listener in listeners)
            {
                if (listener.gameObject == mainCamera.gameObject)
                {
                    hasListener = true;
                }
                else
                {
                    Object.DestroyImmediate(listener);
                }
            }
            if (!hasListener)
            {
                mainCamera.gameObject.AddComponent<AudioListener>();
            }

            // Mark scene dirty and save
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[SetupGameCamera] Camera '{CAMERA_NAME}' configured successfully in {GAME_SCENE_PATH}");
            Debug.Log($"  - Tag: {mainCamera.tag}");
            Debug.Log($"  - Orthographic: {mainCamera.orthographic}");
            Debug.Log($"  - Size: {mainCamera.orthographicSize}");
            Debug.Log($"  - Position: {mainCamera.transform.position}");
            Debug.Log($"  - Enabled: {mainCamera.enabled}");
        }

        // Auto-run on domain reload if scene needs fixing (optional)
        [InitializeOnLoadMethod]
        private static void CheckOnLoad()
        {
            // Don't auto-run, just register the menu item
            // User can manually run via Tools > Setup > Ensure Game Camera
        }
    }
}
#endif
