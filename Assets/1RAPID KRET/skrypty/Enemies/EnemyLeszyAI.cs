using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyBase))]
public class EnemyLeszyAI : MonoBehaviour
{
    [Header("Boss Settings (Ogólne)")]
    public string bossName = "LESZY";
    public float actionCooldown = 3f; 
    public float phase2ActionCooldown = 1.5f; 

    [Header("Patern 1: Korzenie BOOOM")]
    public int boomWaves = 3; 
    public float delayBetweenBooms = 1f; 
    public float telegraphToDamageRatio = 0.6f; 
    public GameObject coneTelegraphPrefab; 
    public GameObject coneHitboxPrefab;    
    public GameObject circleTelegraphPrefab; 
    public GameObject circleHitboxPrefab;

    [Header("Patern 2: Kruki")]
    public GameObject crowProjectilePrefab; 
    public int crowsAmount = 8;         
    public float crowSpawnRadius = 8f;  
    public float delayBetweenCrows = 0.2f; 

    [Header("Patern 3: Korzenie Summon")]
    public float rootTrackingTime = 2.5f; 
    public float rootEscapeTime = 0.8f; 
    public float rootLifetime = 5f;
    public float rootSpawnYOffset = 0.1f; 
    public GameObject rootSummonIndicator; 
    public GameObject rootObstaclePrefab; 

    [Header("Faza 2 (30% HP)")]
    public GameObject arenaRootsBoundary; 

    private EnemyBase enemyBase;
    private Transform playerTransform;
    private bool isPhase2 = false;

    void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowBossBar(bossName);
            GameUIManager.Instance.UpdateBossHP(enemyBase.maxHP, enemyBase.maxHP);
        }

        if (arenaRootsBoundary) arenaRootsBoundary.SetActive(false);
        if (rootSummonIndicator) rootSummonIndicator.SetActive(false);

        StartCoroutine(BossLoop());
    }

    void Update()
    {
        if (enemyBase == null || enemyBase.currentHP <= 0) 
        {
            if (GameUIManager.Instance != null) GameUIManager.Instance.HideBossBar();
            return;
        }

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateBossHP(enemyBase.currentHP, enemyBase.maxHP);
        }

        if (!isPhase2 && enemyBase.currentHP <= enemyBase.maxHP * 0.3f)
        {
            isPhase2 = true;
            Debug.Log("FAZA 2! Mashup!");
            if (arenaRootsBoundary) arenaRootsBoundary.SetActive(true); 
            StopAllCoroutines(); 
            StartCoroutine(Phase2Loop());
        }
    }

    IEnumerator BossLoop()
    {
        yield return new WaitForSeconds(2f); 

        while (!isPhase2 && enemyBase.currentHP > 0)
        {
            int pattern = Random.Range(1, 4);

            if (pattern == 1) yield return StartCoroutine(Pattern1_RootsBoom());
            else if (pattern == 2) yield return StartCoroutine(Pattern2_Crows());
            else if (pattern == 3) yield return StartCoroutine(Pattern3_RootSummon(rootTrackingTime));

            yield return new WaitForSeconds(actionCooldown);
        }
    }

    IEnumerator Phase2Loop()
    {
        while (enemyBase.currentHP > 0)
        {
            int attack = Random.Range(1, 3);
            if (attack == 1) yield return StartCoroutine(Pattern3_RootSummon(rootTrackingTime * 0.6f)); 
            else if (attack == 2) yield return StartCoroutine(Pattern2_Crows()); 

            yield return new WaitForSeconds(phase2ActionCooldown);
        }
    }

    IEnumerator Pattern1_RootsBoom()
    {
        Debug.Log("Patern 1: Korzenie BOOOM w losowe strony!");
        
        float warningTime = delayBetweenBooms * telegraphToDamageRatio;
        float damageTime = delayBetweenBooms * (1f - telegraphToDamageRatio);

        for (int i = 0; i < boomWaves; i++)
        {
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            GameObject tel = Instantiate(coneTelegraphPrefab, transform.position, randomRotation);
            yield return new WaitForSeconds(warningTime);
            Destroy(tel);

            GameObject dmg = Instantiate(coneHitboxPrefab, transform.position, randomRotation);
            yield return new WaitForSeconds(damageTime);
            Destroy(dmg);
        }

        GameObject finalTel = Instantiate(circleTelegraphPrefab, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        Destroy(finalTel);

        GameObject finalDmg = Instantiate(circleHitboxPrefab, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(0.5f);
        Destroy(finalDmg);
    }

    IEnumerator Pattern2_Crows()
    {
        Debug.Log("Patern 2: Kruki progresywnie lecą z obrzeży do Leszego!");
        
        yield return new WaitForSeconds(1.5f);

        if (crowProjectilePrefab != null)
        {
            for (int i = 0; i < crowsAmount; i++)
            {
                Vector2 randomCirclePoint = Random.insideUnitCircle.normalized * crowSpawnRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomCirclePoint.x, 1f, randomCirclePoint.y); 

                Vector3 directionToLeszy = transform.position - spawnPos;
                directionToLeszy.y = 0; 
                Quaternion lookAtBossRotation = Quaternion.LookRotation(directionToLeszy);

                Instantiate(crowProjectilePrefab, spawnPos, lookAtBossRotation);
                
                yield return new WaitForSeconds(delayBetweenCrows);
            }
        }
        
        yield return new WaitForSeconds(1f);
    }

    IEnumerator Pattern3_RootSummon(float trackingTime)
    {
        Debug.Log("Patern 3: Korzenie Summon z korektą wysokości!");
        if (rootSummonIndicator) rootSummonIndicator.SetActive(true);

        float timer = 0f;
        while (timer < trackingTime)
        {
            Vector3 groundPos = new Vector3(playerTransform.position.x, rootSpawnYOffset, playerTransform.position.z);
            rootSummonIndicator.transform.position = groundPos;
            
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(rootEscapeTime);
        rootSummonIndicator.SetActive(false);

        if (rootObstaclePrefab)
        {
            Vector3 finalPos = rootSummonIndicator.transform.position;
            finalPos.y = rootSpawnYOffset;
            
            GameObject roots = Instantiate(rootObstaclePrefab, finalPos, Quaternion.identity);
            Destroy(roots, rootLifetime); 
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, crowSpawnRadius);
    }
}