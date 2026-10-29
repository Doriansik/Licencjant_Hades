using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayTest : MonoBehaviour
{
    #region Constants
    private const string SaveKey = "HasSaveTest";
    private const int SaveExistsValue = 1;
    private const int MainMenuSceneIndex = 0;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        GameStateManager.ChangeState(GameState.Exploration);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMenu();
        }
    }
    #endregion

    #region Private Methods
    private void SaveGame()
    {
        GameStateManager.ChangeState(GameState.Saving);
        PlayerPrefs.SetInt(SaveKey, SaveExistsValue);
        PlayerPrefs.Save();
        Debug.Log("GameSaved");
        GameStateManager.ChangeState(GameState.Exploration);
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene(MainMenuSceneIndex);
    }
    #endregion
}