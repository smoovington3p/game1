using NUnit.Framework;
using UnityEngine;
using BlockPuzzle.Grid;
using BlockPuzzle.Pieces;
using System.Collections.Generic;

namespace BlockPuzzle.Tests
{
    [TestFixture]
    public class GridTests
    {
        private GridData _grid;

        [SetUp]
        public void Setup()
        {
            _grid = new GridData(9, 9);
        }

        [Test]
        public void GridData_NewGrid_IsEmpty()
        {
            Assert.IsTrue(_grid.IsEmpty());
            Assert.AreEqual(0, _grid.GetFilledCellCount());
        }

        [Test]
        public void GridData_FillCell_CellIsFilled()
        {
            _grid.FillCell(0, 0);
            Assert.IsTrue(_grid.IsCellFilled(0, 0));
            Assert.AreEqual(1, _grid.GetFilledCellCount());
        }

        [Test]
        public void GridData_ClearCell_CellIsEmpty()
        {
            _grid.FillCell(0, 0);
            _grid.ClearCell(0, 0);
            Assert.IsTrue(_grid.IsCellEmpty(0, 0));
        }

        [Test]
        public void GridData_InvalidPosition_ReturnsFilled()
        {
            Assert.IsTrue(_grid.IsCellFilled(-1, 0));
            Assert.IsTrue(_grid.IsCellFilled(9, 0));
            Assert.IsTrue(_grid.IsCellFilled(0, -1));
            Assert.IsTrue(_grid.IsCellFilled(0, 9));
        }

        [Test]
        public void GridData_SerializationRoundTrip_PreservesState()
        {
            _grid.FillCell(0, 0);
            _grid.FillCell(5, 5);
            _grid.FillCell(8, 8);

            var data = _grid.ToIntArray();
            var newGrid = new GridData(9, 9);
            newGrid.FromIntArray(data);

            Assert.IsTrue(newGrid.IsCellFilled(0, 0));
            Assert.IsTrue(newGrid.IsCellFilled(5, 5));
            Assert.IsTrue(newGrid.IsCellFilled(8, 8));
            Assert.AreEqual(3, newGrid.GetFilledCellCount());
        }
    }

    [TestFixture]
    public class PlacementValidatorTests
    {
        private GridData _grid;

        [SetUp]
        public void Setup()
        {
            _grid = new GridData(9, 9);
            PieceLibrary.Initialize();
        }

        [Test]
        public void CanPlace_EmptyGrid_SingleDot_ReturnsTrue()
        {
            var piece = PieceLibrary.GetPiece(1); // Single dot
            Assert.IsTrue(PlacementValidator.CanPlace(_grid, piece, new Vector2Int(0, 0)));
            Assert.IsTrue(PlacementValidator.CanPlace(_grid, piece, new Vector2Int(8, 8)));
        }

        [Test]
        public void CanPlace_OutOfBounds_ReturnsFalse()
        {
            var piece = PieceLibrary.GetPiece(2); // Domino (2 tiles horizontal)
            Assert.IsFalse(PlacementValidator.CanPlace(_grid, piece, new Vector2Int(8, 0))); // Would go to 9,0
            Assert.IsFalse(PlacementValidator.CanPlace(_grid, piece, new Vector2Int(-1, 0)));
        }

        [Test]
        public void CanPlace_Overlap_ReturnsFalse()
        {
            _grid.FillCell(5, 5);
            var piece = PieceLibrary.GetPiece(1); // Single dot
            Assert.IsFalse(PlacementValidator.CanPlace(_grid, piece, new Vector2Int(5, 5)));
        }

        [Test]
        public void CanPlace_AdjacentToFilled_ReturnsTrue()
        {
            _grid.FillCell(5, 5);
            var piece = PieceLibrary.GetPiece(1); // Single dot
            Assert.IsTrue(PlacementValidator.CanPlace(_grid, piece, new Vector2Int(4, 5)));
            Assert.IsTrue(PlacementValidator.CanPlace(_grid, piece, new Vector2Int(6, 5)));
        }

        [Test]
        public void CanPlaceAnywhere_EmptyGrid_LargePiece_ReturnsTrue()
        {
            var piece = PieceLibrary.GetPiece(21); // 3x3 square
            Assert.IsTrue(PlacementValidator.CanPlaceAnywhere(_grid, piece));
        }

        [Test]
        public void CanPlaceAnywhere_NearlyFullGrid_SmallPiece_FindsSpot()
        {
            // Fill almost entire grid except one cell
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (!(x == 4 && y == 4))
                        _grid.FillCell(x, y);
                }
            }

            var piece = PieceLibrary.GetPiece(1); // Single dot
            Assert.IsTrue(PlacementValidator.CanPlaceAnywhere(_grid, piece));
        }
    }

    [TestFixture]
    public class ClearDetectorTests
    {
        private GridData _grid;

        [SetUp]
        public void Setup()
        {
            _grid = new GridData(9, 9);
        }

        [Test]
        public void DetectClears_EmptyGrid_NoClears()
        {
            var result = ClearDetector.DetectClears(_grid);
            Assert.IsFalse(result.HasClears);
            Assert.AreEqual(0, result.ClearedRows.Count);
            Assert.AreEqual(0, result.ClearedColumns.Count);
        }

        [Test]
        public void DetectClears_FullRow_DetectsRow()
        {
            for (int x = 0; x < 9; x++)
            {
                _grid.FillCell(x, 0);
            }

            var result = ClearDetector.DetectClears(_grid);
            Assert.IsTrue(result.HasClears);
            Assert.AreEqual(1, result.ClearedRows.Count);
            Assert.AreEqual(0, result.ClearedRows[0]);
        }

        [Test]
        public void DetectClears_FullColumn_DetectsColumn()
        {
            for (int y = 0; y < 9; y++)
            {
                _grid.FillCell(0, y);
            }

            var result = ClearDetector.DetectClears(_grid);
            Assert.IsTrue(result.HasClears);
            Assert.AreEqual(1, result.ClearedColumns.Count);
            Assert.AreEqual(0, result.ClearedColumns[0]);
        }

        [Test]
        public void DetectClears_Full3x3Block_DetectsBlock()
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    _grid.FillCell(x, y);
                }
            }

            var result = ClearDetector.DetectClears(_grid, check3x3Blocks: true);
            Assert.IsTrue(result.HasClears);
            Assert.AreEqual(1, result.Cleared3x3Blocks.Count);
        }

        [Test]
        public void DetectClears_RowAndColumn_DetectsBoth()
        {
            // Fill row 4
            for (int x = 0; x < 9; x++)
            {
                _grid.FillCell(x, 4);
            }
            // Fill column 4
            for (int y = 0; y < 9; y++)
            {
                _grid.FillCell(4, y);
            }

            var result = ClearDetector.DetectClears(_grid);
            Assert.IsTrue(result.HasClears);
            Assert.AreEqual(1, result.ClearedRows.Count);
            Assert.AreEqual(1, result.ClearedColumns.Count);
            Assert.AreEqual(2, result.TotalLinesCleared);
        }

        [Test]
        public void ApplyClears_RemovesCells()
        {
            for (int x = 0; x < 9; x++)
            {
                _grid.FillCell(x, 0);
            }

            var result = ClearDetector.DetectClears(_grid);
            ClearDetector.ApplyClears(_grid, result);

            Assert.AreEqual(0, _grid.GetFilledCellCount());
        }
    }

    [TestFixture]
    public class GameOverDetectorTests
    {
        private GridData _grid;

        [SetUp]
        public void Setup()
        {
            _grid = new GridData(9, 9);
            PieceLibrary.Initialize();
        }

        [Test]
        public void IsGameOver_EmptyGrid_WithPieces_NotGameOver()
        {
            var pieces = new List<PieceData>
            {
                PieceLibrary.GetPiece(1),
                PieceLibrary.GetPiece(2),
                PieceLibrary.GetPiece(5)
            };

            Assert.IsFalse(GameOverDetector.IsGameOver(_grid, pieces));
        }

        [Test]
        public void IsGameOver_NoPieces_NotGameOver()
        {
            var pieces = new List<PieceData>();
            Assert.IsFalse(GameOverDetector.IsGameOver(_grid, pieces));
        }

        [Test]
        public void IsGameOver_FullGrid_SingleDot_IsGameOver()
        {
            // Fill entire grid
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    _grid.FillCell(x, y);
                }
            }

            var pieces = new List<PieceData> { PieceLibrary.GetPiece(1) };
            Assert.IsTrue(GameOverDetector.IsGameOver(_grid, pieces));
        }

        [Test]
        public void IsGameOver_AlmostFullGrid_SmallPieceFits_NotGameOver()
        {
            // Fill all but one cell
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (!(x == 4 && y == 4))
                        _grid.FillCell(x, y);
                }
            }

            var pieces = new List<PieceData> { PieceLibrary.GetPiece(1) }; // Single dot
            Assert.IsFalse(GameOverDetector.IsGameOver(_grid, pieces));
        }

        [Test]
        public void IsGameOver_LargePieceCannotFit_SmallPieceCan_NotGameOver()
        {
            // Create a pattern where only small pieces fit
            // Leave a single cell empty
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (!(x == 0 && y == 0))
                        _grid.FillCell(x, y);
                }
            }

            var pieces = new List<PieceData>
            {
                PieceLibrary.GetPiece(21), // 3x3 square - won't fit
                PieceLibrary.GetPiece(1)   // Single dot - will fit
            };

            Assert.IsFalse(GameOverDetector.IsGameOver(_grid, pieces));
        }

        [Test]
        public void IsGameOver_NoFalsePositives_CheckeredPattern()
        {
            // Create checkered pattern - should still allow some pieces
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if ((x + y) % 2 == 0)
                        _grid.FillCell(x, y);
                }
            }

            var pieces = new List<PieceData> { PieceLibrary.GetPiece(1) }; // Single dot
            // Checkered pattern leaves half the cells empty
            Assert.IsFalse(GameOverDetector.IsGameOver(_grid, pieces));
        }

        [Test]
        public void IsGameOver_RotationAllowsPlacement_NotGameOver()
        {
            // Create narrow vertical corridor
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    if (x != 4) // Leave column 4 empty
                        _grid.FillCell(x, y);
                }
            }

            // I-Tromino can fit vertically even if generated horizontally
            var pieces = new List<PieceData> { PieceLibrary.GetPiece(4) }; // I-Tromino
            Assert.IsFalse(GameOverDetector.IsGameOver(_grid, pieces));
        }
    }

    [TestFixture]
    public class PieceRotationTests
    {
        [SetUp]
        public void Setup()
        {
            PieceLibrary.Initialize();
        }

        [Test]
        public void Rotation_DotPiece_SingleRotation()
        {
            var piece = PieceLibrary.GetPiece(1);
            Assert.AreEqual(1, piece.GetRotationCount());
        }

        [Test]
        public void Rotation_DominoPiece_TwoRotations()
        {
            var piece = PieceLibrary.GetPiece(2);
            Assert.AreEqual(2, piece.GetRotationCount());
        }

        [Test]
        public void Rotation_SquarePiece_SingleRotation()
        {
            var piece = PieceLibrary.GetPiece(5); // O-Tetromino (square)
            Assert.AreEqual(1, piece.GetRotationCount());
        }

        [Test]
        public void Rotation_LPiece_FourRotations()
        {
            var piece = PieceLibrary.GetPiece(9); // L-Tetromino
            Assert.AreEqual(4, piece.GetRotationCount());
        }

        [Test]
        public void Rotation_FullCycle_ReturnsSamePiece()
        {
            var piece = PieceLibrary.GetPiece(6); // T-Tetromino
            var rotated = piece;

            for (int i = 0; i < piece.GetRotationCount(); i++)
            {
                rotated = rotated.RotateClockwise();
            }

            // After full rotation cycle, should be back to original
            Assert.AreEqual(piece.RotationIndex, rotated.RotationIndex);
        }

        [Test]
        public void Rotation_PreservesOffsetsIntegrity()
        {
            var piece = PieceLibrary.GetPiece(6);
            var rotated = piece.RotateClockwise();

            // Rotated piece should have same number of tiles
            Assert.AreEqual(piece.TileCount, rotated.TileCount);

            // Rotated piece should have normalized offsets (start from 0,0)
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var offset in rotated.Offsets)
            {
                if (offset.x < minX) minX = offset.x;
                if (offset.y < minY) minY = offset.y;
            }
            Assert.AreEqual(0, minX);
            Assert.AreEqual(0, minY);
        }
    }
}
