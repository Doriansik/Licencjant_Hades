using UnityEngine;

[CreateAssetMenu(fileName = "PixieSO", menuName = "Unit/Pixie SO")]
public class PixieSO : UnitSO
{
    #region Serialized Fields

    [Header("Combat Stats")]
    public int damage = 10;
    public float attackCooldown = 2f;

    [Header("Swarm AI Settings")]
    public float aggroDistance = 6f;
    public float attackDistance = 1.5f;
    public float circlingRadius = 4f;
    public float circlingSpeedMultiplier = 1.2f;
    public float separationRadius = 1.5f;

    [Header("Attack Mechanics")]
    public float warningDuration = 0.5f;
    public float dashSpeedMultiplier = 2.5f;
    public float fleeDistance = 5f;
    public float fleeDuration = 1.5f;
    public float attackRotationSpeed = 10f;

    [Header("Death Effects Timing")]
    public float corpseDarkenDuration = 1f;
    public float corpseDecayTime = 2f;

    #endregion
}