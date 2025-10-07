using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
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
    
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference jump;
    
    private Vector2 moveDir;
    private Vector3 currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        currentVelocity = Vector3.zero;
    }

    void Update()
    {
        GroundCheck();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void OnEnable()
    {
        jump.action.started += Jump;
    }

    private void OnDisable()
    {
        jump.action.started -= Jump;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }

    private void Move()
    {
        moveDir = move.action.ReadValue<Vector2>();
        
        // Calculate desired movement direction in world space
        Vector3 inputDir = (transform.right * moveDir.x + transform.forward * moveDir.y).normalized;
        Vector3 desiredVelocity = inputDir * speed;
        
        // Get current horizontal velocity
        Vector3 currentHorizontalVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        // Calculate velocity change needed
        Vector3 velocityChange = desiredVelocity - currentHorizontalVel;
        
        // Determine if we're accelerating or decelerating/changing direction
        bool isChangingDirection = moveDir.magnitude > 0.1f && Vector3.Dot(currentHorizontalVel.normalized, inputDir) < 0.3f;
        bool isStopping = moveDir.magnitude < 0.1f;
        
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
}