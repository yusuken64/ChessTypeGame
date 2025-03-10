using System;
using System.Collections;
using System.Linq;
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
        var boardData = "8/3p4/8/8/8/8/P7/RNBQKBNR w KQkq - 0 1";
        var fen = FENParser.ParseFEN(boardData);
        var boardRecord = fen.Pieces.Select(x => new PieceRecord()
        {
            IsWhite = x.Player == ChessColor.w,
            PieceType = ToPieceMovement(x.Piece),
            X = x.X,
            Y = x.Y
        });

        Board.SetState(boardRecord);
        Board.PieceMoved += Board_PieceMoved;

        PieceRecord?[,] boardData2 = Solver.ToBoardData(Board);
        var what = FENParser.BoardToFEN(boardData2, 8, 8);
    }

    private void OnDestroy()
    {
        Board.PieceMoved -= Board_PieceMoved;
    }

    private void Board_PieceMoved(Piece movedPiece, Piece capturedPiece)
    {
        if (movedPiece.PieceColor == ChessColor.w)
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

    private PieceType ToPieceMovement(char piece)
    {
        switch (char.ToLower(piece)) // Normalize input to lowercase
        {
            case 'k': return PieceType.King;
            case 'q': return PieceType.Queen;
            case 'b': return PieceType.Bishop;
            case 'r': return PieceType.Rook;
            case 'n': return PieceType.Knight;
            case 'p': return PieceType.Pawn;
            default:
                return PieceType.Pawn;
        }
    }
}
