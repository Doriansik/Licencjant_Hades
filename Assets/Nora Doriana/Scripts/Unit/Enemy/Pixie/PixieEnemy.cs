using UnityEngine;

public class PixieEnemy : UnitBase
{
    #region Enums

    private enum SwarmState
    {
        Circling,
        Warning,
        Dashing,
        Fleeing,
        Dead
    }

    #endregion

    #region Serialized Fields

    [Header("Pixie Stats Asset")]
    [SerializeField] private PixieSO pixieStats;

    #endregion

    #region Private Fields

    private PixieMovement pixieMovement;
    private PixieCombat pixieCombat;
    private PixieDeathHandler deathHandler;

    private SwarmState currentState;
    private Transform target;
    private UnitBase playerUnitRef;

    private float stateTimer;
    private float originalAcceleration;

    private const string PLAYER_TAG = "Player";
    private const float STATE_RESET_TIME = 0f;
    private const float FLEE_DESTINATION_MULTIPLIER = 1.5f;
    private const float ROTATION_SMOOTHNESS = 10f;

    #endregion

    #region Initialization & Lifecycle

    protected override void Start()
    {
        base.Start();

        pixieMovement = GetComponent<PixieMovement>();
        pixieCombat = GetComponent<PixieCombat>();
        deathHandler = GetComponent<PixieDeathHandler>();

        if (agent != null)
        {
            originalAcceleration = agent.acceleration;
        }

        currentState = SwarmState.Circling;
        Debug.Log("[PixieEnemy] Start! Initial state: CIRCLING.");

        GameObject playerObject = GameObject.FindGameObjectWithTag(PLAYER_TAG); // must be changed later
        if (playerObject != null)
        {
            target = playerObject.transform;
            Debug.Log("[PixieEnemy] Player found! Starting tracking.");

            if (playerObject.TryGetComponent<UnitBase>(out playerUnitRef))
            {
                playerUnitRef.onDeath += HandlePlayerDeath;
            }
        }
    }

    private void OnDestroy()
    {
        if (playerUnitRef != null)
        {
            playerUnitRef.onDeath -= HandlePlayerDeath;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isDead || target == null || agent == null || pixieStats == null)
            return;

        switch (currentState)
        {
            case SwarmState.Circling:
                HandleCircling();
                break;
            case SwarmState.Warning:
                HandleWarning();
                break;
            case SwarmState.Dashing:
                HandleDashing();
                break;
            case SwarmState.Fleeing:
                HandleFleeing();
                break;
        }
    }

    #endregion

    #region State Machine Logic

    private void HandleCircling()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= pixieStats.aggroDistance && pixieCombat.CanAttack(pixieStats.attackCooldown))
        {
            Debug.Log($"[PixieEnemy] Distance ({distanceToTarget}) <= Aggro ({pixieStats.aggroDistance}). Starting attack!");
            TransitionToWarning();
            return;
        }

        pixieMovement.MoveInCircle(target, pixieStats, unitStats.baseMoveSpeed);
    }

    private void TransitionToWarning()
    {
        currentState = SwarmState.Warning;
        stateTimer = STATE_RESET_TIME;
        pixieMovement.Stop();

        Debug.Log("[PixieEnemy] State changed to: WARNING.");
    }

    private void HandleWarning()
    {
        stateTimer += Time.deltaTime;

        Vector3 lookDirection = target.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * pixieStats.attackRotationSpeed);
        }

        if (stateTimer >= pixieStats.warningDuration)
        {
            TransitionToDashing();
        }
    }

    private void TransitionToDashing()
    {
        currentState = SwarmState.Dashing;
        pixieMovement.Resume();
        pixieMovement.SetAcceleration(originalAcceleration * pixieStats.dashSpeedMultiplier);

        Debug.Log("[PixieEnemy] State changed to: DASHING.");
    }

    private void HandleDashing()
    {
        pixieMovement.MoveToDestination(target.position, unitStats.baseMoveSpeed * pixieStats.dashSpeedMultiplier);

        if (target.TryGetComponent<MainCharacter>(out MainCharacter player) && player.IsDashing)
        {
            Debug.Log("[PixieEnemy] Player dashed during my attack! Missed!");
            TransitionToFleeing();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= pixieStats.attackDistance)
        {
            pixieCombat.ExecuteAttack(target, pixieStats);
            TransitionToFleeing();
        }
    }

    private void TransitionToFleeing()
    {
        currentState = SwarmState.Fleeing;
        stateTimer = STATE_RESET_TIME;
        pixieMovement.SetAcceleration(originalAcceleration);

        Debug.Log("[PixieEnemy] State changed to: FLEEING.");
    }

    private void HandleFleeing()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= pixieStats.fleeDuration)
        {
            currentState = SwarmState.Circling;
            Debug.Log("[PixieEnemy] Fleeing ended. State changed to: CIRCLING.");
            return;
        }

        Vector3 fleeDirection = (transform.position - target.position).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * pixieStats.fleeDistance * FLEE_DESTINATION_MULTIPLIER;

        Vector3 lookDirection = agent.steeringTarget - transform.position;
        lookDirection.y = 0f;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * ROTATION_SMOOTHNESS);
        }

        pixieMovement.MoveToDestination(fleeTarget, unitStats.baseMoveSpeed);
    }

    #endregion

    #region Combat & Events

    private void HandlePlayerDeath()
    {
        target = null;
        Debug.Log("[PixieEnemy] Player died! Stopping tracking and resetting path.");
        pixieMovement.Stop();
    }

    #endregion

    #region Death Effects

    protected override void Die()
    {
        currentState = SwarmState.Dead;
        Debug.Log("[PixieEnemy] Pixie died. Starting death sequence.");

        base.Die();
        deathHandler.HandleDeath(pixieStats);
    }

    #endregion
}