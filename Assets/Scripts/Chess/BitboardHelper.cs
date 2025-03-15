using System;
using System.Collections.Generic;
using System.Linq;

public static class BitboardHelper
{
    public static IEnumerable<Move> GetCandidateMoves(
        ref ChessBitboard bitboard,
        (int x, int y) position)
    {
        int positionIndex = position.y * bitboard._rankMax + position.x;
        return bitboard.GetCandidateMoves(positionIndex);
    }

    public static IEnumerable<Move> GetLegalMovesForPosition(
        ref ChessBitboard bitboard, 
        (int x, int y) position)
    {
        int positionIndex = position.y * bitboard._rankMax + position.x;
        var candidateMoves = bitboard.GetCandidateMoves(positionIndex);

        // Create a copy of the board state
        var boardCopy = bitboard.Clone();
        return candidateMoves.Where(move => IsMoveLegal(move, boardCopy));
    }

    internal static bool IsMoveLegal(Move move, ChessBitboard boardCopy)
    {
        var kingColor = boardCopy.IsAlliedPieceAt(move.From, ChessColor.w) ? ChessColor.w : ChessColor.b;

        // Apply the move to the copied board
        var newBoard = boardCopy.MakeMove(move);

        // Determine if the current player's king is in check after the move
        return !newBoard.IsKingInCheck(kingColor);
    }

    public static (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) CheckGameOver(
        ref ChessBitboard bitboard,
        ChessColor player)
    {
        return bitboard.CheckGameOver(player);
    }

    public static ChessBitboard MakeMove(ref ChessBitboard bitboard, Move move)
    {
        return bitboard.MakeMove(move);
    }

    public static bool IsCapture(ref ChessBitboard bitboard, 
        Move move, ChessColor currentPlayer)
    {
        return bitboard.IsEnemyPieceAt(move.To, currentPlayer);
    }

    public static PieceType? GetPieceAt( ref   ChessBitboard bitboard,
        int position,
        ChessColor currentPlayer)
    {
        return bitboard.GetPieceAt(position, currentPlayer);
    }

    //this should only be called 1 time per move for performance reasons
    //to go from Unity to Bitboard
    internal static ChessBitboard FromFen(string fen, int rankMax, int fileMax, bool promote)
    {
        // Initialize the bitboards to 0 for each piece
        ulong whitePawns = 0, blackPawns = 0;
        ulong whiteKnights = 0, blackKnights = 0;
        ulong whiteBishops = 0, blackBishops = 0;
        ulong whiteRooks = 0, blackRooks = 0;
        ulong whiteQueens = 0, blackQueens = 0;
        ulong whiteKings = 0, blackKings = 0;

        // Split the FEN string into parts: the first part contains the board layout
        var spaceIndex = fen.IndexOf(' ');
        if (spaceIndex == -1) throw new ArgumentException("Invalid FEN format.");

        string boardData = fen[..spaceIndex]; // Get only board layout
        var boardLayout = boardData.Split('/');

        if (boardLayout.Length != rankMax)
            throw new ArgumentException("Invalid FEN: Rank count mismatch.");

        int rankOffset = rankMax - 1;
        for (int rank = 0; rank < rankMax; rank++)
        {
            string rankString = boardLayout[rank];
            int file = 0;

            for (int i = 0; i < rankString.Length; i++)
            {
                char c = rankString[i];
                if (c >= '1' && c <= '8') // Numeric empty squares
                {
                    file += c - '0'; // Skip the number of empty squares
                }
                else // Piece present
                {
                    // Calculate the position for the bitboard
                    int position = (rankOffset - rank) * fileMax + file;

                    // Set the bit for the piece
                    switch (c)
                    {
                        case 'P': // White Pawn
                            whitePawns |= (1UL << position);
                            break;
                        case 'p': // Black Pawn
                            blackPawns |= (1UL << position);
                            break;
                        case 'N': // White Knight
                            whiteKnights |= (1UL << position);
                            break;
                        case 'n': // Black Knight
                            blackKnights |= (1UL << position);
                            break;
                        case 'B': // White Bishop
                            whiteBishops |= (1UL << position);
                            break;
                        case 'b': // Black Bishop
                            blackBishops |= (1UL << position);
                            break;
                        case 'R': // White Rook
                            whiteRooks |= (1UL << position);
                            break;
                        case 'r': // Black Rook
                            blackRooks |= (1UL << position);
                            break;
                        case 'Q': // White Queen
                            whiteQueens |= (1UL << position);
                            break;
                        case 'q': // Black Queen
                            blackQueens |= (1UL << position);
                            break;
                        case 'K': // White King
                            whiteKings |= (1UL << position);
                            break;
                        case 'k': // Black King
                            blackKings |= (1UL << position);
                            break;
                        default:
                            throw new ArgumentException($"Invalid piece character in FEN: {c}");
                    }

                    file++; // Move to the next file
                }
            }
        }

        return new ChessBitboard(
            whitePawns, blackPawns, whiteKnights, blackKnights,
            whiteBishops, blackBishops, whiteRooks, blackRooks,
            whiteQueens, blackQueens, whiteKings, blackKings,
            rankMax, fileMax, promote);
    }
}
