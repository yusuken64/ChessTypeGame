using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int X;
    public int Y;
    public bool IsWall;

    public SpriteRenderer CellSprite;
    public SpriteRenderer Selection;

    public Color BlackCellColor;
    public Color WhiteCellColor;
    public Color WallColor;

    public Piece CurrentPiece;

    [ContextMenu("SetToBlack")]
    internal void SetToBlack()
    {
        CellSprite.color = BlackCellColor;
    }

    [ContextMenu("SetToWhite")]
    internal void SetToWhite()
    {
        CellSprite.color = WhiteCellColor;
    }

    [ContextMenu("SetToWall")]
    internal void SetToWall()
    {
        CellSprite.color = WallColor;
        IsWall = true;
    }

    [ContextMenu("SetPiece_WhiteKing")]
    internal void SetPiece_WhiteKing()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.KingPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.White);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_WhiteQueen")]
    internal void SetPiece_WhiteQueen()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.QueenPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.White);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_WhiteBishop")]
    internal void SetPiece_WhiteBishop()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.BishopPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.White);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_WhiteRook")]
    internal void SetPiece_WhiteRook()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.RookPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.White);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_WhiteKnight")]
    internal void SetPiece_WhiteKnight()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.KnightPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.White);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_WhitePawn")]
    internal void SetPiece_WhitePawn()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.PawnPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.White);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_BlackKing")]
    internal void SetPiece_BlackKing()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.KingPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.Black);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_BlackQueen")]
    internal void SetPiece_BlackQueen()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.QueenPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.Black);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_BlackBishop")]
    internal void SetPiece_BlackBishop()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.BishopPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.Black);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_BlackRook")]
    internal void SetPiece_BlackRook()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.RookPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.Black);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_BlackKnight")]
    internal void SetPiece_BlackKnight()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.KnightPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.Black);
        SetPiece(prefab);
    }

    [ContextMenu("SetPiece_BlackPawn")]
    internal void SetPiece_BlackPawn()
    {
        var board = FindObjectOfType<Board>();
        var prefab = Instantiate(board.PawnPrefab, board.PiecesContainer);
        prefab.SetColor(PieceColor.Black);
        SetPiece(prefab);
    }

    internal void Capture(Piece piece)
    {
        CurrentPiece.SetColor(piece.PieceColor);
    }

    internal void SetPiece(Piece piece)
    {
        CurrentPiece = piece;
        if (CurrentPiece != null)
        {
            SnapToCell();
        }
    }

    private void SnapToCell()
    {
        CurrentPiece.transform.position = this.transform.position + new Vector3(0, 0, -0.1f);
    }

    internal void ResetPiece()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.Error);
        SnapToCell();
    }

    internal void SetToDroppable()
    {
        Selection.gameObject.SetActive(true);
    }

    internal void ClearDroppable()
    {
        Selection.gameObject.SetActive(false);
    }
}
