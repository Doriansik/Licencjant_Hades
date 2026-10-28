using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Unit/Unit SO")]
public class UnitSO : ScriptableObject
{
    #region Serialized Fields

    [Header("Base Unit Stats")]
    public int maxHealth = 100;
    public float baseMoveSpeed = 5f;
    public float baseRotationSpeed = 15f;

    #endregion
}