using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float      moveSpeed = 80.0f;
    [SerializeField] private float      crouchSpeed = 40.0f;
    [SerializeField] private float      jumpForce = 5.0f;
    [SerializeField] private float      groundDetectorRadius = 2.0f;
    [SerializeField] private float      groundDetectorExtraRadius = 6.0f;
    [SerializeField] private float      coyoteTime = 0.1f;
    [SerializeField] private int        maxJumps = 1;
    [SerializeField] private Transform  groundDetector;
    [SerializeField] private LayerMask  groundMask;
    [SerializeField] private Collider2D groundCollider;
    [SerializeField] private Collider2D airCollider;
    [SerializeField] private Collider2D crouchedCollider;

    private Rigidbody2D rb;
    private Animator    animator;
    private bool        onGround = false;
    private bool        isBackwards = false;
    private bool        isCrouched = false;
    private float       speedX;
    private float       originalMoveSpeed;
    private float       lastGroundTime;
    private float       lastJumpTime;
    private int         nJumps = 0;
    private FollowMouse followMouse;
    
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        followMouse = GetComponentInChildren<FollowMouse>();
        originalMoveSpeed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Update character horizontal velocity
        Vector2 currentVelocity = rb.velocity;
        speedX = Input.GetAxis("Horizontal");

        DetectGround();

        if (onGround)
        {
            lastGroundTime = Time.time;
        }
        if ((Time.time - lastGroundTime) <= coyoteTime)
        {
            if (currentVelocity.y <= 0)
            {
                nJumps = maxJumps;
            }
        }
        else
        {
            if (nJumps == maxJumps)
            {
                nJumps = 0;
            }
        }

        groundCollider.enabled = onGround && !isCrouched;
        airCollider.enabled = !onGround;
        crouchedCollider.enabled = onGround && isCrouched;

        // Crouch when S is pressed
        if (Input.GetKey(KeyCode.S))
        {
            isCrouched = true;
            moveSpeed = crouchSpeed;
        }
        else if (!Input.GetKeyUp(KeyCode.S))
        {
            isCrouched = false;
            moveSpeed = originalMoveSpeed;
        }

        // Apply movement
        currentVelocity.x = speedX * moveSpeed;

        if (Input.GetButtonDown("Jump") && (nJumps > 0))
        {
            // Calculate the velocity needed to achieve the desired jump height
            currentVelocity.y = Mathf.Sqrt(2f * rb.gravityScale * jumpForce * rb.mass);
            // Apply gravity
            currentVelocity.y -= rb.gravityScale * Time.deltaTime;
            lastJumpTime = Time.time;
            lastGroundTime = 0;
            nJumps--;
        }
        else
        {
            lastJumpTime = 0;
        }

        // Apply movement
        rb.velocity = currentVelocity;

        // If the player is pointing right does nothing
        if (followMouse.PointingRight())
            transform.rotation = Quaternion.identity;
        // If the player is pointing left, rotate everything 180 degrees
        else if (!followMouse.PointingRight())
            transform.rotation = Quaternion.Euler(0, 180, 0);

        // Check if the player is moving backwards
        if (speedX >= 0 && !followMouse.PointingRight() ||
            speedX <= 0 && followMouse.PointingRight())
            isBackwards = true;
        else
            isBackwards = false;

        // Change visuals
        animator.SetFloat("Speed", Mathf.Abs(speedX));
        animator.SetBool("onGround", onGround);
        animator.SetBool("isBackwards", isBackwards);
        animator.SetBool("isCrouched", isCrouched);
    }

    void DetectGround()
    {
        Collider2D collider = Physics2D.OverlapCircle(groundDetector.position, groundDetectorRadius, groundMask);
        if (collider != null) onGround = true;
        else
        {
            collider = Physics2D.OverlapCircle(groundDetector.position - Vector3.right * groundDetectorExtraRadius, groundDetectorRadius, groundMask);
            if (collider != null) onGround = true;
            else
            {
                collider = Physics2D.OverlapCircle(groundDetector.position + Vector3.right * groundDetectorExtraRadius, groundDetectorRadius, groundMask);
                if (collider != null) onGround = true;
                else onGround = false;
            }
        }
    }
}