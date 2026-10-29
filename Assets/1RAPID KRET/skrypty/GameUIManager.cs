using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PrimeTween; 
using TMPro; 

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("Status Bars (Player)")]
    public Image hpFillImage; 
    public Image spFillImage;

    [Header("Deck & Bench UI (WYMAGA 5 SLOTÓW!)")]
    public Image[] handSlots; 
    public Image[] benchSlots;
    
    [Header("Texts")]
    public TextMeshProUGUI mergeReadyText; 
    public TextMeshProUGUI deckCountText; 

    [Header("Boss UI")]
    public GameObject bossBarParent; 
    public Image bossHpFill;  
    public TextMeshProUGUI bossNameText;

    [Header("Colors")]
    public Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 1f); 
    public Color selectedColor = Color.white; 

    private Vector3[] originalPositions;
    private Vector3[] originalScales;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject.transform.root.gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject.transform.root.gameObject); 

        if (mergeReadyText) mergeReadyText.gameObject.SetActive(false);
        if (bossBarParent) bossBarParent.SetActive(false); 

        originalPositions = new Vector3[handSlots.Length];
        originalScales = new Vector3[handSlots.Length];
        for(int i = 0; i < handSlots.Length; i++)
        {
            originalPositions[i] = handSlots[i].rectTransform.anchoredPosition3D;
            originalScales[i] = handSlots[i].rectTransform.localScale;
        }
    }

    public void UpdateStatus(float curHP, float maxHP, float curSP, float maxSP)
    {
        if (hpFillImage) hpFillImage.fillAmount = curHP / maxHP;
        if (spFillImage) spFillImage.fillAmount = curSP / maxSP;
    }

    public void UpdateHandUI(List<CardSO> availableCards, int totalCards, int selectedIndex, int scrollDir = 0)
    {
        if (deckCountText) deckCountText.text = $"{availableCards.Count} / {totalCards}";

        if (availableCards.Count == 0)
        {
            foreach (var slot in handSlots) slot.enabled = false;
            return;
        }

        int count = availableCards.Count;
        int[] indices = new int[5];
        indices[0] = (selectedIndex - 2 + count) % count; 
        indices[1] = (selectedIndex - 1 + count) % count; 
        indices[2] = selectedIndex;                       
        indices[3] = (selectedIndex + 1) % count;         
        indices[4] = (selectedIndex + 2) % count;         

        for (int i = 0; i < handSlots.Length; i++)
        {
            if (i < handSlots.Length && count > 0) 
            {
                handSlots[i].enabled = true;
                handSlots[i].sprite = availableCards[indices[i]].cardIcon;
                handSlots[i].color = (i == 2) ? selectedColor : unselectedColor; 

                Tween.StopAll(handSlots[i].rectTransform);

                if (scrollDir != 0)
                {
                    int prevAnchorIndex = Mathf.Clamp(i + scrollDir, 0, handSlots.Length - 1);
                    
                    handSlots[i].rectTransform.anchoredPosition3D = originalPositions[prevAnchorIndex];
                    handSlots[i].rectTransform.localScale = originalScales[prevAnchorIndex];
                    
                    Tween.UIAnchoredPosition(handSlots[i].rectTransform, originalPositions[i], 0.25f, Ease.OutQuad);
                    Tween.Scale(handSlots[i].rectTransform, originalScales[i], 0.25f, Ease.OutQuad);
                }
                else
                {
                    handSlots[i].rectTransform.anchoredPosition3D = originalPositions[i];
                    handSlots[i].rectTransform.localScale = originalScales[i];
                }
            }
            else
            {
                handSlots[i].enabled = false;
            }
        }
    }

    public void UpdateBenchUI(List<CardSO> benchCards)
    {
        for (int i = 0; i < benchSlots.Length; i++)
        {
            if (i < benchCards.Count)
            {
                benchSlots[i].enabled = true;
                benchSlots[i].sprite = benchCards[i].cardIcon;
            }
            else
            {
                benchSlots[i].enabled = false;
            }
        }
        if (mergeReadyText) mergeReadyText.gameObject.SetActive(benchCards.Count == 3);
    }

    // --- FUNKCJE DLA BOSSA ---
    public void ShowBossBar(string bossName)
    {
        if (bossBarParent) bossBarParent.SetActive(true);
        if (bossNameText) bossNameText.text = bossName;
    }

    public void UpdateBossHP(float current, float max)
    {
        if (bossHpFill) bossHpFill.fillAmount = current / max;
    }

    public void HideBossBar()
    {
        if (bossBarParent) bossBarParent.SetActive(false);
    }
}