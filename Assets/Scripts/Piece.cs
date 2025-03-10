using UnityEngine;

public class Piece : MonoBehaviour
{
    public ChessColor PieceColor;
    public SpriteRenderer CurrentSprite;
    public Sprite WhiteSprite;
    public Sprite BlackSprite;

    public PieceType PieceMovement;

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
            board.PieceDropped(this, cell);
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