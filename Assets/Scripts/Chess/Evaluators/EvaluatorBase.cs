using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class EvaluatorBase
{
    public abstract int Score(ChessBitboard bitboard, ChessColor currentPlayer);
}

[Serializable]
public class PieceValueEvaluator : EvaluatorBase
{
    public override int Score(ChessBitboard bitboard, ChessColor currentPlayer)
    {
        int score = 0;
                
        foreach (var record in bitboard.GetAllPieces())
        {
            int pieceValue = MinimaxABSolver.GetPieceValue(record.pieceType);
            if (record.color == currentPlayer)
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
    public override int Score(ChessBitboard bitboard, ChessColor currentPlayer)
    {
        int score = 0;

        foreach (var piece in bitboard.GetAllPieces())
        {
            var legalMoveCount = 
                BitboardHelper.GetLegalMovesForPosition(ref bitboard, Board.FromIndex(piece.position, bitboard._fileMax))
                .Count();
            
            if (piece.color == currentPlayer)
            {
                score += (int)legalMoveCount;
            }
            else
            {
                score -= (int)legalMoveCount;
            }
        }

        return score;
    }
}

[Serializable]
public class AttackingValueEvaluator : EvaluatorBase
{
    public override int Score(ChessBitboard bitboard, ChessColor currentPlayer)
    {
        int score = 0;

        // Mobility (more valid moves is usually good)
        IEnumerable<Move> playerLegalMoves = SolverBase.GetLegalMoves(bitboard, currentPlayer);
        int playerMoves = playerLegalMoves.Count();
        int enemyMoves = SolverBase.GetLegalMoves(bitboard, currentPlayer.Opponent()).Count();
        score += (int)((playerMoves - enemyMoves));

        // Reward for attacking enemy pieces
        foreach (var move in playerLegalMoves)
        {
            if (BitboardHelper.IsCapture(ref bitboard, move, currentPlayer))
            {
                var capturedPiece = BitboardHelper.GetPieceAt(ref bitboard, move.To, currentPlayer);
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
