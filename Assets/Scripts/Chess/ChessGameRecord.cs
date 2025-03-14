using System;
using System.Collections.Generic;
using System.Linq;

public struct ChessGameRecord
{
    public ChessGameRecord(string fen, int rankMax, int fileMax, bool promote)
    {
        this.fen = fen;
        this.rankMax = rankMax;
        this.fileMax = fileMax;
        this.canPawnPromote = promote;
        FenData = FENParser.ParseFEN(fen, rankMax, fileMax);
        ChessBitboard = new ChessBitboard(fen, rankMax, fileMax, promote);
    }

    public string fen;
    public FENData FenData;
    public ChessBitboard ChessBitboard;

    public readonly int rankMax;
    public readonly int fileMax;
    public readonly bool canPawnPromote;

    internal IEnumerable<Move> GetCandidateMoves((int x, int y) position)
    {
        int positionIndex = position.y * fileMax + position.x;
        return ChessBitboard.GetCandidateMoves(positionIndex);
    }

    internal IEnumerable<Move> GetLegalMoves((int x, int y) position)
    {
        int positionIndex = position.y * fileMax + position.x;
        var candidateMoves = ChessBitboard.GetCandidateMoves(positionIndex);

        // Create a copy of the board state
        var boardCopy = ChessBitboard.Clone();
        return candidateMoves.Where(move => IsMoveLegal(move, boardCopy));
    }

    internal static bool IsMoveLegal(Move move, ChessBitboard boardCopy)
    {
        var kingColor = boardCopy.IsAlliedPieceAt(move.From, ChessColor.w) ? ChessColor.w : ChessColor.b;

        // Apply the move to the copied board
        boardCopy.MakeMove(move);

        // Determine if the current player's king is in check after the move
        return !boardCopy.IsKingInCheck(kingColor);
    }


    internal bool IsInCheck(ChessColor whoseTurn)
    {
        return ChessBitboard.IsKingInCheck(whoseTurn);
    }

    internal (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) CheckGameOver(ChessColor player)
    {
        return ChessBitboard.CheckGameOver(player);
    }

    internal void MakeMove(Move move)
    {
        ChessBitboard.MakeMove(move);
    }

    internal bool IsCapture(Move move, ChessColor currentPlayer)
    {
        return ChessBitboard.IsEnemyPieceAt(move.To, currentPlayer);
    }

    internal PieceType? GetPieceAt(int position, ChessColor currentPlayer)
    {
        return ChessBitboard.GetPieceAt(position, currentPlayer);
    }
}
