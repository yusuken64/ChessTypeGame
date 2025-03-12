using System;
using System.Collections.Generic;
using System.Text;

public class FENParser
{
    public static string STANDARDGAMESETUP = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static FENData ParseFEN(string fen, int rankMax, int fileMax)
    {
        if (fen == null || fen.Length < 15) // Quick check for basic validity
            throw new ArgumentException("FEN string is too short or null.");

        ReadOnlySpan<char> parts = fen.AsSpan();
        int spaceCount = 0;
        Span<int> spaceIndices = stackalloc int[6];

        for (int i = 0; i < parts.Length && spaceCount < 6; i++)
        {
            if (parts[i] == ' ') spaceIndices[spaceCount++] = i;
        }
        if (spaceCount < 5) throw new ArgumentException("Invalid FEN format: Missing required fields.");

        FENData fenData = new FENData { Pieces = new List<FenRecord>() };
        ReadOnlySpan<char> board = parts.Slice(0, spaceIndices[0]);
        ReadOnlySpan<char> activeColorSpan = parts.Slice(spaceIndices[0] + 1, 1);
        ReadOnlySpan<char> castlingRights = parts.Slice(spaceIndices[1] + 1, spaceIndices[2] - spaceIndices[1] - 1);
        ReadOnlySpan<char> enPassant = parts.Slice(spaceIndices[2] + 1, spaceIndices[3] - spaceIndices[2] - 1);
        ReadOnlySpan<char> halfMoveClockSpan = parts.Slice(spaceIndices[3] + 1, spaceIndices[4] - spaceIndices[3] - 1);
        ReadOnlySpan<char> fullMoveNumberSpan = parts.Slice(spaceIndices[4] + 1, spaceCount == 6 ? spaceIndices[5] - spaceIndices[4] - 1 : parts.Length - spaceIndices[4] - 1);

        fenData.ActiveColor = activeColorSpan[0] == 'w' ? ChessColor.w : ChessColor.b;
        fenData.CastlingRights = castlingRights.ToString();
        fenData.EnPassant = enPassant.ToString();

        if (!int.TryParse(halfMoveClockSpan, out int halfMoveClock) || !int.TryParse(fullMoveNumberSpan, out int fullMoveNumber) || fullMoveNumber < 1)
            throw new ArgumentException("Invalid half-move clock or full-move number.");

        fenData.HalfMoveClock = halfMoveClock.ToString();
        fenData.FullMoveNumber = fullMoveNumber.ToString();

        int y = rankMax - 1, x = 0;
        for (int i = 0; i < board.Length; i++)
        {
            char c = board[i];
            if (c == '/')
            {
                if (x != fileMax) throw new ArgumentException("Invalid FEN: Incorrect number of squares in a rank.");
                y--; x = 0;
            }
            else if (c >= '1' && c <= '8')
            {
                x += c - '0';
                if (x > fileMax) throw new ArgumentException("Invalid FEN: Too many squares in a rank.");
            }
            else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
            {
                if (x >= fileMax || y < 0) throw new ArgumentException("Invalid FEN: Piece placement out of bounds.");
                fenData.Pieces.Add(new FenRecord { X = x++, Y = y, Piece = char.ToUpper(c), Player = c < 'a' ? ChessColor.w : ChessColor.b });
            }
            else throw new ArgumentException("Invalid FEN: Unexpected character in board setup.");
        }

        if (y != 0 || x != fileMax) throw new ArgumentException("Invalid FEN: Board does not have the correct number of squares.");
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
        StringBuilder fenBuilder = new StringBuilder(rowCount * (colCount + 1)); // Preallocate reasonable size

        for (int row = 0; row < rowCount; row++) // Rank 8 to Rank 1 (0-indexed)
        {
            int emptyCount = 0;

            for (int col = 0; col < colCount; col++) // File 'a' to 'h'
            {
                ref PieceRecord? pieceMovement = ref pieceRecords[col, rowCount - 1 - row]; // Avoid array bound checks
                if (pieceMovement.HasValue)
                {
                    if (emptyCount > 0)
                    {
                        fenBuilder.Append((char)('0' + emptyCount)); // Avoid .ToString() allocation
                        emptyCount = 0;
                    }
                    fenBuilder.Append(ToFEN(pieceMovement.Value.PieceType, pieceMovement.Value.IsWhite));
                }
                else
                {
                    emptyCount++;
                }
            }
            if (emptyCount > 0)
                fenBuilder.Append((char)('0' + emptyCount)); // Append trailing empty squares

            if (row != rowCount - 1) // Avoid extra trailing slash
                fenBuilder.Append('/');
        }

        // Combine all components
        fenBuilder.Append(' ').Append(activeColor)
            .Append(' ').Append(castlingRights)
            .Append(' ').Append(enPassant)
            .Append(' ').Append(halfMoveClock)
            .Append(' ').Append(fullMoveNumber);

        return fenBuilder.ToString();
    }

    // Converts a PieceMovement to FEN character
    public static char ToFEN(PieceType piece, bool isWhite)
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
