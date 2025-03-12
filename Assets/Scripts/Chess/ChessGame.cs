using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ChessGame : MonoBehaviour
{
    public Board Board;
    public Solver Solver;

    public ChessColor ActivePlayer;

    public bool AutoWhiteTurn;
    public bool AutoBlackTurn;

    void Start()
    {
        //ResetGame();
        Board.PieceMoved += Board_PieceMoved;
        Board.PieceCanceled += Board_PieceCanceled;
    }

    public void ResetGame()
    {
        //var boardData = "8/3p4/8/8/8/8/P7/RNBQKBNR w KQkq - 0 1";
        var boardData = FENParser.STANDARDGAMESETUP;
        var fenData = FENParser.ParseFEN(boardData, 8, 8);
        ActivePlayer = fenData.ActiveColor;
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
        Board.PieceCanceled -= Board_PieceCanceled;
    }

    private void Board_PieceMoved(Piece movedPiece, Cell originalCell, Piece capturedPiece, Cell destinationCell)
    {
        HandleSpecialMoves(movedPiece, originalCell, capturedPiece, destinationCell);

        var otherColor = Solver.OtherColor(movedPiece.PieceColor);
        (bool isCheckmate, bool isStalemate, bool isCheck) = HandleCheck(otherColor);

        var chessUI = FindObjectOfType<ChessUI>();
        if (isCheckmate)
        {
            chessUI.CurrentMessage = "Check Mate";
        }
        else if (isStalemate)
        {
            chessUI.CurrentMessage = "Stale Mate";
        }
        else if (isCheck)
        {
            chessUI.CurrentMessage = "Check";
        }
        else
        {
            chessUI.CurrentMessage = string.Empty;
        }

        var algebraicNotation = GetAlgebraicNotation(
            movedPiece,
            originalCell,
            capturedPiece,
            destinationCell,
            isCheck,
            isCheckmate);

        FindObjectOfType<ChessUI>().MoveList.Add(algebraicNotation);
        ActivePlayer = Solver.OtherColor(movedPiece.PieceColor);

        chessUI.UpdateUI();
        if (!isCheckmate && !isStalemate)
        {
            if (ActivePlayer == ChessColor.b &&
                AutoBlackTurn)
            {
                DoBlackTurn();
            }
            else if (ActivePlayer == ChessColor.w &&
                AutoWhiteTurn)
            {
                DoWhiteTurn();
            }
        }
    }

    internal ChessColor GetActivePlayer()
    {
        return ActivePlayer;
    }

    private void HandleSpecialMoves(Piece movedPiece, Cell originalCell, Piece capturedPiece, Cell destinationCell)
    {
        if (movedPiece.PieceType == PieceType.Pawn && (destinationCell.Y == Board.Height - 1 || destinationCell.Y == 0))
        {
            PromotePawn(movedPiece, destinationCell);
        }
    }

    private void PromotePawn(Piece pawn, Cell destinationCell)
    {
        PieceType promotionChoice = PieceType.Queen; // Default choice (can be changed by UI)

        Board.ReplacePiece(destinationCell, promotionChoice, pawn);
        Console.WriteLine($"Pawn promoted to {promotionChoice} at {destinationCell}");
    }

    private void Board_PieceCanceled(Piece movedPiece, Cell originalCell, string reason)
    {
        ChessUI chessUI = FindObjectOfType<ChessUI>();
        chessUI.CurrentMessage = reason;
        chessUI.UpdateUI();
    }


    private (bool isCheckmate, bool isStalemate, bool isCheck) HandleCheck(ChessColor chessColor)
    {
        var boardData = Solver.ToBoardData(Board);
        var fen = FENParser.BoardToFEN(boardData, boardData.GetLength(0), boardData.GetLength(1));
        ChessGameRecord game = new ChessGameRecord(fen, boardData.GetLength(0), boardData.GetLength(1));

        return game.ChessBitboard.CheckGameOver(chessColor);
    }

    public void DoBlackTurn()
    {
        StartCoroutine(DoBlackTurnRoutine());
    }

    private IEnumerator DoBlackTurnRoutine()
    {
        var move = Solver.GetNextMove(Board, ChessColor.b);
        yield return null;

        (int fromX, int fromY) from = Board.FromIndex(move.From, Board.Width);
        (int toX, int toY) to = Board.FromIndex(move.To, Board.Width);
        var oldPiece = Board.Cells[from.fromX, from.fromY].CurrentPiece;
        yield return new WaitForSecondsRealtime(1f);

        Board.PieceDropped(oldPiece, Board.Cells[to.toX, to.toY]);
    }

    public void DoWhiteTurn()
    {
        StartCoroutine(DoWhiteTurnRoutine());
    }

    private IEnumerator DoWhiteTurnRoutine()
    {
        var move = Solver.GetNextMove(Board, ChessColor.w);
        yield return null;

        (int fromX, int fromY) from = Board.FromIndex(move.From, Board.Width);
        (int toX, int toY) to = Board.FromIndex(move.To, Board.Width);
        var oldPiece = Board.Cells[from.fromX, from.fromY].CurrentPiece;

        yield return new WaitForSecondsRealtime(1f);
        Board.PieceDropped(oldPiece, Board.Cells[to.toX, to.toY]);
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

    public string GetAlgebraicNotation(
        Piece movedPiece,
        Cell originalCell,
        Piece capturedPiece,
        Cell newCell,
        bool isCheck,
        bool isMate
        )
    {
        // If the move involves a capture
        if (capturedPiece != null)
        {
            // If the moved piece is a pawn
            if (movedPiece.PieceType == PieceType.Pawn)
            {
                // For pawn captures, include the file from where the pawn moved (e.g., e5).
                return $"{ToFile(originalCell.X)}{GetCaptureSymbol()}{ToFile(newCell.X)}{ToRank(newCell.Y)}{GetCheckOrMate(isCheck, isMate)}";
            }
            else
            {
                // For other pieces (non-pawn), include the piece type and the capture.
                return $"{FENParser.ToFEN(movedPiece.PieceType, movedPiece.PieceColor == ChessColor.w)}" +
                    $"{ToFile(originalCell.X)}{ToRank(originalCell.Y)}{GetCaptureSymbol()}" +
                    $"{ToFile(newCell.X)}{ToRank(newCell.Y)}{GetCheckOrMate(isCheck, isMate)}";
            }
        }
        else
        {
            // If there's no capture, it's a regular move.
            if (movedPiece.PieceType == PieceType.Pawn)
            {
                // For pawns, you can omit the piece type, as it's implied.
                return $"{ToFile(newCell.X)}{ToRank(newCell.Y)}{GetCheckOrMate(isCheck, isMate)}";
            }
            else
            {
                // For other pieces, include the piece type.
                return $"{FENParser.ToFEN(movedPiece.PieceType, movedPiece.PieceColor == ChessColor.w)}{ToFile(newCell.X)}{ToRank(newCell.Y)}{GetCheckOrMate(isCheck, isMate)}";
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

    private string GetCheckOrMate(bool check, bool mate)
    {
        if (mate)
            return "#";
        if (check)
            return "+";

        return ""; // No check or mate
    }
}
