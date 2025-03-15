using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int Level;
    public int Score;
    public int HighScore;
    public int Combo;
    public bool Perfect;
    public int Retries;
    public int ComboMultiplier;
    public float TimeLeft;
    public float BonusTime;
    public float ResetPenalty;
    public float StageStartTimeLeft;

    //Game Rules
    public float TimeLimit;

    public Board Board;
    public GameGenerator GameGenerator;

    public GameState CurrentGameState;
    public IEnumerable<PieceRecord> CurrentLevelRecord;

    private void Awake()
    {
        Board.PieceMoved += Board_PieceMoved;
    }

    private void OnDestroy()
    {
        Board.PieceMoved -= Board_PieceMoved;
    }

    private void Board_PieceMoved(Piece originalPiece, Cell originalCell, Piece capturedPiece, Cell newCell)
    {
        if (capturedPiece == null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.Error);
            Combo = 0;
            Perfect = false;
            return;
        }

        if (CurrentGameState == GameState.Paused)
        {
            UpdateDraggable();
            return;
        }

        AudioManager.Instance.PlaySFX(AudioManager.Instance.Capture);
        Combo++;
        Score += 100 + (ComboMultiplier * Combo);
        HighScore = Mathf.Max(Score, HighScore);

        var blackPieceCount = Board.Cells.Cast<Cell>()
            .Where(x => x.CurrentPiece != null)
            .Select(x => x.CurrentPiece)
            .Count(x => x.PieceColor == ChessColor.b);

        if (blackPieceCount == 0)
        {
            AdvanceStage();
        }
        else
        {
            UpdateDraggable();
        }
    }

    private void Start()
    {
        CurrentGameState = GameState.Paused;
    }

    public void StartGame()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.GameMusic);

        CurrentGameState = GameState.Running;

        Board.Cells.Cast<Cell>()
            .Where(x => x.CurrentPiece != null)
            .Select(x => x.CurrentPiece)
            .Where(x => x.PieceColor == ChessColor.w)
            .ToList().ForEach(x => x.GetComponent<Draggable>().IsDraggable = true);
        TimeLeft = TimeLimit;
        Score = 0;
        Combo = 0;
        Level = 0;

        AdvanceStage();
    }

    public void ResetCurrentLevel()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.Error);
        Perfect = false;
        Combo = 0;
        Retries++;
        Board.SetState(CurrentLevelRecord);
        TimeLeft -= ResetPenalty;
    }

    public void RestoryByHistory(HistoryEntry historyEntry)
    {
        Board.SetState(historyEntry.LevelRecord);
        CurrentLevelRecord = Board.GetCurrentLevelRecord().ToList();
        Board.SetSolution(historyEntry.Solution);
    }

    private void AdvanceStage()
    {
        var historyPanel = FindObjectOfType<HistoryPanel>(true);
        historyPanel.SetLastLevelResults(new LevelResult()
        {
            Cleared = true,
            Perfect = Perfect,
            Retries = Retries,
            ClearTime = StageStartTimeLeft - TimeLeft
        });

        AudioManager.Instance.PlaySFX(AudioManager.Instance.Level);
        Level++;
        TimeLeft += BonusTime;
        TimeLeft = Mathf.Min(TimeLimit, TimeLeft);
        GameGenerator.GenerateLevel(Level);
        CurrentLevelRecord = Board.GetCurrentLevelRecord().ToList();
        Perfect = true;
        Retries = 0;
        StageStartTimeLeft = TimeLeft;

        historyPanel.AddEntry(new HistoryEntry()
        {
            LevelRecord = CurrentLevelRecord,
            Solution = Board.Solution,
            LevelNum = Level
        });

        UpdateDraggable();
    }

    public void UpdateDraggable()
    {
        Board.Cells.OfType<Cell>()
            .ToList()
            .ForEach(x => x.CurrentPiece?.SetIsDraggable(x.CurrentPiece.PieceColor == ChessColor.w));
    }

    private void Update()
    {
        if (CurrentGameState == GameState.Running)
        {
            TimeLeft -= Time.deltaTime;

            if (TimeLeft <= 0)
            {
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        CurrentGameState = GameState.Paused;
        Board.Cells.Cast<Cell>()
            .Where(x => x.CurrentPiece != null)
            .Select(x => x.CurrentPiece)
            .Where(x => x.PieceColor == ChessColor.w)
            .ToList().ForEach(x => x.GetComponent<Draggable>().IsDraggable = false);

        //show prompt;
        FindObjectOfType<UI>().ShowGameOver();
    }
}

public enum GameState
{
    Paused,
    Running
}