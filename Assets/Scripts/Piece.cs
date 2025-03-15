using DG.Tweening;
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
            var board = FindObjectOfType<Board>();
            if (board.Echo)
            {
                if (this.PieceColor == ChessColor.w)
                {
                    board.PiecePickedUp(this);
                }
                else
                {
                    //can't pick up
                }
            }
            else
            {
                if (FindObjectOfType<ChessGame>()?.ActivePlayer == this.PieceColor)
                {
                    board.PiecePickedUp(this);
                }
            }
        };
        draggable.OnReleased = (cell) =>
        {
            var board = FindObjectOfType<Board>();
            if (board.CanDrop(this, cell, out string reason))
            {
                board.PieceDropped(this, cell);
            }
            else
            {
                board.PieceDroppedCanceled(this, cell, reason);
                //Debug.Log("Can't place there!");

                //reset dropped piece
                var cells = board.Cells.Cast<Cell>().ToList();
                var thisCell = cells.First(x => x.CurrentPiece == this);
                thisCell.ResetPiece();
            }

            board.Cells.Cast<Cell>().ToList().ForEach(x => x.ClearDroppable());
        };
    }

    private void OnDestroy()
    {
        this.transform.DOKill(false);
    }

    internal void SetColor(ChessColor pieceColor)
    {
        this.PieceColor = pieceColor;
        if (pieceColor == ChessColor.w)
        {
            CurrentSprite.sprite = WhiteSprite;
        }
        else
        {
            CurrentSprite.sprite = BlackSprite;
        }
    }

    public void SetIsDraggable(bool isDraggable)
    {
        var draggable = GetComponent<Draggable>();
        //draggable.IsDraggable = false;
        draggable.IsDraggable = isDraggable;

    }
}