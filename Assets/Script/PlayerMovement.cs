using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private HealthBar healthBar;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Combat")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 1.8f;

    [Header("Sword HitBox")]
    [SerializeField] private SwordHitBox swordHitBox;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Mobile")]
    [SerializeField] private MobileJoystick joystick;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private Vector3 velocity;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private int currentHealth;

    private bool isAttacking;
    private bool isHitted;
    private bool isDead;
    private bool isJumping;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (cameraTransform == null)
        {
            Camera cam = Camera.main;

            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
        }

        if (joystick == null)
        {
            joystick = FindFirstObjectByType<MobileJoystick>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (swordHitBox == null)
        {
            swordHitBox = GetComponentInChildren<SwordHitBox>();
        }
    }

    private void Update()
    {
        // =====================================================
        // ĐÃ CHẾT
        // =====================================================

        if (isDead)
        {
            ApplyGravity();
            return;
        }

        HandleAttack();

        // =====================================================
        // BỊ ĐÁNH
        // =====================================================

        if (isHitted)
        {
            StopMovementAnimation();
            ApplyGravity();

            return;
        }

        // =====================================================
        // ĐANG ĐÁNH
        // =====================================================

        if (isAttacking)
        {
            StopMovementAnimation();
            ApplyGravity();

            return;
        }

        // =====================================================
        // BÌNH THƯỜNG
        // =====================================================

        Move();
        HandleJump();
        ApplyGravity();
    }

    // =========================================================
    // ATTACK INPUT
    // =========================================================

    private void HandleAttack()
    {
        // PC
        if (Input.GetKeyDown(KeyCode.F))
        {
            MobileAttack();
        }
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void Move()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (joystick != null &&
            joystick.Input.sqrMagnitude > 0.01f)
        {
            horizontal = joystick.Input.x;
            vertical = joystick.Input.y;
        }
        else
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }

        Vector3 direction;

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            direction =
                forward * vertical +
                right * horizontal;
        }
        else
        {
            direction =
                transform.forward * vertical +
                transform.right * horizontal;
        }

        direction = Vector3.ClampMagnitude(direction, 1f);

        controller.Move(
            direction * moveSpeed * Time.deltaTime
        );

        // Rotation
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Animation
        bool walking =
            direction.sqrMagnitude > 0.01f;

        if (animator != null)
        {
            animator.SetBool(
                "isWalking",
                walking
            );
        }
    }

    // =========================================================
    // JUMP
    // =========================================================

    private void HandleJump()
{
    // =====================================================
    // GROUND / COYOTE TIME
    // =====================================================

    if (controller.isGrounded)
    {
        coyoteTimer = 0.15f;

        // Đã chạm đất
        if (velocity.y <= 0f)
        {
            velocity.y = -2f;

            if (isJumping)
            {
                isJumping = false;

                if (animator != null)
                {
                    animator.SetBool("isJumping", false);
                }
            }
        }
    }
    else
    {
        coyoteTimer -= Time.deltaTime;
    }

    // =====================================================
    // JUMP INPUT - PC
    // =====================================================

    if (Input.GetKeyDown(KeyCode.Space))
    {
        jumpBufferTimer = 0.15f;
    }

    // =====================================================
    // PERFORM JUMP
    // =====================================================

    if (jumpBufferTimer > 0f &&
        coyoteTimer > 0f &&
        !isJumping)
    {
        velocity.y =
            Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );

        jumpBufferTimer = 0f;
        coyoteTimer = 0f;

        isJumping = true;

        if (animator != null)
        {
            animator.SetBool("isJumping", true);
        }
    }

    jumpBufferTimer -= Time.deltaTime;
}

    public void MobileJump()
    {
        if (isDead)
            return;

        if (isAttacking)
            return;

        if (isHitted)
            return;

        jumpBufferTimer = 0.15f;
    }

    // =========================================================
    // GRAVITY
    // =========================================================

    private void ApplyGravity()
    {
        if (controller.isGrounded &&
            velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(
            velocity * Time.deltaTime
        );
    }

    // =========================================================
    // ATTACK
    // =========================================================

    public void MobileAttack()
    {
        if (isDead)
            return;

        if (animator == null)
            return;

        if (isAttacking)
            return;

        if (isHitted)
            return;

        isAttacking = true;

        animator.SetBool(
            "isWalking",
            false
        );

        animator.SetBool(
            "isAttacking",
            true
        );
    }

    // =========================================================
    // STOP ATTACK
    // Animation Event cuối Attack
    // =========================================================

    public void StopAttack()
    {
        if (isDead)
            return;

        isAttacking = false;

        if (animator != null)
        {
            animator.SetBool(
                "isAttacking",
                false
            );
        }
    }

    // =========================================================
    // TAKE DAMAGE
    // =========================================================

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (isHitted)
            return;

        currentHealth -= damage;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        Debug.Log(
            gameObject.name +
            " received damage: " +
            damage +
            " | HP: " +
            currentHealth
        );

        // =====================================================
        // DEATH
        // =====================================================

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // =====================================================
        // HIT
        // =====================================================

        isAttacking = false;
        isHitted = true;

        if (animator != null)
        {
            animator.SetBool(
                "isWalking",
                false
            );

            animator.SetBool(
                "isAttacking",
                false
            );

            animator.SetBool(
                "isHitted",
                true
            );
        }
    }

    // =========================================================
    // STOP HIT
    // Animation Event cuối Hit
    // =========================================================

    public void StopHit()
    {
        if (isDead)
            return;

        isHitted = false;

        if (animator != null)
        {
            animator.SetBool(
                "isHitted",
                false
            );
        }

        Debug.Log(
            gameObject.name +
            " hết HIT"
        );
    }

    // =========================================================
    // DEAL DAMAGE
    // Animation Event trong Attack
    // =========================================================

    public void DealDamage()
    {
        if (isDead || isHitted)
            return;

        /*
         * Player đang sử dụng SwordHitBox.
         *
         * Damage thực tế được xử lý bởi SwordHitBox
         * trong khoảng thời gian:
         *
         * EnableSwordHitBox()
         *          ↓
         *     Animation đánh
         *          ↓
         * DisableSwordHitBox()
         *
         * Vì vậy không gây damage trực tiếp ở đây
         * để tránh Player gây damage 2 lần.
         */

        Debug.Log(
            gameObject.name +
            " DealDamage Animation Event"
        );
    }

    // =========================================================
    // DEATH
    // =========================================================

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        isAttacking = false;
        isHitted = false;

        // Tắt Sword HitBox
        DisableSwordHitBox();

        Debug.Log(
            gameObject.name +
            " bắt đầu chết!"
        );

        if (animator != null)
        {
            animator.SetBool(
                "isWalking",
                false
            );

            animator.SetBool(
                "isAttacking",
                false
            );

            animator.SetBool(
                "isHitted",
                false
            );

            animator.SetBool(
                "isDead",
                true
            );
        }
    }

    // =========================================================
    // FINISH DEATH
    // Animation Event cuối Die
    // =========================================================

    public void FinishDeath()
    {
        if (!isDead)
            return;

        Debug.Log(
            gameObject.name +
            " Die animation finished!"
        );

        // Tắt CharacterController
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Tắt Player
        gameObject.SetActive(false);
    }

    // =========================================================
    // SWORD HIT BOX
    // =========================================================

    public void EnableSwordHitBox()
    {
        if (isDead)
            return;

        if (swordHitBox != null)
        {
            swordHitBox.EnableHitBox();
        }
        else
        {
            Debug.LogError(
                "PlayerMovement: Chưa gán SwordHitBox!"
            );
        }
    }

    public void DisableSwordHitBox()
    {
        if (swordHitBox != null)
        {
            swordHitBox.DisableHitBox();
        }
        else
        {
            Debug.LogError(
                "PlayerMovement: Chưa gán SwordHitBox!"
            );
        }
    }

    // =========================================================
    // STOP MOVEMENT ANIMATION
    // =========================================================

    private void StopMovementAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(
                "isWalking",
                false
            );
        }
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    public bool IsHitted()
    {
        return isHitted;
    }

    // =========================================================
// START JUMP
// Animation Event
// =========================================================

public void StartJump()
{
    if (isDead)
        return;

    if (isAttacking)
        return;

    if (isHitted)
        return;

    isJumping = true;

    if (animator != null)
    {
        animator.SetBool(
            "isJumping",
            true
        );
    }

    Debug.Log(
        gameObject.name +
        " bắt đầu JUMP"
    );
}

// =========================================================
// STOP JUMP
// Animation Event
// =========================================================

public void StopJump()
{
    if (isDead)
        return;

    isJumping = false;

    if (animator != null)
    {
        animator.SetBool(
            "isJumping",
            false
        );
    }

    Debug.Log(
        gameObject.name +
        " kết thúc JUMP"
    );
}
}