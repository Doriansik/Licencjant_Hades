using System.Collections;
using UnityEngine;

public class PixieDeathHandler : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Renderer[] bodyRenderers; // must be changed when sprites are available

    #endregion

    #region Private Fields

    private const string COLOR_PROPERTY = "_Color";
    private const float STATE_RESET_TIME = 0f;

    #endregion

    #region Public Methods

    public void HandleDeath(PixieSO stats)
    {
        StartCoroutine(DeathSequenceCoroutine(stats));
    }

    #endregion

    #region Coroutines

    private IEnumerator DeathSequenceCoroutine(PixieSO stats)
    {
        float elapsed = STATE_RESET_TIME;
        Color targetColor = Color.black;

        Material[] materials = new Material[bodyRenderers.Length];
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] != null)
            {
                materials[i] = bodyRenderers[i].material;
            }
        }

        while (elapsed < stats.corpseDarkenDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / stats.corpseDarkenDuration;

            foreach (Material mat in materials)
            {
                if (mat != null && mat.HasProperty(COLOR_PROPERTY))
                {
                    mat.color = Color.Lerp(mat.color, targetColor, t);
                }
            }
            yield return null;
        }

        yield return new WaitForSeconds(stats.corpseDecayTime);

        Destroy(gameObject);
    }

    #endregion
}