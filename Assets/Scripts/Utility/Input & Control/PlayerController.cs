using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float touchSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private float heightDisplacement = 15f;
    private float deadzone = 20f;
    private float groundRadius = 0.2f;

    private Rigidbody2D rb;
    private bool isGrounded = true;

    private InputManager inputManager;


    private void Start()
    {
        isGrounded = true;
        rb = GetComponent<Rigidbody2D>();
        inputManager = InputManager.Instance;
    }


    private void Update()
    {
        HandleJump();
        HandleMovement();
        Interact();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGrounded();
    }

    private void HandleMovement()
    {
        if(rb == null)
        {
            Debug.LogError("Rigidbody is NULL; Core Error.");
            return;
        }

        rb.linearVelocity = new Vector2(inputManager.MoveInput.x * moveSpeed, rb.linearVelocity.y);

        //If they're not on both at the same time, the above line will be 0 by default.
        if (inputManager.TouchPressInput)
        {
            Vector2 displacement = inputManager.TouchPosCurr - inputManager.TouchPosInput;
            if (displacement.magnitude < deadzone)
            {
                Vector2 zeroHorizontal = new Vector2 (0f, rb.linearVelocity.y);
                rb.linearVelocity = zeroHorizontal;
                return;
            }
            Vector2 normalDis = displacement.normalized;
            rb.linearVelocity = new Vector2(normalDis.x * moveSpeed, rb.linearVelocity.y);
        }

        if(rb.linearVelocity.x > 0f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if(rb.linearVelocity.x < 0f)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

    }

    private void HandleJump()
    {
        if(inputManager.JumpInput && isGrounded)
        {
            isGrounded = false;

            Vector2 jumpVector = new Vector2(0f, jumpForce);
            rb.linearVelocity += jumpVector;
            return;
        }

        if (inputManager.TouchPressInput && isGrounded)
        {
            Vector2 displacement = inputManager.TouchPosCurr - inputManager.TouchPosInput;
            bool isUpward = Vector2.Dot(displacement.normalized, Vector2.up) > 0.7f;
            if (displacement.magnitude > deadzone*2 && isUpward)
            {
                isGrounded = false;

                Vector2 jumpVector = new Vector2(0f, jumpForce);
                rb.linearVelocity += jumpVector;
                return;
            }
        }
    }

    private void CheckGrounded()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(groundCheck.position, groundRadius, whatIsGround);
        if (cols.Length > 0)
        {
            isGrounded = true;
        }
    }

    private void Interact()
    {
        if (inputManager.InteractInput)
        {
            Debug.Log("PLAYER CONTROLLER - Interact Key Pressed!");
            inputManager.InteractInput = false;

            Vector2 interactOrigin = (Vector2)transform.position + Vector2.up * heightDisplacement;
            Collider2D[] cols = Physics2D.OverlapCircleAll(interactOrigin, interactionRadius);
            foreach (Collider2D col in cols)
            {
                //Check if the object has an inherited of IInteractable and then null check it and run the Interact() function.
                IInteractable interactable = col.GetComponent<IInteractable>();
                if(interactable != null)
                {
                    Debug.Log("PLAYER CONTROLLER - Interactable is not null, calling the function now");
                    interactable.Interact();
                }
            }

        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 interactOrigin = (Vector2)transform.position + Vector2.up * heightDisplacement;
        //Gizmos.DrawWireSphere(interactOrigin, interactionRadius);
    }

}
