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
    public bool Echo;

    public Cell CellPrefab;

    public Cell[,] Cells;

    public Piece KingPrefab;
    public Piece QueenPrefab;
    public Piece BishopPrefab;
    public Piece RookPrefab;
    public Piece KnightPrefab;
    public Piece PawnPrefab;

    public delegate void PieceMovedDelegate(Piece movedPiece, Cell originalCell, Piece capturedPiece, Cell destinationCell);
    public PieceMovedDelegate PieceMoved;

    public delegate void PieceCanceledDelegate(Piece movedPiece, Cell originalCell, string reason);
    public PieceCanceledDelegate PieceCanceled;

    public Solution Solution { get; private set; }
    public bool CanQueenPromote;

    //check if it causes check
    internal bool CanDrop(Piece piece, Cell cell, out string reason)
    {
        reason = string.Empty;
        if (cell == null) { return false; }
        if (Echo) { return true; }

        var originalCell = Cells.OfType<Cell>().First(x => x.CurrentPiece == piece);
        PieceRecord?[,] boardData = MinimaxABSolver.ToBoardData(this);
        var originalRecord = boardData[originalCell.X, originalCell.Y];
        boardData[originalCell.X, originalCell.Y] = null;
        boardData[cell.X, cell.Y] = originalRecord;

        var fen = FENParser.BoardToFEN(boardData, boardData.GetLength(0), boardData.GetLength(1));
        var bitboard = BitboardHelper.FromFen(fen, boardData.GetLength(0), boardData.GetLength(1), CanQueenPromote);

        var isInCheck = bitboard.IsKingInCheck(piece.PieceColor);

        if (isInCheck)
        {
            reason = "Check";
        }
        return !isInCheck;
    }

    void Start()
    {
        ReloadExistingBoard();
    }

    internal void ReplacePiece(Cell destinationCell, PieceType promotionChoice, Piece originalPiece)
    {
        Destroy(destinationCell.CurrentPiece.gameObject);
        destinationCell.CurrentPiece = null;

        switch (promotionChoice)
        {
            case PieceType.King:
                destinationCell.SetPiece_BlackKing();
                break;
            case PieceType.Queen:
                destinationCell.SetPiece_BlackQueen();
                break;
            case PieceType.Bishop:
                destinationCell.SetPiece_BlackBishop();
                break;
            case PieceType.Rook:
                destinationCell.SetPiece_BlackRook();
                break;
            case PieceType.Knight:
                destinationCell.SetPiece_BlackKnight();
                break;
            case PieceType.Pawn:
                destinationCell.SetPiece_BlackPawn();
                break;
        }

        destinationCell.CurrentPiece.SetColor(originalPiece.PieceColor);
    }

    private void ReloadExistingBoard()
    {
        Cells = new Cell[Width, Height];
        var cells = FindObjectsOfType<Cell>();
        foreach (var cell in cells)
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
            switch (record.PieceType)
            {
                case PieceType.King:
                    cell.SetPiece_BlackKing();
                    break;
                case PieceType.Queen:
                    cell.SetPiece_BlackQueen();
                    break;
                case PieceType.Bishop:
                    cell.SetPiece_BlackBishop();
                    break;
                case PieceType.Rook:
                    cell.SetPiece_BlackRook();
                    break;
                case PieceType.Knight:
                    cell.SetPiece_BlackKnight();
                    break;
                case PieceType.Pawn:
                    cell.SetPiece_BlackPawn();
                    break;
            }
            ChessColor pieceColor = record.IsWhite ? ChessColor.w : ChessColor.b;
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
        IEnumerable<Cell> movableCells = GetMovabableCells(piece);

        foreach (var cell in movableCells)
        {
            cell.SetToDroppable();
        }
    }

    public IEnumerable<Cell> GetMovabableCells(Piece piece)
    {
        var originCell = Cells.Cast<Cell>().FirstOrDefault(cell => cell.CurrentPiece == piece);
        PieceRecord?[,] boardData = SolverBase.ToBoardData(this);
        string fen = FENParser.BoardToFEN(boardData, Cells.GetLength(0), Cells.GetLength(1));

        var bitboard = BitboardHelper.FromFen(fen, Cells.GetLength(0), Cells.GetLength(1), CanQueenPromote);
        var index = ToIndex(originCell.X, originCell.Y, Cells.GetLength(0));
        var validMoves = bitboard.GetCandidateMoves(index).ToList();

        var movableCells = validMoves.Select(x => FromIndex(x.To, Cells.GetLength(0)))
            .Select(x => Cells[x.x, x.y]);
        return movableCells;
    }

    public static (int x, int y) FromIndex(int positionIndex, int boardSize)
    {
        return (positionIndex % boardSize, positionIndex / boardSize);
    }

    public static int ToIndex(int x, int y, int boardSize)
    {
        return y * boardSize + x;
    }

    public void PieceDropped(Piece movedPiece, Cell destinationCell)
    {
        if (destinationCell != null)
        {
            var originalCell = Cells.Cast<Cell>().FirstOrDefault(cell => cell.CurrentPiece == movedPiece);
            var (x, y) = FindCellIndex(Cells, destinationCell);
            if (originalCell != null)
            {
                IEnumerable<Cell> movableCells = GetMovabableCells(movedPiece);

                if (originalCell != destinationCell &&
                    movableCells.Contains(destinationCell))
                {
                    var capturedPiece = originalCell.CurrentPiece;
                    originalCell.SetPiece(null);

                    if (Echo)
                    {
                        if (destinationCell.CurrentPiece != null &&
                            destinationCell.CurrentPiece.PieceColor != capturedPiece.PieceColor)
                        {
                            //capture
                            Destroy(capturedPiece.gameObject);
                            destinationCell.CaptureEcho(movedPiece);

                            PieceMoved?.Invoke(capturedPiece, originalCell, movedPiece, destinationCell);
                        }
                        else
                        {
                            destinationCell.SetPiece(movedPiece);
                            PieceMoved?.Invoke(capturedPiece, originalCell, null, destinationCell);
                        }
                    }
                    else
                    {
                        if (destinationCell.CurrentPiece != null)
                        {
                            //capture
                            Destroy(destinationCell.CurrentPiece.gameObject);
                            destinationCell.SetPiece(movedPiece);
                            PieceMoved?.Invoke(movedPiece, originalCell, capturedPiece, destinationCell);
                        }
                        else
                        {
                            destinationCell.SetPiece(movedPiece, false);
                            PieceMoved?.Invoke(movedPiece, originalCell, capturedPiece, destinationCell);
                        }
                    }
                }
                else
                {
                    if (originalCell.CurrentPiece != null)
                    {
                        originalCell.ResetPiece();
                    }
                }
            };
        }
        else
        {
            var originalCell = Cells.Cast<Cell>().FirstOrDefault(cell => cell.CurrentPiece == movedPiece);
            originalCell.ResetPiece();
        }
    }

    public void PieceDroppedCanceled(Piece piece, Cell cell, string reason)
    {
        PieceCanceled?.Invoke(piece, cell, reason);
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
    public PieceType PieceType;
    public bool IsWhite;

    public PieceRecord(Cell cell)
    {
        this.X = cell.X;
        this.Y = cell.Y;
        this.PieceType = cell.CurrentPiece.PieceType;
        this.IsWhite = cell.CurrentPiece.PieceColor == ChessColor.w;
    }

    public PieceRecord(PieceType pieceMovement,
        bool isWhite,
        int x,
        int y)
    {
        this.X = x;
        this.Y = y;
        this.PieceType = pieceMovement;
        this.IsWhite = isWhite;
    }

    internal PieceRecord? Clone()
    {
        return new PieceRecord
        {
            X = this.X,
            Y = this.Y,
            PieceType = this.PieceType,
            IsWhite = this.IsWhite
        };
    }
}

public class Solution
{
    public List<(int x, int y)> Steps { get; internal set; }
    public PieceRecord StartingPiece { get; internal set; }
}