using UnityEngine;
using BlockPuzzle.Pieces;

namespace BlockPuzzle.Grid
{
    /// <summary>
    /// Validates piece placement on the grid.
    /// Pure static methods for testability.
    /// </summary>
    public static class PlacementValidator
    {
        /// <summary>
        /// Checks if a piece can be placed at the given grid position.
        /// </summary>
        public static bool CanPlace(GridData grid, PieceData piece, Vector2Int position)
        {
            if (grid == null || piece == null) return false;

            foreach (var offset in piece.Offsets)
            {
                int x = position.x + offset.x;
                int y = position.y + offset.y;

                // Bounds check
                if (!grid.IsValidPosition(x, y))
                {
                    return false;
                }

                // Overlap check
                if (grid.IsCellFilled(x, y))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a piece (any rotation) can be placed anywhere on the grid.
        /// </summary>
        public static bool CanPlaceAnywhere(GridData grid, PieceData piece)
        {
            if (grid == null || piece == null) return false;

            // Check all rotations
            int rotationCount = piece.GetRotationCount();
            for (int r = 0; r < rotationCount; r++)
            {
                var rotatedPiece = piece.GetRotation(r);

                // Check all positions
                for (int x = 0; x < grid.Width; x++)
                {
                    for (int y = 0; y < grid.Height; y++)
                    {
                        if (CanPlace(grid, rotatedPiece, new Vector2Int(x, y)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds all valid positions for a piece on the grid.
        /// </summary>
        public static System.Collections.Generic.List<Vector2Int> FindValidPositions(GridData grid, PieceData piece)
        {
            var positions = new System.Collections.Generic.List<Vector2Int>();

            if (grid == null || piece == null) return positions;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (CanPlace(grid, piece, pos))
                    {
                        positions.Add(pos);
                    }
                }
            }

            return positions;
        }
    }
}
