using System;
using System.Collections.Generic;

public readonly struct ChessBitboard
{
    public static Array PIECE_TYPES = Enum.GetValues(typeof(PieceType));

    // Each bitboard represents a piece type for a specific color
    public readonly ulong WhitePawns;
    public readonly ulong BlackPawns;
    public readonly ulong WhiteKnights;
    public readonly ulong BlackKnights;
    public readonly ulong WhiteBishops;
    public readonly ulong BlackBishops;
    public readonly ulong WhiteRooks;
    public readonly ulong BlackRooks;
    public readonly ulong WhiteQueens;
    public readonly ulong BlackQueens;
    public readonly ulong WhiteKings;
    public readonly ulong BlackKings;

    public readonly int _rankMax;
    public readonly int _fileMax;
    public readonly bool _promote;

    //rankMax and fileMax should be between 1 and 8
    public ChessBitboard(
    ulong whitePawns, ulong blackPawns, ulong whiteKnights, ulong blackKnights,
    ulong whiteBishops, ulong blackBishops, ulong whiteRooks, ulong blackRooks,
    ulong whiteQueens, ulong blackQueens, ulong whiteKings, ulong blackKings,
    int rankMax, int fileMax, bool canPromote)
    {
        WhitePawns = whitePawns;
        BlackPawns = blackPawns;
        WhiteKnights = whiteKnights;
        BlackKnights = blackKnights;
        WhiteBishops = whiteBishops;
        BlackBishops = blackBishops;
        WhiteRooks = whiteRooks;
        BlackRooks = blackRooks;
        WhiteQueens = whiteQueens;
        BlackQueens = blackQueens;
        WhiteKings = whiteKings;
        BlackKings = blackKings;
        _rankMax = rankMax;
        _fileMax = fileMax;
        _promote = canPromote;
    }

    //Instead of calling With repeatedly, batch updates into a single With call.
    //board = board.With(whitePawns: board.WhitePawns & mask, whiteKnights: board.WhiteKnights & mask);
    public ChessBitboard With(
        ulong? whitePawns = null, ulong? blackPawns = null,
        ulong? whiteKnights = null, ulong? blackKnights = null,
        ulong? whiteBishops = null, ulong? blackBishops = null,
        ulong? whiteRooks = null, ulong? blackRooks = null,
        ulong? whiteQueens = null, ulong? blackQueens = null,
        ulong? whiteKings = null, ulong? blackKings = null,
        int? rankMax = null, int? fileMax = null, bool? canPromote = null)
    {
        return new ChessBitboard(
            whitePawns ?? WhitePawns,
            blackPawns ?? BlackPawns,
            whiteKnights ?? WhiteKnights,
            blackKnights ?? BlackKnights,
            whiteBishops ?? WhiteBishops,
            blackBishops ?? BlackBishops,
            whiteRooks ?? WhiteRooks,
            blackRooks ?? BlackRooks,
            whiteQueens ?? WhiteQueens,
            blackQueens ?? BlackQueens,
            whiteKings ?? WhiteKings,
            blackKings ?? BlackKings,
            fileMax ?? _fileMax,
            rankMax ?? _rankMax,
            canPromote ?? _promote
        );
    }

    // Method that returns IEnumerable of pieces and their positions
    public IEnumerable<(PieceType pieceType, int position, ChessColor color)> GetAllPieces()
    {
        // Yield pieces for each type, white and black
        foreach (var (bitboard, pieceType, color) in GetPieceBitboards())
        {
            for (int position = 0; position < 64; position++)
            {
                if ((bitboard & (1UL << position)) != 0)
                {
                    yield return (pieceType, position, color);
                }
            }
        }
    }

    // Helper method to iterate over the bitboards and corresponding piece types
    public IEnumerable<(ulong positionIndex, PieceType pieceType, ChessColor color)> GetPieceBitboards()
    {
        // Yield each bitboard with the corresponding piece type and color
        yield return (WhitePawns, PieceType.Pawn, ChessColor.w); // White Pawns
        yield return (BlackPawns, PieceType.Pawn, ChessColor.b); // Black Pawns
        yield return (WhiteKnights, PieceType.Knight, ChessColor.w); // White Knights
        yield return (BlackKnights, PieceType.Knight, ChessColor.b); // Black Knights
        yield return (WhiteBishops, PieceType.Bishop, ChessColor.w); // White Bishops
        yield return (BlackBishops, PieceType.Bishop, ChessColor.b); // Black Bishops
        yield return (WhiteRooks, PieceType.Rook, ChessColor.w); // White Rooks
        yield return (BlackRooks, PieceType.Rook, ChessColor.b); // Black Rooks
        yield return (WhiteQueens, PieceType.Queen, ChessColor.w); // White Queens
        yield return (BlackQueens, PieceType.Queen, ChessColor.b); // Black Queens
        yield return (WhiteKings, PieceType.King, ChessColor.w); // White Kings
        yield return (BlackKings, PieceType.King, ChessColor.b); // Black Kings
    }

    public IEnumerable<Move> GetCandidateMoves(int positionIndex)
    {
        List<Move> validMoves = new List<Move>();

        // Check if there is a piece at the given position (if it's a 1 on any bitboard)
        if (((WhitePawns | BlackPawns | WhiteKnights | BlackKnights | WhiteBishops | BlackBishops | WhiteRooks | BlackRooks | WhiteQueens | BlackQueens | WhiteKings | BlackKings) & (1UL << positionIndex)) == 0)
            yield break; // No piece at this position

        // Get the piece at the position (check which bitboard the position belongs to)
        if ((WhitePawns & (1UL << positionIndex)) != 0)
        {
            // White Pawn: Can move forward one square or capture diagonally
            validMoves.AddRange(GetPawnMoves(positionIndex, ChessColor.w));
        }
        else if ((BlackPawns & (1UL << positionIndex)) != 0)
        {
            // Black Pawn: Can move forward one square or capture diagonally
            validMoves.AddRange(GetPawnMoves(positionIndex, ChessColor.b));
        }
        else if ((WhiteKnights & (1UL << positionIndex)) != 0)
        {
            // White Knight: L-shaped moves
            validMoves.AddRange(GetKnightMoves(positionIndex, ChessColor.w));
        }
        else if ((BlackKnights & (1UL << positionIndex)) != 0)
        {
            // Black Knight: L-shaped moves
            validMoves.AddRange(GetKnightMoves(positionIndex, ChessColor.b));
        }
        else if ((WhiteBishops & (1UL << positionIndex)) != 0)
        {
            // White Bishop: Diagonal moves
            validMoves.AddRange(GetBishopMoves(positionIndex, ChessColor.w));
        }
        else if ((BlackBishops & (1UL << positionIndex)) != 0)
        {
            // Black Bishop: Diagonal moves
            validMoves.AddRange(GetBishopMoves(positionIndex, ChessColor.b));
        }
        else if ((WhiteRooks & (1UL << positionIndex)) != 0)
        {
            // White Rook: Horizontal and vertical moves
            validMoves.AddRange(GetRookMoves(positionIndex, ChessColor.w));
        }
        else if ((BlackRooks & (1UL << positionIndex)) != 0)
        {
            // Black Rook: Horizontal and vertical moves
            validMoves.AddRange(GetRookMoves(positionIndex, ChessColor.b));
        }
        else if ((WhiteQueens & (1UL << positionIndex)) != 0)
        {
            // White Queen: Horizontal, vertical, and diagonal moves
            validMoves.AddRange(GetQueenMoves(positionIndex, ChessColor.w));
        }
        else if ((BlackQueens & (1UL << positionIndex)) != 0)
        {
            // Black Queen: Horizontal, vertical, and diagonal moves
            validMoves.AddRange(GetQueenMoves(positionIndex, ChessColor.b));
        }
        else if ((WhiteKings & (1UL << positionIndex)) != 0)
        {
            // White King: One square in any direction
            validMoves.AddRange(GetKingMoves(positionIndex, ChessColor.w));
        }
        else if ((BlackKings & (1UL << positionIndex)) != 0)
        {
            // Black King: One square in any direction
            validMoves.AddRange(GetKingMoves(positionIndex, ChessColor.b));
        }

        foreach (var move in validMoves)
        {
            yield return move;
        }
    }

    //used for echo chess
    public IEnumerable<Move> GetPlacableMoves(PieceType pieceType, int positionIndex, ChessColor color)
    {
        List<Move> placableMoves = new List<Move>();

        switch (pieceType)
        {
            case PieceType.King:
                placableMoves.AddRange(GetKingMoves(positionIndex, color));
                break;
            case PieceType.Queen:
                placableMoves.AddRange(GetQueenMoves(positionIndex, color));
                break;
            case PieceType.Bishop:
                placableMoves.AddRange(GetBishopMoves(positionIndex, color));
                break;
            case PieceType.Rook:
                placableMoves.AddRange(GetRookMoves(positionIndex, color));
                break;
            case PieceType.Knight:
                placableMoves.AddRange(GetKnightMoves(positionIndex, color));
                break;
            case PieceType.Pawn:
                placableMoves.AddRange(GetPlacablePawnMoves(positionIndex, color));
                break;
            default:
                break;
        };

        foreach (var move in placableMoves)
        {
            yield return move;
        }
    }

    // Get possible pawn moves (can move one square forward or capture diagonally)
    private IEnumerable<Move> GetPawnMoves(int positionIndex, ChessColor color)
    {
        List<Move> moves = new List<Move>();

        int boardSize = _fileMax;  // Number of files (columns)
        int rankCount = _rankMax;  // Number of ranks (rows)

        int direction = (color == ChessColor.w) ? boardSize : -boardSize; // White moves up, Black moves down
        int startRank = (color == ChessColor.w) ? 1 : (rankCount - 2); // Adapt to custom board sizes

        int forwardOne = positionIndex + direction;
        int forwardTwo = positionIndex + (2 * direction);
        int leftCapture = positionIndex + direction - 1;
        int rightCapture = positionIndex + direction + 1;

        // Move one square forward
        if (IsOnBoard(forwardOne) && IsSquareEmpty(forwardOne))
            moves.Add(new Move(positionIndex, forwardOne));

        // First move can be two squares forward if pawn is on the starting rank
        if ((positionIndex / boardSize) == startRank && IsOnBoard(forwardTwo) &&
            IsSquareEmpty(forwardOne) && IsSquareEmpty(forwardTwo))
            moves.Add(new Move(positionIndex, forwardTwo));

        // Capture diagonally (ensure it doesn't wrap around the board)
        if (positionIndex % boardSize > 0 && IsOnBoard(leftCapture) && IsEnemyPieceAt(leftCapture, color))
            moves.Add(new Move(positionIndex, leftCapture));

        if (positionIndex % boardSize < boardSize - 1 && IsOnBoard(rightCapture) && IsEnemyPieceAt(rightCapture, color))
            moves.Add(new Move(positionIndex, rightCapture));

        return moves;
    }

    private IEnumerable<Move> GetPlacablePawnMoves(int positionIndex, ChessColor color)
    {
        List<Move> moves = new List<Move>();

        var boardSize = _fileMax;

        int direction = (color == ChessColor.w) ? boardSize : -boardSize; // White moves up, Black moves down

        int leftCapture = positionIndex + direction - 1;
        int rightCapture = positionIndex + direction + 1;

        // Capture diagonally (ensure it doesn't wrap around the board)
        if (positionIndex % boardSize > 0 && IsOnBoard(leftCapture) && IsSquareEmpty(leftCapture))
            moves.Add(new Move(positionIndex, leftCapture));
        if (positionIndex % boardSize < boardSize - 1 && IsOnBoard(rightCapture) && IsSquareEmpty(rightCapture))
            moves.Add(new Move(positionIndex, rightCapture));

        return moves;
    }

    // Get possible knight moves (L-shaped)
    private IEnumerable<Move> GetKnightMoves(int positionIndex, ChessColor color)
    {
        List<Move> moves = new List<Move>();

        var boardSize = _fileMax;

        int[] knightOffsets =
        {
        -2 * boardSize - 1, -2 * boardSize + 1, // Up-Left, Up-Right
        -boardSize - 2, -boardSize + 2,         // Left-Up, Right-Up
        boardSize - 2, boardSize + 2,           // Left-Down, Right-Down
        2 * boardSize - 1, 2 * boardSize + 1    // Down-Left, Down-Right
    };

        int x = positionIndex % boardSize;
        int y = positionIndex / boardSize;

        foreach (int offset in knightOffsets)
        {
            int newIndex = positionIndex + offset;
            int newX = newIndex % boardSize;
            int newY = newIndex / boardSize;

            // Ensure move stays within valid board range and doesn't wrap around
            if (IsOnBoard(newIndex) && Math.Abs(newX - x) <= 2 && Math.Abs(newY - y) <= 2 && !IsAlliedPieceAt(newIndex, color))
            {
                moves.Add(new Move(positionIndex, newIndex));
            }
        }

        return moves;
    }

    // Get possible bishop moves (diagonal)
    private IEnumerable<Move> GetBishopMoves(int positionIndex, ChessColor color)
    {
        List<Move> moves = new List<Move>();

        var boardSize = _fileMax;
        int[] directions = { -boardSize - 1, -boardSize + 1, boardSize + 1, boardSize - 1 };

        foreach (int direction in directions)
        {
            int nextIndex = positionIndex;

            while (true)
            {
                int previousFile = nextIndex % boardSize; // Column before moving
                nextIndex += direction;
                int currentFile = nextIndex % boardSize; // Column after moving

                if (!IsOnBoard(nextIndex) || Math.Abs(previousFile - currentFile) != 1)
                    break; // Stop if out of bounds or wrapped around

                if (IsAlliedPieceAt(nextIndex, color))
                    break; // Stop if hitting an allied piece

                moves.Add(new Move(positionIndex, nextIndex));

                if (IsEnemyPieceAt(nextIndex, color))
                    break; // Stop after capturing an enemy piece
            }
        }

        return moves;
    }

    // Get possible rook moves (vertical and horizontal)
    private IEnumerable<Move> GetRookMoves(int positionIndex, ChessColor color)
    {
        List<Move> moves = new List<Move>();

        var boardSize = _fileMax;

        int[] directions = { -boardSize, boardSize, -1, 1 }; // Up, Down, Left, Right

        foreach (int direction in directions)
        {
            int nextIndex = positionIndex;

            while (true)
            {
                int previousFile = nextIndex % boardSize; // Column before moving
                nextIndex += direction;
                int currentFile = nextIndex % boardSize; // Column after moving

                if (!IsOnBoard(nextIndex))
                    break; // Stop if out of bounds

                // Prevent horizontal wraparound
                if ((direction == -1 && previousFile == 0) || // Moving left but at leftmost column
                    (direction == 1 && previousFile == boardSize - 1)) // Moving right but at rightmost column
                    break;

                if (IsAlliedPieceAt(nextIndex, color))
                    break; // Stop if hitting an allied piece

                moves.Add(new Move(positionIndex, nextIndex));

                if (IsEnemyPieceAt(nextIndex, color))
                    break; // Stop after capturing an enemy piece
            }
        }

        return moves;
    }

    // Get possible queen moves (combines bishop and rook moves)
    private IEnumerable<Move> GetQueenMoves(int positionIndex, ChessColor color)
    {
        List<Move> moves = new List<Move>();
        moves.AddRange(GetBishopMoves(positionIndex, color));
        moves.AddRange(GetRookMoves(positionIndex, color));
        return moves;
    }

    // Get possible king moves (one square in any direction)
    private IEnumerable<Move> GetKingMoves(int positionIndex, ChessColor color)
    {
        List<Move> moves = new List<Move>();

        var boardSize = _fileMax;

        int[] kingOffsets =
        {
            -boardSize - 1, -boardSize, -boardSize + 1, // Up-Left, Up, Up-Right
            -1, 1,                                      // Left, Right
            boardSize - 1, boardSize, boardSize + 1     // Down-Left, Down, Down-Right
        };

        int x = positionIndex % boardSize;

        foreach (int offset in kingOffsets)
        {
            int newIndex = positionIndex + offset;
            int newX = newIndex % boardSize;

            // Ensure move is within board bounds and doesn't wrap horizontally
            if (IsOnBoard(newIndex) && Math.Abs(newX - x) <= 1 && !IsAlliedPieceAt(newIndex, color))
                moves.Add(new Move(positionIndex, newIndex));
        }

        return moves;
    }

    // Helper methods
    private bool IsOnBoard(int positionIndex) => positionIndex >= 0 && positionIndex < _rankMax * _fileMax;

    private bool IsSquareEmpty(int positionIndex) =>
        ((WhitePawns | BlackPawns | WhiteKnights | BlackKnights | WhiteBishops | BlackBishops |
          WhiteRooks | BlackRooks | WhiteQueens | BlackQueens | WhiteKings | BlackKings) & (1UL << positionIndex)) == 0;

    public bool IsEnemyPieceAt(int positionIndex, ChessColor color) =>
        ((color == ChessColor.w ?
          (BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKings) :
          (WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKings)) & (1UL << positionIndex)) != 0;

    public bool IsAlliedPieceAt(int positionIndex, ChessColor color) =>
        ((color == ChessColor.w ?
          (WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKings) :
          (BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKings)) & (1UL << positionIndex)) != 0;

    // Set a bit for a specific piece and color
    public ChessBitboard SetPiece(ulong mask, ChessColor color, PieceType type)
    {
        return color == ChessColor.w
            ? type switch
            {
                PieceType.Pawn => new ChessBitboard(WhitePawns | mask, BlackPawns, WhiteKnights, BlackKnights, WhiteBishops, BlackBishops, WhiteRooks, BlackRooks, WhiteQueens, BlackQueens, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.Knight => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights | mask, BlackKnights, WhiteBishops, BlackBishops, WhiteRooks, BlackRooks, WhiteQueens, BlackQueens, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.Bishop => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights, BlackKnights, WhiteBishops | mask, BlackBishops, WhiteRooks, BlackRooks, WhiteQueens, BlackQueens, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.Rook => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights, BlackKnights, WhiteBishops, BlackBishops, WhiteRooks | mask, BlackRooks, WhiteQueens, BlackQueens, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.Queen => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights, BlackKnights, WhiteBishops, BlackBishops, WhiteRooks, BlackRooks, WhiteQueens | mask, BlackQueens, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.King => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights, BlackKnights, WhiteBishops, BlackBishops, WhiteRooks, BlackRooks, WhiteQueens, BlackQueens, WhiteKings | mask, BlackKings, _rankMax, _fileMax, _promote),
                _ => this
            }
            : type switch
            {
                PieceType.Pawn => new ChessBitboard(WhitePawns, BlackPawns | mask, WhiteKnights, BlackKnights, WhiteBishops, BlackBishops, WhiteRooks, BlackRooks, WhiteQueens, BlackQueens, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.Knight => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights, BlackKnights | mask, WhiteBishops, BlackBishops, WhiteRooks, BlackRooks, WhiteQueens, BlackQueens, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.Bishop => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights, BlackKnights, WhiteBishops, BlackBishops | mask, WhiteRooks, BlackRooks, WhiteQueens, BlackQueens, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.Rook => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights, BlackKnights, WhiteBishops, BlackBishops, WhiteRooks, BlackRooks | mask, WhiteQueens, BlackQueens, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.Queen => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights, BlackKnights, WhiteBishops, BlackBishops, WhiteRooks, BlackRooks, WhiteQueens, BlackQueens | mask, WhiteKings, BlackKings, _rankMax, _fileMax, _promote),
                PieceType.King => new ChessBitboard(WhitePawns, BlackPawns, WhiteKnights, BlackKnights, WhiteBishops, BlackBishops, WhiteRooks, BlackRooks, WhiteQueens, BlackQueens, WhiteKings, BlackKings | mask, _rankMax, _fileMax, _promote),
                _ => this
            };
    }

    public PieceType? GetPieceAt(int position, ChessColor color)
    {
        ulong mask = 1UL << position;
        if (color == ChessColor.w)
        {
            if ((WhitePawns & mask) != 0) return PieceType.Pawn;
            if ((WhiteKnights & mask) != 0) return PieceType.Knight;
            if ((WhiteBishops & mask) != 0) return PieceType.Bishop;
            if ((WhiteRooks & mask) != 0) return PieceType.Rook;
            if ((WhiteQueens & mask) != 0) return PieceType.Queen;
            if ((WhiteKings & mask) != 0) return PieceType.King;
            return null; // No piece at this position
        }
        else
        {
            if ((BlackPawns & mask) != 0) return PieceType.Pawn;
            if ((BlackKnights & mask) != 0) return PieceType.Knight;
            if ((BlackBishops & mask) != 0) return PieceType.Bishop;
            if ((BlackRooks & mask) != 0) return PieceType.Rook;
            if ((BlackQueens & mask) != 0) return PieceType.Queen;
            if ((BlackKings & mask) != 0) return PieceType.King;
            return null; // No piece at this position
        }
    }

    // Check if a specific bit is set for a piece and color
    public bool IsPieceSet(int position, ChessColor color, PieceType type)
    {
        ulong mask = 1UL << position;
        switch (color)
        {
            case ChessColor.w:
                return IsWhitePieceSet(mask, type);
            case ChessColor.b:
                return IsBlackPieceSet(mask, type);
            default:
                return false;
        }
    }

    private bool IsWhitePieceSet(ulong mask, PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn:
                return (WhitePawns & mask) != 0;
            case PieceType.Knight:
                return (WhiteKnights & mask) != 0;
            case PieceType.Bishop:
                return (WhiteBishops & mask) != 0;
            case PieceType.Rook:
                return (WhiteRooks & mask) != 0;
            case PieceType.Queen:
                return (WhiteQueens & mask) != 0;
            case PieceType.King:
                return (WhiteKings & mask) != 0;
            default:
                return false;
        }
    }

    private bool IsBlackPieceSet(ulong mask, PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn:
                return (BlackPawns & mask) != 0;
            case PieceType.Knight:
                return (BlackKnights & mask) != 0;
            case PieceType.Bishop:
                return (BlackBishops & mask) != 0;
            case PieceType.Rook:
                return (BlackRooks & mask) != 0;
            case PieceType.Queen:
                return (BlackQueens & mask) != 0;
            case PieceType.King:
                return (BlackKings & mask) != 0;
            default:
                return false;
        }
    }

    // Get the bitboard for a particular piece and color
    public ulong GetPieceBitboard(ChessColor color, PieceType type)
    {
        switch (color)
        {
            case ChessColor.w:
                return GetWhitePieceBitboard(type);
            case ChessColor.b:
                return GetBlackPieceBitboard(type);
            default:
                return 0;
        }
    }

    private ulong GetWhitePieceBitboard(PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn: return WhitePawns;
            case PieceType.Knight: return WhiteKnights;
            case PieceType.Bishop: return WhiteBishops;
            case PieceType.Rook: return WhiteRooks;
            case PieceType.Queen: return WhiteQueens;
            case PieceType.King: return WhiteKings;
            default: return 0;
        }
    }

    private ulong GetBlackPieceBitboard(PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn: return BlackPawns;
            case PieceType.Knight: return BlackKnights;
            case PieceType.Bishop: return BlackBishops;
            case PieceType.Rook: return BlackRooks;
            case PieceType.Queen: return BlackQueens;
            case PieceType.King: return BlackKings;
            default: return 0;
        }
    }

    public ChessBitboard MakeMove(Move move)
    {
        int startPosition = (int)move.From;
        int endPosition = (int)move.To;
        ulong startMask = 1UL << startPosition;
        ulong endMask = 1UL << endPosition;

        // Determine piece type and color
        PieceType pieceType = default;
        ChessColor pieceColor = default;

        foreach (PieceType type in (PieceType[])Enum.GetValues(typeof(PieceType)))
        {
            if ((GetBitboard(type, ChessColor.w) & startMask) != 0)
            {
                pieceType = type;
                pieceColor = ChessColor.w;
                break;
            }
            if ((GetBitboard(type, ChessColor.b) & startMask) != 0)
            {
                pieceType = type;
                pieceColor = ChessColor.b;
                break;
            }
        }

        ChessBitboard newBoard = this;

        // Remove piece from the starting position
        newBoard = newBoard.ClearPiece(startPosition, pieceType, pieceColor);

        // Capture logic: Remove opponent's piece at the destination
        if (IsEnemyPieceAt(endPosition, pieceColor))
        {
            foreach (PieceType type in (PieceType[])Enum.GetValues(typeof(PieceType)))
            {
                if ((GetBitboard(type, pieceColor.Opponent()) & endMask) != 0)
                {
                    newBoard = newBoard.ClearPiece(endPosition, type, pieceColor.Opponent());
                    break;
                }
            }
        }

        // Promotion logic
        if (_promote && pieceType == PieceType.Pawn)
        {
            int lastRankStart = _rankMax * (_rankMax - 1); // First position of last rank
            int lastRankEnd = _rankMax * _rankMax - 1; // Last position of last rank
            int firstRankStart = 0;
            int firstRankEnd = _rankMax - 1;

            if ((pieceColor == ChessColor.w && endPosition >= lastRankStart && endPosition <= lastRankEnd) ||
                (pieceColor == ChessColor.b && endPosition >= firstRankStart && endPosition <= firstRankEnd))
            {
                pieceType = PieceType.Queen; // Promote Pawn to Queen
            }
        }

        // Set piece at the new position
        return newBoard.SetPiece(endMask, pieceColor, pieceType);
    }
    private ulong GetBitboard(PieceType type, ChessColor color)
    {
        return color == ChessColor.w ? type switch
        {
            PieceType.Pawn => WhitePawns,
            PieceType.Knight => WhiteKnights,
            PieceType.Bishop => WhiteBishops,
            PieceType.Rook => WhiteRooks,
            PieceType.Queen => WhiteQueens,
            PieceType.King => WhiteKings,
            _ => 0
        }
        : type switch
        {
            PieceType.Pawn => BlackPawns,
            PieceType.Knight => BlackKnights,
            PieceType.Bishop => BlackBishops,
            PieceType.Rook => BlackRooks,
            PieceType.Queen => BlackQueens,
            PieceType.King => BlackKings,
            _ => 0
        };
    }

    private ChessBitboard ClearPiece(int position, PieceType type, ChessColor color)
    {
        ulong mask = ~(1UL << position);

        return color == ChessColor.w ? type switch
        {
            PieceType.Pawn => this.With(whitePawns: WhitePawns & mask),
            PieceType.Knight => this.With(whiteKnights: WhiteKnights & mask),
            PieceType.Bishop => this.With(whiteBishops: WhiteBishops & mask),
            PieceType.Rook => this.With(whiteRooks: WhiteRooks & mask),
            PieceType.Queen => this.With(whiteQueens: WhiteQueens & mask),
            PieceType.King => this.With(whiteKings: WhiteKings & mask),
            _ => this
        }
        : type switch
        {
            PieceType.Pawn => this.With(blackPawns: BlackPawns & mask),
            PieceType.Knight => this.With(blackKnights: BlackKnights & mask),
            PieceType.Bishop => this.With(blackBishops: BlackBishops & mask),
            PieceType.Rook => this.With(blackRooks: BlackRooks & mask),
            PieceType.Queen => this.With(blackQueens: BlackQueens & mask),
            PieceType.King => this.With(blackKings: BlackKings & mask),
            _ => this
        };
    }

    public bool IsKingInCheck(ChessColor kingColor)
    {
        // Get the king's position
        ulong kingBitboard = kingColor == ChessColor.w ? WhiteKings : BlackKings;
        int kingPosition = BitScanForward(kingBitboard);
        if (kingPosition == -1) return false; // King not found

        // Iterate over all opponent pieces and check if they can move to the king's position
        ChessColor opponentColor = kingColor == ChessColor.w ? ChessColor.b : ChessColor.w;

        // Check all possible attacking pieces
        foreach (PieceType pieceType in PIECE_TYPES)
        {
            // Get all positions of this piece type for the opponent
            ulong pieceBitboard = GetPieceBitboard(opponentColor, pieceType);

            while (pieceBitboard != 0)
            {
                int piecePosition = BitScanForward(pieceBitboard);
                pieceBitboard &= pieceBitboard - 1; // Remove the least significant bit

                // Get all valid moves for this piece
                IEnumerable<Move> moves = GetCandidateMoves(piecePosition);

                // If any move can capture the king, return true (king is in check)
                foreach (Move move in moves)
                {
                    if (move.To == kingPosition)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    internal (bool isCheckmate, bool isStalemate, bool isCheck, bool isDraw) CheckGameOver(ChessColor player)
    {
        bool inCheck = IsKingInCheck(player);
        bool hasLegalMoves = false;

        foreach ((PieceType pieceType, int position, ChessColor color) piece in GetAllPieces())
        {
            if (player == piece.color)
            {
                IEnumerable<Move> moves = GetCandidateMoves(piece.position);
                foreach (Move move in moves)
                {
                    var newState = MakeMove(move);
                    if (!newState.IsKingInCheck(player))
                    {
                        hasLegalMoves = true;
                        break; // Early exit once a legal move is found
                    }
                }
            }

            if (hasLegalMoves) break; // Break outer loop if a legal move is found
        }

        if (!hasLegalMoves)
        {
            if (inCheck)
                return (true, false, true, false); // Checkmate
            else
                return (false, true, false, false); // Stalemate
        }

        // Check for draw conditions
        if (IsInsufficientMaterial()
            //|| IsThreefoldRepetition()
            //|| IsFiftyMoveRule())
            )
            return (false, false, false, true); // Draw

        return (false, false, inCheck, false); // Game is ongoing
    }

    private bool IsInsufficientMaterial()
    {
        //TODO add other cases
        var whiteCount = BitboardPopulationCount(GetAlliedMask(ChessColor.w));
        var blackCount = BitboardPopulationCount(GetAlliedMask(ChessColor.b));

        return whiteCount + blackCount <= 2;
    }

    ulong GetAlliedMask(ChessColor color) => color == ChessColor.b ?
        (BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKings) :
          (WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKings);

    private int BitboardPopulationCount(ulong bitboard)
    {
        int count = 0;
        while (bitboard != 0)
        {
            bitboard &= bitboard - 1; // Removes the least significant bit set to 1
            count++;
        }
        return count;
    }

    public ChessBitboard Clone()
    {
        return new ChessBitboard(
            WhitePawns, BlackPawns,
            WhiteKnights, BlackKnights,
            WhiteBishops, BlackBishops,
            WhiteRooks, BlackRooks,
            WhiteQueens, BlackQueens,
            WhiteKings, BlackKings,
            _rankMax, _fileMax, _promote);
    }

    private int BitScanForward(ulong bitboard)
    {
        if (bitboard == 0) return -1; // No bits set

        int index = 0;
        while ((bitboard & 1) == 0)
        {
            bitboard >>= 1;
            index++;
        }
        return index;
    }
}

public enum PieceType
{
    King,
    Queen,
    Bishop,
    Rook,
    Knight,
    Pawn
}