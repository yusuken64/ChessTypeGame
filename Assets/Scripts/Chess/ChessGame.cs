using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ChessGame : MonoBehaviour
{
    public Board Board;
    public Solver Solver;

    /// <summary>
    /// White Pieces:
    //Pawn: P
    //Knight: N
    //Bishop: B
    //Rook: R
    //Queen: Q
    //King: K
    //Black Pieces:
    //Pawn: p
    //Knight: n
    //Bishop: b
    //Rook: r
    //Queen: q
    //King: k

    //rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
    /// </summary>
    void Start()
    {
        //var boardData = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        var boardData = "8/3p4/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        var fen = FENParser.ParseFEN(boardData);
        var boardRecord = fen.Pieces.Select(x => new PieceRecord()
        {
            IsWhite = x.Player == Player.w,
            PieceMovement = ToPieceMovement(x.Piece),
            X = x.X,
            Y = x.Y
        });

        Board.SetState(boardRecord);
        Board.PieceMoved += Board_PieceMoved;

        PieceRecord?[,] boardData2 = Solver.ToBoardData(Board);
        var what = FENParser.BoardToFEN(boardData2);
    }

    private void OnDestroy()
    {
        Board.PieceMoved -= Board_PieceMoved;
    }

    private void Board_PieceMoved(Piece movedPiece, Piece capturedPiece)
    {
        if (movedPiece.PieceColor == PieceColor.White)
        {
            DoBlackTurn(capturedPiece);
        }
    }

    private void DoBlackTurn(Piece piece)
    {
        var move = Solver.GetNextMove(Board);

        (int fromX, int fromY) from = FromIndex(move.From);
        (int toX, int toY) to = FromIndex(move.To);
        var oldPiece = Board.Cells[from.fromX, from.fromY].CurrentPiece;

        Board.PieceDropped(oldPiece, Board.Cells[to.toX, to.toY]);
    }

    public (int x, int y) FromIndex(int positionIndex)
    {
        return (positionIndex % 8, positionIndex / 8);
    }

    private PieceMovement ToPieceMovement(char piece)
    {
        switch (char.ToLower(piece)) // Normalize input to lowercase
        {
            case 'k': return PieceMovement.King;
            case 'q': return PieceMovement.Queen;
            case 'b': return PieceMovement.Bishop;
            case 'r': return PieceMovement.Rook;
            case 'n': return PieceMovement.Knight;
            case 'p': return PieceMovement.Pawn;
            default:
                return PieceMovement.Pawn;
        }
    }

    internal void MakeMove(Move move, bool v)
    {
        throw new NotImplementedException();
    }
}

public class FENParser
{
    public static FENData ParseFEN(string fen)
    {
        FENData fenData = new FENData
        {
            Pieces = new List<FenRecord>()
        };

        string[] parts = fen.Split(' ');
        string board = parts[0]; // The piece placement data
        fenData.ActiveColor = parts[1];
        fenData.CastlingRights = parts[2];
        fenData.EnPassant = parts[3];
        fenData.HalfMoveClock = parts[4];
        fenData.FullMoveNumber = parts[5];

        int y = 7; // Start from rank 8 (FEN starts from the top row)
        int x = 0;

        foreach (char c in board)
        {
            if (c == '/')
            {
                y--; // Move to next rank
                x = 0; // Reset file position
            }
            else if (char.IsDigit(c))
            {
                x += c - '0'; // Empty squares
            }
            else
            {
                fenData.Pieces.Add(new FenRecord
                {
                    X = x,
                    Y = y,
                    Piece = char.ToUpper(c),
                    Player = char.IsUpper(c) ? Player.w : Player.b
                });
                x++; // Move to next file
            }
        }

        return fenData;
    }

    public static string BoardToFEN(
        PieceRecord?[,] pieceRecords,
        string activeColor = "w",
        string castlingRights = "KQkq",
        string enPassant = "-",
        string halfMoveClock = "0",
        string fullMoveNumber = "1")
    {
        List<string> ranks = new List<string>();

        for (int row = 0; row < 8; row++) // Rank 8 to Rank 1 (0-indexed)
        {
            int emptyCount = 0;
            StringBuilder rankFEN = new StringBuilder();

            for (int col = 0; col < 8; col++) // File 'a' to 'h'
            {
                PieceRecord? pieceMovement = pieceRecords[col, 7 - row];
                if (pieceMovement.HasValue) // Piece exists
                {
                    if (emptyCount > 0)
                    {
                        rankFEN.Append(emptyCount); // Append empty count if any
                        emptyCount = 0;
                    }
                    rankFEN.Append(ToFEN(pieceMovement.Value.PieceMovement, pieceMovement.Value.IsWhite));
                }
                else
                {
                    emptyCount++;
                }
            }
            if (emptyCount > 0) rankFEN.Append(emptyCount); // Append trailing empty squares

            ranks.Add(rankFEN.ToString());
        }
        string piecePlacement = string.Join("/", ranks);

        // Combine all components
        return $"{piecePlacement} {activeColor} {castlingRights} {enPassant} {halfMoveClock} {fullMoveNumber}";
    }

    // Converts a PieceMovement to FEN character
    private static char ToFEN(PieceMovement piece, bool isWhite)
    {
        return piece switch
        {
            PieceMovement.King => isWhite ? 'K' : 'k',
            PieceMovement.Queen => isWhite ? 'Q' : 'q',
            PieceMovement.Bishop => isWhite ? 'B' : 'b',
            PieceMovement.Rook => isWhite ? 'R' : 'r',
            PieceMovement.Knight => isWhite ? 'N' : 'n',
            PieceMovement.Pawn => isWhite ? 'P' : 'p',
            _ => throw new ArgumentException($"Invalid piece: {piece}"),
        };
    }
}

public class FENData
{
    public List<FenRecord> Pieces { get; set; }
    public string ActiveColor { get; set; }
    public string CastlingRights { get; set; }
    public string EnPassant { get; set; }
    public string HalfMoveClock { get; set; }
    public string FullMoveNumber { get; set; }
}

public struct FenRecord
{
    public int X { get; internal set; }
    public int Y { get; internal set; }
    public char Piece { get; internal set; }
    public Player Player { get; internal set; }
}