using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SolverBase
{
    public static (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) CheckGameOver(ChessGameRecord game, ChessColor player)
    {
        return game.CheckGameOver(player);
    }

    public static IEnumerable<Move> GetLegalMoves(ChessGameRecord game, ChessColor player)
    {
        return game.FenData.Pieces
            .Where(x => x.Player == player)
            .SelectMany(piece => game.GetLegalMoves((piece.X, piece.Y)));
    }

    public static ChessColor OtherColor(ChessColor color)
    {
        return color == ChessColor.w ? ChessColor.b : ChessColor.w;
    }
    public static PieceRecord?[,] ToBoardData(Board board)
    {
        PieceRecord?[,] boardData = new PieceRecord?[board.Cells.GetLength(0), board.Cells.GetLength(1)];
        for (int i = 0; i < board.Cells.GetLength(0); i++)
        {
            for (int j = 0; j < board.Cells.GetLength(1); j++)
            {
                Cell cell = board.Cells[i, j];
                if (cell.CurrentPiece != null)
                {
                    boardData[i, j] = new PieceRecord(cell);
                }
            }

        }

        return boardData;
    }
    public abstract Move GetNextMove(ChessGameRecord game, ChessColor color, IEnumerable<Move> legalMoves);
}
