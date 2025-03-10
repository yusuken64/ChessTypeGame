using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public PieceColor PieceColor;
    public SpriteRenderer CurrentSprite;
    public Sprite WhiteSprite;
    public Sprite BlackSprite;

    public PieceMovement PieceMovement;

    private void Start()
    {
        var draggable = GetComponent<Draggable>();
        draggable.OnHold = () =>
        {
            if (this.PieceColor == PieceColor.White)
            {
                var board = FindObjectOfType<Board>();
                board.PiecePickedUp(this);
            }
            else
            {
                //can't pick up
            }
        };
        draggable.OnReleased = (cell) =>
        {
            var board = FindObjectOfType<Board>();
            board.PieceDropped(this, cell);
        };
    }

    internal List<Cell> GetMovableSquares(Cell[,] cells, Cell originCell, PieceColor pieceColor, bool attackOnly = false)
    {
        var (x, y) = Board.FindCellIndex(cells, originCell);

        PieceRecord?[,] records = new PieceRecord?[cells.GetLength(0), cells.GetLength(1)];
        foreach(var cell in cells.Cast<Cell>().Where(x => x.CurrentPiece != null))
        {
            records[cell.X, cell.Y] = new PieceRecord()
            {
                X = cell.X,
                Y = cell.Y,
                PieceMovement = cell.CurrentPiece.PieceMovement,
                IsWhite = cell.CurrentPiece.PieceColor == PieceColor.White
            };
        }
        List<(int x, int y)> movements = GetByMovement(records, PieceMovement, x, y, pieceColor, attackOnly);
        
        var ret = new List<Cell>();
        foreach(var movement in movements)
        {
            ret.Add(cells[movement.x, movement.y]);
        }
        return ret;
    }

    public static List<(int x, int y)> GetByMovement(
        PieceRecord?[,] cells,
        PieceMovement pieceMovement,
        int x,
        int y,
        PieceColor pieceColor,
        bool attackOnly)
    {
        List<(int x, int y)> returnCells = null;
        switch (pieceMovement)
        {
            case PieceMovement.King:
                returnCells = GetKingMovement(cells, x, y, pieceColor, attackOnly);
                break;
            case PieceMovement.Queen:
                returnCells = GetQueenMovement(cells, x, y, pieceColor, attackOnly);
                break;
            case PieceMovement.Bishop:
                returnCells = GetBishopMovement(cells, x, y, pieceColor, attackOnly);
                break;
            case PieceMovement.Rook:
                returnCells = GetRookMovement(cells, x, y, pieceColor, attackOnly);
                break;
            case PieceMovement.Knight:
                returnCells = GetKnightMovement(cells, x, y, pieceColor, attackOnly);
                break;
            case PieceMovement.Pawn:
                returnCells = GetPawnMovement(cells, x, y, pieceColor, attackOnly);
                break;
        }

        return returnCells;
    }

    private static List<(int x, int y)> GetKingMovement(PieceRecord?[,] pieces, int x, int y, PieceColor pieceColor, bool attackOnly)
    {
        var list = new List<(int x, int y)>();
        (int dx, int dy)[] directions =
        {
            (1, 1), (0, 1), (-1, 1),
            (1, 0),        (-1, 0),
            (1, -1), (0, -1), (-1, -1)
        };

        foreach (var (dx, dy) in directions)
        {
            int newX = x + dx, newY = y + dy;

            if (GetMovableCell(pieces, newX, newY) && (!attackOnly || pieces[newX, newY] == null))
            {
                list.Add((newX, newY));
            }
        }

        return list;
    }

    private static List<(int x, int y)> GetQueenMovement(PieceRecord?[,] pieces, int x, int y, PieceColor pieceColor, bool attackOnly)
    {
        List<(int x, int y)> moves = new List<(int x, int y)>();

        AddMovesInDirection(pieces, x, y, 1, 0, moves, pieceColor, attackOnly); // Right
        AddMovesInDirection(pieces, x, y, -1, 0, moves, pieceColor, attackOnly); // Left
        AddMovesInDirection(pieces, x, y, 0, 1, moves, pieceColor, attackOnly); // Up
        AddMovesInDirection(pieces, x, y, 0, -1, moves, pieceColor, attackOnly); // Down
        AddMovesInDirection(pieces, x, y, 1, 1, moves, pieceColor, attackOnly); // Top-right
        AddMovesInDirection(pieces, x, y, -1, 1, moves, pieceColor, attackOnly); // Top-left
        AddMovesInDirection(pieces, x, y, 1, -1, moves, pieceColor, attackOnly); // Bottom-right
        AddMovesInDirection(pieces, x, y, -1, -1, moves, pieceColor, attackOnly); // Bottom-left

        return moves;
    }

    private static List<(int x, int y)> GetBishopMovement(PieceRecord?[,] pieces, int x, int y, PieceColor pieceColor, bool attackOnly)
    {
        List<(int x, int y)> moves = new List<(int x, int y)>();

        AddMovesInDirection(pieces, x, y, 1, 1, moves, pieceColor, attackOnly); // Top-right
        AddMovesInDirection(pieces, x, y, -1, 1, moves, pieceColor, attackOnly); // Top-left
        AddMovesInDirection(pieces, x, y, 1, -1, moves, pieceColor, attackOnly); // Bottom-right
        AddMovesInDirection(pieces, x, y, -1, -1, moves, pieceColor, attackOnly); // Bottom-left

        return moves;
    }

    private static List<(int x, int y)> GetRookMovement(PieceRecord?[,] pieces, int x, int y, PieceColor pieceColor, bool attackOnly)
    {
        List<(int x, int y)> moves = new List<(int x, int y)>();

        AddMovesInDirection(pieces, x, y, 1, 0, moves, pieceColor, attackOnly); // Right
        AddMovesInDirection(pieces, x, y, -1, 0, moves, pieceColor, attackOnly); // Left
        AddMovesInDirection(pieces, x, y, 0, 1, moves, pieceColor, attackOnly); // Up
        AddMovesInDirection(pieces, x, y, 0, -1, moves, pieceColor, attackOnly); // Down

        return moves;
    }

    private static List<(int x, int y)> GetKnightMovement(PieceRecord?[,] pieces, int x, int y, PieceColor pieceColor, bool attackOnly)
    {
        var list = new List<(int x, int y)>();
        (int dx, int dy)[] directions =
        {
            (+1, +2),
            (+1, -2),
            (-1, +2),
            (-1, -2),
            (+2, +1),
            (+2, -1),
            (-2, +1),
            (-2, -1),
        };

        foreach (var (dx, dy) in directions)
        {
            int newX = x + dx, newY = y + dy;

            if (GetMovableCell(pieces, newX, newY) && (!attackOnly || pieces[newX, newY] == null))
            {
                list.Add((newX, newY));
            }
        }

        return list;
    }

    private static List<(int x, int y)> GetPawnMovement(PieceRecord?[,] cells, int x, int y, PieceColor? pieceColor, bool attackOnly)
    {
        List<(int x, int y)> moves = new List<(int x, int y)>();

        int yOffset = pieceColor == PieceColor.White ? 1 : -1;
        int yTarget = y + yOffset;
        if (!attackOnly)
        {
            if (IsMovable(cells, x, yTarget) &&
                cells[x, yTarget] == null)
            {
                moves.Add(new (x, yTarget));
            }
            if (IsMovable(cells, x + 1, yTarget) &&
                cells[x + 1, yTarget] != null &&
                cells[x + 1, yTarget]?.IsWhite != (pieceColor == PieceColor.White))
            {
                moves.Add(new(x + 1, yTarget));
            }

            if (IsMovable(cells, x - 1, yTarget) &&
                cells[x - 1, yTarget] != null &&
                cells[x - 1, yTarget]?.IsWhite != (pieceColor == PieceColor.White))
            {
                moves.Add(new(x - 1, yTarget));
            }
        }
        else
        {
            if (IsMovable(cells, x + 1, yTarget) &&
                cells[x + 1, yTarget] == null)
            {
                moves.Add(new(x + 1, yTarget));
            }

            if (IsMovable(cells, x - 1, yTarget) &&
                cells[x - 1, yTarget] == null)
            {
                moves.Add(new(x - 1, yTarget));
            }
        }

        return moves;
    }

    // Helper method to add valid moves in a specific direction
    private static void AddMovesInDirection(
        PieceRecord?[,] cells,
        int x,
        int y,
        int dx,
        int dy,
        List<(int x, int y)> moves,
        PieceColor pieceColor,
        bool attackOnly)
    {
        int i = 1;
        while (IsMovable(cells, x + i * dx, y + i * dy))
        {
            if (cells[x + i * dx, y + i * dy] != null)
            {
                if (cells[x + i * dx, y + i * dy].Value.IsWhite == (pieceColor == PieceColor.White) ||
                    attackOnly)
                {
                    break;
                }
                else
                {
                    moves.Add(new (x + i * dx, y + i * dy));
                    break;
                }
            }
            else
            {
                moves.Add(new(x + i * dx, y + i * dy));
                i++;
            }
        }
    }

    // checks if in bound and not a wall
    private static bool IsMovable(PieceRecord?[,] cells, int x, int y)
    {
        if (IsInBound(cells, x, y))
        {
            var cell = cells[x, y];
            //return !cell.IsWall; //TODO adapt records for walls
            return true;
        }

        return false;
    }

    private static bool IsInBound<T>(T[,] cells, int col, int row)
    {
        return row >= 0 && row < cells.GetLength(0) && col >= 0 && col < cells.GetLength(1);
    }

    internal void SetColor(PieceColor pieceColor)
    {
        this.PieceColor = pieceColor;
        if (pieceColor == PieceColor.White)
        {
            CurrentSprite.sprite = WhiteSprite;
            var draggable = GetComponent<Draggable>();
            draggable.IsDraggable = true;
        }
        else
        {
            CurrentSprite.sprite = BlackSprite;
            var draggable = GetComponent<Draggable>();
            draggable.IsDraggable = false;
        }
    }

    public static bool GetMovableCell(PieceRecord?[,] array, int row, int col)
    {
        if (IsMovable(array, row, col))
        {
            return true;
        }
        return false;
    }
}

public enum PieceColor
{
    White,
    Black
}

public enum PieceMovement
{
    King,
    Queen,
    Bishop,
    Rook,
    Knight,
    Pawn
}