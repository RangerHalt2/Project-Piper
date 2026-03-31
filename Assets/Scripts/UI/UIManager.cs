// Created By: Ryan Lupoli
// Iterated By: Kyle Woo
// Manages the UI during gameplay and allows for easy navigation between pages
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Variables
    public static UIManager instance;

    [Header("Page Management")]
    [Tooltip("The Pages (or Panels) managed by the UI Manager.")]
    public List<UIPage> pages;

    [Tooltip("The index of the currently active page in the UI.")]
    public int currentPage = 0;

    [Tooltip("The index of the page the UI should start on when the UI Manager starts up.")]
    public int defaultPage = 0;
    #endregion

    private void Awake()
    {
        Time.timeScale = 1f;

        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        InitilizeFirstPage();
    }

    private void InitilizeFirstPage()
    {
        GoToPage(defaultPage);
    }

    public void GoToPage(int pageIndex)
    {
        if (pages != null && pageIndex >= 0 && pageIndex < pages.Count && pages[pageIndex] != null)
        {
            SetActiveAllPages(false);
            pages[pageIndex].gameObject.SetActive(true);
            currentPage = pageIndex;
        }
    }

    public void SetActiveAllPages(bool activated)
    {
        if (pages == null)
        {
            return;
        }

        foreach (UIPage page in pages)
        {
            if (page != null)
            {
                page.gameObject.SetActive(activated);
            }
        }
    }
}
