using UnityEngine;
using UnityEngine.AI;

public class PixieMovement : MonoBehaviour
{
    #region Serialized Fields

    [Header("Settings")]
    [SerializeField] private LayerMask swarmLayerMask;

    #endregion

    #region Private Fields

    private NavMeshAgent agent;
    private float circlingAngle;

    private const float ANGLE_MULTIPLIER = Mathf.Deg2Rad;
    private const float CIRCLE_CLOSING_FACTOR = 0.5f;

    #endregion

    #region Initialization

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    #endregion

    #region Movement Methods

    public void MoveInCircle(Transform target, PixieSO stats, float baseMoveSpeed)
    {
        agent.speed = baseMoveSpeed * stats.circlingSpeedMultiplier;

        circlingAngle += Time.deltaTime * agent.speed;
        float rad = circlingAngle * ANGLE_MULTIPLIER;

        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * stats.circlingRadius;
        Vector3 targetPosition = target.position + offset;

        targetPosition = Vector3.Lerp(transform.position, targetPosition, CIRCLE_CLOSING_FACTOR);
        targetPosition += GetSeparationVector(stats.separationRadius);

        agent.SetDestination(targetPosition);
    }

    public void MoveToDestination(Vector3 destination, float speed)
    {
        agent.speed = speed;
        agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (agent.isOnNavMesh)
        {
            agent.speed = 0f;
            agent.ResetPath();
            agent.isStopped = true;
        }
    }

    public void Resume()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    public void SetAcceleration(float acceleration)
    {
        agent.acceleration = acceleration;
    }

    private Vector3 GetSeparationVector(float separationRadius)
    {
        Vector3 separation = Vector3.zero;
        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius, swarmLayerMask);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            Vector3 direction = transform.position - hit.transform.position;
            float distance = direction.magnitude;

            if (distance > 0f)
            {
                separation += direction.normalized / distance;
            }
        }

        return separation;
    }

    #endregion
}