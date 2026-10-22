using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    #region Constants
    private const int MainGameSceneBuildIndex = 1;
    private const float SceneActivationThreshold = 0.9f;
    #endregion

    #region Public Methods
    public void LoadMainGameScene()
    {
        GameStateManager.ChangeState(GameState.Loading);
        StartCoroutine(LoadSceneAsync(MainGameSceneBuildIndex));
    }
    #endregion

    #region Private Methods
    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        while (operation.progress < SceneActivationThreshold)
        {
            yield return null;
        }

        operation.allowSceneActivation = true;
        GameStateManager.ChangeState(GameState.Exploration);
    }
    #endregion
}