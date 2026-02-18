using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats stats;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dashAction;

    private Rigidbody2D rb;
    private float moveInput;
    private float currentVelocityX;
    private bool isDashing;
    private float lastDashTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (moveAction != null)
            moveAction.action.Enable();
        if (dashAction != null)
            dashAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
            moveAction.action.Disable();
        if (dashAction != null)
            dashAction.action.Disable();
    }

    private void Start()
    {
        if (stats == null)
        {
            Debug.LogError("PlayerStats not assigned to PlayerMovement on " + gameObject.name);
        }
    }

    private void Update()
    {
        if (stats == null) return;

        GetInput();
        HandleDashInput();
    }

    private void FixedUpdate()
    {
        if (stats == null || isDashing) return;

        HandleMovement();
    }

    private void GetInput()
    {
        if (moveAction != null)
        {
            Vector2 moveVector = moveAction.action.ReadValue<Vector2>();
            moveInput = moveVector.x;
        }
    }

    private void HandleDashInput()
    {
        if (dashAction != null && dashAction.action.WasPressedThisFrame() && CanDash())
        {
            PerformDash();
        }
    }

    private void HandleMovement()
    {
        float targetVelocityX = moveInput * stats.moveSpeed;

        float accelerationRate = Mathf.Abs(moveInput) > 0 ? stats.acceleration : stats.deceleration;

        currentVelocityX = Mathf.MoveTowards(
            currentVelocityX,
            targetVelocityX,
            accelerationRate * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);
    }

    private bool CanDash()
    {
        return !isDashing && Time.time >= lastDashTime + stats.dashCooldown && Mathf.Abs(moveInput) > 0.1f;
    }

    private void PerformDash()
    {
        lastDashTime = Time.time;
        isDashing = true;

        float dashDirection = Mathf.Sign(moveInput);
        float dashVelocity = dashDirection * (stats.dashDistance / stats.dashDuration);

        rb.linearVelocity = new Vector2(dashVelocity, rb.linearVelocity.y);

        Invoke(nameof(EndDash), stats.dashDuration);
    }

    private void EndDash()
    {
        isDashing = false;
        currentVelocityX = rb.linearVelocity.x;
    }

    public PlayerStats GetStats()
    {
        return stats;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public float GetDashCooldownRemaining()
    {
        return Mathf.Max(0, stats.dashCooldown - (Time.time - lastDashTime));
    }
}
