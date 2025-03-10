using System;
using System.Collections.Generic;

public struct ChessBitboard
{
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

    public static readonly ulong FileA = 0x0101010101010101;  // Mask for the a-file (1st column)
    public static readonly ulong FileB = 0x0202020202020202;  // Mask for the b-file (2nd column)
    public static readonly ulong FileG = 0x4000400040004000;  // Mask for the g-file (7th column)
    public static readonly ulong FileH = 0x8080808080808080;  // Mask for the h-file (8th column)

    // Constructor to initialize all bitboards (empty board)
    public ChessBitboard(ulong whitePawns = 0, ulong blackPawns = 0, ulong whiteKnights = 0, ulong blackKnights = 0,
                          ulong whiteBishops = 0, ulong blackBishops = 0, ulong whiteRooks = 0, ulong blackRooks = 0,
                          ulong whiteQueens = 0, ulong blackQueens = 0, ulong whiteKings = 0, ulong blackKings = 0)
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
    }

    public ChessBitboard(string fen)
    {
        // Initialize all the bitboards to empty
        WhitePawns = BlackPawns = WhiteKnights = BlackKnights = 0;
        WhiteBishops = BlackBishops = WhiteRooks = BlackRooks = 0;
        WhiteQueens = BlackQueens = WhiteKings = BlackKings = 0;

        // Split the FEN into board layout and other parts
        var fenParts = fen.Split(' ');
        var boardLayout = fenParts[0].Split('/');

        // Parse the board layout
        for (int rank = 0; rank < 8; rank++)
        {
            string rankString = boardLayout[rank];
            int file = 0;

            foreach (char c in rankString)
            {
                if (char.IsDigit(c)) // Empty squares
                {
                    file += (int)char.GetNumericValue(c);
                }
                else // Piece present
                {
                    SetPieceForFEN(c, 7 - rank, file);
                    file++;
                }
            }
        }
    }

    // Helper method to set a piece on the correct bitboard based on the FEN character
    private void SetPieceForFEN(char piece, int rank, int file)
    {
        int position = (rank * 8) + file;
        switch (piece)
        {
            case 'P':
                SetPiece(position, PieceColor.White, PieceType.Pawn);
                break;
            case 'p':
                SetPiece(position, PieceColor.Black, PieceType.Pawn);
                break;
            case 'N':
                SetPiece(position, PieceColor.White, PieceType.Knight);
                break;
            case 'n':
                SetPiece(position, PieceColor.Black, PieceType.Knight);
                break;
            case 'B':
                SetPiece(position, PieceColor.White, PieceType.Bishop);
                break;
            case 'b':
                SetPiece(position, PieceColor.Black, PieceType.Bishop);
                break;
            case 'R':
                SetPiece(position, PieceColor.White, PieceType.Rook);
                break;
            case 'r':
                SetPiece(position, PieceColor.Black, PieceType.Rook);
                break;
            case 'Q':
                SetPiece(position, PieceColor.White, PieceType.Queen);
                break;
            case 'q':
                SetPiece(position, PieceColor.Black, PieceType.Queen);
                break;
            case 'K':
                SetPiece(position, PieceColor.White, PieceType.King);
                break;
            case 'k':
                SetPiece(position, PieceColor.Black, PieceType.King);
                break;
            default:
                throw new ArgumentException("Invalid piece character in FEN string.");
        }
    }

    internal bool IsInCheck(Player player)
    {
        return false;
        //PieceColor color = player == Player.w ? PieceColor.White : PieceColor.Black;
        //int kingPosition = color == PieceColor.White ? GetWhiteKingPosition() : GetBlackKingPosition();
        //return IsKingUnderAttack(kingPosition, color);
    }

    internal bool IsInCheckMate(Player player)
    {
        return false;
    }

    private bool IsKingUnderAttack(ulong kingPosition, PieceColor color)
    {
        // Determine the enemy color
        PieceColor enemyColor = (color == PieceColor.White) ? PieceColor.Black : PieceColor.White;

        // Get the enemy pieces' bitboards based on the color
        ulong enemyPawns = (enemyColor == PieceColor.White) ? WhitePawns : BlackPawns;
        ulong enemyKnights = (enemyColor == PieceColor.White) ? WhiteKnights : BlackKnights;
        ulong enemyBishops = (enemyColor == PieceColor.White) ? WhiteBishops : BlackBishops;
        ulong enemyRooks = (enemyColor == PieceColor.White) ? WhiteRooks : BlackRooks;
        ulong enemyQueens = (enemyColor == PieceColor.White) ? WhiteQueens : BlackQueens;
        ulong enemyKings = (enemyColor == PieceColor.White) ? WhiteKings : BlackKings;

        // Check for pawn attacks (pawns attack diagonally)
        if ((enemyPawns & GetPawnAttackMask(kingPosition, enemyColor)) != 0)
        {
            return true;
        }

        // Check for knight attacks
        if ((enemyKnights & GetKnightAttackMask(kingPosition)) != 0)
        {
            return true;
        }

        // Check for bishop attacks (diagonal attack)
        if ((enemyBishops & GetBishopAttackMask(kingPosition)) != 0)
        {
            return true;
        }

        // Check for rook attacks (horizontal and vertical attack)
        if ((enemyRooks & GetRookAttackMask(kingPosition)) != 0)
        {
            return true;
        }

        // Check for queen attacks (rook + bishop combined attack)
        if ((enemyQueens & GetQueenAttackMask(kingPosition)) != 0)
        {
            return true;
        }

        // Check for king attacks (king moves 1 square in any direction)
        if ((enemyKings & GetKingAttackMask(kingPosition)) != 0)
        {
            return true;
        }

        // If no enemy pieces can attack the king, return false
        return false;
    }

    private ulong GetPawnAttackMask(ulong kingPosition, PieceColor enemyColor)
    {
        ulong attackMask;
        if (enemyColor == PieceColor.White)
        {
            // White pawns attack diagonally up-left and up-right
            attackMask = (kingPosition << 7) & ~FileH; // up-left (avoid crossing the a-file)
            attackMask |= (kingPosition << 9) & ~FileA; // up-right (avoid crossing the h-file)
        }
        else
        {
            // Black pawns attack diagonally down-left and down-right
            attackMask = (kingPosition >> 7) & ~FileA; // down-left (avoid crossing the a-file)
            attackMask |= (kingPosition >> 9) & ~FileH; // down-right (avoid crossing the h-file)
        }

        return attackMask;
    }
    private ulong GetKnightAttackMask(ulong kingPosition)
    {
        ulong attackMask = 0;

        // Knight's 8 possible L-shaped moves (2 squares in one direction, 1 square in the perpendicular direction)
        attackMask |= (kingPosition << 15) & ~FileA; // 2 squares up and 1 square left
        attackMask |= (kingPosition << 17) & ~FileH; // 2 squares up and 1 square right
        attackMask |= (kingPosition << 6) & ~FileA & ~FileB; // 1 square up and 2 squares left
        attackMask |= (kingPosition << 10) & ~FileG & ~FileH; // 1 square up and 2 squares right
        attackMask |= (kingPosition >> 15) & ~FileH; // 2 squares down and 1 square left
        attackMask |= (kingPosition >> 17) & ~FileA; // 2 squares down and 1 square right
        attackMask |= (kingPosition >> 6) & ~FileG & ~FileH; // 1 square down and 2 squares left
        attackMask |= (kingPosition >> 10) & ~FileA & ~FileB; // 1 square down and 2 squares right

        return attackMask;
    }

    private ulong GetBishopAttackMask(ulong kingPosition)
    {
        ulong attackMask = 0;

        // Up-right diagonal
        ulong upRight = kingPosition;
        while ((upRight & FileH) == 0) // Make sure we don't go off the right side
        {
            upRight <<= 9; // Move diagonally up-right
            attackMask |= upRight;
        }

        // Up-left diagonal
        ulong upLeft = kingPosition;
        while ((upLeft & FileA) == 0) // Make sure we don't go off the left side
        {
            upLeft <<= 7; // Move diagonally up-left
            attackMask |= upLeft;
        }

        // Down-right diagonal
        ulong downRight = kingPosition;
        while ((downRight & FileH) == 0) // Make sure we don't go off the right side
        {
            downRight >>= 7; // Move diagonally down-right
            attackMask |= downRight;
        }

        // Down-left diagonal
        ulong downLeft = kingPosition;
        while ((downLeft & FileA) == 0) // Make sure we don't go off the left side
        {
            downLeft >>= 9; // Move diagonally down-left
            attackMask |= downLeft;
        }

        return attackMask;
    }

    private ulong GetRookAttackMask(ulong kingPosition)
    {
        ulong attackMask = 0;

        // Upward (Vertical)
        ulong up = kingPosition;
        while ((up & FileH) == 0)
        {
            up <<= 8; // Move vertically up
            attackMask |= up;
        }

        // Downward (Vertical)
        ulong down = kingPosition;
        while ((down & FileA) == 0)
        {
            down >>= 8; // Move vertically down
            attackMask |= down;
        }

        // Leftward (Horizontal)
        ulong left = kingPosition;
        while ((left & FileA) == 0)
        {
            left <<= 1; // Move horizontally left
            attackMask |= left;
        }

        // Rightward (Horizontal)
        ulong right = kingPosition;
        while ((right & FileH) == 0)
        {
            right >>= 1; // Move horizontally right
            attackMask |= right;
        }

        return attackMask;
    }
    private ulong GetQueenAttackMask(ulong kingPosition)
    {
        return GetRookAttackMask(kingPosition) | GetBishopAttackMask(kingPosition);
    }

    private ulong GetKingAttackMask(ulong kingPosition)
    {
        ulong attackMask = 0;

        // 8 possible surrounding squares (1 square in each direction)
        attackMask |= (kingPosition << 8);  // Up
        attackMask |= (kingPosition >> 8);  // Down
        attackMask |= (kingPosition << 1);  // Left
        attackMask |= (kingPosition >> 1);  // Right
        attackMask |= (kingPosition << 9);  // Up-left
        attackMask |= (kingPosition << 7);  // Up-right
        attackMask |= (kingPosition >> 9);  // Down-left
        attackMask |= (kingPosition >> 7);  // Down-right

        return attackMask;
    }

    private bool IsRookOrQueen(int position, PieceColor color)
    {
        ulong pieceBitboard = (color == PieceColor.White) ? (WhiteRooks | WhiteQueens) : (BlackRooks | BlackQueens);
        return (pieceBitboard & (1UL << position)) != 0;
    }

    private bool IsBishopOrQueen(int position, PieceColor color)
    {
        ulong pieceBitboard = (color == PieceColor.White) ? (WhiteBishops | WhiteQueens) : (BlackBishops | BlackQueens);
        return (pieceBitboard & (1UL << position)) != 0;
    }

    //private int GetWhiteKingPosition()
    //{
    //    return GetKingPosition(WhiteKings);
    //}

    //private int GetBlackKingPosition()
    //{
    //    return GetKingPosition(BlackKings);
    //}

    //private int GetKingPosition(ulong kingBitboard)
    //{
    //    for (int i = 0; i < 64; i++)
    //    {
    //        if ((kingBitboard & (1UL << i)) != 0)
    //            return i; // Return the first found position of the king
    //    }

    //    throw new InvalidOperationException("King not found on the board.");
    //}

    public IEnumerable<Move> GetValidMoves(int positionIndex)
    {
        List<Move> validMoves = new List<Move>();

        // Check if there is a piece at the given position (if it's a 1 on any bitboard)
        if (((WhitePawns | BlackPawns | WhiteKnights | BlackKnights | WhiteBishops | BlackBishops | WhiteRooks | BlackRooks | WhiteQueens | BlackQueens | WhiteKings | BlackKings) & (1UL << positionIndex)) == 0)
            yield break; // No piece at this position

        // Get the piece at the position (check which bitboard the position belongs to)
        if ((WhitePawns & (1UL << positionIndex)) != 0)
        {
            // White Pawn: Can move forward one square or capture diagonally
            validMoves.AddRange(GetPawnMoves(positionIndex, PieceColor.White));
        }
        else if ((BlackPawns & (1UL << positionIndex)) != 0)
        {
            // Black Pawn: Can move forward one square or capture diagonally
            validMoves.AddRange(GetPawnMoves(positionIndex, PieceColor.Black));
        }
        else if ((WhiteKnights & (1UL << positionIndex)) != 0)
        {
            // White Knight: L-shaped moves
            validMoves.AddRange(GetKnightMoves(positionIndex, PieceColor.White));
        }
        else if ((BlackKnights & (1UL << positionIndex)) != 0)
        {
            // Black Knight: L-shaped moves
            validMoves.AddRange(GetKnightMoves(positionIndex, PieceColor.Black));
        }
        else if ((WhiteBishops & (1UL << positionIndex)) != 0)
        {
            // White Bishop: Diagonal moves
            validMoves.AddRange(GetBishopMoves(positionIndex, PieceColor.White));
        }
        else if ((BlackBishops & (1UL << positionIndex)) != 0)
        {
            // Black Bishop: Diagonal moves
            validMoves.AddRange(GetBishopMoves(positionIndex, PieceColor.Black));
        }
        else if ((WhiteRooks & (1UL << positionIndex)) != 0)
        {
            // White Rook: Horizontal and vertical moves
            validMoves.AddRange(GetRookMoves(positionIndex, PieceColor.White));
        }
        else if ((BlackRooks & (1UL << positionIndex)) != 0)
        {
            // Black Rook: Horizontal and vertical moves
            validMoves.AddRange(GetRookMoves(positionIndex, PieceColor.Black));
        }
        else if ((WhiteQueens & (1UL << positionIndex)) != 0)
        {
            // White Queen: Horizontal, vertical, and diagonal moves
            validMoves.AddRange(GetQueenMoves(positionIndex, PieceColor.White));
        }
        else if ((BlackQueens & (1UL << positionIndex)) != 0)
        {
            // Black Queen: Horizontal, vertical, and diagonal moves
            validMoves.AddRange(GetQueenMoves(positionIndex, PieceColor.Black));
        }
        else if ((WhiteKings & (1UL << positionIndex)) != 0)
        {
            // White King: One square in any direction
            validMoves.AddRange(GetKingMoves(positionIndex, PieceColor.White));
        }
        else if ((BlackKings & (1UL << positionIndex)) != 0)
        {
            // Black King: One square in any direction
            validMoves.AddRange(GetKingMoves(positionIndex, PieceColor.Black));
        }

        foreach (var move in validMoves)
        {
            yield return move;
        }
    }

    // Get possible pawn moves (can move one square forward or capture diagonally)
    private IEnumerable<Move> GetPawnMoves(int positionIndex, PieceColor color)
    {
        List<Move> moves = new List<Move>();
        int direction = (color == PieceColor.White) ? 8 : -8; // White moves up, Black moves down
        int startRank = (color == PieceColor.White) ? 8 : 48; // Starting index for pawns (rank 1 or rank 6)

        int forwardOne = positionIndex + direction;
        int forwardTwo = positionIndex + (2 * direction);
        int leftCapture = positionIndex + direction - 1;
        int rightCapture = positionIndex + direction + 1;

        // Move one square forward
        if (IsOnBoard(forwardOne) && IsSquareEmpty(forwardOne))
            moves.Add(new Move(positionIndex, forwardOne));

        // First move can be two squares forward
        if (positionIndex >= startRank && positionIndex < startRank + 8 &&
            IsOnBoard(forwardTwo) && IsSquareEmpty(forwardOne) && IsSquareEmpty(forwardTwo))
            moves.Add(new Move(positionIndex, forwardTwo));

        // Capture diagonally (ensure it doesn't wrap around the board)
        if (positionIndex % 8 > 0 && IsOnBoard(leftCapture) && IsEnemyPieceAt(leftCapture, color))
            moves.Add(new Move(positionIndex, leftCapture));
        if (positionIndex % 8 < 7 && IsOnBoard(rightCapture) && IsEnemyPieceAt(rightCapture, color))
            moves.Add(new Move(positionIndex, rightCapture));

        return moves;
    }

    // Get possible knight moves (L-shaped)
    private IEnumerable<Move> GetKnightMoves(int positionIndex, PieceColor color)
    {
        List<Move> moves = new List<Move>();
        int[] knightOffsets = { -17, -15, -10, -6, 6, 10, 15, 17 };

        int x = positionIndex % 8;

        foreach (int offset in knightOffsets)
        {
            int newIndex = positionIndex + offset;
            int newX = newIndex % 8;

            // Ensure move stays within the same relative file range to prevent board wrapping
            if (IsOnBoard(newIndex) && Math.Abs(newX - x) <= 2 && !IsAlliedPieceAt(newIndex, color))
                moves.Add(new Move(positionIndex, newIndex));
        }

        return moves;
    }

    // Get possible bishop moves (diagonal)
    private IEnumerable<Move> GetBishopMoves(int positionIndex, PieceColor color)
    {
        List<Move> moves = new List<Move>();

        int[][] moveOffsets = new int[][]
        {
        new int[] { positionIndex - 9, positionIndex - 18, positionIndex - 27, positionIndex - 36, positionIndex - 45, positionIndex - 54, positionIndex - 63 }, // Up-Left
        new int[] { positionIndex - 7, positionIndex - 14, positionIndex - 21, positionIndex - 28, positionIndex - 35, positionIndex - 42, positionIndex - 49 }, // Up-Right
        new int[] { positionIndex + 9, positionIndex + 18, positionIndex + 27, positionIndex + 36, positionIndex + 45, positionIndex + 54, positionIndex + 63 }, // Down-Right
        new int[] { positionIndex + 7, positionIndex + 14, positionIndex + 21, positionIndex + 28, positionIndex + 35, positionIndex + 42, positionIndex + 49 }  // Down-Left
        };

        foreach (var direction in moveOffsets)
        {
            foreach (int nextIndex in direction)
            {
                if (!IsOnBoard(nextIndex) || (Math.Abs((nextIndex % 8) - (positionIndex % 8)) > 1))
                    break; // Stop if out of bounds or wraps around the board

                if (IsAlliedPieceAt(nextIndex, color))
                    break; // Stop if it's an allied piece

                moves.Add(new Move(positionIndex, nextIndex));
                if (IsEnemyPieceAt(nextIndex, color))
                    break; // Stop if it's an enemy piece (capture)
            }
        }

        return moves;
    }

    // Get possible rook moves (vertical and horizontal)
    private IEnumerable<Move> GetRookMoves(int positionIndex, PieceColor color)
    {
        List<Move> moves = new List<Move>();
        int[][] moveOffsets = new int[][]
        {
        new int[] { positionIndex - 8, positionIndex - 16, positionIndex - 24, positionIndex - 32, positionIndex - 40, positionIndex - 48, positionIndex - 56 }, // Up
        new int[] { positionIndex + 8, positionIndex + 16, positionIndex + 24, positionIndex + 32, positionIndex + 40, positionIndex + 48, positionIndex + 56 }, // Down
        new int[] { positionIndex + 1, positionIndex + 2, positionIndex + 3, positionIndex + 4, positionIndex + 5, positionIndex + 6, positionIndex + 7 }, // Right
        new int[] { positionIndex - 1, positionIndex - 2, positionIndex - 3, positionIndex - 4, positionIndex - 5, positionIndex - 6, positionIndex - 7 }  // Left
        };

        foreach (var direction in moveOffsets)
        {
            foreach (int nextIndex in direction)
            {
                if (!IsOnBoard(nextIndex))
                    break; // Stop if out of bounds

                // Ensure rook doesn't wrap around the board edges
                if (Math.Abs((nextIndex % 8) - (positionIndex % 8)) > 0 &&
                    (positionIndex % 8 == 0 || positionIndex % 8 == 7))
                    break;

                if (IsAlliedPieceAt(nextIndex, color))
                    break; // Stop if it's an allied piece

                moves.Add(new Move(positionIndex, nextIndex));
                if (IsEnemyPieceAt(nextIndex, color))
                    break; // Stop if it's an enemy piece (capture)
            }
        }

        return moves;
    }

    // Get possible queen moves (combines bishop and rook moves)
    private IEnumerable<Move> GetQueenMoves(int positionIndex, PieceColor color)
    {
        List<Move> moves = new List<Move>();
        moves.AddRange(GetBishopMoves(positionIndex, color));
        moves.AddRange(GetRookMoves(positionIndex, color));
        return moves;
    }

    // Get possible king moves (one square in any direction)
    private IEnumerable<Move> GetKingMoves(int positionIndex, PieceColor color)
    {
        List<Move> moves = new List<Move>();
        int[] kingOffsets = { -9, -8, -7, -1, 1, 7, 8, 9 };

        int x = positionIndex % 8;

        foreach (int offset in kingOffsets)
        {
            int newIndex = positionIndex + offset;
            int newX = newIndex % 8;

            // Ensure move doesn't wrap around board edges
            if (IsOnBoard(newIndex) && Math.Abs(newX - x) <= 1 && !IsAlliedPieceAt(newIndex, color))
                moves.Add(new Move(positionIndex, newIndex));
        }

        return moves;
    }

    // Helper methods
    private bool IsOnBoard(int positionIndex) => positionIndex >= 0 && positionIndex < 64;

    private bool IsSquareEmpty(int positionIndex) =>
        ((WhitePawns | BlackPawns | WhiteKnights | BlackKnights | WhiteBishops | BlackBishops |
          WhiteRooks | BlackRooks | WhiteQueens | BlackQueens | WhiteKings | BlackKings) & (1UL << positionIndex)) == 0;

    private bool IsEnemyPieceAt(int positionIndex, PieceColor color) =>
        ((color == PieceColor.White ?
          (BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKings) :
          (WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKings)) & (1UL << positionIndex)) != 0;

    private bool IsAlliedPieceAt(int positionIndex, PieceColor color) =>
        ((color == PieceColor.White ?
          (WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKings) :
          (BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKings)) & (1UL << positionIndex)) != 0;

    // Set a bit for a specific piece and color
    public void SetPiece(int position, PieceColor color, PieceType type)
    {
        ulong mask = 1UL << position;
        switch (color)
        {
            case PieceColor.White:
                SetWhitePiece(mask, type);
                break;
            case PieceColor.Black:
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
    public bool IsPieceSet(int position, PieceColor color, PieceType type)
    {
        ulong mask = 1UL << position;
        switch (color)
        {
            case PieceColor.White:
                return IsWhitePieceSet(mask, type);
            case PieceColor.Black:
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
    public ulong GetPieceBitboard(PieceColor color, PieceType type)
    {
        switch (color)
        {
            case PieceColor.White:
                return GetWhitePieceBitboard(type);
            case PieceColor.Black:
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

    internal void MakeMove(Move move)
    {
        var startPosition = (int)move.From;
        var endPosition = (int)move.To;

        // Get the piece at the start position
        PieceColor pieceColor = default; // Track the color of the piece
        PieceType pieceType = default; // Track the color of the piece
        if ((WhitePawns & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.White;
            pieceType = PieceType.Pawn;
            WhitePawns &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackPawns & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.Black;
            pieceType = PieceType.Pawn;
            BlackPawns &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteKnights & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.White;
            pieceType = PieceType.Knight;
            WhiteKnights &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackKnights & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.Black;
            pieceType = PieceType.Knight;
            BlackKnights &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteBishops & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.White;
            pieceType = PieceType.Bishop;
            WhiteBishops &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackBishops & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.Black;
            pieceType = PieceType.Bishop;
            BlackBishops &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteRooks & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.White;
            pieceType = PieceType.Rook;
            WhiteRooks &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackRooks & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.Black;
            pieceType = PieceType.Rook;
            BlackRooks &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteQueens & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.White;
            pieceType = PieceType.Queen;
            WhiteQueens &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackQueens & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.Black;
            pieceType = PieceType.Queen;
            BlackQueens &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((WhiteKings & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.White;
            pieceType = PieceType.King;
            WhiteKings &= ~(1UL << startPosition); // Remove from start position
        }
        else if ((BlackKings & (1UL << startPosition)) != 0)
        {
            pieceColor = PieceColor.Black;
            pieceType = PieceType.King;
            BlackKings &= ~(1UL << startPosition); // Remove from start position
        }

        // Capture logic (remove captured piece if any)
        if (IsEnemyPieceAt(endPosition, pieceColor))
        {
            // Remove the captured piece from the appropriate enemy bitboard
            if (pieceColor == PieceColor.White)
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