using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 50f;
    public float currentHP;
    public int boneChipsDrop = 5;
    public float spDrop = 10f;

    [Header("References")]
    public Renderer enemyRenderer;
    private Color originalColor;

    private bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
        if (enemyRenderer != null) originalColor = enemyRenderer.material.color;
    }

    public void TakeDamage(float damageAmount, Vector3 damageSourcePosition)
    {
        if (isDead) return;

        currentHP -= damageAmount;
        StartCoroutine(HitFlash());

        EnemyBiesAI biesAI = GetComponent<EnemyBiesAI>();
        if (biesAI != null)
        {
            biesAI.OnDamagedFromDistance(damageSourcePosition);
        }

        if (currentHP <= 0) Die();
    }

    IEnumerator HitFlash()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (!isDead) enemyRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        isDead = true;
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
        if (GetComponent<EnemyLichoAI>()) GetComponent<EnemyLichoAI>().enabled = false;
        if (GetComponent<EnemyBiesAI>()) GetComponent<EnemyBiesAI>().enabled = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentBoneChips += boneChipsDrop;
            GameManager.Instance.ModifyPlayerSP(spDrop);
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        if (enemyRenderer != null) enemyRenderer.material.color = Color.gray;
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}