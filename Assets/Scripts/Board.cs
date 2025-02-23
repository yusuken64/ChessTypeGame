using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Transform TileContainer;
    public Transform PiecesContainer;
    public int Width;
    public int Height;

    public float xOffset;
    public float yOffset;

    public Cell CellPrefab;

    public Cell[,] Cells;

    public Piece KingPrefab;
    public Piece QueenPrefab;
    public Piece BishopPrefab;
    public Piece RookPrefab;
    public Piece KnightPrefab;
    public Piece PawnPrefab;

    public delegate void PieceCapturedDeleate(Piece piece);
    public PieceCapturedDeleate PieceCaptured;

    public Solution Solution { get; private set; }

    void Start()
    {
        Cells = new Cell[Width, Height];
        var cells = FindObjectsOfType<Cell>();
        foreach(var cell in cells)
        {
            Cells[cell.X, cell.Y] = cell;
        }
    }

    [ContextMenu("CreateBoard")]
    public void CreateBoard()
    {
        ClearBoard();

        float halfWidth = Height * yOffset / 2f;
        float halfHeight = Height * yOffset / 2f;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                var cell = Instantiate(CellPrefab, TileContainer.transform);
                Cells[i, j] = cell;
                cell.name = $"Cell ({i},{j})";
                cell.X = i;
                cell.Y = j;
                cell.transform.position = new Vector3(
                    i * xOffset - halfWidth,
                    j * yOffset - halfHeight,
                    0);

                if ((i + j) % 2 == 0)
                {
                    cell.SetToBlack();
                }
                else
                {
                    cell.SetToWhite();
                }
            }
        }
    }

    internal void AutoSolve()
    {
        StartCoroutine(AutoSolveRoutine());
    }

    private IEnumerator AutoSolveRoutine()
    {
        if (Solution == null)
        {
            yield break;
        }

        var firstStep = Solution.Steps.First();
        Piece piece = Cells[firstStep.x, firstStep.y].CurrentPiece;
        foreach (var step in Solution.Steps.Skip(1))
        {
            var cell = Cells[step.x, step.y];
            PieceDropped(piece, cell);
            piece = cell.CurrentPiece;

            yield return new WaitForSeconds(0.2f);
        }

        yield return null;
    }

    internal void SetSolution(Solution solution)
    {
        Solution = solution;
    }

    internal void SetState(IEnumerable<PieceRecord> boardRecord)
    {
        CreateBoard();
        foreach(var record in boardRecord)
        {
            var cell = Cells[record.X, record.Y];
            switch (record.PieceMovement)
            {
                case PieceMovement.King:
                    cell.SetPiece_BlackKing();
                    break;
                case PieceMovement.Queen:
                    cell.SetPiece_BlackQueen();
                    break;
                case PieceMovement.Bishop:
                    cell.SetPiece_BlackBishop();
                    break;
                case PieceMovement.Rook:
                    cell.SetPiece_BlackRook();
                    break;
                case PieceMovement.Knight:
                    cell.SetPiece_BlackKnight();
                    break;
                case PieceMovement.Pawn:
                    cell.SetPiece_BlackPawn();
                    break;
            }
            PieceColor pieceColor = record.IsWhite ? PieceColor.White : PieceColor.Black;
            cell.CurrentPiece.SetColor(pieceColor);
        }
    }

    [ContextMenu("Clear Board")]
    public void ClearBoard()
    {
        foreach (Transform child in TileContainer)
        {
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
    Destroy(child.gameObject);
#endif
        }

        foreach (Transform child in PiecesContainer)
        {
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
    Destroy(child.gameObject);
#endif
        }

        var allCells = FindObjectsOfType<Cell>();
        var allPieces = FindObjectsOfType<Piece>();

        foreach(Cell cell in allCells)
        {
#if UNITY_EDITOR
            DestroyImmediate(cell.gameObject);
#else
    Destroy(cell.gameObject);
#endif
        }

        foreach (Piece piece in allPieces)
        {
#if UNITY_EDITOR
            DestroyImmediate(piece.gameObject);
#else
    Destroy(piece.gameObject);
#endif
        }

        Cells = new Cell[Width, Height];
    }

    internal IEnumerable<PieceRecord> GetCurrentLevelRecord()
    {
        return Cells.Cast<Cell>()
            .Where(x => x.CurrentPiece != null)
            .Select(x => new PieceRecord(x));
    }

    public void PiecePickedUp(Piece piece)
    {
        var originCell = Cells.Cast<Cell>().FirstOrDefault(cell => cell.CurrentPiece == piece);
        var movableCells = piece.GetMovableSquares(Cells, originCell, piece.PieceColor);
        foreach(var cell in movableCells)
        {
            cell.SetToDroppable();
        }
    }

    public void PieceDropped(Piece piece, Cell cell)
    {
        if (cell != null)
        {
            var originalCell = Cells.Cast<Cell>().FirstOrDefault(cell => cell.CurrentPiece == piece);
            var (x, y) = FindCellIndex(Cells, cell);
            var newCell = Cells[x, y];
            if (originalCell != null)
            {
                var movableCells = piece.GetMovableSquares(Cells, originalCell, piece.PieceColor);

                if (originalCell != newCell &&
                    movableCells.Contains(newCell))
                {
                    var originalPiece = originalCell.CurrentPiece;
                    originalCell.SetPiece(null);

                    if (newCell.CurrentPiece != null &&
                        newCell.CurrentPiece.PieceColor != originalPiece.PieceColor)
                    {
                        //capture
                        Destroy(originalPiece.gameObject);
                        newCell.Capture(piece);

                        PieceCaptured(piece);
                    }
                    else
                    {
                        newCell.SetPiece(piece);
                        PieceCaptured(null);
                    }
                }
                else
                {
                    originalCell.ResetPiece();
                }
            };
        }
        else
        {
            var originalCell = Cells.Cast<Cell>().FirstOrDefault(cell => cell.CurrentPiece == piece);
            originalCell.ResetPiece();
        }

        Cells.Cast<Cell>().ToList().ForEach(x => x.ClearDroppable());
    }

    public static (int, int) FindCellIndex<T>(T[,] grid, T target)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (EqualityComparer<T>.Default.Equals(grid[i, j], target))
                {
                    return (i, j);
                }
            }
        }

        throw new ArgumentException("Cell not found in the grid.");
    }
}

public struct PieceRecord
{
    public int X;
    public int Y;
    public PieceMovement PieceMovement;
    public bool IsWhite;

    public PieceRecord(Cell cell)
    {
        this.X = cell.X;
        this.Y = cell.Y;
        this.PieceMovement = cell.CurrentPiece.PieceMovement;
        this.IsWhite = cell.CurrentPiece.PieceColor == PieceColor.White;
    }

    public PieceRecord(PieceMovement pieceMovement,
        bool isWhite,
        int x,
        int y)
    {
        this.X = x;
        this.Y = y;
        this.PieceMovement = pieceMovement;
        this.IsWhite = isWhite;
    }

    internal PieceRecord? Clone()
    {
        return new PieceRecord
        {
            X = this.X,
            Y = this.Y,
            PieceMovement = this.PieceMovement,
            IsWhite = this.IsWhite
        };
    }
}

public class Solution
{
    public List<(int x, int y)> Steps { get; internal set; }
    public PieceRecord StartingPiece { get; internal set; }
}