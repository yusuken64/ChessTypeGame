using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ChessGame : MonoBehaviour
{
    public Board Board;

    public ChessColor ActivePlayer;

    public bool AutoWhiteTurn;
    public bool AutoBlackTurn;

    public int HalfMoveClock;
    public int FullMove;
    public int MoveMax;

    public SolverDefinitionBase WhiteSolverDefinition;
    public SolverDefinitionBase BlackSolverDefinition;

    private SolverBase _whiteSolver;
    private SolverBase _blackSolver;

    public bool Thinking = false; //is a solver is busy

    void Start()
    {
        Debug.Log("Start Chess Game Object");
        Board.PieceMoved += Board_PieceMoved;
        Board.PieceCanceled += Board_PieceCanceled;
    }

    public void ResetGame()
    {
        Debug.Log("Reset Game");
        StopAllCoroutines();
        FullMove = 0;
        HalfMoveClock = 0;
        Thinking = false;

        //var boardData = "8/3p4/8/8/8/8/P7/RNBQKBNR w KQkq - 0 1";
        var boardData = FENParser.STANDARDGAMESETUP;
        var fenData = FENParser.ParseFEN(boardData, 8, 8);
        ActivePlayer = fenData.ActiveColor;
        var boardRecord = fenData.Pieces.Select(x => new PieceRecord()
        {
            IsWhite = x.Player == ChessColor.w,
            PieceType = x.Piece,
            X = x.X,
            Y = x.Y
        });

        Board.SetState(boardRecord);

        _whiteSolver = WhiteSolverDefinition?.GetSolver();
        _blackSolver = BlackSolverDefinition?.GetSolver();

        if (ActivePlayer == ChessColor.w &&
            AutoWhiteTurn)
        {
            DoWhiteTurn();
        }
        UpdateDraggable();
    }

    private void OnDestroy()
    {
        Board.PieceMoved -= Board_PieceMoved;
        Board.PieceCanceled -= Board_PieceCanceled;
    }

    private void Board_PieceMoved(Piece movedPiece, Cell originalCell, Piece capturedPiece, Cell destinationCell)
    {
        HandleSpecialMoves(movedPiece, originalCell, capturedPiece, destinationCell);

        var otherColor = movedPiece.PieceColor.Opponent();
        (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) = HandleCheck(otherColor);

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
        else if (isDraw)
        {
            chessUI.CurrentMessage = "Draw";
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

        FindObjectOfType<ChessUI>().MoveList.Add($"{FullMove} {HalfMoveClock} {algebraicNotation}");
        if (movedPiece.PieceColor == ChessColor.b)
        {
            FullMove++;
        }
        HalfMoveClock++;
        ActivePlayer = movedPiece.PieceColor.Opponent();

        bool moveLimit = FullMove >= MoveMax;
        if (moveLimit)
        {
            chessUI.CurrentMessage = "Move Limit";
        }

        chessUI.UpdateUI();
        if (!isCheckmate && !isStalemate && !isDraw && !moveLimit)
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

        UpdateDraggable();
    }

    private void UpdateDraggable()
    {
        Board.Cells.OfType<Cell>()
            .ToList()
            .ForEach(x => x.CurrentPiece?.SetIsDraggable(x.CurrentPiece.PieceColor == ActivePlayer));
    }

    internal ChessColor GetActivePlayer()
    {
        return ActivePlayer;
    }

    private void HandleSpecialMoves(Piece movedPiece, Cell originalCell, Piece capturedPiece, Cell destinationCell)
    {
        if (Board.CanQueenPromote &&
            movedPiece.PieceType == PieceType.Pawn &&
            (destinationCell.Y == Board.Height - 1 || destinationCell.Y == 0))
        {
            PromotePawn(movedPiece, destinationCell);
        }
    }

    private void PromotePawn(Piece pawn, Cell destinationCell)
    {
        PieceType promotionChoice = PieceType.Queen;
        Board.ReplacePiece(destinationCell, promotionChoice, pawn);
    }

    private void Board_PieceCanceled(Piece movedPiece, Cell originalCell, string reason)
    {
        ChessUI chessUI = FindObjectOfType<ChessUI>();
        chessUI.CurrentMessage = reason;
        chessUI.UpdateUI();
    }

    private (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) HandleCheck(ChessColor chessColor)
    {
        var boardData = SolverBase.ToBoardData(Board);
        var fen = FENParser.BoardToFEN(boardData, boardData.GetLength(0), boardData.GetLength(1));
        var bitboard = BitboardHelper.FromFen(fen, boardData.GetLength(0), boardData.GetLength(1), Board.CanQueenPromote);

        return bitboard.CheckGameOver(chessColor);
    }

    public void DoBlackTurn()
    {
        StartCoroutine(DoAutoTurnRoutine(ChessColor.b));
    }

    public void DoWhiteTurn()
    {
        StartCoroutine(DoAutoTurnRoutine(ChessColor.w));
    }

    private IEnumerator DoAutoTurnRoutine(ChessColor color)
    {
        yield return null;

        var boardData = SolverBase.ToBoardData(Board);
        var fen = FENParser.BoardToFEN(boardData, boardData.GetLength(0), boardData.GetLength(1));
        var bitboard = BitboardHelper.FromFen(fen, boardData.GetLength(0), boardData.GetLength(1), Board.CanQueenPromote);

        var solver = GetSolverFor(color);
        if (solver == null)
        {
            throw new Exception($"Solver not defined for {color}");
        }

        var legalMoves = SolverBase.GetLegalMoves(bitboard, color);
        Thinking = true;
        FindObjectOfType<ChessUI>().UpdateUI();

#if UNITY_WEBGL
        // For WebGL, simulate async work without Task.Run() (as WebGL doesn't support multi-threading)
        yield return null;
        var move = solver.GetNextMove(bitboard, color, legalMoves);
#else
    // For other platforms, you can use Task.Run() or threading logic
    var task = Task.Run(() =>
    {
        return solver.GetNextMove(game, color, legalMoves);
    });

    // Wait until the task completes
    while (!task.IsCompleted)
    {
        yield return null;
    }

    var move = task.Result; // Safely get the result once the task is done
#endif

        Thinking = false;

        // Using your existing logic for processing the move
        (int fromX, int fromY) from = Board.FromIndex(move.From, Board.Width);
        (int toX, int toY) to = Board.FromIndex(move.To, Board.Width);
        var oldPiece = Board.Cells[from.fromX, from.fromY].CurrentPiece;

        Board.PieceDropped(oldPiece, Board.Cells[to.toX, to.toY]);
    }
    
    private SolverBase GetSolverFor(ChessColor color)
    {
        return color == ChessColor.w ? _whiteSolver : _blackSolver;
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
