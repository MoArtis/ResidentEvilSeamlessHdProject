using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] protected CanvasGroup progressBarCg;
    [SerializeField] protected Scrollbar progressBar;
    [SerializeField] protected Text progressBarText;

    string labelText;
    string fileCountText;

    public void Init()
    {
        labelText = "";
        fileCountText = "";
        progressBarText.text = "";
        progressBar.size = 0f;
    }

    public void Show()
    {
        progressBarCg.alpha = 1f;
    }

    public void Hide()
    {
        progressBarCg.alpha = 0f;
    }

    public void ChangeValue(float value)
    {
        progressBar.size = Mathf.Clamp01(value);
    }

    public void ChangeText(string text)
    {
        labelText = text;
        UpdateText();
    }

    public void ChangeCount(string fileTypeName, int current, int total)
    {
        if (total <= 0)
        {
            if (fileTypeName != "")
                fileCountText = string.Concat(" - ", fileTypeName);
            else
                fileCountText = "";
        }
        else
        {
            if (fileTypeName != "")
                fileCountText = string.Concat(string.Format("{0,4}", current), "/", string.Format("{0,-4}", total), " ", fileTypeName);
            else
                fileCountText = string.Concat(string.Format("{0,4}", current), "/", string.Format("{0,-4}", total));
        }
        UpdateText();
    }

    void UpdateText()
    {
        progressBarText.text = string.Concat(labelText, " ", fileCountText);
    }
}