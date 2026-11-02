using UnityEngine;

public class AttackAreaVisualizer : MonoBehaviour
{
    #region Serialized Fields

    [Header("Visual Setup")]
    [SerializeField] private GameObject attackAreaVisualPrefab;
    [SerializeField] private Vector3 visualOffset = new Vector3(0f, 0.05f, 1f);

    #endregion

    #region Private Fields

    private CharacterCombat combat;
    private GameObject visualInstance;

    #endregion

    #region Initialization & Lifecycle

    private void Awake()
    {
        combat = GetComponent<CharacterCombat>();
        InitializeVisual();
    }

    private void LateUpdate()
    {
        if (visualInstance == null || combat == null)
            return;

        if (combat.IsAttacking && !visualInstance.activeSelf)
        {
            visualInstance.SetActive(true);
        }
        else if (!combat.IsAttacking && visualInstance.activeSelf)
        {
            visualInstance.SetActive(false);
        }
    }

    #endregion

    #region Visualization Control

    private void InitializeVisual()
    {
        if (attackAreaVisualPrefab == null)
        {
            return;
        }

        visualInstance = Instantiate(attackAreaVisualPrefab, transform);
        visualInstance.transform.localPosition = visualOffset;
        visualInstance.SetActive(false);
    }

    #endregion
}