using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public NetworkBool jumpPressed;
    public float mouseX;
}

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 60f;
    [SerializeField] private float airControl = 0.3f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 25f;
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Wall Handling")]
    [SerializeField] private float wallSlideSpeed = 0.7f;
    [SerializeField] private float wallAngleThreshold = 45f;
    [SerializeField] private int maxCollisionSolveIterations = 3;
    
    private CapsuleCollider col;
    private Rigidbody rb;
    private bool isGrounded;
    
    // Only used for local input
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference jump;
    
    [SerializeField] private PlayerCamera playerCamera;
    private Vector2 moveDir;
    private Vector3 currentVelocity;
    

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        currentVelocity = Vector3.zero;
    }

    public override void FixedUpdateNetwork()
    {
        // Only move if we have authority over this object
        if (!Object.HasStateAuthority) return;

        if (GetInput(out NetworkInputData data))
        {
            // Apply movement based on network data
            MoveWithNetworkInput(data.direction);
            
            // Handle jump input
            if (data.jumpPressed && isGrounded)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            }
        }
        data.mouseX = playerCamera.GetMouseX();
        RotatePlayer(Vector3.up * data.mouseX);
        
        GroundCheck();
    }
    
    // This method is called for the local player to get movement direction
    public Vector3 GetMovementDirection()
    {
        if (move == null) return Vector3.zero;
        
        moveDir = move.action.ReadValue<Vector2>();
        // Convert 2D input to 3D world direction
        Vector3 inputDir = (transform.right * moveDir.x + transform.forward * moveDir.y).normalized;
        return inputDir;
    }

    // This method uses the networked input to move the player
    private void MoveWithNetworkInput(Vector3 inputDir)
    {
        Vector3 desiredVelocity = inputDir * speed;
        
        // Get current horizontal velocity
        Vector3 currentHorizontalVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        // Calculate velocity change needed
        Vector3 velocityChange = desiredVelocity - currentHorizontalVel;
        
        // Determine if we're accelerating or decelerating/changing direction
        bool isChangingDirection = inputDir.magnitude > 0.1f && Vector3.Dot(currentHorizontalVel.normalized, inputDir) < 0.3f;
        bool isStopping = inputDir.magnitude < 0.1f;
        
        // Use different rates for acceleration vs direction changes/stopping
        float accelRate;
        if (isStopping || isChangingDirection)
        {
            accelRate = deceleration;
        }
        else
        {
            accelRate = acceleration;
        }
        
        // Apply acceleration (with air control when not grounded)
        float accelMultiplier = isGrounded ? 1f : airControl;
        float maxSpeedChange = accelRate * accelMultiplier * Time.fixedDeltaTime;
        
        if (velocityChange.magnitude > maxSpeedChange)
        {
            velocityChange = velocityChange.normalized * maxSpeedChange;
        }
        
        Vector3 targetVelocity = currentHorizontalVel + velocityChange;
        
        // Collision resolution with wall sliding
        targetVelocity = ResolveCollisions(targetVelocity);
        
        // Apply the velocity
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
    }

    private Vector3 ResolveCollisions(Vector3 velocity)
    {
        // Keep your existing collision resolution code
        if (velocity.magnitude < 0.001f)
            return velocity;
        
        Vector3 resolvedVelocity = velocity;
        float moveDistance = velocity.magnitude * Time.fixedDeltaTime;
        
        Vector3 bottom = GetBottom();
        Vector3 top = GetTop();
        float radius = col.radius * 0.95f;
        
        // Iteratively resolve collisions for smooth wall sliding
        for (int i = 0; i < maxCollisionSolveIterations; i++)
        {
            Vector3 moveDir = resolvedVelocity.normalized;
            float checkDistance = moveDistance + col.radius * 0.1f;
            
            if (Physics.CapsuleCast(bottom, top, radius, moveDir, out RaycastHit hit, checkDistance, 
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                // Check if this is a wall (not floor/ceiling)
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                
                if (angle > wallAngleThreshold && angle < 180f - wallAngleThreshold)
                {
                    // Project velocity onto wall surface (slide along wall)
                    Vector3 wallNormal = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;
                    resolvedVelocity = Vector3.ProjectOnPlane(resolvedVelocity, wallNormal);
                    
                    // Reduce speed when sliding along walls
                    float slideAmount = Mathf.Abs(Vector3.Dot(velocity.normalized, wallNormal));
                    float speedReduction = Mathf.Lerp(1f, wallSlideSpeed, slideAmount);
                    resolvedVelocity *= speedReduction;
                    
                    moveDistance = resolvedVelocity.magnitude * Time.fixedDeltaTime;
                    
                    // If velocity is too small after projection, stop
                    if (resolvedVelocity.magnitude < 0.001f)
                        return Vector3.zero;
                }
                else
                {
                    // Hit floor or ceiling, stop horizontal movement in that direction
                    break;
                }
            }
            else
            {
                // No collision, we're good
                break;
            }
        }
        
        return resolvedVelocity;
    }

    private Vector3 GetBottom()
    {
        return transform.position + Vector3.up * col.radius;
    }

    private Vector3 GetTop()
    {
        return transform.position + Vector3.up * (col.height - col.radius);
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    public void RotatePlayer(Vector3 cameraRotation) {
        transform.Rotate(cameraRotation);
    }
}