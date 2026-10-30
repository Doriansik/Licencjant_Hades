using UnityEngine;
using System.Collections;

public class EnemyLichoAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float stopDistance = 2f;
    public float aggroRadius = 15f;

    [Header("Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    public float warningTime = 0.5f;

    public GameObject warningIndicator;
    public GameObject damageHitbox;

    private Transform playerTransform;
    private bool isAttacking = false;
    private bool canAttack = true;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (warningIndicator != null) warningIndicator.SetActive(false);
        if (damageHitbox != null) damageHitbox.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null || isAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > aggroRadius) return;

        if (distanceToPlayer > stopDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
            transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));
        }
        else if (canAttack)
        {
            StartCoroutine(AttackSequence());
        }
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        canAttack = false;

        if (warningIndicator != null) warningIndicator.SetActive(true);
        yield return new WaitForSeconds(warningTime);
        if (warningIndicator != null) warningIndicator.SetActive(false);

        transform.position += transform.forward * 1.5f;

        if (damageHitbox != null) damageHitbox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        if (damageHitbox != null) damageHitbox.SetActive(false);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}