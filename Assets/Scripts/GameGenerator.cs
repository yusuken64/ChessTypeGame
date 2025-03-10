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
            PieceMovement = PieceMovement.King,
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

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    private List<(int x, int y)>? FindSolution(PieceRecord?[,] board, PieceRecord startingPiece)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    {
        var count = board.OfType<PieceRecord>().Count();
        if (count == 0) { return new List<(int x, int y)> { (startingPiece.X, startingPiece.Y) }; }

        List<(int x, int y)> movableSquares = Piece.GetByMovement(
            board,
            startingPiece.PieceMovement,
            startingPiece.X,
            startingPiece.Y,
            PieceColor.White,
            false);

        foreach (var movableSquare in movableSquares)
        {
            PieceRecord? piece = board[movableSquare.x, movableSquare.y];

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

        var pieces = new List<PieceMovement>()
        {
            PieceMovement.King,
            PieceMovement.Queen,
            PieceMovement.Bishop,
            PieceMovement.Rook,
            PieceMovement.Knight,
            PieceMovement.Pawn
        };
        var pieceBag = pieces.OrderBy(x => Guid.NewGuid()).ToList();
        PieceMovement nextPiece = default;

        while (pieceBag.Any())
        {
            nextPiece = pieceBag.First();
            pieceBag.Remove(nextPiece);

            var movableSquares = Piece.GetByMovement(
                board,
                lastPiece.Value.PieceMovement,
                lastPiece.Value.X,
                lastPiece.Value.Y,
                PieceColor.White,
                attackOnly);

            var randomMovableSquares = movableSquares.OrderBy(x => Guid.NewGuid()).ToList();
            foreach (var cell in randomMovableSquares)
            {
                PieceRecord? record = new PieceRecord()
                {
                    IsWhite = false,
                    PieceMovement = nextPiece,
                    X = cell.x,
                    Y = cell.y
                };
                board[cell.x, cell.y] = record;
                
                if (PlacePieces(board, record, --count, true))
                {
                    return true;
                }

                board[cell.x, cell.y] = null;
            }
        }

        return false;
    }

    // Method to find a valid cell where nextPiece can capture the last piece
    private static Cell FindCaptureCell(Cell lastCell, Cell[,] board)
    {
        var movableSquares = lastCell.CurrentPiece.GetMovableSquares(board, lastCell, PieceColor.White, true);

        // If there are valid capture cells, choose one (you could select based on a strategy)
        if (movableSquares.Count > 0)
        {
            return movableSquares.OrderBy(x => Guid.NewGuid()).First(); // You can add a more advanced selection logic if necessary
        }

        // Return a default value (invalid capture) if no valid cell was found
        return null;
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
}
