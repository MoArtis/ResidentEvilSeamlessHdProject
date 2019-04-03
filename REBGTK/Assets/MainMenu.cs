﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BgTk;

public class MainMenu : MonoBehaviour
{
    [SerializeField] protected Dropdown matchTexturesDropdown;
    [SerializeField] protected Dropdown recreateTexturesDropdown;
    [SerializeField] protected CanvasGroup canvasGroup;

    public void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
    }


    public void UpdateMatchTexturesFormatOptions(DumpFormat[] dumpFormats, int selectedIndex)
    {
        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        for (int i = 0; i < dumpFormats.Length; i++)
        {
            optionDatas.Add(new Dropdown.OptionData(dumpFormats[i].name));
        }

        matchTexturesDropdown.options = optionDatas;

        matchTexturesDropdown.value = selectedIndex;
        matchTexturesDropdown.RefreshShownValue();
    }

    public void UpdateRecreateTexturesFormatOptions(DumpFormat[] dumpFormats, int selectedIndex)
    {
        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        for (int i = 0; i < dumpFormats.Length; i++)
        {
            optionDatas.Add(new Dropdown.OptionData(dumpFormats[i].name));
        }

        recreateTexturesDropdown.options = optionDatas;

        recreateTexturesDropdown.value = selectedIndex;
        recreateTexturesDropdown.RefreshShownValue();
    }
}
