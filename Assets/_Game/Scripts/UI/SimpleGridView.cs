using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockPuzzle.Grid;
using BlockPuzzle.Pieces;
using BlockPuzzle.Core;

namespace BlockPuzzle.UI
{
    /// <summary>
    /// Simple visual representation of the grid and piece tray.
    /// Uses UI Image components for cells.
    /// </summary>
    public class SimpleGridView : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private float _cellSize = 40f;
        [SerializeField] private float _cellSpacing = 2f;

        [Header("Piece Tray")]
        [SerializeField] private RectTransform _pieceTrayContainer;
        [SerializeField] private PieceTraySlot[] _pieceSlots;

        [Header("Colors")]
        [SerializeField] private Color _emptyColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private Color _filledColor = new Color(0.3f, 0.5f, 0.9f);
        [SerializeField] private Color _previewColor = new Color(0.5f, 0.7f, 1f, 0.5f);

        [Header("Game Reference")]
        [SerializeField] private SimpleGameController _gameController;

        private GridData _grid;
        private Image[,] _cellImages;
        private int _selectedPieceIndex = -1;

        public void Initialize(GridData grid)
        {
            _grid = grid;
            CreateGridVisuals();
            RefreshGrid();
        }

        private void CreateGridVisuals()
        {
            if (_gridContainer == null || _cellPrefab == null || _grid == null) return;

            // Clear existing cells
            foreach (Transform child in _gridContainer)
            {
                Destroy(child.gameObject);
            }

            _cellImages = new Image[_grid.Width, _grid.Height];

            float totalWidth = _grid.Width * (_cellSize + _cellSpacing) - _cellSpacing;
            float totalHeight = _grid.Height * (_cellSize + _cellSpacing) - _cellSpacing;

            // Center the grid
            _gridContainer.sizeDelta = new Vector2(totalWidth, totalHeight);

            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    var cellObj = Instantiate(_cellPrefab, _gridContainer);
                    var rectTransform = cellObj.GetComponent<RectTransform>();

                    // Position from bottom-left
                    float posX = x * (_cellSize + _cellSpacing) - totalWidth / 2f + _cellSize / 2f;
                    float posY = y * (_cellSize + _cellSpacing) - totalHeight / 2f + _cellSize / 2f;
                    rectTransform.anchoredPosition = new Vector2(posX, posY);
                    rectTransform.sizeDelta = new Vector2(_cellSize, _cellSize);

                    var image = cellObj.GetComponent<Image>();
                    _cellImages[x, y] = image;

                    // Add click handler
                    int capturedX = x;
                    int capturedY = y;
                    var button = cellObj.GetComponent<Button>();
                    if (button == null) button = cellObj.AddComponent<Button>();
                    button.onClick.AddListener(() => OnCellClicked(capturedX, capturedY));
                }
            }
        }

        public void RefreshGrid()
        {
            if (_grid == null || _cellImages == null) return;

            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    if (_cellImages[x, y] != null)
                    {
                        _cellImages[x, y].color = _grid.IsCellFilled(x, y) ? _filledColor : _emptyColor;
                    }
                }
            }
        }

        public void UpdatePieceTray(List<PieceData> pieces)
        {
            if (_pieceSlots == null) return;

            for (int i = 0; i < _pieceSlots.Length; i++)
            {
                if (i < pieces.Count && pieces[i] != null)
                {
                    _pieceSlots[i].SetPiece(pieces[i], i);
                    _pieceSlots[i].SetSelected(i == _selectedPieceIndex);
                }
                else
                {
                    _pieceSlots[i].ClearPiece();
                }
            }
        }

        public void SelectPiece(int index)
        {
            _selectedPieceIndex = index;

            // Update visual selection
            if (_pieceSlots != null)
            {
                for (int i = 0; i < _pieceSlots.Length; i++)
                {
                    _pieceSlots[i].SetSelected(i == _selectedPieceIndex);
                }
            }

            Debug.Log($"[GridView] Selected piece index: {index}");
        }

        private void OnCellClicked(int x, int y)
        {
            if (_selectedPieceIndex < 0)
            {
                Debug.Log("[GridView] No piece selected");
                return;
            }

            Debug.Log($"[GridView] Cell clicked: ({x}, {y})");

            if (_gameController != null)
            {
                bool success = _gameController.TryPlacePiece(_selectedPieceIndex, new Vector2Int(x, y));
                if (success)
                {
                    _selectedPieceIndex = -1;
                }
            }
        }

        public void ShowPreview(PieceData piece, Vector2Int position)
        {
            // Could implement preview highlighting here
        }

        public void ClearPreview()
        {
            // Could implement preview clearing here
        }
    }

    [System.Serializable]
    public class PieceTraySlot : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Image _selectionFrame;
        [SerializeField] private float _cellSize = 20f;

        private int _pieceIndex;
        private List<GameObject> _cells = new List<GameObject>();

        public void SetPiece(PieceData piece, int index)
        {
            _pieceIndex = index;
            ClearCells();

            if (piece == null || _container == null || _cellPrefab == null) return;

            foreach (var offset in piece.Offsets)
            {
                var cell = Instantiate(_cellPrefab, _container);
                var rect = cell.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(offset.x * _cellSize, offset.y * _cellSize);
                rect.sizeDelta = new Vector2(_cellSize - 2, _cellSize - 2);
                _cells.Add(cell);
            }
        }

        public void ClearPiece()
        {
            ClearCells();
            _pieceIndex = -1;
        }

        private void ClearCells()
        {
            foreach (var cell in _cells)
            {
                if (cell != null) Destroy(cell);
            }
            _cells.Clear();
        }

        public void SetSelected(bool selected)
        {
            if (_selectionFrame != null)
            {
                _selectionFrame.enabled = selected;
            }
        }

        public void OnClick()
        {
            var gridView = GetComponentInParent<SimpleGridView>();
            if (gridView != null)
            {
                gridView.SelectPiece(_pieceIndex);
            }
        }
    }
}
