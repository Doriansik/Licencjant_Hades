using UnityEngine;

public class RoomTransitionKret : MonoBehaviour
{
    public Transform targetTeleportPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && targetTeleportPoint != null)
        {
            other.transform.position = targetTeleportPoint.position;
        }
    }
}