using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MinimaxABSolver : SolverBase
{
    [Range(2, 10)]
    public int SolveDepth;
    public List<WeightedEvaluator> Evaluators;

    public override Move GetNextMove(ChessGameRecord game, ChessColor color, IEnumerable<Move> legalMoves)
    {
        bool moveFound = false;
        Move bestMove = default;
        int bestScore = int.MinValue; // Start maximizing

        int alpha = int.MinValue;
        int beta = int.MaxValue;
        var solveDepth = SolveDepth;

        foreach (var move in legalMoves)
        {
            ChessGameRecord newGame = new ChessGameRecord(game.fen, game.rankMax, game.fileMax, game.canPawnPromote);
            newGame.MakeMove(move);

            int moveScore = Negamax(newGame, solveDepth - 1, alpha, beta, game.rankMax, game.fileMax, color);

            //Debug.Log($"Score {moveScore}");

            if (!moveFound ||
                moveScore > bestScore )
            {
                moveFound = true;
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

    private int Negamax(ChessGameRecord game, int depth, int alpha, int beta, int rankMax, int fileMax, ChessColor currentPlayer)
    {
        ChessColor enemyColor = currentPlayer == ChessColor.w ? ChessColor.b : ChessColor.w;
        int sign = currentPlayer == ChessColor.w ? 1 : -1;

        var gameOver = CheckGameOver(game, currentPlayer);
        if (depth <= 0 || gameOver.isCheckmate || gameOver.isStalemate)
        {
            return sign * EvaluateBoard(game, currentPlayer, gameOver, CheckGameOver(game, enemyColor));
        }

        var possibleMoves = GetLegalMoves(game, currentPlayer);

        // Handle case of no legal moves (either checkmate or stalemate)
        if (!possibleMoves.Any())
        {
            return gameOver.isCheckmate ? (int.MinValue + depth) * sign : 0; // Favor faster wins/losses
        }

        int bestScore = int.MinValue;

        foreach (var move in OrderMoves(possibleMoves, game, currentPlayer))
        {
            ChessGameRecord newGame = new ChessGameRecord(game.fen, rankMax, fileMax, game.canPawnPromote);
            newGame.MakeMove(move);
            int score = -Negamax(newGame, depth - 1, -beta, -alpha, rankMax, fileMax, enemyColor);

            bestScore = Math.Max(bestScore, score);
            alpha = Math.Max(alpha, score);
            if (alpha >= beta) break; // Alpha-beta pruning
        }
        return bestScore;
    }

    private IEnumerable<Move> OrderMoves(IEnumerable<Move> possibleMoves, ChessGameRecord game, ChessColor currentPlayer)
    {
        // Prioritize captures first, then checks, and finally other moves.
        var orderedMoves = possibleMoves
            .OrderByDescending(move => game.IsCapture(move, currentPlayer))    // Capture moves first (descending to get captures first)
            .ThenBy(move => IsCheckMove(move, game, currentPlayer))
            .ThenBy(move =>
            {
                ChessGameRecord newGame = new ChessGameRecord(game.fen, game.rankMax, game.fileMax, game.canPawnPromote);
                newGame.MakeMove(move);
                int score = 0;
                foreach (var evaluator in Evaluators)
                {
                    var evaluatorScore = evaluator.Evaluator.Score(newGame, currentPlayer);
                    score += (int)(evaluatorScore * evaluator.Weight);
                }
                return score;
            })
            ;    // Then prioritize check moves

        return orderedMoves;
    }

    private bool IsCheckMove(Move move, ChessGameRecord game, ChessColor currentPlayer)
    {
        // Simulate the move to check if it places the opponent in check
        ChessGameRecord newGame = new ChessGameRecord(game.fen, game.rankMax, game.fileMax, game.canPawnPromote);
        newGame.MakeMove(move);
        var gameOver = CheckGameOver(newGame, currentPlayer);
        return gameOver.isCheck;
    }

    public int EvaluateBoard(
        ChessGameRecord game,
        ChessColor currentPlayer,
        (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) gameOverPlayer,
        (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) gameOverEnemy)
    {
        int score = 0;

        foreach(var evaluator in Evaluators)
        {
            var evaluatorScore = evaluator.Evaluator.Score(game, currentPlayer);
            score += (int)(evaluatorScore * evaluator.Weight);
        }

        // King Safety Consideration
        if (gameOverEnemy.isCheckmate)
        {
            return int.MaxValue; // Winning position
        }
        if (gameOverPlayer.isCheckmate)
        {
            return int.MinValue; // Losing position
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
