using System;
using System.Collections;
using System.Collections.Generic;
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

            int pieceValue = Solver.GetPieceValue(pieceType);
            if (record.Player == currentPlayer)
            {
                score += pieceValue;
                score += GetPositionalBonus(pieceType, record.X, record.Y, game.rankMax, game.fileMax);
            }
            else
            {
                score -= pieceValue;
                score -= GetPositionalBonus(pieceType, record.X, record.Y, game.rankMax, game.fileMax);
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
        int playerMoves = Solver.GetValidMoves(game, currentPlayer).Count;
        int enemyMoves = Solver.GetValidMoves(game, Solver.OtherColor(currentPlayer)).Count;
        score += (int)((playerMoves - enemyMoves));

        return score;
    }
}