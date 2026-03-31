using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private GameObject pauseCanvas;

    [Header("Action Maps")]
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string uiActionMapName = "UI";
    [SerializeField] private string pauseActionName = "Pause";
    [SerializeField] private string resumeActionName = "Cancel";

    [Header("Pause Settings")]
    [SerializeField] private int pausePageIndex = 1;
    [SerializeField] private bool usePagePauseRules = true;
    [SerializeField] private bool allowPause = true;
    [SerializeField] private bool lockCursorOnResume = true;
    [SerializeField] private bool hideCursorOnResume = true;

    private InputActionMap playerActionMap;
    private InputActionMap uiActionMap;
    private InputAction pauseAction;
    private InputAction resumeAction;
    private int resumePageIndex;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (uiManager == null)
        {
            uiManager = UIManager.instance;
        }

        playerActionMap = inputActions != null ? inputActions.FindActionMap(playerActionMapName, false) : null;
        uiActionMap = inputActions != null ? inputActions.FindActionMap(uiActionMapName, false) : null;
        pauseAction = playerActionMap?.FindAction(pauseActionName, false);
        resumeAction = uiActionMap?.FindAction(resumeActionName, false);
        resumePageIndex = -1;

        if (inputActions == null)
        {
            Debug.LogError($"{nameof(PauseManager)} on {gameObject.name} is missing an InputActionAsset reference.", this);
        }
    }

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed += OnTogglePausePressed;
        }

        if (resumeAction != null)
        {
            resumeAction.performed += OnResumePressed;
        }

        SetPauseCanvasActive(false);
        SetInputMode(false);
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnTogglePausePressed;
        }

        if (resumeAction != null)
        {
            resumeAction.performed -= OnResumePressed;
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            Resume();
            return;
        }

        Pause();
    }

    public void Pause()
    {
        if (IsPaused || !CanPause())
        {
            return;
        }

        if (uiManager == null)
        {
            Debug.LogError($"{nameof(PauseManager)} on {gameObject.name} could not find a {nameof(UIManager)} reference.", this);
            return;
        }

        resumePageIndex = uiManager.currentPage;
        uiManager.GoToPage(pausePageIndex);
        SetPauseCanvasActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsPaused = true;
        SetInputMode(true);
    }

    public void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        if (uiManager != null)
        {
            int targetPageIndex = resumePageIndex >= 0 ? resumePageIndex : uiManager.defaultPage;
            uiManager.GoToPage(targetPageIndex);
        }

        SetPauseCanvasActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = lockCursorOnResume ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !hideCursorOnResume;
        IsPaused = false;
        resumePageIndex = -1;
        SetInputMode(false);
    }

    public void SetPauseAllowed(bool isAllowed)
    {
        allowPause = isAllowed;
    }

    private bool CanPause()
    {
        if (!allowPause)
        {
            return false;
        }

        if (!usePagePauseRules || uiManager == null || uiManager.pages == null)
        {
            return true;
        }

        int pageIndex = uiManager.currentPage;
        if (pageIndex < 0 || pageIndex >= uiManager.pages.Count)
        {
            return true;
        }

        UIPage currentPage = uiManager.pages[pageIndex];
        return currentPage == null || currentPage.AllowPauseOnThisPage;
    }

    private void SetPauseCanvasActive(bool isActive)
    {
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(isActive);
        }
    }

    private void SetInputMode(bool useUIInput)
    {
        if (useUIInput)
        {
            playerActionMap?.Disable();
            uiActionMap?.Enable();
            pauseAction?.Enable();
            return;
        }

        uiActionMap?.Disable();
        playerActionMap?.Enable();
    }

    private void OnTogglePausePressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePause();
        }
    }

    private void OnResumePressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Resume();
        }
    }
}