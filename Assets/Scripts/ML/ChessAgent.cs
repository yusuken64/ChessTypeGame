using System;
using System.Collections;
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
    public Solver Solver;

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
            var boardData = Solver.ToBoardData(ChessGame.Board);
            string fen = FENParser.BoardToFEN(boardData, ChessGame.Board.Width, ChessGame.Board.Height);
            ChessGameRecord game = new ChessGameRecord(fen, ChessGame.Board.Width, ChessGame.Board.Height);
            var position = Board.FromIndex(from, ChessGame.Board.Width);
            IEnumerable<Move> validMoves = game.GetCandidateMoves((position.x, position.y));
            IEnumerable<Move> validMoves2 = validMoves.Where(x =>
                game.ChessBitboard.IsAlliedPieceAt(from, ChessColor) &&
                x.From == from &&
                x.To == to
            );
            if (validMoves2.Any())
            {
                HasValidMove = true;
                currentValidMove = validMoves2.First();
                var fromPos = Board.FromIndex(currentValidMove.From, ChessGame.Board.Width);
                var toPos = Board.FromIndex(currentValidMove.To, ChessGame.Board.Width);

                var enemyColor = Solver.OtherColor(ChessColor);
                (bool isCheckmate, bool isStalemate, bool isCheck) gameOverPlayer = Solver.CheckGameOver(game, ChessColor);
                (bool isCheckmate, bool isStalemate, bool isCheck) gameOverEnemy = Solver.CheckGameOver(game, enemyColor);
                int score = Solver.EvaluateBoard(game, ChessColor, gameOverPlayer, gameOverEnemy);

                game.MakeMove(currentValidMove);

                (bool isCheckmate, bool isStalemate, bool isCheck) gameOverPlayer2 = Solver.CheckGameOver(game, ChessColor);
                (bool isCheckmate, bool isStalemate, bool isCheck) gameOverEnemy2 = Solver.CheckGameOver(game, enemyColor);

                if (gameOverPlayer2.isCheckmate || gameOverPlayer2.isCheck)
                {
                    AddReward(InvalidMoveReward);
                    _thinking = false;
                    return;
                }

                int score2 = Solver.EvaluateBoard(game, ChessColor, gameOverPlayer2, gameOverEnemy2);

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
        var boardData = Solver.ToBoardData(ChessGame.Board);
        string fen = FENParser.BoardToFEN(boardData, ChessGame.Board.Width, ChessGame.Board.Height);
        ChessGameRecord game = new ChessGameRecord(fen, ChessGame.Board.Width, ChessGame.Board.Height);
        
        sensor.AddObservation(ChessColor == ChessColor.w ? 0 : 1);
        sensor.AddObservation(game.rankMax);
        sensor.AddObservation(game.fileMax);

        List<ulong> bitBoards = new List<ulong>()
        {
            game.ChessBitboard.WhitePawns,
            game.ChessBitboard.BlackPawns,
            game.ChessBitboard.WhiteKnights,
            game.ChessBitboard.BlackKnights,
            game.ChessBitboard.WhiteBishops,
            game.ChessBitboard.BlackBishops,
            game.ChessBitboard.WhiteRooks,
            game.ChessBitboard.BlackRooks,
            game.ChessBitboard.WhiteQueens,
            game.ChessBitboard.BlackQueens,
            game.ChessBitboard.WhiteKings,
            game.ChessBitboard.BlackKings,
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
