using System.Collections.Generic;
using UnityEngine;
using BlockPuzzle.Grid;

namespace BlockPuzzle.Pieces
{
    /// <summary>
    /// Generates pieces with weighted random selection and difficulty scaling.
    /// </summary>
    public class PieceGenerator
    {
        private System.Random _random;
        private int _currentLevel;
        private float _smallPieceWeight;
        private float _largePieceMaxWeight;
        private int _difficultyStartLevel;

        public PieceGenerator(int seed = -1)
        {
            _random = seed >= 0 ? new System.Random(seed) : new System.Random();
            _currentLevel = 1;
            _smallPieceWeight = 0.6f;
            _largePieceMaxWeight = 0.5f;
            _difficultyStartLevel = 10;
        }

        public void SetDifficultyParams(float smallPieceWeight, float largePieceMaxWeight, int difficultyStartLevel)
        {
            _smallPieceWeight = smallPieceWeight;
            _largePieceMaxWeight = largePieceMaxWeight;
            _difficultyStartLevel = difficultyStartLevel;
        }

        public void SetLevel(int level)
        {
            _currentLevel = Mathf.Max(1, level);
        }

        public void SetSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        /// <summary>
        /// Generates a set of pieces, attempting to ensure at least one is placeable.
        /// </summary>
        public List<PieceData> GeneratePieceSet(int count, GridData grid = null)
        {
            var pieces = new List<PieceData>();

            for (int i = 0; i < count; i++)
            {
                pieces.Add(GenerateSinglePiece());
            }

            // If grid provided, try to guarantee at least one placeable piece
            if (grid != null && !HasPlaceablePiece(pieces, grid))
            {
                // Try to find a placeable piece
                var placeablePiece = FindPlaceablePiece(grid);
                if (placeablePiece != null && pieces.Count > 0)
                {
                    pieces[0] = placeablePiece;
                }
            }

            return pieces;
        }

        public PieceData GenerateSinglePiece()
        {
            // Calculate weights based on difficulty
            float smallWeight, mediumWeight, largeWeight;
            CalculateWeights(out smallWeight, out mediumWeight, out largeWeight);

            // Select category
            float roll = (float)_random.NextDouble();
            List<int> pieceIds;

            if (roll < smallWeight)
            {
                pieceIds = PieceLibrary.GetSmallPieceIds();
            }
            else if (roll < smallWeight + mediumWeight)
            {
                pieceIds = PieceLibrary.GetMediumPieceIds();
            }
            else
            {
                pieceIds = PieceLibrary.GetLargePieceIds();
            }

            // Fallback if category is empty
            if (pieceIds.Count == 0)
            {
                pieceIds = PieceLibrary.GetAllPieceIds();
            }

            // Select random piece from category
            int pieceId = pieceIds[_random.Next(pieceIds.Count)];

            // Select random rotation
            var rotations = PieceLibrary.GetAllRotations(pieceId);
            int rotationIndex = _random.Next(rotations.Length);

            return rotations[rotationIndex];
        }

        private void CalculateWeights(out float small, out float medium, out float large)
        {
            // Start with base weights
            small = _smallPieceWeight;
            large = 0.1f;
            medium = 1f - small - large;

            // Scale difficulty after threshold
            if (_currentLevel > _difficultyStartLevel)
            {
                float progress = Mathf.Min(1f, (_currentLevel - _difficultyStartLevel) / 20f);
                large = Mathf.Lerp(0.1f, _largePieceMaxWeight, progress);
                small = Mathf.Max(0.2f, _smallPieceWeight - progress * 0.3f);
                medium = 1f - small - large;
            }
        }

        private bool HasPlaceablePiece(List<PieceData> pieces, GridData grid)
        {
            foreach (var piece in pieces)
            {
                if (PlacementValidator.CanPlaceAnywhere(grid, piece))
                {
                    return true;
                }
            }
            return false;
        }

        private PieceData FindPlaceablePiece(GridData grid)
        {
            // Try small pieces first (most likely to fit)
            var smallIds = PieceLibrary.GetSmallPieceIds();
            foreach (var id in smallIds)
            {
                var rotations = PieceLibrary.GetAllRotations(id);
                foreach (var piece in rotations)
                {
                    if (PlacementValidator.CanPlaceAnywhere(grid, piece))
                    {
                        return piece;
                    }
                }
            }

            // Try medium pieces
            var mediumIds = PieceLibrary.GetMediumPieceIds();
            foreach (var id in mediumIds)
            {
                var rotations = PieceLibrary.GetAllRotations(id);
                foreach (var piece in rotations)
                {
                    if (PlacementValidator.CanPlaceAnywhere(grid, piece))
                    {
                        return piece;
                    }
                }
            }

            return null; // No placeable piece exists
        }
    }
}
