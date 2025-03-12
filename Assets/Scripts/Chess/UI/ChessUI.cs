using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChessUI : MonoBehaviour
{
    public TextMeshProUGUI InfoText;
    public TextMeshProUGUI MoveText;
    public TextMeshProUGUI TurnText;

    public string CurrentMessage;
    public ChessGame ChessGame;

    public List<string> MoveList;

    public TMP_InputField StateInput;
    public Toggle AutoWhiteToggle;
    public Toggle AutoBlackToggle;

    private void Start()
    {
        CurrentMessage = string.Empty;
        MoveList.Clear();
        ChessGame.AutoWhiteTurn = AutoWhiteToggle.isOn;
        ChessGame.AutoBlackTurn = AutoBlackToggle.isOn;

        ChessGame.ResetGame();
        UpdateUI();
    }

    public void Reset_Clicked() 
    {
        ChessGame.ResetGame();
        MoveList.Clear();
        CurrentMessage = "Game Reset";
        UpdateUI();
        GetState_Clicked();
    }

    public void Auto_Clicked()
    {
        ChessGame.DoWhiteTurn();
    }

    public void UpdateUI()
    {
        InfoText.text = CurrentMessage;

        var moveString = string.Join(Environment.NewLine, MoveList.TakeLast(6));
        MoveText.text = moveString;

        var activePlayer = ChessGame.GetActivePlayer();
        if (activePlayer == ChessColor.w)
        {
            TurnText.text = "White Turn";
        }
        else
        {
            TurnText.text = "Black Turn";
        }
    }

    public void GetState_Clicked()
    {
        var board = ChessGame.Board;
        PieceRecord?[,] boardData = Solver.ToBoardData(board);
        var fen = FENParser.BoardToFEN(boardData, board.Cells.GetLength(0), board.Cells.GetLength(1), ChessGame.ActivePlayer.ToString());

        string result = fen.Replace(@"/", @"/" + "\n");
        StateInput.text = result;
    }

    public void SetState_Clicked()
    {
        var stateText = StateInput.text.Trim();
        string singleLine = stateText.Replace("\n", "").Replace("\r", "");
        FENData fenData = FENParser.ParseFEN(singleLine, ChessGame.Board.Width, ChessGame.Board.Height);
        var boardRecord = fenData.Pieces.Select(x => new PieceRecord()
        {
            IsWhite = x.Player == ChessColor.w,
            PieceType = ChessGame.ToPieceType(x.Piece),
            X = x.X,
            Y = x.Y
        });

        ChessGame.Board.SetState(boardRecord);
        ChessGame.ActivePlayer = fenData.ActiveColor;

        MoveList.Clear();
        CurrentMessage = "Game Restored";
        UpdateUI();
    }

    public void OnToggleChanged_White(bool isOn)
    {
        ChessGame.AutoWhiteTurn = isOn;
    }

    public void OnToggleChanged_Black(bool isOn)
    {
        ChessGame.AutoBlackTurn = isOn;
    }

    public void GoBack_Clicked()
    {
        SceneManager.LoadScene("MainGame");
    }
}
