using UnityEngine;

public class PlayerDamageHitbox : MonoBehaviour
{
    public float damage = 20f;
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        
        if (enemy != null)
        {
            Vector3 sourcePos = playerTransform != null ? playerTransform.position : transform.position;
            enemy.TakeDamage(damage, sourcePos);
        }
    }
}