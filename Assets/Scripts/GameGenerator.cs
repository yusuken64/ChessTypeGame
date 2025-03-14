using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameGenerator : MonoBehaviour
{
    public void GenerateLevel(int level)
    {
        var board = FindObjectOfType<Board>();
        board.CreateBoard();

        var boardData = new PieceRecord?[board.Cells.GetLength(0), board.Cells.GetLength(1)];
        var x = UnityEngine.Random.Range(1, board.Cells.GetLength(0) -1);
        var y = UnityEngine.Random.Range(1, (board.Cells.GetLength(1) - 1) / 2);

        PieceRecord startingPiece = new PieceRecord()
        {
            IsWhite = true,
            PieceType = PieceType.King,
            X = x,
            Y = y
        };
        List<(int x, int y)> solution = null;

        for (int i = 0; i < 5; i++)
        {
            boardData = new PieceRecord?[board.Cells.GetLength(0), board.Cells.GetLength(1)];
            boardData[x, y] = startingPiece;

            var success = PlacePieces(boardData, startingPiece, level, false);
            if (success) {
                var boardCopy = CopyBoard(boardData);
                boardCopy[startingPiece.X, startingPiece.Y] = null;
                solution = FindSolution(boardCopy, startingPiece);
                if (success && solution != null)
                {
                    break;
                }
                Debug.Log($"Failed to solve {i}");
            }
            else
            {
                Debug.Log($"Failed to place pieces {i}");
            }
        }

        board.SetState(boardData.Cast<PieceRecord?>().OfType<PieceRecord>());
        board.SetSolution(new Solution()
        {
            Steps = solution,
            StartingPiece = startingPiece
        });
    }

    private PieceRecord?[,] CopyBoard(PieceRecord?[,] boardData)
    {
        var boardCopy = new PieceRecord?[boardData.GetLength(0), boardData.GetLength(1)];
        for (int x = 0; x < boardData.GetLength(0); x++)
        {
            for (int y = 0; y < boardData.GetLength(1); y++)
            {
                if (boardData[x, y].HasValue)
                {
                    boardCopy[x, y] = boardData[x, y].Value.Clone(); // Ensure a deep copy
                }
            }
        }
        return boardCopy;
    }

    private List<(int x, int y)> FindSolution(PieceRecord?[,] board, PieceRecord startingPiece)
    {
        var count = board.OfType<PieceRecord>().Count();
        if (count == 0) { return new List<(int x, int y)> { (startingPiece.X, startingPiece.Y) }; }

        var movableSquares = GetPlacableMoves(board, startingPiece);

        foreach (var movableSquare in movableSquares)
        {
            (int x, int y) position = Board.FromIndex(movableSquare.To, board.GetLength(0));
            PieceRecord? piece = board[position.x, position.y];

            if (piece.HasValue)
            {
                PieceRecord nextPiece = piece.Value;
                board[nextPiece.X, nextPiece.Y] = null;
                var solutionPath = FindSolution(board, nextPiece);

                if (solutionPath != null)
                {
                    solutionPath.Insert(0, (startingPiece.X, startingPiece.Y));
                    return solutionPath;
                }
                board[nextPiece.X, nextPiece.Y] = nextPiece;
            }
        }

        return null;
    }

    public bool PlacePieces(PieceRecord?[,] board, PieceRecord? lastPiece, int count, bool attackOnly)
    {
        if (count == 0)
        {
            return true;
        }

        var pieces = new List<PieceType>()
        {
            PieceType.King,
            PieceType.Queen,
            PieceType.Bishop,
            PieceType.Rook,
            PieceType.Knight,
            PieceType.Pawn
        };
        var pieceBag = pieces.OrderBy(x => Guid.NewGuid()).ToList();
        PieceType nextPiece = default;

        while (pieceBag.Any())
        {
            nextPiece = pieceBag.First();
            pieceBag.Remove(nextPiece);

            var placableSquares = GetPlacableMoves(board, lastPiece.Value);

            var randomPlacableSquares = placableSquares.OrderBy(x => Guid.NewGuid()).ToList();
            foreach (var cell in randomPlacableSquares)
            {
                (int x, int y) position = Board.FromIndex(cell.To, board.GetLength(0));
                PieceRecord? record = new PieceRecord()
                {
                    IsWhite = false,
                    PieceType = nextPiece,
                    X = position.x,
                    Y = position.y
                };
                board[position.x, position.y] = record;
                
                if (PlacePieces(board, record, --count, true))
                {
                    return true;
                }

                board[position.x, position.y] = null;
            }
        }

        return false;
    }

    private IEnumerable<Move> GetPlacableMoves(PieceRecord?[,] board, PieceRecord value)
    {
        string fen = FENParser.BoardToFEN(board, board.GetLength(0), board.GetLength(1));
        ChessGameRecord game = new ChessGameRecord(fen, board.GetLength(0), board.GetLength(1));
        var positionIndex = Board.ToIndex(value.X, value.Y, board.GetLength(0));
        //IEnumerable<Move> candidateMoves = 
        //    game.ChessBitboard.GetPlacableMoves(value.PieceType, positionIndex, value.IsWhite ? ChessColor.w : ChessColor.b);

        //in echo always assume white current piece is white
        IEnumerable<Move> candidateMoves =
    game.ChessBitboard.GetPlacableMoves(value.PieceType, positionIndex, ChessColor.w);

        return candidateMoves;
    }

    [ContextMenu("StartGame_1")]
    public void StartGame()
    {
        GenerateLevel(1);
    }

    [ContextMenu("StartGame_2")]
    public void StartGame_2()
    {
        GenerateLevel(2);
    }

    [ContextMenu("StartGame_3")]
    public void StartGame_3()
    {
        GenerateLevel(3);
    }

    [ContextMenu("StartGame_5")]
    public void StartGame_5()
    {
        GenerateLevel(5);
    }

    [ContextMenu("StartGame_10")]
    public void StartGame_10()
    {
        GenerateLevel(10);
    }

    [ContextMenu("StartGame_20")]
    public void StartGame_20()
    {
        GenerateLevel(20);
    }
}
