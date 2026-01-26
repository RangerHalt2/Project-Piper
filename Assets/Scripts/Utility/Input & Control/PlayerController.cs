using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float touchSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform groundCheck;
    private float deadzone = 20f;
    private float groundRadius = 0.2f;

    private Rigidbody2D rb;
    private bool isGrounded = false;

    private InputManager inputManager;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        inputManager = InputManager.Instance;
    }


    private void Update()
    {

        HandleJump();
        HandleMovement();

    }


    private void HandleMovement()
    {
        if(rb == null)
        {
            Debug.LogError("Rigidbody is NULL; Core Error.");
            return;
        }

        rb.linearVelocity = new Vector2(inputManager.MoveInput.x * moveSpeed, 0f);

        //If they're not on both at the same time, the above line will be 0 by default.
        if (inputManager.TouchPressInput)
        {
            Vector2 displacement = inputManager.TouchPosCurr - inputManager.TouchPosInput;
            if (displacement.magnitude < deadzone)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            Vector2 normalDis = displacement.normalized;
            rb.linearVelocity = new Vector2(normalDis.x * moveSpeed, 0f);
        }

    }

    private void HandleJump()
    {

    }

}
