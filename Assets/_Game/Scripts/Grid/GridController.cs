using System;
using System.Collections.Generic;
using UnityEngine;
using BlockPuzzle.Core;
using BlockPuzzle.Pieces;

namespace BlockPuzzle.Grid
{
    public class GridController : MonoBehaviour
    {
        public event Action<ClearDetector.ClearResult> OnClearsDetected;
        public event Action OnGridChanged;
        public event Action OnPerfectClear;

        [SerializeField] private Transform _gridParent;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _cellSpacing = 0.1f;

        private GridData _gridData;
        private GameObject[,] _cellObjects;
        private bool _enable3x3Clears = true;

        public GridData Data => _gridData;

        private void Awake()
        {
            var config = GameManager.Instance?.Config;
            int width = config?.GridWidth ?? GridData.DEFAULT_WIDTH;
            int height = config?.GridHeight ?? GridData.DEFAULT_HEIGHT;
            _enable3x3Clears = config?.Enable3x3BlockClears ?? true;

            _gridData = new GridData(width, height);
        }

        public void Initialize()
        {
            CreateVisualGrid();
        }

        public void Initialize(GridData existingData)
        {
            _gridData = new GridData(existingData);
            CreateVisualGrid();
            RefreshVisuals();
        }

        private void CreateVisualGrid()
        {
            if (_cellPrefab == null || _gridParent == null) return;

            _cellObjects = new GameObject[_gridData.Width, _gridData.Height];
            float totalWidth = _gridData.Width * (_cellSize + _cellSpacing) - _cellSpacing;
            float totalHeight = _gridData.Height * (_cellSize + _cellSpacing) - _cellSpacing;
            Vector3 startPos = new Vector3(-totalWidth / 2f + _cellSize / 2f, -totalHeight / 2f + _cellSize / 2f, 0);

            for (int x = 0; x < _gridData.Width; x++)
            {
                for (int y = 0; y < _gridData.Height; y++)
                {
                    Vector3 pos = startPos + new Vector3(
                        x * (_cellSize + _cellSpacing),
                        y * (_cellSize + _cellSpacing),
                        0);

                    var cell = Instantiate(_cellPrefab, pos, Quaternion.identity, _gridParent);
                    cell.name = $"Cell_{x}_{y}";
                    _cellObjects[x, y] = cell;
                }
            }
        }

        public bool CanPlacePiece(PieceData piece, Vector2Int gridPosition)
        {
            return PlacementValidator.CanPlace(_gridData, piece, gridPosition);
        }

        public bool TryPlacePiece(PieceData piece, Vector2Int gridPosition)
        {
            if (!CanPlacePiece(piece, gridPosition)) return false;

            // Place piece
            foreach (var offset in piece.Offsets)
            {
                int x = gridPosition.x + offset.x;
                int y = gridPosition.y + offset.y;
                _gridData.FillCell(x, y);
            }

            RefreshVisuals();
            OnGridChanged?.Invoke();

            // Check for clears
            var clearResult = ClearDetector.DetectClears(_gridData, _enable3x3Clears);
            if (clearResult.HasClears)
            {
                ClearDetector.ApplyClears(_gridData, clearResult);
                RefreshVisuals();
                OnClearsDetected?.Invoke(clearResult);

                // Check for perfect clear
                if (_gridData.IsEmpty())
                {
                    OnPerfectClear?.Invoke();
                }
            }

            return true;
        }

        public void RefreshVisuals()
        {
            if (_cellObjects == null) return;

            for (int x = 0; x < _gridData.Width; x++)
            {
                for (int y = 0; y < _gridData.Height; y++)
                {
                    var cell = _cellObjects[x, y];
                    if (cell == null) continue;

                    var renderer = cell.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.color = _gridData.IsCellFilled(x, y) ? Color.blue : Color.gray;
                    }
                }
            }
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            if (_gridParent == null) return Vector2Int.zero;

            Vector3 localPos = _gridParent.InverseTransformPoint(worldPos);
            float totalWidth = _gridData.Width * (_cellSize + _cellSpacing) - _cellSpacing;
            float totalHeight = _gridData.Height * (_cellSize + _cellSpacing) - _cellSpacing;

            float normalizedX = (localPos.x + totalWidth / 2f) / (_cellSize + _cellSpacing);
            float normalizedY = (localPos.y + totalHeight / 2f) / (_cellSize + _cellSpacing);

            return new Vector2Int(Mathf.FloorToInt(normalizedX), Mathf.FloorToInt(normalizedY));
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            if (_gridParent == null) return Vector3.zero;

            float totalWidth = _gridData.Width * (_cellSize + _cellSpacing) - _cellSpacing;
            float totalHeight = _gridData.Height * (_cellSize + _cellSpacing) - _cellSpacing;
            Vector3 startPos = new Vector3(-totalWidth / 2f + _cellSize / 2f, -totalHeight / 2f + _cellSize / 2f, 0);

            Vector3 localPos = startPos + new Vector3(
                gridPos.x * (_cellSize + _cellSpacing),
                gridPos.y * (_cellSize + _cellSpacing),
                0);

            return _gridParent.TransformPoint(localPos);
        }

        public void ResetGrid()
        {
            _gridData.Clear();
            RefreshVisuals();
            OnGridChanged?.Invoke();
        }

        public void LoadFromData(int[] data)
        {
            _gridData.FromIntArray(data);
            RefreshVisuals();
        }

        public int[] SaveToData()
        {
            return _gridData.ToIntArray();
        }
    }
}
