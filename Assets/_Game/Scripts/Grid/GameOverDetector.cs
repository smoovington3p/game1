using System.Collections.Generic;
using BlockPuzzle.Pieces;

namespace BlockPuzzle.Grid
{
    /// <summary>
    /// Brute-force game over detection.
    /// MANDATORY: Never rely on heuristics - check every piece, every rotation, every position.
    /// </summary>
    public static class GameOverDetector
    {
        /// <summary>
        /// Checks if game is over (no pieces can be placed).
        /// Uses exhaustive brute-force scan for reliability.
        /// </summary>
        public static bool IsGameOver(GridData grid, List<PieceData> availablePieces)
        {
            if (grid == null) return true;
            if (availablePieces == null || availablePieces.Count == 0) return false;

            // For each available piece
            foreach (var piece in availablePieces)
            {
                if (piece == null) continue;

                // Check if this piece (any rotation) can be placed anywhere
                if (CanPlacePieceAnywhere(grid, piece))
                {
                    return false; // At least one piece can be placed -> NOT game over
                }
            }

            // No piece can be placed -> game over
            return true;
        }

        /// <summary>
        /// Brute-force check: can this piece be placed anywhere on the grid?
        /// Checks all rotations at all positions.
        /// </summary>
        private static bool CanPlacePieceAnywhere(GridData grid, PieceData piece)
        {
            int rotationCount = piece.GetRotationCount();

            // Check all rotations
            for (int rotation = 0; rotation < rotationCount; rotation++)
            {
                var rotatedPiece = piece.GetRotation(rotation);

                // Check all grid positions
                for (int x = 0; x < grid.Width; x++)
                {
                    for (int y = 0; y < grid.Height; y++)
                    {
                        if (CanPlaceAt(grid, rotatedPiece, x, y))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if piece can be placed at specific position.
        /// Inline implementation to avoid any external dependencies in critical path.
        /// </summary>
        private static bool CanPlaceAt(GridData grid, PieceData piece, int gridX, int gridY)
        {
            foreach (var offset in piece.Offsets)
            {
                int x = gridX + offset.x;
                int y = gridY + offset.y;

                // Bounds check
                if (x < 0 || x >= grid.Width || y < 0 || y >= grid.Height)
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
        /// Debug helper: returns detailed info about why game is over.
        /// </summary>
        public static string GetGameOverReason(GridData grid, List<PieceData> availablePieces)
        {
            if (grid == null) return "Grid is null";
            if (availablePieces == null || availablePieces.Count == 0)
                return "No pieces available (not game over)";

            var reasons = new System.Text.StringBuilder();
            reasons.AppendLine($"Grid fill: {grid.GetFilledCellCount()}/{grid.Width * grid.Height}");

            foreach (var piece in availablePieces)
            {
                if (piece == null)
                {
                    reasons.AppendLine("- Null piece (skipped)");
                    continue;
                }

                bool canPlace = CanPlacePieceAnywhere(grid, piece);
                reasons.AppendLine($"- {piece.Name} (id:{piece.PieceId}): {(canPlace ? "CAN place" : "CANNOT place")}");
            }

            return reasons.ToString();
        }
    }
}
