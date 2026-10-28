using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class UnitBase : MonoBehaviour
{
    #region Events

    public event Action<int> onHealthChanged;
    public event Action onDamageTaken;
    public event Action onDeath;

    #endregion

    #region Serialized Fields

    [Header("Stats Configuration")]
    [SerializeField] protected UnitSO unitStats;

    [Header("Base References")]
    [SerializeField] protected Animator animator;

    #endregion

    #region Protected & Private Fields

    protected NavMeshAgent agent;
    protected int currentHealth;
    protected bool isDead;

    private const int DEAD_HEALTH_THRESHOLD = 0;
    private const float STOPPED_SPEED = 0f;

    #endregion

    #region Properties

    public int CurrentHealth => currentHealth;
    public int MaxHealth => unitStats != null ? unitStats.maxHealth : 100;
    public bool IsDead => isDead;
    public NavMeshAgent Agent => agent;

    #endregion

    #region Initialization & Lifecycle

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        InitializeStats();
    }

    protected virtual void Update()
    {

    }

    #endregion

    #region Core Mechanics

    protected virtual void InitializeStats()
    {
        if (unitStats != null)
        {
            currentHealth = unitStats.maxHealth;
        }
        else
        {
            currentHealth = 100;
        }

        isDead = false;

        if (agent != null && unitStats != null)
        {
            agent.speed = unitStats.baseMoveSpeed;
            agent.angularSpeed = unitStats.baseRotationSpeed;
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;

        onHealthChanged?.Invoke(currentHealth);
        onDamageTaken?.Invoke();

        if (currentHealth <= DEAD_HEALTH_THRESHOLD)
        {
            Die();
        }
    }

    public virtual void Heal(int healAmount)
    {
        if (isDead)
            return;

        currentHealth += healAmount;

        int maxH = unitStats != null ? unitStats.maxHealth : currentHealth;
        if (currentHealth > maxH)
        {
            currentHealth = maxH;
        }

        onHealthChanged?.Invoke(currentHealth);
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentHealth = DEAD_HEALTH_THRESHOLD;

        if (agent != null)
        {
            agent.speed = STOPPED_SPEED;
            agent.isStopped = true;
            agent.enabled = false;
        }

        Collider unitCollider = GetComponent<Collider>();
        if (unitCollider != null)
        {
            unitCollider.enabled = false;
        }

        onDeath?.Invoke();
    }

    #endregion

    #region Movement

    public virtual void MoveToPosition(Vector3 destination)
    {
        if (isDead || agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public virtual void StopMovement()
    {
        if (isDead || agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    #endregion
}