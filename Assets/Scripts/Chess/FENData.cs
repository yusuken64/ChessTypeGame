using System.Collections.Generic;

public class FENData
{
    public List<FenRecord> Pieces { get; set; }
    public ChessColor ActiveColor{ get; set; }
    public string CastlingRights { get; set; }
    public string EnPassant { get; set; }
    public string HalfMoveClock { get; set; }
    public string FullMoveNumber { get; set; }
}
