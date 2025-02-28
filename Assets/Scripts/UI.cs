using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Game Game;

    public GameObject StartPrompt;
    public TextMeshProUGUI HighScoreText;
    public GameObject ResetPrompt;
    public Instructions Instructions;

    //Game Elements
    public GameObject GameElementsObject;
    public Slider TimeSlider;
    public TextMeshProUGUI TimeLeftText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI ComboText;
    public Button ResignButton;

    public Button AutoSolveButton;
    public HistoryPanel HistoryPanel;

    //Background;
    public SpriteRenderer BackgroundSpriteRenderer;
    public Color NormalColor;
    public Color DangerColor;
    public Color GameOverColor;

    private void Start()
    {
        SetToMainPrompt();
    }

    private void SetToMainPrompt()
    {
        StartPrompt.gameObject.SetActive(true);
        ResetPrompt.gameObject.SetActive(false);
        GameElementsObject.gameObject.SetActive(false);
        Instructions.gameObject.SetActive(false);
        BackgroundSpriteRenderer.color = NormalColor;

        HighScoreText.text = $"Hi Score:{Game.HighScore}";
        AudioManager.Instance.PlayMusic(AudioManager.Instance.MainMenuMusic, false);
    }

    public void Start_Clicked()
    {
        StartPrompt.gameObject.SetActive(false);
        ResetPrompt.gameObject.SetActive(false);
        GameElementsObject.gameObject.SetActive(true);
        Game.StartGame();
        HistoryPanel.ClearHistory();
        AutoSolveButton.gameObject.SetActive(false);
        ResignButton.gameObject.SetActive(true);
    }

    public void Retry_Clicked()
    {
        Game.ResetCurrentLevel();
    }

    public void ResetGame_Clicked()
    {
        SetToMainPrompt();
    }

    public void AutoSolve_Clicked()
    {
        Game.ResetCurrentLevel();
        FindObjectOfType<Board>().AutoSolve();
    }

    public void Resign_Clicked()
    {
        Game.TimeLeft = 0;
    }

    public void HowToPlay_Clicked()
    {
        Instructions.OpenInstructions();
    }

    private void Update()
    {
        float t = Game.TimeLeft / Game.TimeLimit;
        TimeSlider.value = t;

        float timeLeft = Game.TimeLeft;
        timeLeft = Mathf.Max(0, timeLeft);
        TimeLeftText.text = $"{timeLeft:F2}";
        LevelText.text = $"{Game.Level}";
        LevelText.text = $"{Game.Level}";
        ScoreText.text = $"{Game.Score}";
        ComboText.text = $"{Game.Combo}";

        if (Game.CurrentGameState == GameState.Running)
        {
            if (t > 0.3f)
            {
                BackgroundSpriteRenderer.color = NormalColor;
            }
            else if (t > 0)
            {
                BackgroundSpriteRenderer.color = DangerColor;
            }
            else
            {
                BackgroundSpriteRenderer.color = GameOverColor;
            }
        }
    }

    internal void ShowGameOver()
    {
        StartPrompt.gameObject.SetActive(false);
        ResetPrompt.gameObject.SetActive(true);
        GameElementsObject.gameObject.SetActive(true);

        AutoSolveButton.gameObject.SetActive(true);
        ResignButton.gameObject.SetActive(false);
        BackgroundSpriteRenderer.color = GameOverColor;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.MainMenuMusic, false);
    }
}
