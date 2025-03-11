using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ChessUI : MonoBehaviour
{
    public TextMeshProUGUI InfoText;
    public TextMeshProUGUI MoveText;

    public ChessGame ChessGame;

    public List<string> Messages;

    public List<string> MoveList;

    private void Start()
    {
        Messages.Clear();
        MoveList.Clear();
        UpdateUI();
    }

    public void Reset_Clicked() 
    {
        ChessGame.ResetGame();
        Messages.Clear();
        MoveList.Clear();
        Messages.Add("Game Reset");
        UpdateUI();
    }

    public void Auto_Clicked()
    {
        ChessGame.DoWhiteTurn();
    }

    public void UpdateUI()
    {
        var messageString = string.Join(Environment.NewLine, Messages);
        InfoText.text = messageString;

        var moveString = string.Join(Environment.NewLine, MoveList.TakeLast(6));
        MoveText.text = moveString;
    }
}