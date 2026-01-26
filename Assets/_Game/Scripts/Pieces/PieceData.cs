using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockPuzzle.Pieces
{
    /// <summary>
    /// Immutable piece data with precomputed rotations.
    /// </summary>
    [Serializable]
    public class PieceData
    {
        public readonly int PieceId;
        public readonly string Name;
        public readonly Vector2Int[] Offsets;
        public readonly int TileCount;
        public readonly PieceSize Size;
        public readonly int RotationIndex;

        private readonly PieceData[] _rotations;

        public enum PieceSize { Small, Medium, Large }

        public PieceData(int pieceId, string name, Vector2Int[] offsets, int rotationIndex = 0, PieceData[] rotations = null)
        {
            PieceId = pieceId;
            Name = name;
            Offsets = offsets;
            TileCount = offsets.Length;
            RotationIndex = rotationIndex;
            _rotations = rotations;

            // Determine size based on tile count
            if (TileCount <= 2) Size = PieceSize.Small;
            else if (TileCount <= 4) Size = PieceSize.Medium;
            else Size = PieceSize.Large;
        }

        /// <summary>
        /// Gets the piece rotated by 90 degrees clockwise.
        /// Rotations are precomputed, no runtime float math.
        /// </summary>
        public PieceData GetRotation(int rotationIndex)
        {
            if (_rotations == null || _rotations.Length == 0) return this;
            int index = ((rotationIndex % _rotations.Length) + _rotations.Length) % _rotations.Length;
            return _rotations[index];
        }

        public PieceData RotateClockwise()
        {
            return GetRotation(RotationIndex + 1);
        }

        public int GetRotationCount()
        {
            return _rotations?.Length ?? 1;
        }

        /// <summary>
        /// Gets bounding box of the piece.
        /// </summary>
        public void GetBounds(out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = int.MaxValue;
            maxX = int.MinValue;
            minY = int.MaxValue;
            maxY = int.MinValue;

            foreach (var offset in Offsets)
            {
                if (offset.x < minX) minX = offset.x;
                if (offset.x > maxX) maxX = offset.x;
                if (offset.y < minY) minY = offset.y;
                if (offset.y > maxY) maxY = offset.y;
            }
        }

        public int GetWidth()
        {
            GetBounds(out int minX, out int maxX, out _, out _);
            return maxX - minX + 1;
        }

        public int GetHeight()
        {
            GetBounds(out _, out _, out int minY, out int maxY);
            return maxY - minY + 1;
        }

        /// <summary>
        /// Creates all rotations for a piece.
        /// </summary>
        public static PieceData[] CreateWithRotations(int pieceId, string name, Vector2Int[] baseOffsets)
        {
            var normalizedBase = NormalizeOffsets(baseOffsets);
            var uniqueRotations = new List<Vector2Int[]> { normalizedBase };

            // Generate 90, 180, 270 degree rotations
            var current = normalizedBase;
            for (int i = 0; i < 3; i++)
            {
                current = RotateOffsets90(current);
                current = NormalizeOffsets(current);

                // Check if this rotation is unique
                bool isDuplicate = false;
                foreach (var existing in uniqueRotations)
                {
                    if (AreOffsetsEqual(current, existing))
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    uniqueRotations.Add(current);
                }
            }

            // Create piece data array
            var pieces = new PieceData[uniqueRotations.Count];
            for (int i = 0; i < uniqueRotations.Count; i++)
            {
                pieces[i] = new PieceData(pieceId, name, uniqueRotations[i], i, pieces);
            }

            return pieces;
        }

        private static Vector2Int[] RotateOffsets90(Vector2Int[] offsets)
        {
            var rotated = new Vector2Int[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                // 90 degree clockwise: (x, y) -> (y, -x)
                rotated[i] = new Vector2Int(offsets[i].y, -offsets[i].x);
            }
            return rotated;
        }

        private static Vector2Int[] NormalizeOffsets(Vector2Int[] offsets)
        {
            if (offsets.Length == 0) return offsets;

            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var offset in offsets)
            {
                if (offset.x < minX) minX = offset.x;
                if (offset.y < minY) minY = offset.y;
            }

            var normalized = new Vector2Int[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                normalized[i] = new Vector2Int(offsets[i].x - minX, offsets[i].y - minY);
            }

            // Sort for consistent comparison
            Array.Sort(normalized, (a, b) =>
            {
                int cmp = a.y.CompareTo(b.y);
                return cmp != 0 ? cmp : a.x.CompareTo(b.x);
            });

            return normalized;
        }

        private static bool AreOffsetsEqual(Vector2Int[] a, Vector2Int[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}
