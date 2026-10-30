using UnityEngine;

public class EnemyDamageHitbox : MonoBehaviour
{
    public float damage = 10f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ModifyPlayerHP(-damage);
                Debug.Log("Licho uderzyło gracza! Zostało HP: " + GameManager.Instance.playerCurrentHP);
            }
        }
    }
}