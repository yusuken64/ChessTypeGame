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

    public delegate void PieceMovedDelegate(Piece movedPiece, Cell originalCell, Piece capturedPiece, Cell newCell);
    public PieceMovedDelegate PieceMoved;

    public delegate void PieceCanceledDelegate(Piece movedPiece, Cell originalCell, string reason);
    public PieceCanceledDelegate PieceCanceled;

    public Solution Solution { get; private set; }

    //check if it causes check
    internal bool CanDrop(Piece piece, Cell cell, out string reason)
    {
        reason = string.Empty;
        if (cell == null) { return false; }
        if (Echo) { return true; }

        var originalCell = Cells.OfType<Cell>().First(x => x.CurrentPiece == piece);
        PieceRecord?[,] boardData = Solver.ToBoardData(this);
        var originalRecord = boardData[originalCell.X, originalCell.Y];
        boardData[originalCell.X, originalCell.Y] = null;
        boardData[cell.X, cell.Y] = originalRecord;

        string fen = FENParser.BoardToFEN(boardData, boardData.GetLength(0), boardData.GetLength(1));
        ChessGameRecord game = new ChessGameRecord(fen, boardData.GetLength(0), boardData.GetLength(1));
        var isInCheck = game.IsInCheck(piece.PieceColor);

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

    public void ReinitializeData()
    {
        string data = ",,,,," +
                      ",,,,," +
                      ",,,,," +
                      ",BR,BB,BN,," +
                      ",BQ,WP,BN,";

        string[] csv = data.Split(',');

        IEnumerable<PieceRecord?> boardRecord = csv.Select((value, index) =>
        {
            if (value.Length == 0) { return null; }

            PieceType pieceMovement = default;
            char letter = value[1];
            switch (letter)
            {
                case 'K':
                    pieceMovement = PieceType.King;
                    break;
                case 'Q':
                    pieceMovement = PieceType.Queen;
                    break;
                case 'N':
                    pieceMovement = PieceType.Knight;
                    break;
                case 'B':
                    pieceMovement = PieceType.Bishop;
                    break;
                case 'R':
                    pieceMovement = PieceType.Rook;
                    break;
                case 'P':
                    pieceMovement = PieceType.Pawn;
                    break;
            }

            int x = index % 5;
            int y = 4 - index / 5;

            return new PieceRecord?(new PieceRecord(
                pieceMovement,
                value.StartsWith('W'),
                x,
                y
            ));
        });

        SetState(boardRecord.OfType<PieceRecord>());
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
        PieceRecord?[,] boardData2 = Solver.ToBoardData(this);
        string fen = FENParser.BoardToFEN(boardData2, Cells.GetLength(0), Cells.GetLength(1));
        ChessGameRecord game = new ChessGameRecord(fen, Cells.GetLength(0), Cells.GetLength(1));
        IEnumerable<Move> validMoves = game.GetCandidateMoves((originCell.X, originCell.Y));
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

    public void PieceDropped(Piece piece, Cell cell)
    {
        if (cell != null)
        {
            var originalCell = Cells.Cast<Cell>().FirstOrDefault(cell => cell.CurrentPiece == piece);
            var (x, y) = FindCellIndex(Cells, cell);
            var newCell = Cells[x, y];
            if (originalCell != null)
            {
                IEnumerable<Cell> movableCells = GetMovabableCells(piece);

                if (originalCell != newCell &&
                    movableCells.Contains(newCell))
                {
                    var originalPiece = originalCell.CurrentPiece;
                    originalCell.SetPiece(null);

                    if (Echo)
                    {
                        if (newCell.CurrentPiece != null &&
                            newCell.CurrentPiece.PieceColor != originalPiece.PieceColor)
                        {
                            //capture
                            Destroy(originalPiece.gameObject);
                            newCell.CaptureEcho(piece);

                            PieceMoved?.Invoke(originalPiece, originalCell, piece, newCell);
                        }
                        else
                        {
                            newCell.SetPiece(piece);
                            PieceMoved?.Invoke(originalPiece, originalCell, null, newCell);
                        }
                    }
                    else
                    {
                        if (newCell.CurrentPiece != null &&
                            newCell.CurrentPiece.PieceColor != originalPiece.PieceColor)
                        {
                            //capture
                            Destroy(newCell.CurrentPiece.gameObject);
                            newCell.Capture(piece);
                            PieceMoved?.Invoke(originalPiece, originalCell, piece, newCell);
                        }
                        else
                        {
                            newCell.SetPiece(piece, false);
                            PieceMoved?.Invoke(originalPiece, originalCell, null, newCell);
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
            var originalCell = Cells.Cast<Cell>().FirstOrDefault(cell => cell.CurrentPiece == piece);
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