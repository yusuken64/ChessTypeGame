using System;
using System.Collections.Generic;

public struct ChessBitboard
{
    public static Array PIECE_TYPES = Enum.GetValues(typeof(PieceType));

    // Each bitboard represents a piece type for a specific color
    public ulong WhitePawns;
    public ulong BlackPawns;
    public ulong WhiteKnights;
    public ulong BlackKnights;
    public ulong WhiteBishops;
    public ulong BlackBishops;
    public ulong WhiteRooks;
    public ulong BlackRooks;
    public ulong WhiteQueens;
    public ulong BlackQueens;
    public ulong WhiteKings;
    public ulong BlackKings;

    private int _rankMax;
    private int _fileMax;

    //rankMax and fileMax should be between 1 and 8
    public ChessBitboard(string fen, int rankMax, int fileMax)
    {
        _rankMax = rankMax;
        _fileMax = fileMax;

        // Initialize all the bitboards to empty
        WhitePawns = BlackPawns = WhiteKnights = BlackKnights = 0;
        WhiteBishops = BlackBishops = WhiteRooks = BlackRooks = 0;
        WhiteQueens = BlackQueens = WhiteKings = BlackKings = 0;

        // Split only if there is enough data
        var spaceIndex = fen.IndexOf(' ');
        if (spaceIndex == -1) throw new ArgumentException("Invalid FEN format.");

        string boardData = fen[..spaceIndex]; // Get only board layout
        var boardLayout = boardData.Split('/');

        if (boardLayout.Length != rankMax)
            throw new ArgumentException("Invalid FEN: Rank count mismatch.");

        int rankOffset = rankMax - 1;
        for (int rank = 0; rank < rankMax; rank++)
        {
            string rankString = boardLayout[rank];
            int file = 0;

            for (int i = 0; i < rankString.Length; i++)
            {
                char c = rankString[i];
                if (c >= '1' && c <= '8') // Numeric empty squares
                {
                    file += c - '0';
                }
                else // Piece present
                {
                    SetPieceForFEN(c, rankOffset - rank, file);
                    file++;
                }
            }
        }
    }

    // Helper method to set a piece on the correct bitboard based on the FEN character
    private void SetPieceForFEN(char piece, int rank, int file)
    {
        int position = (rank * _fileMax) + file;
        ChessColor color = char.IsUpper(piece) ? ChessColor.w : ChessColor.b;

        PieceType type = piece switch
        {
            'P' or 'p' => PieceType.Pawn,
            'N' or 'n' => PieceType.Knight,
            'B' or 'b' => PieceType.Bishop,
            'R' or 'r' => PieceType.Rook,
            'Q' or 'q' => PieceType.Queen,
            'K' or 'k' => PieceType.King,
            _ => throw new ArgumentException($"Invalid piece character '{piece}' in FEN string.")
        };

        SetPiece(position, color, type);
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
    public void SetPiece(int position, ChessColor color, PieceType type)
    {
        ulong mask = 1UL << position;
        switch (color)
        {
            case ChessColor.w:
                SetWhitePiece(mask, type);
                break;
            case ChessColor.b:
                SetBlackPiece(mask, type);
                break;
        }
    }

    private void SetWhitePiece(ulong mask, PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn:
                WhitePawns |= mask;
                break;
            case PieceType.Knight:
                WhiteKnights |= mask;
                break;
            case PieceType.Bishop:
                WhiteBishops |= mask;
                break;
            case PieceType.Rook:
                WhiteRooks |= mask;
                break;
            case PieceType.Queen:
                WhiteQueens |= mask;
                break;
            case PieceType.King:
                WhiteKings |= mask;
                break;
        }
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

    private void SetBlackPiece(ulong mask, PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn:
                BlackPawns |= mask;
                break;
            case PieceType.Knight:
                BlackKnights |= mask;
                break;
            case PieceType.Bishop:
                BlackBishops |= mask;
                break;
            case PieceType.Rook:
                BlackRooks |= mask;
                break;
            case PieceType.Queen:
                BlackQueens |= mask;
                break;
            case PieceType.King:
                BlackKings |= mask;
                break;
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

    public void MakeMove(Move move)
    {
        var startPosition = (int)move.From;
        var endPosition = (int)move.To;

        // Get the piece at the start position
        ChessColor pieceColor = default; // Track the color of the piece
        PieceType pieceType = default; // Track the color of the piece
        if ((WhitePawns & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.w;
            pieceType = PieceType.Pawn;
            WhitePawns &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackPawns & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.b;
            pieceType = PieceType.Pawn;
            BlackPawns &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteKnights & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.w;
            pieceType = PieceType.Knight;
            WhiteKnights &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackKnights & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.b;
            pieceType = PieceType.Knight;
            BlackKnights &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteBishops & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.w;
            pieceType = PieceType.Bishop;
            WhiteBishops &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackBishops & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.b;
            pieceType = PieceType.Bishop;
            BlackBishops &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteRooks & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.w;
            pieceType = PieceType.Rook;
            WhiteRooks &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackRooks & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.b;
            pieceType = PieceType.Rook;
            BlackRooks &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteQueens & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.w;
            pieceType = PieceType.Queen;
            WhiteQueens &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackQueens & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.b;
            pieceType = PieceType.Queen;
            BlackQueens &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteKings & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.w;
            pieceType = PieceType.King;
            WhiteKings &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackKings & (1UL << startPosition)) != 0)
        {
            pieceColor = ChessColor.b;
            pieceType = PieceType.King;
            BlackKings &= ~(1UL << startPosition); // Remove from start position
        }

        // Capture logic (remove captured piece if any)
        if (IsEnemyPieceAt(endPosition, pieceColor))
        {
            // Remove the captured piece from the appropriate enemy bitboard
            if (pieceColor == ChessColor.w)
            {
                // Remove from Black pieces bitboard
                BlackPawns &= ~(1UL << endPosition);
                BlackKnights &= ~(1UL << endPosition);
                BlackBishops &= ~(1UL << endPosition);
                BlackRooks &= ~(1UL << endPosition);
                BlackQueens &= ~(1UL << endPosition);
                BlackKings &= ~(1UL << endPosition);
            }
            else
            {
                // Remove from White pieces bitboard
                WhitePawns &= ~(1UL << endPosition);
                WhiteKnights &= ~(1UL << endPosition);
                WhiteBishops &= ~(1UL << endPosition);
                WhiteRooks &= ~(1UL << endPosition);
                WhiteQueens &= ~(1UL << endPosition);
                WhiteKings &= ~(1UL << endPosition);
            }
        }

        SetPiece(endPosition, pieceColor, pieceType);
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

    internal (bool isCheckmate, bool isStalemate, bool isCheck) CheckGameOver(ChessColor player)
    {
        bool inCheck = IsKingInCheck(player);

        // Clone the board to avoid modifying the original state
        var originalState = Clone();

        // Iterate over all pieces of the player and check if any legal move exists
        foreach (PieceType pieceType in PIECE_TYPES)
        {
            ulong pieceBitboard = GetPieceBitboard(player, pieceType);

            while (pieceBitboard != 0)
            {
                int piecePosition = BitScanForward(pieceBitboard);
                pieceBitboard &= pieceBitboard - 1; // Remove the least significant bit

                // Get all valid moves for this piece
                IEnumerable<Move> moves = GetCandidateMoves(piecePosition);

                foreach (Move move in moves)
                {
                    ChessBitboard state = originalState.Clone();
                    state.MakeMove(move);

                    // If any legal move exists, it's neither checkmate nor stalemate
                    if (!state.IsKingInCheck(player))
                    {
                        return (false, false, inCheck);
                    }
                }
            }
        }

        // If no legal moves exist, determine checkmate or stalemate
        return inCheck ? (true, false, true) : (false, true, false);
    }

    public ChessBitboard Clone()
    {
        return new ChessBitboard()
        {
            WhitePawns = this.WhitePawns,
            BlackPawns = this.BlackPawns,
            WhiteKnights = this.WhiteKnights,
            BlackKnights = this.BlackKnights,
            WhiteBishops = this.WhiteBishops,
            BlackBishops = this.BlackBishops,
            WhiteRooks = this.WhiteRooks,
            BlackRooks = this.BlackRooks,
            WhiteQueens = this.WhiteQueens,
            BlackQueens = this.BlackQueens,
            WhiteKings = this.WhiteKings,
            BlackKings = this.BlackKings,
            _rankMax = this._rankMax,
            _fileMax = this._fileMax,
        };
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