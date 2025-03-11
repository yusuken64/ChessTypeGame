using System.Collections.Generic;

public struct ChessGameRecord
{
    public ChessGameRecord(string fen, int rankMax, int fileMax)
    {
        this.fen = fen;
        this.rankMax = rankMax;
        this.fileMax = fileMax;
        FenData = FENParser.ParseFEN(fen, rankMax, fileMax);
        ChessBitboard = new ChessBitboard(fen, rankMax, fileMax);
    }

    public string fen;
    public FENData FenData;
    public ChessBitboard ChessBitboard;

    public readonly int rankMax;
    public readonly int fileMax;

    internal IEnumerable<Move> GetValidMoves((int x, int y) position)
    {
        int positionIndex = position.y * fileMax + position.x;
        return ChessBitboard.GetValidMoves(positionIndex);
    }

    internal IEnumerable<Move> GetValidMoves(PieceType pieceType, (int x, int y) position)
    {
        int positionIndex = position.y * fileMax + position.x;
        return ChessBitboard.GetValidMoves(pieceType, positionIndex);
    }

    internal IEnumerable<Move> GetPlacableSquares(PieceType pieceType, (int x, int y) position)
    {
        int positionIndex = position.y * fileMax + position.x;
        return ChessBitboard.GetPlacableSquares(pieceType, positionIndex);
    }

    internal bool IsInCheck(ChessColor whoseTurn)
    {
        return ChessBitboard.IsKingInCheck(whoseTurn);
    }

    internal bool IsInCheckmate(ChessColor player)
    {
        return ChessBitboard.IsInCheckMate(player);
    }

    internal void MakeMove(Move move)
    {
        ChessBitboard.MakeMove(move);
    }
}
