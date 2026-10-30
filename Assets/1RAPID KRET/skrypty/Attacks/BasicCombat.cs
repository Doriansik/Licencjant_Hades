using UnityEngine;
using System.Collections;

public class BasicCombat : MonoBehaviour
{
    public GameObject attackHitbox; 
    public float attackDuration = 0.1f; 
    public float attackCooldown = 0.3f;
    
    private bool canAttack = true;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canAttack) 
        {
            StartCoroutine(PerformBasicAttack());
        }
    }

    IEnumerator PerformBasicAttack()
    {
        canAttack = false;
        
        attackHitbox.SetActive(true);
        
        yield return new WaitForSeconds(attackDuration);
        attackHitbox.SetActive(false);
        
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}