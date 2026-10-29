using UnityEngine;

public class AoEProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;
    public GameObject explosionVFX; 

    private bool hasExploded = false;

    void Start()
    {
        if (explosionVFX) explosionVFX.SetActive(false);
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        if (!hasExploded)
        {
            transform.position += transform.forward * speed * Time.deltaTime; 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !hasExploded)
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;
        speed = 0f; 

        if (explosionVFX) explosionVFX.SetActive(true); 
        if (GetComponent<Renderer>()) GetComponent<Renderer>().enabled = false; 
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 0.3f); 
    }
}