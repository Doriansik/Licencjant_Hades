using UnityEngine;
using System.Collections;

public class EnemyBiesAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f; 

    [Header("Ranges")]
    public float tapTapRange = 2.5f; 
    public float chargeRange = 10f;  

    [Header("Combat Cooldowns")]
    public float cooldownBetweenAttacks = 3f; 

    [Header("Charge Attack (Szarża)")]
    public float chargePrepTime = 1.5f;
    public float chargeSpeed = 15f;
    public float chargeDuration = 0.5f;
    public GameObject chargeTelegraphLine; 
    public GameObject chargeHitbox; 

    [Header("TapTap Attack (AoE)")]
    public float tapTapPrepTime = 1f;
    public GameObject tapTapTelegraphCircle; 
    public GameObject tapTapHitbox; 

    private Transform playerTransform;
    private bool isAttacking = false;
    private bool isOnCooldown = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (chargeTelegraphLine) chargeTelegraphLine.SetActive(false);
        if (tapTapTelegraphCircle) tapTapTelegraphCircle.SetActive(false);
        if (chargeHitbox) chargeHitbox.SetActive(false);
        if (tapTapHitbox) tapTapHitbox.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null || isAttacking) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (!isOnCooldown)
        {
            if (distance <= tapTapRange)
            {
                StartCoroutine(TapTapSequence());
            }
            else if (distance <= chargeRange && distance > tapTapRange)
            {
                StartCoroutine(ChargeSequence());
            }
            else
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));
    }

    public void OnDamagedFromDistance(Vector3 sourcePosition)
    {
        if (isAttacking || isOnCooldown) return;

        float distance = Vector3.Distance(transform.position, sourcePosition);
        if (distance > tapTapRange)
        {
            Debug.Log("Bies wkurzony atakiem dystansowym! SZARŻA!");
            StartCoroutine(ChargeSequence());
        }
    }

    IEnumerator TapTapSequence()
    {
        isAttacking = true;
        
        if (tapTapTelegraphCircle) tapTapTelegraphCircle.SetActive(true);
        yield return new WaitForSeconds(tapTapPrepTime);
        if (tapTapTelegraphCircle) tapTapTelegraphCircle.SetActive(false);

        if (tapTapHitbox) tapTapHitbox.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        if (tapTapHitbox) tapTapHitbox.SetActive(false);

        StartCoroutine(AttackCooldown());
    }

    IEnumerator ChargeSequence()
    {
        isAttacking = true;

        transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z));
        Vector3 chargeDirection = transform.forward;
        
        if (chargeTelegraphLine) chargeTelegraphLine.SetActive(true);
        yield return new WaitForSeconds(chargePrepTime);
        if (chargeTelegraphLine) chargeTelegraphLine.SetActive(false);

        if (chargeHitbox) chargeHitbox.SetActive(true);
        
        float timer = 0f;
        while(timer < chargeDuration)
        {
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        if (chargeHitbox) chargeHitbox.SetActive(false);

        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        isAttacking = false;
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownBetweenAttacks);
        isOnCooldown = false;
    }
}