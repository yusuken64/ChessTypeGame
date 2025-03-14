using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class EvaluatorBase
{
    public abstract int Score(ChessGameRecord game, ChessColor currentPlayer);
}

[Serializable]
public class PieceValueEvaluator : EvaluatorBase
{
    public override int Score(ChessGameRecord game, ChessColor currentPlayer)
    {
        int score = 0;

        foreach (FenRecord record in game.FenData.Pieces)
        {
            PieceType pieceType = ChessGame.ToPieceType(record.Piece);

            int pieceValue = MinimaxABSolver.GetPieceValue(pieceType);
            if (record.Player == currentPlayer)
            {
                score += pieceValue;
                //score += GetPositionalBonus(pieceType, record.X, record.Y, game.rankMax, game.fileMax);
            }
            else
            {
                score -= pieceValue;
                //score -= GetPositionalBonus(pieceType, record.X, record.Y, game.rankMax, game.fileMax);
            }
        }
        return score;
    }

    private static int GetPositionalBonus(PieceType piece, int x, int y, int rankMax, int fileMax)
    {
        // Example: Give bonuses for center control
        int centerX = fileMax / 2;
        int centerY = rankMax / 2;

        int distanceToCenter = Math.Abs(x - centerX) + Math.Abs(y - centerY);

        return 5 - distanceToCenter; // Closer to center gets higher score
    }
}

[Serializable]
public class PositionalValueEvaluator : EvaluatorBase
{
    public override int Score(ChessGameRecord game, ChessColor currentPlayer)
    {
        int score = 0;

        // Mobility (more valid moves is usually good)
        int playerMoves = SolverBase.GetLegalMoves(game, currentPlayer).Count();
        int enemyMoves = SolverBase.GetLegalMoves(game, MinimaxABSolver.OtherColor(currentPlayer)).Count(); ;
        score += (int)((playerMoves - enemyMoves));

        return score;
    }
}

[Serializable]
public class AttackingValueEvaluator : EvaluatorBase
{
    public override int Score(ChessGameRecord game, ChessColor currentPlayer)
    {
        int score = 0;

        // Mobility (more valid moves is usually good)
        int playerMoves = MinimaxABSolver.GetLegalMoves(game, currentPlayer).Count();
        int enemyMoves = MinimaxABSolver.GetLegalMoves(game, MinimaxABSolver.OtherColor(currentPlayer)).Count();
        score += (int)((playerMoves - enemyMoves));

        // Reward for attacking enemy pieces
        foreach (var move in MinimaxABSolver.GetLegalMoves(game, currentPlayer))
        {
            if (game.IsCapture(move, currentPlayer))
            {
                var capturedPiece = game.GetPieceAt(move.To, currentPlayer);
                if (capturedPiece.HasValue)
                {
                    score += GetPieceValue(capturedPiece.Value); // Reward based on piece value
                }
            }
        }

        return score;
    }

    private int GetPieceValue(PieceType piece)
    {
        return piece switch
        {
            PieceType.Pawn => 1,
            PieceType.Knight => 3,
            PieceType.Bishop => 3,
            PieceType.Rook => 5,
            PieceType.Queen => 9,
            _ => 0,
        };
    }
}
