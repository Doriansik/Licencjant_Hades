using UnityEngine;

public class CharacterCombat : MonoBehaviour
{
    #region Serialized Fields

    [Header("Combat Settings")]
    [SerializeField] private int attackDamage;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackDuration;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackAngle;
    [SerializeField] private LayerMask hittableLayers;
    [SerializeField] private Transform attackPoint;

    #endregion

    #region Private Fields

    private float attackCooldownTimer;
    private float attackTimer;
    private bool isAttacking;

    private const float TIMER_END = 0f;
    private const int MAX_COLLIDERS_CAPACITY = 10;

    #endregion

    #region Properties

    public bool IsAttacking => isAttacking;
    public float AttackDuration => attackDuration;
    public float AttackRange => attackRange;
    public float AttackAngle => attackAngle;
    public Transform AttackPoint => attackPoint;

    #endregion

    #region Update

    private void Update()
    {
        HandleTimers();
    }

    private void HandleTimers()
    {
        if (attackCooldownTimer > TIMER_END)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= TIMER_END)
            {
                isAttacking = false;
            }
        }
    }

    #endregion

    #region Combat Actions

    public bool CanAttack()
    {
        return attackCooldownTimer <= TIMER_END;
    }

    public void TriggerAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        attackCooldownTimer = attackCooldown;

        DetectHits();
    }

    private void DetectHits()
    {
        Vector3 center = attackPoint != null ? attackPoint.position : transform.position + transform.forward;
        Collider[] hitColliders = new Collider[MAX_COLLIDERS_CAPACITY];
        int numColliders = Physics.OverlapSphereNonAlloc(center, attackRange, hitColliders, hittableLayers);

        for (int i = 0; i < numColliders; i++)
        {
            Collider col = hitColliders[i];
            if (col == null)
                continue;

            if (col.gameObject == gameObject)
                continue;

            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget <= attackAngle / 2f)
            {
                if (col.TryGetComponent<UnitBase>(out UnitBase targetUnit))
                {
                    if (targetUnit is MainCharacter)
                        continue;

                    targetUnit.TakeDamage(attackDamage);
                    Debug.Log($"[CharacterCombat] Damage dealt: {attackDamage} Name: {col.name}");
                }
            }
        }
    }

    #endregion
}