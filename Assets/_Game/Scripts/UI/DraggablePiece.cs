using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BlockPuzzle.Core;
using BlockPuzzle.Grid;
using BlockPuzzle.Pieces;

namespace BlockPuzzle.UI
{
    /// <summary>
    /// Handles drag-and-drop for piece placement.
    /// Attach to PieceTraySlot to enable dragging pieces onto the grid.
    /// </summary>
    public class DraggablePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform _pieceContainer;
        [SerializeField] private float _dragScale = 1.5f;
        [SerializeField] private float _dragOffsetY = 100f;

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private SimpleGridView _gridView;
        private SimpleGameController _gameController;
        private PieceTraySlot _slot;
        private int _pieceIndex = -1;
        private PieceData _pieceData;

        private Vector3 _originalPosition;
        private Vector3 _originalScale;
        private Transform _originalParent;
        private bool _isDragging;

        private void Awake()
        {
            _slot = GetComponent<PieceTraySlot>();
        }

        private void Start()
        {
            // Find references
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                _canvasRect = _canvas.GetComponent<RectTransform>();
            }

            _gridView = FindObjectOfType<SimpleGridView>();
            if (_gridView != null)
            {
                _gameController = _gridView.GameController;
            }
            if (_gameController == null)
            {
                _gameController = FindObjectOfType<SimpleGameController>();
            }

            Debug.Log($"[DraggablePiece] Start - GridView: {_gridView != null}, GameController: {_gameController != null}, Canvas: {_canvas != null}");
        }

        public void SetPieceData(PieceData piece, int index)
        {
            _pieceData = piece;
            _pieceIndex = index;
            Debug.Log($"[DraggablePiece] SetPieceData index={index}, piece={(piece != null ? piece.Name : "null")}");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Only handle click if not dragging
            if (!_isDragging && _slot != null && _pieceData != null)
            {
                Debug.Log($"[DraggablePiece] Click on piece {_pieceIndex}");
                _slot.OnClick();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_pieceData == null)
            {
                Debug.Log("[DraggablePiece] BeginDrag - No piece data, ignoring");
                return;
            }
            if (_pieceContainer == null)
            {
                Debug.LogWarning("[DraggablePiece] BeginDrag - No piece container!");
                return;
            }

            _isDragging = true;
            _originalPosition = _pieceContainer.localPosition;
            _originalScale = _pieceContainer.localScale;
            _originalParent = _pieceContainer.parent;

            // Move to canvas root so it renders on top
            _pieceContainer.SetParent(_canvasRect, true);

            // Scale up for better visibility while dragging
            _pieceContainer.localScale = _originalScale * _dragScale;

            // Select this piece
            if (_gridView != null)
            {
                _gridView.SelectPiece(_pieceIndex);
            }

            Debug.Log($"[DraggablePiece] BeginDrag piece {_pieceIndex} ({_pieceData.Name})");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _pieceContainer == null) return;

            // Move piece container to follow pointer
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, eventData.position, eventData.pressEventCamera, out localPoint))
            {
                // Offset above finger for visibility
                localPoint.y += _dragOffsetY;
                _pieceContainer.anchoredPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            _isDragging = false;

            // Reset scale
            _pieceContainer.localScale = _originalScale;

            // Try to place piece at drop location
            bool placed = TryPlaceAtPosition(eventData.position);

            // Return to original parent
            _pieceContainer.SetParent(_originalParent, true);

            if (!placed)
            {
                // Return to original position
                _pieceContainer.localPosition = _originalPosition;
                Debug.Log($"[DraggablePiece] EndDrag piece {_pieceIndex} - NOT placed, returning to slot");
            }
            else
            {
                Debug.Log($"[DraggablePiece] EndDrag piece {_pieceIndex} - PLACED successfully");
            }
        }

        private bool TryPlaceAtPosition(Vector2 screenPosition)
        {
            if (_gameController == null)
            {
                Debug.LogWarning("[DraggablePiece] TryPlace - No GameController!");
                return false;
            }
            if (_pieceData == null)
            {
                Debug.LogWarning("[DraggablePiece] TryPlace - No piece data!");
                return false;
            }
            if (_pieceIndex < 0)
            {
                Debug.LogWarning("[DraggablePiece] TryPlace - Invalid piece index!");
                return false;
            }
            if (_gridView == null)
            {
                Debug.LogWarning("[DraggablePiece] TryPlace - No GridView!");
                return false;
            }

            // Get grid container from GridView
            var gridContainer = _gridView.GridContainer;
            if (gridContainer == null)
            {
                Debug.LogWarning("[DraggablePiece] TryPlace - No grid container!");
                return false;
            }

            // Convert screen position to grid container local position
            Vector2 localPoint;
            Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridContainer, screenPosition, cam, out localPoint))
            {
                Debug.Log("[DraggablePiece] TryPlace - Screen point not in grid container");
                return false;
            }

            // Get grid dimensions from game controller
            var grid = _gameController.Grid;
            if (grid == null)
            {
                Debug.LogWarning("[DraggablePiece] TryPlace - Grid is null!");
                return false;
            }

            // Use actual values from GridView
            float cellSize = _gridView.CellSize;
            float cellSpacing = _gridView.CellSpacing;
            float totalWidth = grid.Width * (cellSize + cellSpacing) - cellSpacing;
            float totalHeight = grid.Height * (cellSize + cellSpacing) - cellSpacing;

            // Convert local point to grid coordinates
            float gridX = (localPoint.x + totalWidth / 2f) / (cellSize + cellSpacing);
            float gridY = (localPoint.y + totalHeight / 2f) / (cellSize + cellSpacing);

            int cellX = Mathf.FloorToInt(gridX);
            int cellY = Mathf.FloorToInt(gridY);

            Debug.Log($"[DraggablePiece] TryPlace - localPoint: {localPoint}, gridCoords: ({cellX}, {cellY})");

            // Check bounds
            if (cellX < 0 || cellX >= grid.Width || cellY < 0 || cellY >= grid.Height)
            {
                Debug.Log($"[DraggablePiece] TryPlace - Out of bounds: ({cellX}, {cellY})");
                return false;
            }

            // Try to place
            int scoreBefore = GameManager.Instance?.Score ?? 0;
            bool result = _gameController.TryPlacePiece(_pieceIndex, new Vector2Int(cellX, cellY));
            int scoreAfter = GameManager.Instance?.Score ?? 0;

            if (result)
            {
                Debug.Log($"[DraggablePiece] Placement SUCCESS at ({cellX}, {cellY}), score: {scoreBefore} -> {scoreAfter} (+{scoreAfter - scoreBefore})");
            }
            else
            {
                Debug.Log($"[DraggablePiece] Placement FAILED at ({cellX}, {cellY}) - invalid position");
            }

            return result;
        }
    }
}
