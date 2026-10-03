using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Enemy : MonoBehaviour
{
    // =========================================================
    // HEALTH
    // =========================================================

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("Health Bar")]
    [SerializeField] private HealthBar healthBar;

    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float rotationSpeed = 10f;

    // =========================================================
    // PATROL
    // =========================================================

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolPointReachDistance = 0.3f;
    [SerializeField] private bool loopPatrol = true;

    private int currentPatrolIndex;

    // =========================================================
    // VISION
    // =========================================================

    [Header("Vision")]
    [SerializeField] private float visionDistance = 10f;

    [Tooltip("Góc nhìn tổng cộng. Ví dụ 90 = nhìn 45 độ mỗi bên.")]
    [SerializeField] private float visionAngle = 90f;

    [Tooltip("Vị trí mắt của Enemy. Nếu bỏ trống sẽ dùng vị trí Enemy.")]
    [SerializeField] private Transform eyePoint;

    [Tooltip("Layer của tường/vật cản.")]
    [SerializeField] private LayerMask obstacleMask;

    [Tooltip("Sau khi mất Player, Enemy tiếp tục đuổi trong khoảng thời gian này.")]
    [SerializeField] private float losePlayerDelay = 2f;

    private float losePlayerTimer;

    // =========================================================
    // COMBAT
    // =========================================================

    [Header("Combat")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.2f;

    // =========================================================
    // ENEMY SWORD HITBOX
    // =========================================================

    [Header("Enemy Sword HitBox")]
    [SerializeField] private EnemySwordHitBox enemySwordHitBox;

    // =========================================================
    // TARGET
    // =========================================================

    [Header("Target")]
    [SerializeField] private Transform player;

    // =========================================================
    // ANIMATION
    // =========================================================

    [Header("Animation")]
    [SerializeField] private Animator animator;

    // =========================================================
    // GRAVITY
    // =========================================================

    [Header("Gravity")]
    [SerializeField] private float gravity = -25f;

    [SerializeField] private float groundForce = -5f;

    // =========================================================
    // AI STATE
    // =========================================================

    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    private EnemyState currentState = EnemyState.Patrol;

    // =========================================================
    // COMPONENTS
    // =========================================================

    private CharacterController controller;

    // =========================================================
    // HEALTH
    // =========================================================

    private int currentHealth;

    // =========================================================
    // COMBAT STATE
    // =========================================================

    private bool isAttacking;
    private bool isHitted;
    private bool isDead;

    private float attackTimer;

    // =========================================================
    // GRAVITY
    // =========================================================

    private Vector3 velocity;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (enemySwordHitBox == null)
        {
            enemySwordHitBox =
                GetComponentInChildren<EnemySwordHitBox>();
        }

        if (eyePoint == null)
        {
            eyePoint = transform;
        }

        DisableSwordHitBox();
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindPlayer();

        if (patrolPoints != null &&
            patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
        }
        else
        {
            Debug.LogWarning(
                gameObject.name +
                ": Chưa có Patrol Points!"
            );
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // =====================================================
        // DEAD
        // =====================================================

        if (isDead)
        {
            HandleGravity();
            return;
        }

        // =====================================================
        // GRAVITY
        // =====================================================

        HandleGravity();

        // =====================================================
        // FIND PLAYER
        // =====================================================

        if (player == null)
        {
            FindPlayer();

            if (player == null)
            {
                Patrol();
                return;
            }
        }

        // =====================================================
        // HIT
        // =====================================================

        if (isHitted)
        {
            StopMovementAnimation();
            return;
        }

        // =====================================================
        // ATTACKING
        // =====================================================

        if (isAttacking)
        {
            StopMovementAnimation();
            return;
        }

        attackTimer -= Time.deltaTime;

        // =====================================================
        // CHECK PLAYER VISION
        // =====================================================

        bool canSeePlayer = CanSeePlayer();

        // =====================================================
        // AI
        // =====================================================

        switch (currentState)
        {
            case EnemyState.Patrol:

                if (canSeePlayer)
                {
                    StartChase();
                }
                else
                {
                    Patrol();
                }

                break;


            case EnemyState.Chase:

                if (canSeePlayer)
                {
                    losePlayerTimer = losePlayerDelay;

                    float distance =
                        GetHorizontalDistanceToPlayer();

                    if (distance <= attackRange)
                    {
                        currentState =
                            EnemyState.Attack;

                        Attack();
                    }
                    else
                    {
                        ChasePlayer();
                    }
                }
                else
                {
                    losePlayerTimer -= Time.deltaTime;

                    if (losePlayerTimer <= 0f)
                    {
                        ReturnToPatrol();
                    }
                    else
                    {
                        ChasePlayer();
                    }
                }

                break;


            case EnemyState.Attack:

                float attackDistance =
                    GetHorizontalDistanceToPlayer();

                if (canSeePlayer &&
                    attackDistance <= attackRange)
                {
                    Attack();
                }
                else
                {
                    currentState =
                        EnemyState.Chase;
                }

                break;
        }
    }

    // =========================================================
    // GRAVITY
    // =========================================================

    private void HandleGravity()
    {
        if (controller == null ||
            !controller.enabled)
        {
            return;
        }

        if (controller.isGrounded)
        {
            if (velocity.y < 0f)
            {
                velocity.y = groundForce;
            }
        }
        else
        {
            velocity.y +=
                gravity *
                Time.deltaTime;
        }

        controller.Move(
            Vector3.up *
            velocity.y *
            Time.deltaTime
        );
    }

    // =========================================================
    // FIND PLAYER
    // =========================================================

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player =
                playerObject.transform;
        }
    }

    // =========================================================
    // VISION
    // =========================================================

    private bool CanSeePlayer()
    {
        if (player == null)
            return false;

        if (isDead ||
            isHitted)
        {
            return false;
        }

        Vector3 eyePosition =
            eyePoint != null
                ? eyePoint.position
                : transform.position;

        Vector3 targetPosition =
            player.position;

        // -----------------------------------------------------
        // Khoảng cách
        // -----------------------------------------------------

        Vector3 direction =
            targetPosition -
            eyePosition;

        float distance =
            direction.magnitude;

        if (distance > visionDistance)
        {
            return false;
        }

        // -----------------------------------------------------
        // Góc nhìn
        // -----------------------------------------------------

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        direction.Normalize();

        Vector3 forward =
            transform.forward;

        forward.y = 0f;
        forward.Normalize();

        float angle =
            Vector3.Angle(
                forward,
                direction
            );

        if (angle > visionAngle / 2f)
        {
            return false;
        }

        // -----------------------------------------------------
        // RAYCAST - KIỂM TRA TƯỜNG
        // -----------------------------------------------------

        Vector3 rayDirection =
            targetPosition -
            eyePosition;

        if (Physics.Raycast(
                eyePosition,
                rayDirection.normalized,
                out RaycastHit hit,
                distance,
                obstacleMask))
        {
            // Có vật cản trước Player
            return false;
        }

        return true;
    }

    // =========================================================
    // PATROL
    // =========================================================

    private void Patrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            StopMovementAnimation();
            return;
        }

        Transform targetPoint =
            patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
        {
            MoveToNextPatrolPoint();
            return;
        }

        Vector3 direction =
            targetPoint.position -
            transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        // -----------------------------------------------------
        // Đã tới waypoint
        // -----------------------------------------------------

        if (distance <= patrolPointReachDistance)
        {
            MoveToNextPatrolPoint();
            return;
        }

        direction.Normalize();

        MoveInDirection(direction);

        SetWalkingAnimation(true);
    }

    // =========================================================
    // NEXT PATROL POINT
    // =========================================================

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }

        currentPatrolIndex++;

        // Loop
        if (currentPatrolIndex >= patrolPoints.Length)
        {
            if (loopPatrol)
            {
                currentPatrolIndex = 0;
            }
            else
            {
                currentPatrolIndex =
                    patrolPoints.Length - 1;

                StopMovementAnimation();
            }
        }
    }

    // =========================================================
    // START CHASE
    // =========================================================

    private void StartChase()
    {
        currentState =
            EnemyState.Chase;

        losePlayerTimer =
            losePlayerDelay;

        Debug.Log(
            gameObject.name +
            " phát hiện Player!"
        );

        ChasePlayer();
    }

    // =========================================================
    // CHASE
    // =========================================================

    private void ChasePlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            StopMovementAnimation();
            return;
        }

        direction.Normalize();

        MoveInDirection(direction);

        SetWalkingAnimation(true);
    }

    // =========================================================
    // RETURN PATROL
    // =========================================================

    private void ReturnToPatrol()
    {
        currentState =
            EnemyState.Patrol;

        losePlayerTimer = 0f;

        Debug.Log(
            gameObject.name +
            " mất Player -> quay lại tuần tra."
        );

        Patrol();
    }

    // =========================================================
    // MOVE
    // =========================================================

    private void MoveInDirection(Vector3 direction)
    {
        if (controller == null ||
            !controller.enabled)
        {
            return;
        }

        if (isDead ||
            isHitted ||
            isAttacking)
        {
            return;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.Normalize();

        controller.Move(
            direction *
            moveSpeed *
            Time.deltaTime
        );

        // -----------------------------------------------------
        // Rotate
        // -----------------------------------------------------

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    // =========================================================
    // ATTACK
    // =========================================================

    private void Attack()
    {
        if (isDead ||
            isHitted ||
            isAttacking)
        {
            return;
        }

        if (player == null)
            return;

        // -----------------------------------------------------
        // Kiểm tra khoảng cách
        // -----------------------------------------------------

        float distance =
            GetHorizontalDistanceToPlayer();

        if (distance > attackRange)
        {
            currentState =
                EnemyState.Chase;

            return;
        }

        // -----------------------------------------------------
        // Dừng đi
        // -----------------------------------------------------

        StopMovementAnimation();

        // -----------------------------------------------------
        // Quay về Player
        // -----------------------------------------------------

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }

        // -----------------------------------------------------
        // Cooldown
        // -----------------------------------------------------

        if (attackTimer > 0f)
        {
            return;
        }

        // -----------------------------------------------------
        // START ATTACK
        // -----------------------------------------------------

        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool(
                "isWalking",
                false
            );

            animator.SetBool(
                "isAttacking",
                true
            );
        }

        attackTimer =
            attackCooldown;
    }

    // =========================================================
    // DISTANCE
    // =========================================================

    private float GetHorizontalDistanceToPlayer()
    {
        if (player == null)
            return Mathf.Infinity;

        Vector3 a =
            transform.position;

        Vector3 b =
            player.position;

        a.y = 0f;
        b.y = 0f;

        return Vector3.Distance(a, b);
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

        DisableSwordHitBox();

        if (animator != null)
        {
            animator.SetBool(
                "isAttacking",
                false
            );
        }

        // Sau khi đánh xong:
        // nếu Player vẫn trong tầm -> Attack
        // nếu không -> Chase

        if (player != null)
        {
            float distance =
                GetHorizontalDistanceToPlayer();

            if (distance <= attackRange &&
                CanSeePlayer())
            {
                currentState =
                    EnemyState.Attack;
            }
            else if (CanSeePlayer())
            {
                currentState =
                    EnemyState.Chase;
            }
            else
            {
                currentState =
                    EnemyState.Patrol;
            }
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
            " nhận " +
            damage +
            " damage. HP: " +
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

        DisableSwordHitBox();

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

        Debug.Log(
            gameObject.name +
            " đang bị HIT"
        );
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

        // Sau khi bị đánh xong,
        // tiếp tục AI bình thường.

        if (player != null &&
            CanSeePlayer())
        {
            currentState =
                EnemyState.Chase;
        }
        else
        {
            currentState =
                EnemyState.Patrol;
        }
    }

    // =========================================================
    // DEAL DAMAGE
    // Animation Event trong Attack
    // =========================================================

    public void DealDamage()
    {
        if (isDead ||
            isHitted)
        {
            return;
        }

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

        DisableSwordHitBox();

        if (controller != null)
        {
            controller.enabled = false;
        }

        gameObject.SetActive(false);
    }

    // =========================================================
    // SWORD HIT BOX
    // =========================================================

    public void EnableSwordHitBox()
    {
        if (isDead)
            return;

        if (isHitted)
            return;

        if (enemySwordHitBox != null)
        {
            enemySwordHitBox.EnableHitBox();

            Debug.Log(
                gameObject.name +
                " ENABLE Enemy Sword HitBox"
            );
        }
        else
        {
            Debug.LogError(
                "Enemy: Chưa gán EnemySwordHitBox!"
            );
        }
    }

    public void DisableSwordHitBox()
    {
        if (enemySwordHitBox != null)
        {
            enemySwordHitBox.DisableHitBox();

            Debug.Log(
                gameObject.name +
                " DISABLE Enemy Sword HitBox"
            );
        }
    }

    // =========================================================
    // ANIMATION
    // =========================================================

    private void SetWalkingAnimation(bool value)
    {
        if (animator != null)
        {
            animator.SetBool(
                "isWalking",
                value
            );
        }
    }

    private void StopMovementAnimation()
    {
        SetWalkingAnimation(false);
    }

    // =========================================================
    // DEBUG VISION
    // =========================================================

    private void OnDrawGizmosSelected()
    {
        // -----------------------------------------------------
        // Vision Distance
        // -----------------------------------------------------

        Gizmos.color =
            Color.yellow;

        Vector3 origin =
            eyePoint != null
                ? eyePoint.position
                : transform.position;

        Gizmos.DrawWireSphere(
            origin,
            visionDistance
        );

        // -----------------------------------------------------
        // Vision cone
        // -----------------------------------------------------

        Vector3 leftDirection =
            Quaternion.Euler(
                0f,
                -visionAngle / 2f,
                0f
            ) *
            transform.forward;

        Vector3 rightDirection =
            Quaternion.Euler(
                0f,
                visionAngle / 2f,
                0f
            ) *
            transform.forward;

        Gizmos.DrawRay(
            origin,
            leftDirection *
            visionDistance
        );

        Gizmos.DrawRay(
            origin,
            rightDirection *
            visionDistance
        );

        // -----------------------------------------------------
        // Patrol
        // -----------------------------------------------------

        if (patrolPoints != null)
        {
            Gizmos.color =
                Color.blue;

            for (int i = 0;
                 i < patrolPoints.Length;
                 i++)
            {
                if (patrolPoints[i] == null)
                    continue;

                Gizmos.DrawSphere(
                    patrolPoints[i].position,
                    0.15f
                );

                if (i + 1 <
                    patrolPoints.Length &&
                    patrolPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(
                        patrolPoints[i].position,
                        patrolPoints[i + 1].position
                    );
                }
                else if (
                    loopPatrol &&
                    patrolPoints.Length > 1 &&
                    patrolPoints[0] != null)
                {
                    Gizmos.DrawLine(
                        patrolPoints[i].position,
                        patrolPoints[0].position
                    );
                }
            }
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

    public void StartJump()
    {
        
    }

    public void StopJump()
    {
        
    }
}