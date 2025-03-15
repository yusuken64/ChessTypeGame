public enum ChessColor
{
    w = 'w',
    b = 'b'
}
public static class ChessColorExtensions
{
    public static ChessColor Opponent(this ChessColor color)
    {
        return color == ChessColor.w ? ChessColor.b : ChessColor.w;
    }
}