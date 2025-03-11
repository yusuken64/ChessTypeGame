using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ChessGame : MonoBehaviour
{
    public Board Board;
    public Solver Solver;

    void Start()
    {
        ResetGame();
        Board.PieceMoved += Board_PieceMoved;
    }

    public void ResetGame()
    {
        //var boardData = "8/3p4/8/8/8/8/P7/RNBQKBNR w KQkq - 0 1";
        var boardData = FENParser.STANDARDGAMESETUP;
        var fenData = FENParser.ParseFEN(boardData, 8, 8);
        var boardRecord = fenData.Pieces.Select(x => new PieceRecord()
        {
            IsWhite = x.Player == ChessColor.w,
            PieceType = ToPieceType(x.Piece),
            X = x.X,
            Y = x.Y
        });

        Board.SetState(boardRecord);
    }

    private void OnDestroy()
    {
        Board.PieceMoved -= Board_PieceMoved;
    }

    private void Board_PieceMoved(Piece movedPiece, Cell OriginalCell, Piece capturedPiece, Cell newCell)
    {
        (bool blackInCheck, bool blackMate, bool whiteInCheck, bool whiteMate) = HandleCheck();

        var algebraicNotation = GetAlgebraicNotation(
            movedPiece,
            OriginalCell,
            capturedPiece,
            newCell,
            blackInCheck,
            blackMate,
            whiteInCheck,
            whiteMate);

        FindObjectOfType<ChessUI>().MoveList.Add(algebraicNotation);
        FindObjectOfType<ChessUI>().UpdateUI();

        if (movedPiece.PieceColor == ChessColor.w &&
            !blackMate)
        {
            DoBlackTurn();
        }
    }

    private (bool blackInCheck, bool blackMate, bool whiteInCheck, bool whiteMate) HandleCheck()
    {
        var boardData = Solver.ToBoardData(Board);
        var fen = FENParser.BoardToFEN(boardData, boardData.GetLength(0), boardData.GetLength(1));
        ChessGameRecord game = new ChessGameRecord(fen, boardData.GetLength(0), boardData.GetLength(1));

        var blackInCheck = game.ChessBitboard.IsKingInCheck(ChessColor.b);
        var whiteInCheck = game.ChessBitboard.IsKingInCheck(ChessColor.w);
        bool blackMate = false;
        bool whiteMate = false;

        if (blackInCheck)
        {
            Debug.Log("Black in Check");
            if (game.ChessBitboard.IsInCheckMate(ChessColor.b))
            {
                blackMate = true;
                Debug.Log("Black is Mated");
            }
        }

        if (whiteInCheck)
        {
            Debug.Log("White in Check");
            if (game.ChessBitboard.IsInCheckMate(ChessColor.w))
            {
                whiteMate = true;
                Debug.Log("White is Mated");
            }
        }

        return (blackInCheck, blackMate, whiteInCheck, whiteMate);
    }

    public void DoBlackTurn()
    {
        var move = Solver.GetNextMove(Board, ChessColor.b);

        (int fromX, int fromY) from = FromIndex(move.From);
        (int toX, int toY) to = FromIndex(move.To);
        var oldPiece = Board.Cells[from.fromX, from.fromY].CurrentPiece;

        Board.PieceDropped(oldPiece, Board.Cells[to.toX, to.toY]);
    }

    //auto
    public void DoWhiteTurn()
    {
        var move = Solver.GetNextMove(Board, ChessColor.w);

        (int fromX, int fromY) from = FromIndex(move.From);
        (int toX, int toY) to = FromIndex(move.To);
        var oldPiece = Board.Cells[from.fromX, from.fromY].CurrentPiece;

        Board.PieceDropped(oldPiece, Board.Cells[to.toX, to.toY]);
    }

    public (int x, int y) FromIndex(int positionIndex)
    {
        return (positionIndex % 8, positionIndex / 8);
    }

    public static PieceType ToPieceType(char piece)
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

    public string GetAlgebraicNotation(Piece movedPiece, Cell originalCell, Piece capturedPiece, Cell newCell, bool blackInCheck, bool blackMate, bool whiteInCheck, bool whiteMate)
    {
        // If the move involves a capture
        if (capturedPiece != null)
        {
            // If the moved piece is a pawn
            if (movedPiece.PieceType == PieceType.Pawn)
            {
                // For pawn captures, include the file from where the pawn moved (e.g., e5).
                return $"{ToFile(originalCell.X)}{GetCaptureSymbol()}{ToFile(newCell.X)}{ToRank(newCell.Y)}{GetCheckOrMate(blackInCheck, blackMate, whiteInCheck, whiteMate)}";
            }
            else
            {
                // For other pieces (non-pawn), include the piece type and the capture.
                return $"{FENParser.ToFEN(movedPiece.PieceType, movedPiece.PieceColor == ChessColor.w)}{ToFile(originalCell.X)}{ToRank(originalCell.Y)}{GetCaptureSymbol()}{ToFile(newCell.X)}{ToRank(newCell.Y)}{GetCheckOrMate(blackInCheck, blackMate, whiteInCheck, whiteMate)}";
            }
        }
        else
        {
            // If there's no capture, it's a regular move.
            if (movedPiece.PieceType == PieceType.Pawn)
            {
                // For pawns, you can omit the piece type, as it's implied.
                return $"{ToFile(newCell.X)}{ToRank(newCell.Y)}{GetCheckOrMate(blackInCheck, blackMate, whiteInCheck, whiteMate)}";
            }
            else
            {
                // For other pieces, include the piece type.
                return $"{FENParser.ToFEN(movedPiece.PieceType, movedPiece.PieceColor == ChessColor.w)}{ToFile(newCell.X)}{ToRank(newCell.Y)}{GetCheckOrMate(blackInCheck, blackMate, whiteInCheck, whiteMate)}";
            }
        }
    }

    public char ToFile(int x)
    {
        // Convert the integer to the corresponding file letter ('a' for 1, 'b' for 2, ..., 'h' for 8)
        return (char)('a' + x);
    }

    public int ToRank(int y)
    {
        return y + 1;
    }

    private string GetCaptureSymbol()
    {
        return "x"; // Standard notation for a capture
    }
    private string GetCheckOrMate(bool blackInCheck, bool blackMate, bool whiteInCheck, bool whiteMate)
    {
        if (blackMate)
            return "#"; // Black is in checkmate
        if (whiteMate)
            return "#"; // White is in checkmate
        if (blackInCheck)
            return "+"; // Black is in check
        if (whiteInCheck)
            return "+"; // White is in check

        return ""; // No check or mate
    }
}
