using System.Collections.Generic;
using UnityEngine;

namespace BlockPuzzle.Pieces
{
    /// <summary>
    /// Contains all 25+ piece definitions with precomputed rotations.
    /// </summary>
    public static class PieceLibrary
    {
        private static Dictionary<int, PieceData[]> _pieces;
        private static List<int> _smallPieceIds;
        private static List<int> _mediumPieceIds;
        private static List<int> _largePieceIds;

        public static int PieceCount => _pieces?.Count ?? 0;

        static PieceLibrary()
        {
            Initialize();
        }

        public static void Initialize()
        {
            _pieces = new Dictionary<int, PieceData[]>();
            _smallPieceIds = new List<int>();
            _mediumPieceIds = new List<int>();
            _largePieceIds = new List<int>();

            // === SMALL PIECES (1-2 tiles) ===

            // 1. Single dot
            AddPiece(1, "Dot", new Vector2Int[] {
                new Vector2Int(0, 0)
            });

            // 2. Domino horizontal
            AddPiece(2, "Domino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0)
            });

            // === MEDIUM PIECES (3-4 tiles) ===

            // 3. L-Tromino
            AddPiece(3, "L-Tromino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1)
            });

            // 4. I-Tromino (line of 3)
            AddPiece(4, "I-Tromino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            });

            // 5. Square Tetromino
            AddPiece(5, "O-Tetromino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1)
            });

            // 6. T-Tetromino
            AddPiece(6, "T-Tetromino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(1, 1)
            });

            // 7. S-Tetromino
            AddPiece(7, "S-Tetromino", new Vector2Int[] {
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1)
            });

            // 8. Z-Tetromino
            AddPiece(8, "Z-Tetromino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1)
            });

            // 9. L-Tetromino
            AddPiece(9, "L-Tetromino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 0)
            });

            // 10. J-Tetromino
            AddPiece(10, "J-Tetromino", new Vector2Int[] {
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(1, 2),
                new Vector2Int(0, 0)
            });

            // 11. I-Tetromino (line of 4)
            AddPiece(11, "I-Tetromino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(3, 0)
            });

            // === LARGE PIECES (5+ tiles) ===

            // 12. Plus/Cross
            AddPiece(12, "Plus", new Vector2Int[] {
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(1, 2)
            });

            // 13. U-Pentomino
            AddPiece(13, "U-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(2, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1)
            });

            // 14. I-Pentomino (line of 5)
            AddPiece(14, "I-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(3, 0),
                new Vector2Int(4, 0)
            });

            // 15. L-Pentomino
            AddPiece(15, "L-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(0, 3),
                new Vector2Int(1, 0)
            });

            // 16. T-Pentomino
            AddPiece(16, "T-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(1, 1),
                new Vector2Int(1, 2)
            });

            // 17. W-Pentomino
            AddPiece(17, "W-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2)
            });

            // 18. Z-Pentomino
            AddPiece(18, "Z-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 2),
                new Vector2Int(1, 2),
                new Vector2Int(1, 1),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            });

            // 19. P-Pentomino
            AddPiece(19, "P-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 1),
                new Vector2Int(1, 2)
            });

            // 20. F-Pentomino
            AddPiece(20, "F-Pentomino", new Vector2Int[] {
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(1, 2),
                new Vector2Int(2, 2)
            });

            // 21. 3x3 Square
            AddPiece(21, "Big-Square", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1),
                new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2)
            });

            // 22. Large L
            AddPiece(22, "Large-L", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(0, 3),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            });

            // 23. Corner 3x3
            AddPiece(23, "Corner", new Vector2Int[] {
                new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
                new Vector2Int(0, 1), new Vector2Int(0, 2)
            });

            // 24. Y-Pentomino
            AddPiece(24, "Y-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(1, 2),
                new Vector2Int(1, 3)
            });

            // 25. N-Pentomino
            AddPiece(25, "N-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(1, 2),
                new Vector2Int(1, 3)
            });

            // 26. V-Pentomino
            AddPiece(26, "V-Pentomino", new Vector2Int[] {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            });

            // 27. X-Pentomino
            AddPiece(27, "X-Pentomino", new Vector2Int[] {
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1),
                new Vector2Int(1, 2)
            });
        }

        private static void AddPiece(int id, string name, Vector2Int[] offsets)
        {
            var rotations = PieceData.CreateWithRotations(id, name, offsets);
            _pieces[id] = rotations;

            // Categorize by size
            int tileCount = offsets.Length;
            if (tileCount <= 2)
                _smallPieceIds.Add(id);
            else if (tileCount <= 4)
                _mediumPieceIds.Add(id);
            else
                _largePieceIds.Add(id);
        }

        public static PieceData GetPiece(int pieceId, int rotationIndex = 0)
        {
            if (!_pieces.TryGetValue(pieceId, out var rotations)) return null;
            int index = ((rotationIndex % rotations.Length) + rotations.Length) % rotations.Length;
            return rotations[index];
        }

        public static PieceData[] GetAllRotations(int pieceId)
        {
            _pieces.TryGetValue(pieceId, out var rotations);
            return rotations;
        }

        public static List<int> GetSmallPieceIds() => new List<int>(_smallPieceIds);
        public static List<int> GetMediumPieceIds() => new List<int>(_mediumPieceIds);
        public static List<int> GetLargePieceIds() => new List<int>(_largePieceIds);
        public static List<int> GetAllPieceIds() => new List<int>(_pieces.Keys);
    }
}
