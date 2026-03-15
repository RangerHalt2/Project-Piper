// Created By: Ryan Lupoli
// Iterated By: Kyle Woo
// Manages the UI during gameplay and allows for easy navigation between pages
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

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
    [Tooltip("If enabled, each page controls whether pause input is allowed while that page is active.")]
    public bool usePagePauseRules = true;

    [Header("Pause Settings")]
    [Tooltip("Reference to InputActions Asset.")]
    [SerializeField] private InputActionAsset UIControls;
    // Reference to pause action from the InputActionsAsset
    private InputAction pauseAction;
    [Tooltip("The index of the pause page in the pages list.")]
    public int pausePageIndex = 1;
    [Tooltip("Determines whether or not the player is allowed to pause. Set to true to enable pausing.")]
    public bool allowPause = true;
    // Whether or not the game is currently paused
    private bool isPaused = false;
    public bool IsPaused => isPaused;


    #endregion

    void Awake()
    {
        Time.timeScale = 1f;
        // Ensure there is only one instance of the UI Manager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            //Destroy(this);
        }

        // Get the pause action from the UI Action Map in InputActions
        var uiMap = UIControls.FindActionMap("UI", true);
        pauseAction = uiMap.FindAction("Pause", true);

        //EW: Added to fix the kill screen. See Die() in Health for more information
        allowPause = true;
    }

    private void OnEnable()
    {
        pauseAction.performed += OnPausePerformed;
        UIControls.Enable();
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPausePerformed;
        UIControls.Disable();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitilizeFirstPage();
    }

    // Sets up the first page. Ensures that only the default page is enabled on startup
    private void InitilizeFirstPage()
    {
        GoToPage(defaultPage);
    }

    // Toggles whether or not the game is currently paused
    public void TogglePause()
    {
        // If pausing is allowed
        if (allowPause)
        {
            // If the game is currently paused, un-pause it
            if (isPaused)
            {
                // Go to the default UI Page
                GoToPage(defaultPage);
                // Set time scale to 1 (normal speed)
                Time.timeScale = 1f;
                // Lock the cursor
                Cursor.lockState = CursorLockMode.Locked;
                // Update pause boolean
                isPaused = false;
            }
            // If the game is not currently paused, pause it
            else
            {
                // Go to pause UI Page
                GoToPage(pausePageIndex);
                // Set time scale to 0 (frozen)
                Time.timeScale = 0f;
                // Lock the cursor
                Cursor.lockState = CursorLockMode.None;
                // Update pause boolean
                isPaused = true;
            }
        }
    }

    // Go to a page by the page's index
    public void GoToPage(int pageIndex)
    {
        // If the page index is within the bounds of pages, and a page has been assigned at that index
        if (pages != null && pageIndex >= 0 && pageIndex < pages.Count && pages[pageIndex] != null)
        {
            // Disable all pages
            SetActiveAllPages(false);
            // Activate the specified page
            pages[pageIndex].gameObject.SetActive(true);
            currentPage = pageIndex;

            if (usePagePauseRules)
            {
                allowPause = pages[pageIndex].AllowPauseOnThisPage;
            }

            // If we navigated to a non-pausable page while paused, force a clean resume.
            if (isPaused && !allowPause)
            {
                isPaused = false;
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    // Turns all pages on or off according to the activated parameter
    public void SetActiveAllPages(bool activated)
    {
        // If pages has at least one page assinged
        if (pages != null)
        {
            // For every UIPage in the list
            foreach (UIPage page in pages)
            {
                if (page != null)
                {
                    // Activate or deactivate the page according to the state of activated
                    page.gameObject.SetActive(activated);
                }
            }
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePause();
        }
    }
}
