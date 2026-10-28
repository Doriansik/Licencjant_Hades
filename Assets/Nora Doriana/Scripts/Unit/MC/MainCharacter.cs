using UnityEngine;

public class MainCharacter : UnitBase
{
    #region Serialized Fields

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeedMultiplier;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCooldown;

    [Header("Death Settings")]
    [SerializeField] private float destroyDelayAfterDeath = 2f;

    #endregion

    #region Private Fields

    private CharacterCombat combat;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;

    private Vector3 currentMovementInput;
    private Vector3 dashDirection;

    private const string HORIZONTAL_AXIS = "Horizontal";
    private const string VERTICAL_AXIS = "Vertical";

    private const float INPUT_THRESHOLD = 0.1f;
    private const float TIMER_END = 0f;

    #endregion

    #region Properties

    public bool IsDashing => isDashing;

    #endregion

    #region Initialization

    protected override void Start()
    {
        base.Start();

        if (agent != null)
        {
            agent.updateRotation = false;
        }

        combat = GetComponent<CharacterCombat>();
    }

    #endregion

    #region Main Loop

    protected override void Update()
    {
        base.Update();

        if (isDead)
            return;

        HandleTimers();

        if (isDashing)
        {
            ExecuteDash();
        }
        else
        {
            HandleMovementInput();
            HandleDashInput();
            HandleAttackInput();
        }
    }

    #endregion

    #region Movement & Input

    private void HandleTimers()
    {
        if (dashCooldownTimer > TIMER_END)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= TIMER_END)
            {
                isDashing = false;
            }
        }
    }

    private void HandleMovementInput()
    {
        float moveX = Input.GetAxisRaw(HORIZONTAL_AXIS);
        float moveZ = Input.GetAxisRaw(VERTICAL_AXIS);

        currentMovementInput = new Vector3(moveX, 0f, moveZ).normalized;

        float moveSpeed = unitStats != null ? unitStats.baseMoveSpeed : 5f;

        if (currentMovementInput.magnitude >= INPUT_THRESHOLD)
        {
            MoveCharacter(currentMovementInput, moveSpeed);
            RotateCharacter(currentMovementInput);
        }
        else
        {
            StopCharacter();
        }
    }

    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= TIMER_END)
        {
            StartDash();
        }
    }

    private void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0) && combat != null && combat.CanAttack())
        {
            StartAttack();
        }
    }

    private void MoveCharacter(Vector3 direction, float speed)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.velocity = direction * speed;
        }
    }

    private void StopCharacter()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.velocity = Vector3.zero;
        }
    }

    private void RotateCharacter(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            float rotSpeed = unitStats != null ? unitStats.baseRotationSpeed : 15f;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region Dash Logic

    private void StartDash()
    {
        isDashing = true;

        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (currentMovementInput.magnitude >= INPUT_THRESHOLD)
        {
            dashDirection = currentMovementInput;
        }
        else
        {
            dashDirection = transform.forward;
        }

    }

    private void ExecuteDash()
    {
        float moveSpeed = unitStats != null ? unitStats.baseMoveSpeed : 5f;
        MoveCharacter(dashDirection, moveSpeed * dashSpeedMultiplier);
        RotateCharacter(dashDirection);
    }

    #endregion

    #region Combat Logic & Damage Override

    private void StartAttack()
    {
        combat.TriggerAttack();
    }

    public override void TakeDamage(int damageAmount)
    {
        if (isDashing)
        {
            Debug.Log("[MainCharacter] Dodge! No damage taken");
            return;
        }

        base.TakeDamage(damageAmount);
        Debug.Log($"[MainCharacter] Damage taken: {damageAmount}. Current HP: {currentHealth}");
    }

    #endregion

    #region Death

    protected override void Die()
    {
        base.Die();

        Debug.Log("[MainCharacter] Player is dead!");

        Destroy(gameObject, destroyDelayAfterDeath);
    }

    #endregion
}