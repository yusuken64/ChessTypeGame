using System;
using System.Collections.Generic;
using System.Text;

public class FENParser
{
    public static string STANDARDGAMESETUP = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static FENData ParseFEN(string fen)
    {
        FENData fenData = new FENData
        {
            Pieces = new List<FenRecord>()
        };

        string[] parts = fen.Split(' ');
        string board = parts[0]; // The piece placement data
        fenData.ActiveColor = parts[1] == "w" ? ChessColor.w : ChessColor.b;
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
                    Player = char.IsUpper(c) ? ChessColor.w : ChessColor.b
                });
                x++; // Move to next file
            }
        }

        return fenData;
    }

    public static string BoardToFEN(
        PieceRecord?[,] pieceRecords,
        int colCount,
        int rowCount,
        string activeColor = "w",
        string castlingRights = "KQkq",
        string enPassant = "-",
        string halfMoveClock = "0",
        string fullMoveNumber = "1")
    {
        List<string> ranks = new List<string>();

        for (int row = 0; row < rowCount; row++) // Rank 8 to Rank 1 (0-indexed)
        {
            int emptyCount = 0;
            StringBuilder rankFEN = new StringBuilder();

            for (int col = 0; col < colCount; col++) // File 'a' to 'h'
            {
                PieceRecord? pieceMovement = pieceRecords[col, rowCount - 1 - row];
                if (pieceMovement.HasValue) // Piece exists
                {
                    if (emptyCount > 0)
                    {
                        rankFEN.Append(emptyCount); // Append empty count if any
                        emptyCount = 0;
                    }
                    rankFEN.Append(ToFEN(pieceMovement.Value.PieceType, pieceMovement.Value.IsWhite));
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
    private static char ToFEN(PieceType piece, bool isWhite)
    {
        return piece switch
        {
            PieceType.King => isWhite ? 'K' : 'k',
            PieceType.Queen => isWhite ? 'Q' : 'q',
            PieceType.Bishop => isWhite ? 'B' : 'b',
            PieceType.Rook => isWhite ? 'R' : 'r',
            PieceType.Knight => isWhite ? 'N' : 'n',
            PieceType.Pawn => isWhite ? 'P' : 'p',
            _ => throw new ArgumentException($"Invalid piece: {piece}"),
        };
    }
}
