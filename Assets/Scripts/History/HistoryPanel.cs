using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryPanel : MonoBehaviour
{
    public Transform HistoryContainer;
    public HistoryItem HistoryItemPrefab;

    public GameObject Content;
    public ScrollRect ScrollRect;

    private HistoryItem _lastItem;

    public void ClearHistory()
    {
        _lastItem = null;

        foreach (Transform child in HistoryContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddEntry(HistoryEntry historyEntry)
    {
        var newItem = Instantiate(HistoryItemPrefab, HistoryContainer);
        newItem.Setup(historyEntry);
        newItem.ClickAction = Item_Clicked;

        _lastItem = newItem;


        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());
        ScrollRect.verticalNormalizedPosition = 1f;
    }

    public void Item_Clicked(HistoryEntry historyEntry)
    {
        Game game = FindObjectOfType<Game>();
        if (game.CurrentGameState == GameState.Paused)
        {
            game.RestoryByHistory(historyEntry);
        }
    }

    internal void SetLastLevelResults(LevelResult levelResult)
    {
        _lastItem?.SetLastLevelResults(levelResult);
    }
}

public class LevelResult
{
    public bool Cleared { get; internal set; }
    public bool Perfect { get; internal set; }
    public int Retries { get; internal set; }
    public float ClearTime { get; internal set; }
}

public class HistoryEntry
{
    public IEnumerable<PieceRecord> LevelRecord { get; internal set; }
    public Solution Solution { get; internal set; }
    public int LevelNum { get; internal set; }
    public LevelResult LevelResult { get; internal set; }
}
