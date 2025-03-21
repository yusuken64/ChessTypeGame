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
    public List<PieceValueTableSO> PieceSquareTables;
    public override int Score(ChessBitboard bitboard, ChessColor currentPlayer)
    {
        int score = 0;

        foreach (var record in bitboard.GetAllPieces())
        {
            int pieceValue = MinimaxABSolver.GetPieceValue(record.pieceType);
            var position = Board.FromIndex(record.position, bitboard._fileMax);
            int piecePositionBonus = GetPositionalBonus(record.pieceType, position.x, position.y, bitboard._rankMax, bitboard._fileMax);

            if (record.color == currentPlayer)
            {
                score += pieceValue + piecePositionBonus;
            }
            else
            {
                score -= pieceValue + piecePositionBonus;
            }
        }
        return score;
    }

    private int GetPositionalBonus(PieceType piece, int x, int y, int rankMax, int fileMax)
    {
        int[,] pieceTable = GetPieceSquareTable(piece);

        // Convert the x, y positions to match the table
        // For example, for an 8x8 board, (0,0) is the top-left and (7,7) is the bottom-right.
        return pieceTable[y, x]; // Table is already aligned to the board coordinates
    }

    private int[,] GetPieceSquareTable(PieceType piece)
    {
        switch (piece)
        {
            case PieceType.Pawn: return PieceSquareTables.First(x => x.pieceType == PieceType.Pawn).AsCachedTable();
            case PieceType.Knight: return PieceSquareTables.First(x => x.pieceType == PieceType.Knight).AsCachedTable();
            case PieceType.Bishop: return PieceSquareTables.First(x => x.pieceType == PieceType.Bishop).AsCachedTable();
            case PieceType.Rook: return PieceSquareTables.First(x => x.pieceType == PieceType.Rook).AsCachedTable();
            case PieceType.Queen: return PieceSquareTables.First(x => x.pieceType == PieceType.Queen).AsCachedTable();
            case PieceType.King: return PieceSquareTables.First(x => x.pieceType == PieceType.King).AsCachedTable();
            default: return new int[8, 8]; // Default (no bonus)
        }
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
