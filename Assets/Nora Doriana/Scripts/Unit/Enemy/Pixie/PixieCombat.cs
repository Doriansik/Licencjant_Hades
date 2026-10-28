using UnityEngine;

public class PixieCombat : MonoBehaviour
{
    #region Private Fields

    private float lastAttackTime;

    #endregion

    #region Combat Logic

    public bool CanAttack(float attackCooldown)
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public bool ExecuteAttack(Transform target, PixieSO stats)
    {
        lastAttackTime = Time.time;
        Debug.Log("[PixieCombat] Attack executed on the player!");

        if (target != null && target.TryGetComponent<MainCharacter>(out MainCharacter player))
        {
            if (player.IsDashing)
            {
                Debug.Log("[PixieCombat] Player dashed during the attack! Missed!");
                return false;
            }

            if (target.TryGetComponent<UnitBase>(out UnitBase playerUnit))
            {
                playerUnit.TakeDamage(stats.damage);
            }
        }

        return true;
    }

    #endregion
}