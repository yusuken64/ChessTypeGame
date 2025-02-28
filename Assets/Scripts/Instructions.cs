using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instructions : MonoBehaviour
{
    public List<GameObject> Pages;
    public int CurrentIndex;

    public void OpenInstructions()
    {
        this.gameObject.SetActive(true);
        CurrentIndex = 0;
        UpdateUI();
    }

    public void NextPage_Clicked()
    {
        CurrentIndex++;
        CurrentIndex = Mathf.Clamp(CurrentIndex, 0, Pages.Count);

        UpdateUI();

        if (CurrentIndex == Pages.Count)
        {
            Close_Clicked();
        }
    }

    public void PreviousPage_Clicked()
    {
        CurrentIndex--;
        CurrentIndex = Mathf.Clamp(CurrentIndex, 0, Pages.Count);

        UpdateUI();
    }

    public void Close_Clicked()
    {
        this.gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        for (int i = 0; i < Pages.Count; i++)
        {
            GameObject page = Pages[i];
            page.gameObject.SetActive(i == CurrentIndex);
        }
    }
}
