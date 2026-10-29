using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public float playerMaxHP = 100f;
    public float playerCurrentHP = 100f;
    public float playerMaxSP = 100f;  
    public float playerCurrentSP = 0f;
    public int currentBoneChips = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        UpdateUI(); 
    }

    public void LoadSpecificRoom(string sceneName)
    {
        Debug.Log("GameManager ładuje konkretny pokój: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void ModifyPlayerHP(float amount)
    {
        playerCurrentHP += amount;
        playerCurrentHP = Mathf.Clamp(playerCurrentHP, 0, playerMaxHP);
        UpdateUI();
    }

    public void ModifyPlayerSP(float amount)
    {
        playerCurrentSP += amount;
        playerCurrentSP = Mathf.Clamp(playerCurrentSP, 0, playerMaxSP);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateStatus(playerCurrentHP, playerMaxHP, playerCurrentSP, playerMaxSP);
        }
    }
}