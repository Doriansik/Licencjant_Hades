using UnityEngine;

public class SceneRoomTransition : MonoBehaviour
{
    [Header("Gdzie prowadzą te drzwi?")]
    public string targetSceneName; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null && !string.IsNullOrEmpty(targetSceneName))
            {
                Debug.Log("Gracz wszedł w trigger. Ładuję scenę: " + targetSceneName);
                GameManager.Instance.LoadSpecificRoom(targetSceneName);
            }
            else
            {
                Debug.LogWarning("Błąd: Nie wpisałeś nazwy sceny w skrypcie przejścia!");
            }
        }
    }
}