using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HistoryItem : MonoBehaviour
{
    public TextMeshProUGUI Label;
    public Image BgImage;

    public Color UnclearedColor;
    public Color ClearedColor;
    public Color PerfectColor;

    private HistoryEntry _historyEntry;

    public Action<HistoryEntry> ClickAction { get; internal set; }

    internal void Setup(HistoryEntry historyEntry)
    {
        this._historyEntry = historyEntry;

        UpdateUI();
    }

    private void UpdateUI()
    {
        string status = "";
        if (_historyEntry.LevelResult == null)
        {
            BgImage.color = UnclearedColor;
        }
        else
        {
            if (_historyEntry.LevelResult.Perfect)
            {
                BgImage.color = PerfectColor;
            }
            else
            {
                BgImage.color = ClearedColor;
            }
            status = $"{_historyEntry.LevelResult.ClearTime:F2}s";
                //Re:{_historyEntry.LevelResult.Retries}";
        }
        Label.text = $"L{_historyEntry.LevelNum - 1} {status}";
    }

    public void Button_Clicked()
    {
        ClickAction(this._historyEntry);
    }

    internal void SetLastLevelResults(LevelResult levelResult)
    {
        _historyEntry.LevelResult = levelResult;
        UpdateUI();
    }
}
