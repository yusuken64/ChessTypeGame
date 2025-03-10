using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Solver : MonoBehaviour
{
    public Move GetNextMove(Board board)
    {
        PieceRecord?[,] boardData = ToBoardData(board);
        var fen = FENParser.BoardToFEN(boardData, "b");
        var nextMove = GetBestMove(fen, 2);

        (int fromX, int fromY) from = FromIndex(nextMove.From);
        (int toX, int toY) to = FromIndex(nextMove.To);

        PieceRecord? piece = boardData[from.fromX, from.fromY];

        Debug.Log($"Best Move: {piece.Value.PieceMovement} from {from} to {to}");
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

    public (int x, int y) FromIndex(int positionIndex)
    {
        return (positionIndex % 8, positionIndex / 8);
    }

    public static Move GetBestMove(string fen, int maxDepth)
    {
        ChessGameRecord game = new ChessGameRecord(fen);
        Player currentPlayer = game.FenData.ActiveColor == "w" ? Player.w : Player.b;
        Stack<(ChessGameRecord, List<Move>, int)> stack = new Stack<(ChessGameRecord, List<Move>, int)>();

        stack.Push((game, new List<Move>(), 0));  // Push initial depth as 0

        Move bestMove = default;
        int bestScore = int.MaxValue;

        while (stack.Count > 0)
        {
            var (currentGame, path, currentDepth) = stack.Pop();

            // If we've reached the max depth, don't explore further
            if (currentDepth >= maxDepth)
            {
                continue;
            }

            if (IsCheckmate(currentGame, currentPlayer))
            {
                return path.Count > 0 ? path[0] : default;
            }

            List<Move> possibleMoves = GetValidMoves(currentGame, currentPlayer);
            possibleMoves.Sort((a, b) => EvaluateMove(a, currentGame).CompareTo(EvaluateMove(b, currentGame))); // Least action heuristic

            foreach (var move in possibleMoves)
            {
                ChessGameRecord newGame = new ChessGameRecord(currentGame.fen);
                newGame.MakeMove(move);

                List<Move> newPath = new List<Move>(path) { move };
                stack.Push((newGame, newPath, currentDepth + 1));  // Increment the depth

                int moveScore = EvaluateMove(move, currentGame);
                if (moveScore < bestScore)
                {
                    bestScore = moveScore;
                    bestMove = move;
                }
            }
        }

        return bestMove;
    }

    private static List<Move> GetValidMoves(ChessGameRecord game, Player player)
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

    private static bool IsCheckmate(ChessGameRecord game, Player player)
    {
        return game.IsInCheckmate(player);
    }

    private static int EvaluateMove(Move move, ChessGameRecord game)
    {
        // Basic heuristic: prefer moves that lead to checkmate sooner
        ChessGameRecord tempGame = new ChessGameRecord(game.fen);
        tempGame.MakeMove(move);

        if (tempGame.IsInCheckmate(tempGame.FenData.ActiveColor == "w" ? Player.w : Player.b))
        {
            return 0; // Immediate checkmate is best
        }

        return tempGame.IsInCheck(tempGame.FenData.ActiveColor == "w" ? Player.w : Player.b) ? 1 : 10; // Prefer checks over neutral moves
    }
}

internal struct ChessGameRecord
{
    public ChessGameRecord(string fen)
    {
        this.fen = fen;

        FenData = FENParser.ParseFEN(fen);
        ChessBitboard = new ChessBitboard(fen);
    }

    public string fen;
    public FENData FenData;
    public ChessBitboard ChessBitboard;

    internal IEnumerable<Move> GetValidMoves((int x, int y) position)
    {
        int positionIndex = position.y * 8 + position.x;
        return ChessBitboard.GetValidMoves(positionIndex);
    }

    internal bool IsInCheck(Player whoseTurn)
    {
        return ChessBitboard.IsInCheck(whoseTurn);
    }

    internal bool IsInCheckmate(Player player)
    {
        return ChessBitboard.IsInCheckMate(player);
    }

    internal void MakeMove(Move move)
    {
        ChessBitboard.MakeMove(move);
    }
}

public enum Player
{
    w = 'w',
    b = 'b'
}

public struct Move
{
    public int From;
    public int To;

    public Move(int from, int to)
    {
        From = from;
        To = to;
    }
}
