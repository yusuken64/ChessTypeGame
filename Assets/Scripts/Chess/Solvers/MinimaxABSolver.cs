using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinimaxABSolver : SolverBase
{
    public int SolveDepth;
    public float TimeLimitSeconds;
    public List<WeightedEvaluator> Evaluators;

    private int positionsSearched = 0;
    public override Move GetNextMove(ChessBitboard bitboard, ChessColor color, IEnumerable<Move> legalMoves)
    {
        positionsSearched = 0;
        bool moveFound = false;
        Move bestMove = default;
        int bestScore = int.MinValue; // Start maximizing

        int alpha = int.MinValue;
        int beta = int.MaxValue;
        var solveDepth = SolveDepth;

        TimeSpan timeLimit = TimeSpan.FromSeconds(TimeLimitSeconds);
        DateTime startTime = DateTime.Now;

        foreach (var move in OrderMoves(legalMoves, bitboard, color))
        {
            // Check if the time limit has been exceeded
            if (DateTime.Now - startTime > timeLimit)
            {
                Debug.Log("Time limit reached, returning best move found so far.");
                Debug.Log($"positionsSearched {positionsSearched}");
                return bestMove; // Return the best move found so far
            }

            ChessBitboard newGame = bitboard.MakeMove(move);
            int moveScore = Negamax(newGame, solveDepth - 1, alpha, beta, bitboard._rankMax, bitboard._fileMax, color);

            if (!moveFound ||
                moveScore > bestScore )
            {
                moveFound = true;
                bestScore = moveScore;
                bestMove = move;
            }
        }

        Debug.Log($"positionsSearched {positionsSearched}");
        if (!moveFound)
        {
            throw new Exception("No legal moves");
        }
        return bestMove;
    }

    private int Negamax(ChessBitboard bitboard, int depth, int alpha, int beta, int rankMax, int fileMax, ChessColor currentPlayer)
    {
        positionsSearched++;
        ChessColor enemyColor = currentPlayer == ChessColor.w ? ChessColor.b : ChessColor.w;
        int sign = currentPlayer == ChessColor.w ? 1 : -1;

        var gameOver = bitboard.CheckGameOver(currentPlayer);
        if (depth <= 0 || gameOver.isCheckmate || gameOver.isStalemate)
        {
            return sign * EvaluateBoard(bitboard, currentPlayer, gameOver, bitboard.CheckGameOver(enemyColor));
        }

        var possibleMoves = GetLegalMoves(bitboard, currentPlayer);

        // Handle case of no legal moves (either checkmate or stalemate)
        if (!possibleMoves.Any())
        {
            return gameOver.isCheckmate ? (int.MinValue + depth) * sign : 0; // Favor faster wins/losses
        }

        int bestScore = int.MinValue;

        foreach (var move in OrderMoves(possibleMoves, bitboard, currentPlayer))
        {
            var newGame = bitboard.MakeMove(move);
            int score = -Negamax(newGame, depth - 1, -beta, -alpha, rankMax, fileMax, enemyColor);

            bestScore = Math.Max(bestScore, score);
            alpha = Math.Max(alpha, score);
            if (alpha >= beta) break; // Alpha-beta pruning
        }
        return bestScore;
    }

    private IEnumerable<Move> OrderMoves(IEnumerable<Move> possibleMoves, ChessBitboard bitboard, ChessColor currentPlayer)
    {
        // Prioritize captures first, then checks, and finally other moves.
        var orderedMoves = possibleMoves
            .OrderByDescending(move => BitboardHelper.IsCapture(ref bitboard, move, currentPlayer))    // Capture moves first (descending to get captures first)
            .ThenBy(move => IsCheckMove(move, bitboard, currentPlayer))
            .ThenBy(move =>
            {
                ChessBitboard newGame = bitboard.MakeMove(move);
                int score = 0;
                foreach (var evaluator in Evaluators)
                {
                    var evaluatorScore = evaluator.Evaluator.Score(newGame, currentPlayer);
                    score += (int)(evaluatorScore * evaluator.Weight);
                }
                return score;
            });

        return orderedMoves;
    }

    private bool IsCheckMove(Move move, ChessBitboard bitboard, ChessColor currentPlayer)
    {
        var newGame = bitboard.MakeMove(move);
        var gameOver = newGame.CheckGameOver(currentPlayer);
        return gameOver.isCheck;
    }

    public int EvaluateBoard(
        ChessBitboard bitboard,
        ChessColor currentPlayer,
        (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) gameOverPlayer,
        (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) gameOverEnemy)
    {
        // King Safety Consideration
        if (gameOverEnemy.isCheckmate)
        {
            return int.MaxValue; // Winning position
        }
        if (gameOverPlayer.isCheckmate)
        {
            return int.MinValue; // Losing position
        }

        int score = 0;

        foreach(var evaluator in Evaluators)
        {
            var evaluatorScore = evaluator.Evaluator.Score(bitboard, currentPlayer);
            score += (int)(evaluatorScore * evaluator.Weight);
        }

        // Handle Stalemate conditions
        if (gameOverEnemy.isStalemate)
        {
            score += 10; // Slightly favorable for the currentPlayer (Stalemate for the opponent)
        }
        if (gameOverPlayer.isStalemate)
        {
            score -= 10; // Slightly unfavorable for the currentPlayer (Stalemate for the currentPlayer)
        }

        // Handle Check conditions
        if (gameOverEnemy.isCheck)
        {
            score += 50; // Favorable for the currentPlayer (Opponent is in check)
        }
        if (gameOverPlayer.isCheck)
        {
            score -= 50; // Unfavorable for the currentPlayer (Player is in check)
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
}
