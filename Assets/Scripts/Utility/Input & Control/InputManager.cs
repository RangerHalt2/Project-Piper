using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    #region Variables

    [SerializeField] private float deadZone = 50f;

    [SerializeField] public InputActionAsset playerControls;

    [SerializeField] public string actionMapName = "Player";//This should be the name of the entire mapping

    //These should be the names of individual input readings for the broad inputs like WASD is part of movement and jumping assigns spacebar and A/Circle on controllers
    [SerializeField] private string movement = "Movement";
    [SerializeField] private string touchPos = "Touch Position";
    [SerializeField] private string touchPress = "Touch Press";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string upSwipe = "Swipe Up";

    //This is an action input, each one needs one assigned
    private InputAction moveAction;
    private InputAction touchPosAction;
    private InputAction touchPressAction;
    private InputAction jumpAction;
    private InputAction upSwipeAction;

    #endregion

    #region Getters/Setters
    public Vector2 MoveInput { get; private set; }
    public Vector2 TouchPosInput { get; private set; }
    public Vector2 TouchPosCurr { get; private set; }
    public bool TouchPressInput { get; private set; }
    public bool JumpInput { get; private set; }

    #endregion

    #region Input Action Context and Values
    public static InputManager Instance { get; private set; }
    private InputActionMap map;

    void Awake()
    {
        //LB: Handles instance related stuff, still unsure where and when the destroy code runs? It never seems necessary
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //Destroy(gameObject);
        }
        map = playerControls.FindActionMap(actionMapName);
        //LB: This assigns the values of the actions to the input actions from the physical asset
        moveAction = map.FindAction(movement);
        touchPosAction = map.FindAction(touchPos);
        touchPressAction = map.FindAction(touchPress);
        jumpAction = map.FindAction(jump);
        upSwipeAction = map.FindAction(upSwipe);
        RegisterInputActions();
    }

    void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        touchPosAction.performed += context => TouchPosCurr = context.ReadValue<Vector2>();
        touchPosAction.canceled += context =>
        {
            TouchPosInput = Vector2.zero;
            TouchPosCurr = Vector2.zero;
        };

        touchPressAction.performed += context =>
        {
            TouchPressInput = true;
            TouchPosInput = touchPosAction.ReadValue<Vector2>();
        };
        touchPressAction.canceled += context => TouchPressInput = false;

        jumpAction.performed += context => JumpInput = true;
        jumpAction.canceled += context => JumpInput = false;
    }
    #endregion


    #region Enable/Disable
    private void OnEnable()
    {
        moveAction.Enable();
        touchPosAction.Enable();
        touchPressAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        touchPosAction.Disable();
        touchPressAction.Disable();
        jumpAction.Disable();
    }
    #endregion

}
