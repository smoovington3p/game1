using System.Collections.Generic;
using UnityEngine;
using BlockPuzzle.Grid;
using BlockPuzzle.Pieces;
using BlockPuzzle.UI;

namespace BlockPuzzle.Core
{
    /// <summary>
    /// Simplified game controller for MVP demonstration.
    /// Handles piece generation, placement, and game over detection.
    /// </summary>
    public class SimpleGameController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimpleGridView _gridView;

        private GridData _grid;
        private PieceGenerator _pieceGenerator;
        private List<PieceData> _currentPieces;
        private int _piecesUsedThisSet;
        private bool _isGameActive;

        public GridData Grid => _grid;
        public List<PieceData> CurrentPieces => _currentPieces;
        public bool IsGameActive => _isGameActive;

        private void Awake()
        {
            // Ensure GameManager exists (fallback for direct scene launch)
            EnsureGameManager();
        }

        private void Start()
        {
            StartNewGame();
        }

        private void EnsureGameManager()
        {
            if (GameManager.Instance != null) return;

            Debug.Log("[SimpleGameController] GameManager not found, creating fallback");

            // Create a temporary GameManager
            var go = new GameObject("GameManager (Fallback)");
            var gm = go.AddComponent<GameManager>();

            // Try to load config from Resources
            var config = Resources.Load<GameConfig>("GameConfig_Default");
            if (config == null)
            {
                Debug.LogWarning("[SimpleGameController] No GameConfig found, using defaults");
                config = ScriptableObject.CreateInstance<GameConfig>();
            }
            gm.SetConfig(config);

            DontDestroyOnLoad(go);
        }

        public void StartNewGame()
        {
            Debug.Log("[SimpleGameController] Starting new game");

            // Ensure GameManager exists
            EnsureGameManager();

            // Initialize grid
            var config = GameManager.Instance?.Config;
            int width = config?.GridWidth ?? 9;
            int height = config?.GridHeight ?? 9;

            _grid = new GridData(width, height);
            _pieceGenerator = new PieceGenerator();
            _currentPieces = new List<PieceData>();
            _piecesUsedThisSet = 0;
            _isGameActive = true;

            // Set game state
            GameManager.Instance?.SetState(GameState.Playing);

            // Generate first pieces
            GenerateNewPieceSet();

            // Update view
            _gridView?.Initialize(_grid);
            _gridView?.UpdatePieceTray(_currentPieces);
        }

        public void GenerateNewPieceSet()
        {
            int count = GameManager.Instance?.Config?.PiecesPerSet ?? 3;
            _currentPieces = _pieceGenerator.GeneratePieceSet(count, _grid);
            _piecesUsedThisSet = 0;

            Debug.Log($"[SimpleGameController] Generated {_currentPieces.Count} pieces");

            // Check for immediate game over
            CheckGameOver();

            _gridView?.UpdatePieceTray(_currentPieces);
        }

        /// <summary>
        /// Attempts to place a piece at the given grid position.
        /// </summary>
        public bool TryPlacePiece(int pieceIndex, Vector2Int gridPosition)
        {
            if (!_isGameActive) return false;
            if (pieceIndex < 0 || pieceIndex >= _currentPieces.Count) return false;

            var piece = _currentPieces[pieceIndex];
            if (piece == null) return false;

            // Validate placement
            if (!PlacementValidator.CanPlace(_grid, piece, gridPosition))
            {
                Debug.Log($"[SimpleGameController] Cannot place piece at {gridPosition}");
                return false;
            }

            // Place piece
            foreach (var offset in piece.Offsets)
            {
                int x = gridPosition.x + offset.x;
                int y = gridPosition.y + offset.y;
                _grid.FillCell(x, y);
            }

            // Add placement points
            int points = GameManager.Instance?.CalculatePlacementPoints(piece.TileCount) ?? piece.TileCount;
            GameManager.Instance?.AddScore(points);

            Debug.Log($"[SimpleGameController] Placed piece {piece.Name} at {gridPosition}, +{points} points");

            // Mark piece as used
            _currentPieces[pieceIndex] = null;
            _piecesUsedThisSet++;

            // Check for clears
            ProcessClears();

            // Update view
            _gridView?.RefreshGrid();
            _gridView?.UpdatePieceTray(_currentPieces);

            // Check if all pieces used
            if (_piecesUsedThisSet >= 3)
            {
                GenerateNewPieceSet();
            }
            else
            {
                CheckGameOver();
            }

            return true;
        }

        private void ProcessClears()
        {
            var config = GameManager.Instance?.Config;
            bool check3x3 = config?.Enable3x3BlockClears ?? true;

            var clearResult = ClearDetector.DetectClears(_grid, check3x3);

            if (clearResult.HasClears)
            {
                // Apply clears
                ClearDetector.ApplyClears(_grid, clearResult);

                // Calculate bonus
                int bonus = GameManager.Instance?.CalculateClearBonus(
                    clearResult.ClearedRows.Count,
                    clearResult.ClearedColumns.Count,
                    clearResult.Cleared3x3Blocks.Count) ?? 0;

                GameManager.Instance?.AddScore(bonus);
                GameManager.Instance?.IncrementCombo();
                GameManager.Instance?.AddClears(clearResult.TotalLinesCleared);

                Debug.Log($"[SimpleGameController] Cleared {clearResult.TotalLinesCleared} lines, +{bonus} bonus");

                // Update view
                _gridView?.RefreshGrid();
            }
            else
            {
                // Reset combo if no clears
                GameManager.Instance?.ResetCombo();
            }
        }

        private void CheckGameOver()
        {
            // Get available (non-null) pieces
            var availablePieces = new List<PieceData>();
            foreach (var piece in _currentPieces)
            {
                if (piece != null) availablePieces.Add(piece);
            }

            if (GameOverDetector.IsGameOver(_grid, availablePieces))
            {
                _isGameActive = false;
                GameManager.Instance?.TriggerGameOver();
                Debug.Log("[SimpleGameController] GAME OVER - No valid moves");
            }
        }

        /// <summary>
        /// Rotates a piece in the tray (cycles through precomputed rotations).
        /// </summary>
        public void RotatePiece(int pieceIndex)
        {
            if (pieceIndex < 0 || pieceIndex >= _currentPieces.Count) return;
            if (_currentPieces[pieceIndex] == null) return;

            _currentPieces[pieceIndex] = _currentPieces[pieceIndex].RotateClockwise();
            _gridView?.UpdatePieceTray(_currentPieces);
        }
    }
}
