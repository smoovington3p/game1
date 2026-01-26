using System.Collections.Generic;
using UnityEngine;

namespace BlockPuzzle.Grid
{
    /// <summary>
    /// Detects and processes line clears on the grid.
    /// </summary>
    public static class ClearDetector
    {
        public struct ClearResult
        {
            public List<int> ClearedRows;
            public List<int> ClearedColumns;
            public List<Vector2Int> Cleared3x3Blocks;
            public HashSet<Vector2Int> AllClearedCells;
            public int TotalLinesCleared;

            public bool HasClears => TotalLinesCleared > 0;
        }

        /// <summary>
        /// Detects all clearable rows, columns, and optionally 3x3 blocks.
        /// </summary>
        public static ClearResult DetectClears(GridData grid, bool check3x3Blocks = true)
        {
            var result = new ClearResult
            {
                ClearedRows = new List<int>(),
                ClearedColumns = new List<int>(),
                Cleared3x3Blocks = new List<Vector2Int>(),
                AllClearedCells = new HashSet<Vector2Int>()
            };

            // Check rows
            for (int y = 0; y < grid.Height; y++)
            {
                if (IsRowFull(grid, y))
                {
                    result.ClearedRows.Add(y);
                    for (int x = 0; x < grid.Width; x++)
                    {
                        result.AllClearedCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            // Check columns
            for (int x = 0; x < grid.Width; x++)
            {
                if (IsColumnFull(grid, x))
                {
                    result.ClearedColumns.Add(x);
                    for (int y = 0; y < grid.Height; y++)
                    {
                        result.AllClearedCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            // Check 3x3 blocks (for 9x9 grid, there are 9 blocks)
            if (check3x3Blocks && grid.Width == 9 && grid.Height == 9)
            {
                for (int blockX = 0; blockX < 3; blockX++)
                {
                    for (int blockY = 0; blockY < 3; blockY++)
                    {
                        int startX = blockX * 3;
                        int startY = blockY * 3;

                        if (Is3x3BlockFull(grid, startX, startY))
                        {
                            result.Cleared3x3Blocks.Add(new Vector2Int(blockX, blockY));
                            for (int dx = 0; dx < 3; dx++)
                            {
                                for (int dy = 0; dy < 3; dy++)
                                {
                                    result.AllClearedCells.Add(new Vector2Int(startX + dx, startY + dy));
                                }
                            }
                        }
                    }
                }
            }

            result.TotalLinesCleared = result.ClearedRows.Count +
                                       result.ClearedColumns.Count +
                                       result.Cleared3x3Blocks.Count;

            return result;
        }

        /// <summary>
        /// Applies the clear result to the grid.
        /// </summary>
        public static void ApplyClears(GridData grid, ClearResult clearResult)
        {
            foreach (var cell in clearResult.AllClearedCells)
            {
                grid.ClearCell(cell.x, cell.y);
            }
        }

        public static bool IsRowFull(GridData grid, int y)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                if (!grid.IsCellFilled(x, y)) return false;
            }
            return true;
        }

        public static bool IsColumnFull(GridData grid, int x)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                if (!grid.IsCellFilled(x, y)) return false;
            }
            return true;
        }

        public static bool Is3x3BlockFull(GridData grid, int startX, int startY)
        {
            for (int dx = 0; dx < 3; dx++)
            {
                for (int dy = 0; dy < 3; dy++)
                {
                    if (!grid.IsCellFilled(startX + dx, startY + dy)) return false;
                }
            }
            return true;
        }
    }
}
