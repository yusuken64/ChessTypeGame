using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public ChessColor PieceColor;
    public SpriteRenderer CurrentSprite;
    public Sprite WhiteSprite;
    public Sprite BlackSprite;

    public PieceType PieceType;

    private void Start()
    {
        var draggable = GetComponent<Draggable>();
        draggable.OnHold = () =>
        {
            if (this.PieceColor == ChessColor.w)
            {
                var board = FindObjectOfType<Board>();
                board.PiecePickedUp(this);
            }
            else
            {
                //can't pick up
            }
        };
        draggable.OnReleased = (cell) =>
        {
            var board = FindObjectOfType<Board>();
            if (board.CanDrop(this, cell))
            {
                board.PieceDropped(this, cell);
            }
            else
            {
                //Debug.Log("Can't place there!");

                //reset dropped piece
                var cells = board.Cells.Cast<Cell>().ToList();
                var thisCell = cells.First(x => x.CurrentPiece == this);
                thisCell.ResetPiece();
            }

            board.Cells.Cast<Cell>().ToList().ForEach(x => x.ClearDroppable());
        };
    }

    internal void SetColor(ChessColor pieceColor)
    {
        this.PieceColor = pieceColor;
        if (pieceColor == ChessColor.w)
        {
            CurrentSprite.sprite = WhiteSprite;
            var draggable = GetComponent<Draggable>();
            draggable.IsDraggable = true;
        }
        else
        {
            CurrentSprite.sprite = BlackSprite;
            var draggable = GetComponent<Draggable>();
            draggable.IsDraggable = false;
        }
    }
}