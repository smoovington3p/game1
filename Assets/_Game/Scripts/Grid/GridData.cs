using System;
using UnityEngine;

namespace BlockPuzzle.Grid
{
    /// <summary>
    /// Pure data representation of the game grid.
    /// No MonoBehaviour - can be used in tests.
    /// </summary>
    [Serializable]
    public class GridData
    {
        public const int DEFAULT_WIDTH = 9;
        public const int DEFAULT_HEIGHT = 9;

        private bool[,] _cells;
        private int _width;
        private int _height;

        public int Width => _width;
        public int Height => _height;

        public GridData() : this(DEFAULT_WIDTH, DEFAULT_HEIGHT) { }

        public GridData(int width, int height)
        {
            _width = width;
            _height = height;
            _cells = new bool[width, height];
        }

        public GridData(GridData source)
        {
            _width = source._width;
            _height = source._height;
            _cells = new bool[_width, _height];
            Array.Copy(source._cells, _cells, source._cells.Length);
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public bool IsCellFilled(int x, int y)
        {
            if (!IsValidPosition(x, y)) return true; // Out of bounds = filled
            return _cells[x, y];
        }

        public bool IsCellEmpty(int x, int y)
        {
            return !IsCellFilled(x, y);
        }

        public void SetCell(int x, int y, bool filled)
        {
            if (!IsValidPosition(x, y)) return;
            _cells[x, y] = filled;
        }

        public void FillCell(int x, int y) => SetCell(x, y, true);
        public void ClearCell(int x, int y) => SetCell(x, y, false);

        public void Clear()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _cells[x, y] = false;
                }
            }
        }

        public int GetFilledCellCount()
        {
            int count = 0;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_cells[x, y]) count++;
                }
            }
            return count;
        }

        public bool IsEmpty()
        {
            return GetFilledCellCount() == 0;
        }

        public bool IsFull()
        {
            return GetFilledCellCount() == _width * _height;
        }

        /// <summary>
        /// Serializes grid state to int array for save system.
        /// </summary>
        public int[] ToIntArray()
        {
            int[] result = new int[_width * _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    result[y * _width + x] = _cells[x, y] ? 1 : 0;
                }
            }
            return result;
        }

        /// <summary>
        /// Deserializes grid state from int array.
        /// </summary>
        public void FromIntArray(int[] data)
        {
            if (data == null || data.Length != _width * _height) return;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _cells[x, y] = data[y * _width + x] == 1;
                }
            }
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            for (int y = _height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _width; x++)
                {
                    sb.Append(_cells[x, y] ? "X" : ".");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
