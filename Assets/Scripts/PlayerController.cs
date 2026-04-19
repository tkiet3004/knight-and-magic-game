using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airWalkSpeed = 3f;
    public float jumpImpulse = 10f;
    Vector2 moveInput;
    TouchingDirections touchingDirections;
    Damageable damageable;

    public event Action Jumped;
    public event Action Attacked;
    public event Action Moved;

    public float CurrentMoveSpeed { get
        {
            // Override canMove check nếu setting cho phép di chuyển khi attack
            bool canActuallyMove = CanMove || canMoveWhileAttacking;
            
            if(canActuallyMove)
            {
                if (IsMoving && !touchingDirections.IsOnWall)
                {
                    if (touchingDirections.IsGrounded)
                    {
                        if (IsRunning)
                        {
                            return runSpeed;
                        }
                        else
                        {
                            return walkSpeed;
                        }
                    }
                    else
                    {
                        // Air Move
                        return airWalkSpeed;
                    }
                }
                else
                {
                    // Idle speed is 0
                    return 0;
                }
            } else
            {
                // Movement locked
                return 0;
            }
            
        } }

    [SerializeField]
    private bool _isMoving = false;

    public bool IsMoving { get
        {
            return _isMoving;
        } 
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.isMoving, value);
        }
    }

    [SerializeField]
    private bool _isRunning = false;

    public bool IsRunning
    {
        get
        {
            return _isRunning;
        }
        set
        {
            _isRunning = value;
            animator.SetBool(AnimationStrings.isRunning, value);
        }
    }

    public bool _isFacingRight = true;

    public bool IsFacingRight { get { return _isFacingRight;  } private set { 
            // Flip only if value is new
            if(_isFacingRight != value)
            {
                
                transform.localScale *= new Vector2(-1, 1);
            }
            
            _isFacingRight = value;
        } }

    public bool CanMove { get
        {
            return animator.GetBool(AnimationStrings.canMove);
        } 
    }

    public bool IsAlive { 
        get
        {
            return animator.GetBool(AnimationStrings.isAlive);
        }
    }

    Rigidbody2D rb;
    Animator animator;

    [Header("VFX")]
    [SerializeField] private GameObject hitParticlesPrefab;

    [Header("Combat Settings")]
    [Tooltip("Cho phép di chuyển khi đang tấn công")]
    [SerializeField] private bool canMoveWhileAttacking = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();

        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Apply Speed Upgrades
        int speedLevel = PlayerPrefs.GetInt("SpeedLevel", 0);
        if (speedLevel > 0)
        {
            walkSpeed += speedLevel * 0.5f;
            runSpeed += speedLevel * 1.0f;
            airWalkSpeed += speedLevel * 0.3f;
        }
    }

    private void FixedUpdate()
    {
        // CurrentMoveSpeed đã xử lý logic canMoveWhileAttacking
        // Chỉ khóa khi bị hit (LockVelocity)
        if(!damageable.LockVelocity)
            rb.linearVelocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.linearVelocity.y);

        animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if(IsAlive)
        {
            IsMoving = moveInput != Vector2.zero;
            if (IsMoving) Moved?.Invoke();

            SetFacingDirection(moveInput);
        } 
        else
        {
            IsMoving = false;
        }
        
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if(moveInput.x > 0 && !IsFacingRight)
        {
            // Face the right
            IsFacingRight = true;
        } 
        else if (moveInput.x < 0 && IsFacingRight)
        {
            // Face the left
            IsFacingRight = false;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;
        } else if(context.canceled)
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // Cho phép nhảy khi attack nếu setting được bật
        bool canActuallyMove = CanMove || canMoveWhileAttacking;
        
        // Check if alive 
        if(context.started && touchingDirections.IsGrounded && canActuallyMove)
        {
            animator.SetTrigger(AnimationStrings.jumpTrigger);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpImpulse);
            Jumped?.Invoke();
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            animator.SetTrigger(AnimationStrings.attackTrigger);
            Attacked?.Invoke();
        }
    }

    public void OnRangedAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            animator.SetTrigger(AnimationStrings.rangedAttackTrigger);
        }
    }

    public void OnHit(int damage, Vector2 knockback)
    {
        rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocity.y + knockback.y);

        if (hitParticlesPrefab)
        {
            var particles = Instantiate(hitParticlesPrefab, transform.position, hitParticlesPrefab.transform.rotation);
            var ps = particles.GetComponent<ParticleSystem>();
            if (ps)
            {
                ps.Play();
                Destroy(particles, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(particles, 0.5f);
            }
        }
    }
}
