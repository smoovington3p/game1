using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockPuzzle.Core
{
    /// <summary>
    /// Single entry point that loads GameConfig and persists core managers.
    /// Attach to a GameObject in MainMenu scene.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        private const string CONFIG_PATH = "GameConfig_Default";

        [SerializeField] private GameConfig _gameConfigOverride;

        private static bool _initialized = false;

        private void Awake()
        {
            if (_initialized)
            {
                // Already bootstrapped from a previous scene load
                Destroy(gameObject);
                return;
            }

            Initialize();
        }

        private void Initialize()
        {
            _initialized = true;
            DontDestroyOnLoad(gameObject);

            // Load or create GameConfig
            GameConfig config = LoadGameConfig();

            // Initialize GameManager
            InitializeGameManager(config);

            Debug.Log("[Bootstrap] Initialization complete");
        }

        private GameConfig LoadGameConfig()
        {
            // Priority 1: Inspector override
            if (_gameConfigOverride != null)
            {
                Debug.Log("[Bootstrap] Using inspector-assigned GameConfig");
                return _gameConfigOverride;
            }

            // Priority 2: Load from Resources
            GameConfig config = Resources.Load<GameConfig>(CONFIG_PATH);
            if (config != null)
            {
                Debug.Log($"[Bootstrap] Loaded GameConfig from Resources/{CONFIG_PATH}");
                return config;
            }

            // Priority 3: Create default config at runtime (fallback)
            Debug.LogWarning($"[Bootstrap] GameConfig not found at Resources/{CONFIG_PATH}, creating runtime default");
            config = ScriptableObject.CreateInstance<GameConfig>();
            return config;
        }

        private void InitializeGameManager(GameConfig config)
        {
            // Check if GameManager already exists
            if (GameManager.Instance != null)
            {
                return;
            }

            // Create GameManager on this persistent object
            var gameManager = gameObject.AddComponent<GameManager>();
            gameManager.SetConfig(config);
        }

        /// <summary>
        /// Can be called to re-bootstrap if needed (e.g., after config change).
        /// </summary>
        public static void Reset()
        {
            _initialized = false;
        }
    }
}
