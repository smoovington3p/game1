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
        private bool _isDragging;

        private void Awake()
        {
            _slot = GetComponent<PieceTraySlot>();
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                _canvasRect = _canvas.GetComponent<RectTransform>();
            }
        }

        private void Start()
        {
            _gridView = FindObjectOfType<SimpleGridView>();
            _gameController = FindObjectOfType<SimpleGameController>();
        }

        public void SetPieceData(PieceData piece, int index)
        {
            _pieceData = piece;
            _pieceIndex = index;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Only handle click if not dragging
            if (!_isDragging && _slot != null)
            {
                _slot.OnClick();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_pieceData == null || _pieceContainer == null) return;

            _isDragging = true;
            _originalPosition = _pieceContainer.localPosition;
            _originalScale = _pieceContainer.localScale;

            // Scale up for better visibility while dragging
            _pieceContainer.localScale = _originalScale * _dragScale;

            // Select this piece
            if (_gridView != null)
            {
                _gridView.SelectPiece(_pieceIndex);
            }

            Debug.Log($"[DraggablePiece] Begin drag piece {_pieceIndex}");
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

                // Convert to container's parent space
                var parentRect = _pieceContainer.parent as RectTransform;
                if (parentRect != null)
                {
                    Vector2 parentLocalPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        parentRect, eventData.position, eventData.pressEventCamera, out parentLocalPoint);
                    parentLocalPoint.y += _dragOffsetY;
                    _pieceContainer.localPosition = parentLocalPoint;
                }
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

            if (!placed)
            {
                // Return to original position
                _pieceContainer.localPosition = _originalPosition;
            }

            Debug.Log($"[DraggablePiece] End drag piece {_pieceIndex}, placed: {placed}");
        }

        private bool TryPlaceAtPosition(Vector2 screenPosition)
        {
            if (_gameController == null || _pieceData == null || _pieceIndex < 0) return false;

            // Find grid container
            var gridContainer = _gridView?.GetComponent<RectTransform>();
            if (gridContainer == null) return false;

            // Convert screen position to grid position
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridContainer, screenPosition, null, out localPoint))
            {
                return false;
            }

            // Get grid dimensions from game controller
            var grid = _gameController.Grid;
            if (grid == null) return false;

            // Calculate cell size (matching SimpleGridView values)
            float cellSize = 80f;
            float cellSpacing = 4f;
            float totalWidth = grid.Width * (cellSize + cellSpacing) - cellSpacing;
            float totalHeight = grid.Height * (cellSize + cellSpacing) - cellSpacing;

            // Convert local point to grid coordinates
            float gridX = (localPoint.x + totalWidth / 2f) / (cellSize + cellSpacing);
            float gridY = (localPoint.y + totalHeight / 2f) / (cellSize + cellSpacing);

            int cellX = Mathf.FloorToInt(gridX);
            int cellY = Mathf.FloorToInt(gridY);

            // Check bounds
            if (cellX < 0 || cellX >= grid.Width || cellY < 0 || cellY >= grid.Height)
            {
                return false;
            }

            // Try to place
            return _gameController.TryPlacePiece(_pieceIndex, new Vector2Int(cellX, cellY));
        }
    }
}
