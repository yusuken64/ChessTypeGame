using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SolverBase
{
    public static IEnumerable<Move> GetLegalMoves(ChessBitboard bitboard, ChessColor player)
    {
        return bitboard.GetAllPieces()
            .Where(x => x.color == player)
            .SelectMany(piece => BitboardHelper.GetLegalMovesForPosition(ref bitboard, Board.FromIndex(piece.position, bitboard._fileMax)));
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
    public abstract Move GetNextMove(ChessBitboard chessBitboard, ChessColor color, IEnumerable<Move> legalMoves);
}
