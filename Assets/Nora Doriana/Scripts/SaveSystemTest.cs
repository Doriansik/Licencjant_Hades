using UnityEngine;

public class SaveSystemTest : MonoBehaviour
{
    #region Constants
    private const string SaveKey = "HasSaveTest";
    private const int SaveExistsValue = 1;
    private const int SaveMissingValue = 0;
    #endregion

    #region Public Methods
    public bool HasSaveGame()
    {
        return PlayerPrefs.GetInt(SaveKey, SaveMissingValue) == SaveExistsValue;
    }

    public void CreateMockSave()
    {
        PlayerPrefs.SetInt(SaveKey, SaveExistsValue);
        PlayerPrefs.Save();
    }

    public void ClearSave()
    {
        PlayerPrefs.SetInt(SaveKey, SaveMissingValue);
        PlayerPrefs.Save();
    }
    #endregion
}