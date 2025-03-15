using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ChessAgent : Agent
{
    public ChessColor ChessColor;
    public ChessGame ChessGame;
    public MinimaxABSolver Solver;

    public int TurnMax;

    public override void OnEpisodeBegin()
    {
        _thinking = false;
        if (ChessColor == ChessColor.w)
        {
            ChessGame.ResetGame();
        }
    }

    private int from;
    private int to;
    private Move currentValidMove;

    public bool HasValidMove = false;

    public float ValidMoveReward;
    public float InvalidMoveReward;

    private bool _thinking;
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (ChessGame.FullMove >= TurnMax) { EndEpisode(); return; }
        if (HasValidMove || _thinking) { return; }
        from = actions.DiscreteActions[0];
        to = actions.DiscreteActions[1];

        if (ChessGame.ActivePlayer == ChessColor)
        {
            _thinking = true;
            var boardData = MinimaxABSolver.ToBoardData(ChessGame.Board);
            string fen = FENParser.BoardToFEN(boardData, ChessGame.Board.Width, ChessGame.Board.Height);
            var bitboard = BitboardHelper.FromFen(fen, boardData.GetLength(0), boardData.GetLength(1), ChessGame.Board.CanQueenPromote);
            var position = Board.FromIndex(from, ChessGame.Board.Width);
            var validPiece = bitboard.IsAlliedPieceAt(from, ChessColor);

            var legalMoveExists = BitboardHelper.GetLegalMovesForPosition(ref bitboard, (position.x, position.y))
                .Any(x => x.To == to && x.From == from);
            if (validPiece && legalMoveExists)
            {
                HasValidMove = true;
                currentValidMove = BitboardHelper.GetLegalMovesForPosition(ref bitboard, (position.x, position.y))
                    .First(x => x.To == to && x.From == from);
                var fromPos = Board.FromIndex(currentValidMove.From, ChessGame.Board.Width);
                var toPos = Board.FromIndex(currentValidMove.To, ChessGame.Board.Width);

                var enemyColor = ChessColor.Opponent();
                (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) gameOverPlayer = bitboard.CheckGameOver(ChessColor);
                (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) gameOverEnemy = bitboard.CheckGameOver(enemyColor);
                int score = Solver.EvaluateBoard(bitboard, ChessColor, gameOverPlayer, gameOverEnemy);

                var afterMoveBitboard = bitboard.MakeMove(currentValidMove);

                (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) gameOverPlayer2 = afterMoveBitboard.CheckGameOver(ChessColor);
                (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) gameOverEnemy2 = afterMoveBitboard.CheckGameOver(enemyColor);

                if (gameOverPlayer2.isCheckmate || gameOverPlayer2.isCheck)
                {
                    AddReward(InvalidMoveReward);
                    _thinking = false;
                    return;
                }

                int score2 = Solver.EvaluateBoard(afterMoveBitboard, ChessColor, gameOverPlayer2, gameOverEnemy2);

                var delta = score2 - score + ValidMoveReward;
                Debug.Log($"{ChessColor}:{delta}");
                AddReward(delta);

                if (gameOverPlayer2.isCheckmate || gameOverPlayer2.isStalemate ||
                    gameOverEnemy2.isCheckmate || gameOverEnemy2.isStalemate)
                {
                    EndEpisode();
                }
            }
            else
            {
                AddReward(InvalidMoveReward);
            }
            _thinking = false;
        }
    }

    internal Move GetNextMove()
    {
        HasValidMove = false;
        return currentValidMove;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var boardData = MinimaxABSolver.ToBoardData(ChessGame.Board);
        string fen = FENParser.BoardToFEN(boardData, ChessGame.Board.Width, ChessGame.Board.Height);
        var bitboard = BitboardHelper.FromFen(fen, boardData.GetLength(0), boardData.GetLength(1), ChessGame.Board.CanQueenPromote);

        sensor.AddObservation(ChessColor == ChessColor.w ? 0 : 1);
        sensor.AddObservation(bitboard._rankMax);
        sensor.AddObservation(bitboard._fileMax);

        List<ulong> bitBoards = new List<ulong>()
        {
            bitboard.WhitePawns,
            bitboard.BlackPawns,
            bitboard.WhiteKnights,
            bitboard.BlackKnights,
            bitboard.WhiteBishops,
            bitboard.BlackBishops,
            bitboard.WhiteRooks,
            bitboard.BlackRooks,
            bitboard.WhiteQueens,
            bitboard.BlackQueens,
            bitboard.WhiteKings,
            bitboard.BlackKings,
        };

        List<float> bitBoardsAsInts = PackUlongsToFloats(bitBoards);
        sensor.AddObservation(bitBoardsAsInts);

        base.CollectObservations(sensor);
    }

    static List<float> PackUlongsToFloats(List<ulong> values)
    {
        List<float> result = new List<float>(values.Count * 2);

        foreach (ulong value in values)
        {
            uint high = (uint)(value >> 32); // Top 32 bits
            uint low = (uint)(value & 0xFFFFFFFF); // Bottom 32 bits

            result.Add(BitConverter.Int32BitsToSingle((int)high));
            result.Add(BitConverter.Int32BitsToSingle((int)low));
        }

        return result;
    }
}
