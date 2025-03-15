public struct FenRecord
{
    public int X { get; internal set; }
    public int Y { get; internal set; }
    public PieceType Piece { get; internal set; }
    public ChessColor Player { get; internal set; }
}