using System;
using System.Collections.Generic;
using UnityEngine;
using BlockPuzzle.Grid;
using BlockPuzzle.Pieces;
using BlockPuzzle.Analytics;
using BlockPuzzle.Save;

namespace BlockPuzzle.Core
{
    /// <summary>
    /// Main game loop controller handling piece placement, clears, and game over.
    /// </summary>
    public class GameLoopController : MonoBehaviour
    {
        public event Action<List<PieceData>> OnPiecesGenerated;
        public event Action<PieceData, int> OnPieceUsed; // piece, index in tray
        public event Action<ClearDetector.ClearResult> OnClears;
        public event Action OnGameOver;
        public event Action<int> OnScoreUpdated;

        [SerializeField] private GridController _gridController;

        private PieceGenerator _pieceGenerator;
        private List<PieceData> _currentPieces;
        private int _piecesUsedThisSet;
        private bool _isGameActive;

        public List<PieceData> CurrentPieces => _currentPieces;
        public bool IsGameActive => _isGameActive;
        public GridController GridController => _gridController;

        private void Awake()
        {
            _pieceGenerator = new PieceGenerator();
            _currentPieces = new List<PieceData>();
        }

        private void Start()
        {
            if (_gridController != null)
            {
                _gridController.OnClearsDetected += HandleClears;
                _gridController.OnPerfectClear += HandlePerfectClear;
            }
        }

        private void OnDestroy()
        {
            if (_gridController != null)
            {
                _gridController.OnClearsDetected -= HandleClears;
                _gridController.OnPerfectClear -= HandlePerfectClear;
            }
        }

        public void StartNewGame(int? seed = null)
        {
            _isGameActive = true;
            _piecesUsedThisSet = 0;

            // Setup generator
            if (seed.HasValue)
            {
                _pieceGenerator.SetSeed(seed.Value);
            }

            var config = GameManager.Instance?.Config;
            if (config != null)
            {
                _pieceGenerator.SetDifficultyParams(
                    config.SmallPieceBaseWeight,
                    config.LargePieceMaxWeight,
                    config.DifficultyScalingStartLevel
                );
            }

            // Initialize grid
            _gridController?.Initialize();

            // Generate first set of pieces
            GenerateNewPieceSet();

            GameManager.Instance?.SetState(GameState.Playing);
            AnalyticsManager.Instance?.TrackRunStart();
        }

        public void LoadGame(SaveData saveData)
        {
            if (saveData == null) return;

            _isGameActive = true;
            _piecesUsedThisSet = saveData.PiecesUsedThisSet;

            // Restore grid
            var gridData = new GridData();
            gridData.FromIntArray(saveData.GridState);
            _gridController?.Initialize(gridData);

            // Restore pieces
            _currentPieces.Clear();
            foreach (var pieceInfo in saveData.CurrentPieces)
            {
                var piece = PieceLibrary.GetPiece(pieceInfo.PieceId, pieceInfo.RotationIndex);
                _currentPieces.Add(piece);
            }

            OnPiecesGenerated?.Invoke(_currentPieces);
            GameManager.Instance?.SetState(GameState.Playing);
        }

        public void GenerateNewPieceSet()
        {
            var config = GameManager.Instance?.Config;
            int count = config?.PiecesPerSet ?? 3;

            _currentPieces = _pieceGenerator.GeneratePieceSet(count, _gridController?.Data);
            _piecesUsedThisSet = 0;

            OnPiecesGenerated?.Invoke(_currentPieces);

            // Check game over immediately after generating pieces
            CheckGameOver();
        }

        /// <summary>
        /// Attempts to place a piece at the given grid position.
        /// Returns true if successful.
        /// </summary>
        public bool TryPlacePiece(int pieceIndex, Vector2Int gridPosition)
        {
            if (!_isGameActive) return false;
            if (pieceIndex < 0 || pieceIndex >= _currentPieces.Count) return false;
            if (_currentPieces[pieceIndex] == null) return false;

            var piece = _currentPieces[pieceIndex];

            // Attempt placement
            if (!_gridController.TryPlacePiece(piece, gridPosition))
            {
                return false;
            }

            // Placement successful
            int placementPoints = GameManager.Instance?.CalculatePlacementPoints(piece.TileCount) ?? piece.TileCount;
            GameManager.Instance?.AddScore(placementPoints);

            AnalyticsManager.Instance?.TrackPlacement(piece.PieceId);

            // Mark piece as used
            OnPieceUsed?.Invoke(piece, pieceIndex);
            _currentPieces[pieceIndex] = null;
            _piecesUsedThisSet++;

            // Save after every move
            SaveManager.Instance?.SaveGame();

            // Check if all pieces used
            if (_piecesUsedThisSet >= _currentPieces.Count)
            {
                GenerateNewPieceSet();
            }
            else
            {
                CheckGameOver();
            }

            return true;
        }

        /// <summary>
        /// Rotates a piece in the tray.
        /// </summary>
        public void RotatePiece(int pieceIndex)
        {
            if (pieceIndex < 0 || pieceIndex >= _currentPieces.Count) return;
            if (_currentPieces[pieceIndex] == null) return;

            _currentPieces[pieceIndex] = _currentPieces[pieceIndex].RotateClockwise();
            OnPiecesGenerated?.Invoke(_currentPieces);
        }

        private void HandleClears(ClearDetector.ClearResult clearResult)
        {
            int clearBonus = GameManager.Instance?.CalculateClearBonus(
                clearResult.ClearedRows.Count,
                clearResult.ClearedColumns.Count,
                clearResult.Cleared3x3Blocks.Count) ?? 0;

            GameManager.Instance?.AddScore(clearBonus);
            GameManager.Instance?.IncrementCombo();
            GameManager.Instance?.AddClears(clearResult.TotalLinesCleared);

            OnClears?.Invoke(clearResult);

            AnalyticsManager.Instance?.TrackClear(
                clearResult.ClearedRows.Count,
                clearResult.ClearedColumns.Count,
                clearResult.Cleared3x3Blocks.Count,
                GameManager.Instance?.Combo ?? 0
            );
        }

        private void HandlePerfectClear()
        {
            var config = GameManager.Instance?.Config;
            int bonus = config?.PerfectClearBonus ?? 100;
            GameManager.Instance?.AddScore(bonus);
        }

        private void CheckGameOver()
        {
            // Get non-null pieces
            var availablePieces = new List<PieceData>();
            foreach (var piece in _currentPieces)
            {
                if (piece != null) availablePieces.Add(piece);
            }

            if (GameOverDetector.IsGameOver(_gridController.Data, availablePieces))
            {
                EndGame("no_moves");
            }
        }

        public void EndGame(string reason)
        {
            if (!_isGameActive) return;

            _isGameActive = false;

            AnalyticsManager.Instance?.TrackRunEnd(
                GameManager.Instance?.Score ?? 0,
                GameManager.Instance?.SessionDuration ?? 0,
                reason
            );

            GameManager.Instance?.TriggerGameOver();
            OnGameOver?.Invoke();
        }

        /// <summary>
        /// Gets current game state for saving.
        /// </summary>
        public void GetSaveData(SaveData saveData)
        {
            if (saveData == null) return;

            saveData.GridState = _gridController?.SaveToData();
            saveData.Score = GameManager.Instance?.Score ?? 0;
            saveData.Combo = GameManager.Instance?.Combo ?? 0;
            saveData.PiecesUsedThisSet = _piecesUsedThisSet;

            saveData.CurrentPieces = new List<SaveData.PieceInfo>();
            foreach (var piece in _currentPieces)
            {
                if (piece != null)
                {
                    saveData.CurrentPieces.Add(new SaveData.PieceInfo
                    {
                        PieceId = piece.PieceId,
                        RotationIndex = piece.RotationIndex
                    });
                }
                else
                {
                    saveData.CurrentPieces.Add(null);
                }
            }
        }
    }
}
