using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using System.Collections.Generic;
using PrimeTween; 

public class PlayerActionController : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement movementScript;
    public GameObject basicAttackHitbox; 
    public GameObject bigSlashHitbox;    
    public Transform shootPoint;         
    public GameObject aoeMergeProjectilePrefab; 

    [Header("UI Over Head / Screen")]
    public Image reloadProgressBar; 
    public SpriteRenderer cardPopupRenderer; 

    [Header("Deck System")]
    public List<CardSO> fullDeck = new List<CardSO>();
    public List<CardSO> availableCards = new List<CardSO>();
    public List<CardSO> discardPile = new List<CardSO>();
    public List<CardSO> bench = new List<CardSO>();
    
    [Header("Combo & Timers")]
    public float actionDuration = 0.15f;    
    public float normalCooldown = 0.3f;     
    public float comboWindow = 1.0f;        
    public float comboResetDelay = 1.2f;    

    [Header("Mawka Ultimate")]
    public float mawkaCostSP = 10f; 
    public float mawkaHealHP = 30f;
    public SpriteRenderer mawkaPopupRenderer; 
    public Sprite mawkaSprite; 

    private int currentComboCount = 0;
    private float comboTimer = 0f;
    private bool isGlobalCooldown = false; 
    private bool isReloading = false;
    private int currentSelectedCardIndex = 0;

    void Start()
    {
        availableCards.AddRange(fullDeck);
        
        if (reloadProgressBar != null)
        {
            reloadProgressBar.fillAmount = 0f;
            reloadProgressBar.gameObject.SetActive(false);
        }

        if (cardPopupRenderer != null)
        {
            cardPopupRenderer.color = new Color(1, 1, 1, 0); 
        }
        
        UpdateAllUI(0);
    }

    void Update()
    {
        HandleMouseAiming();
        HandleComboTimer();
        HandleCombatInputs();
        HandleMawkaSkill(); 
    }

    void HandleMouseAiming()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 lookPoint = ray.GetPoint(rayDistance);
            transform.LookAt(new Vector3(lookPoint.x, transform.position.y, lookPoint.z));
        }
    }

    void HandleComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0 && !isGlobalCooldown)
            {
                currentComboCount = 0;
            }
        }
    }

    void HandleCombatInputs()
    {
        if (isReloading || isGlobalCooldown) return;

        if (Input.GetMouseButtonDown(0)) 
        {
            StartCoroutine(ExecuteAction(null));
        }
        else if (Input.GetMouseButtonDown(1) && availableCards.Count > 0) 
        {
            CardSO cardToUse = availableCards[currentSelectedCardIndex];
            StartCoroutine(ExecuteAction(cardToUse));
        }

        int scrollDir = 0;
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (scrollData > 0f) { ChangeCardSelection(1); scrollDir = 1; }
        else if (scrollData < 0f) { ChangeCardSelection(-1); scrollDir = -1; }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (bench.Count < 3) SendCardToBench();
            else if (bench.Count == 3) ExecuteMergedAttack(); 
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ReloadDeck());
        }
    }

    void HandleMawkaSkill()
    {
        if (Input.GetKeyDown(KeyCode.Q) && !isReloading && !isGlobalCooldown)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerCurrentSP >= mawkaCostSP)
            {
                Debug.Log("MAWKA ULT! Leczenie za " + mawkaHealHP);
                GameManager.Instance.ModifyPlayerSP(-mawkaCostSP);
                GameManager.Instance.ModifyPlayerHP(mawkaHealHP);
                
                // --- EFEKT WIZUALNY MAWKI ---
                if (mawkaPopupRenderer != null && mawkaSprite != null)
                {
                    mawkaPopupRenderer.sprite = mawkaSprite;
                    mawkaPopupRenderer.transform.localPosition = new Vector3(0, 2f, 0);
                    mawkaPopupRenderer.transform.localScale = Vector3.one; 
                    mawkaPopupRenderer.color = Color.white;
                    
                    // Powiększenie z "odbiciem" i lot do góry
                    Tween.Scale(mawkaPopupRenderer.transform, new Vector3(1.5f, 1.5f, 1.5f), 0.6f, Ease.OutBack);
                    Tween.LocalPosition(mawkaPopupRenderer.transform, new Vector3(0, 4.5f, 0), 0.8f, Ease.OutCubic);
                    Tween.Color(mawkaPopupRenderer, new Color(1, 1, 1, 0), 0.8f, Ease.InExpo);
                }

                StartCoroutine(MawkaCooldown());
            }
            else
            {
                Debug.Log("Za mało SP na Mawkę!");
            }
        }
    }

    IEnumerator MawkaCooldown()
    {
        isGlobalCooldown = true;
        yield return new WaitForSeconds(0.5f);
        isGlobalCooldown = false;
    }

    IEnumerator ExecuteAction(CardSO cardUsed)
    {
        isGlobalCooldown = true;
        currentComboCount++;
        comboTimer = comboWindow; 

        if (cardUsed == null)
        {
            StartCoroutine(ShowTempHitbox(basicAttackHitbox, actionDuration));
        }
        else
        {
            if (cardPopupRenderer != null && cardUsed.cardIcon != null)
            {
                cardPopupRenderer.sprite = cardUsed.cardIcon;
                cardPopupRenderer.transform.localPosition = new Vector3(0, 2f, 0); 
                cardPopupRenderer.color = Color.white;
                
                Tween.LocalPosition(cardPopupRenderer.transform, new Vector3(0, 3.5f, 0), 0.6f, Ease.OutCubic);
                Tween.Color(cardPopupRenderer, new Color(1, 1, 1, 0), 0.6f, Ease.InExpo); 
            }

            switch (cardUsed.actionType)
            {
                case CardActionType.BigSlash:
                    StartCoroutine(ShowTempHitbox(bigSlashHitbox, actionDuration));
                    break;
                case CardActionType.Projectile:
                    if (cardUsed.projectilePrefab && shootPoint)
                        Instantiate(cardUsed.projectilePrefab, shootPoint.position, shootPoint.rotation);
                    break;
                case CardActionType.DashHit:
                    if (movementScript != null) movementScript.ForceAttackDash(); 
                    StartCoroutine(ShowTempHitbox(basicAttackHitbox, actionDuration));
                    break;
            }

            discardPile.Add(cardUsed);
            availableCards.RemoveAt(currentSelectedCardIndex);
            if (currentSelectedCardIndex >= availableCards.Count && availableCards.Count > 0)
                currentSelectedCardIndex = availableCards.Count - 1;
            
            UpdateAllUI(0);
        }

        if (currentComboCount >= 3)
        {
            yield return new WaitForSeconds(comboResetDelay);
            currentComboCount = 0; 
        }
        else
        {
            yield return new WaitForSeconds(normalCooldown);
        }

        isGlobalCooldown = false;
    }

    void SendCardToBench()
    {
        if (availableCards.Count == 0 || bench.Count >= 3) return;

        bench.Add(availableCards[currentSelectedCardIndex]);
        availableCards.RemoveAt(currentSelectedCardIndex);

        if (currentSelectedCardIndex >= availableCards.Count && availableCards.Count > 0)
            currentSelectedCardIndex = availableCards.Count - 1;

        UpdateAllUI(0);
    }

    void ExecuteMergedAttack()
    {
        if (aoeMergeProjectilePrefab && shootPoint)
            Instantiate(aoeMergeProjectilePrefab, shootPoint.position, shootPoint.rotation);

        discardPile.AddRange(bench);
        bench.Clear();
        UpdateAllUI(0);
    }

    IEnumerator ReloadDeck()
    {
        isReloading = true;
        float totalTime = 1.5f; 
        float elapsed = 0f;

        if (reloadProgressBar != null) 
        {
            reloadProgressBar.gameObject.SetActive(true);
            reloadProgressBar.fillAmount = 0f;
        }

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            if (reloadProgressBar != null) reloadProgressBar.fillAmount = elapsed / totalTime;
            yield return null; 
        }

        if (reloadProgressBar != null) 
        {
            reloadProgressBar.gameObject.SetActive(false);
            reloadProgressBar.fillAmount = 0f;
        }

        availableCards.AddRange(discardPile);
        availableCards.AddRange(bench);
        discardPile.Clear();
        bench.Clear();
        currentSelectedCardIndex = 0;

        isReloading = false;
        UpdateAllUI(0);
    }

    IEnumerator ShowTempHitbox(GameObject hitbox, float duration)
    {
        if(hitbox) hitbox.SetActive(true);
        yield return new WaitForSeconds(duration);
        if(hitbox) hitbox.SetActive(false);
    }

    void ChangeCardSelection(int direction)
    {
        if (availableCards.Count == 0) return;
        currentSelectedCardIndex += direction;
        if (currentSelectedCardIndex >= availableCards.Count) currentSelectedCardIndex = 0;
        if (currentSelectedCardIndex < 0) currentSelectedCardIndex = availableCards.Count - 1;
        UpdateAllUI(direction); 
    }

    void UpdateAllUI(int scrollDir)
    {
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateHandUI(availableCards, fullDeck.Count, currentSelectedCardIndex, scrollDir);
            GameUIManager.Instance.UpdateBenchUI(bench);
        }
    }
}