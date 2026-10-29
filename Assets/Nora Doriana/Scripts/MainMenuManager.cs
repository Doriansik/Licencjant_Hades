using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject newGameConfirmationPanel;
    [SerializeField] private GameObject exitConfirmationPanel;
    [SerializeField] private Button continueButton;

    [SerializeField] private GameLoader gameLoader;
    [SerializeField] private SaveSystemTest saveSystem;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        GameStateManager.ChangeState(GameState.MainMenu);
        InitializeMenu();
    }
    #endregion

    #region Private Methods
    private void InitializeMenu()
    {
        continueButton.interactable = saveSystem.HasSaveGame();
        ShowPanel(mainPanel);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(false);
        newGameConfirmationPanel.SetActive(false);
        exitConfirmationPanel.SetActive(false);

        panelToShow.SetActive(true);
    }
    #endregion

    #region Public Methods
    public void OnNewGameClicked()
    {
        if (saveSystem.HasSaveGame())
        {
            ShowPanel(newGameConfirmationPanel);
        }
        else
        {
            ConfirmNewGame();
        }
    }

    public void ConfirmNewGame()
    {
        saveSystem.ClearSave();
        gameLoader.LoadMainGameScene();
    }

    public void CancelNewGame()
    {
        ShowPanel(mainPanel);
    }

    public void OnContinueClicked()
    {
        gameLoader.LoadMainGameScene();
    }

    public void OnSettingsClicked()
    {
        ShowPanel(settingsPanel);
    }

    public void CloseSettings()
    {
        ShowPanel(mainPanel);
    }

    public void OnExitClicked()
    {
        ShowPanel(exitConfirmationPanel);
    }

    public void ConfirmExit()
    {
        Application.Quit();
    }

    public void CancelExit()
    {
        ShowPanel(mainPanel);
    }
    #endregion
}