using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Solver : MonoBehaviour
{
    [Range(2, 10)]
    public int SolveDepth;
    public List<WeightedEvaluator> Evaluators;
    public Move GetNextMove(Board board, ChessColor chessColor)
    {

        PieceRecord?[,] boardData = ToBoardData(board);
        var fen = FENParser.BoardToFEN(boardData, board.Cells.GetLength(0), board.Cells.GetLength(1), chessColor.ToString());
        var nextMove = GetBestMove(
            fen,
            board.Cells.GetLength(0),
            board.Cells.GetLength(1),
            SolveDepth,
            chessColor);

        //(int fromX, int fromY) from = FromIndex(nextMove.From);
        //(int toX, int toY) to = FromIndex(nextMove.To);
        //PieceRecord? piece = boardData[from.fromX, from.fromY];
        //Debug.Log($"Best Move: {piece.Value.PieceType} from {from} to {to}");
        return nextMove;
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

    public Move GetBestMove(string fen, int rankMax, int fileMax, int maxDepth, ChessColor currentPlayer)
    {
        bool moveFound = false;
        ChessGameRecord game = new ChessGameRecord(fen, rankMax, fileMax);
        Move bestMove = default;
        int bestScore = int.MinValue; // Start maximizing

        List<Move> possibleMoves = GetValidMoves(game, currentPlayer);

        foreach (var move in possibleMoves.OrderBy(x => Guid.NewGuid()))
        {
            ChessGameRecord newGame = new ChessGameRecord(game.fen, rankMax, fileMax);
            newGame.MakeMove(move);

            if (newGame.IsInCheck(currentPlayer)) { continue; } // Skip illegal moves

            moveFound = true;
            int moveScore = Minimax(newGame, maxDepth - 1, false, rankMax, fileMax, currentPlayer);

            //Debug.Log($"Score {moveScore}");

            if (moveScore > bestScore)
            {
                bestScore = moveScore;
                bestMove = move;
            }
        }

        if (!moveFound)
        {
            throw new Exception("No legal moves");
        }
        return bestMove;
    }

    private int Minimax(ChessGameRecord game, int depth, bool isMaximizing, int rankMax, int fileMax, ChessColor currentPlayer)
    {
        ChessColor enemyColor = currentPlayer == ChessColor.w ? ChessColor.b : ChessColor.w;

        if (depth == 0 || IsCheckmate(game, enemyColor) || IsCheckmate(game, currentPlayer))
        {
            return EvaluateBoard(game, rankMax, fileMax, currentPlayer);
        }

        List<Move> possibleMoves = GetValidMoves(game, isMaximizing ? currentPlayer : enemyColor);

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            foreach (var move in possibleMoves)
            {
                ChessGameRecord newGame = new ChessGameRecord(game.fen, rankMax, fileMax);
                newGame.MakeMove(move);

                if (newGame.IsInCheck(currentPlayer)) { continue; }

                int score = Minimax(newGame, depth - 1, false, rankMax, fileMax, currentPlayer);
                bestScore = Math.Max(bestScore, score);
            }
            return bestScore;
        }
        else // Minimizing for opponent
        {
            int bestScore = int.MaxValue;
            foreach (var move in possibleMoves)
            {
                ChessGameRecord newGame = new ChessGameRecord(game.fen, rankMax, fileMax);
                newGame.MakeMove(move);

                if (newGame.IsInCheck(enemyColor)) { continue; }

                int score = Minimax(newGame, depth - 1, true, rankMax, fileMax, currentPlayer);
                bestScore = Math.Min(bestScore, score);
            }
            return bestScore;
        }
    }

    private int EvaluateBoard(ChessGameRecord game, int rankMax, int fileMax, ChessColor currentPlayer)
    {
        int score = 0;
        ChessColor enemyColor = currentPlayer == ChessColor.w ? ChessColor.b : ChessColor.w;

        foreach(var evaluator in Evaluators)
        {
            var evaluatorScore = evaluator.Evaluator.Score(game, currentPlayer);
            score += (int)(evaluatorScore * evaluator.Weight);
        }

        //// King Safety Consideration
        if (IsCheckmate(game, enemyColor))
        {
            return int.MaxValue; // Winning position
        }
        if (IsCheckmate(game, currentPlayer))
        {
            return int.MinValue; // Losing position
        }

        return score;
    }

    public static int GetPieceValue(PieceType piece)
    {
        return piece switch
        {
            PieceType.Pawn => 1,
            PieceType.Knight => 3,
            PieceType.Bishop => 3,
            PieceType.Rook => 5,
            PieceType.Queen => 9,
            PieceType.King => 1000, // Arbitrary high value to prioritize king safety
            _ => 0
        };
    }

    public static List<Move> GetValidMoves(ChessGameRecord game, ChessColor player)
    {
        List<Move> moves = new List<Move>();

        foreach (FenRecord piece in game.FenData.Pieces
            .Where(x => x.Player == player))
        {
            foreach (var move in game.GetValidMoves((piece.X, piece.Y)))
            {
                moves.Add(move);
            }
        }

        return moves;
    }

    private static bool IsCheckmate(ChessGameRecord game, ChessColor player)
    {
        return game.IsInCheckmate(player);
    }

    public static ChessColor OtherColor(ChessColor color)
    {
        return color == ChessColor.w ? ChessColor.b : ChessColor.w;
    }
}
