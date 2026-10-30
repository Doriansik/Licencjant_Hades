using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [Header("Movement Settings")]
    public float baseMoveSpeed = 8f;
    [HideInInspector] public float currentMoveSpeed;
    
    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    [HideInInspector] public bool canDashGlobally = true;

    [Header("Visual Feedback")]
    public Renderer playerRenderer;
    public Color normalColor = Color.white;
    public Color dashColor = Color.blue;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isDashing = false;
    private bool canDash = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        
        Instance = this;
        rb = GetComponent<Rigidbody>();
        DontDestroyOnLoad(gameObject);
        
        currentMoveSpeed = baseMoveSpeed;
        if (playerRenderer != null) playerRenderer.material.color = normalColor;
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawnPoint = GameObject.FindWithTag("Respawn");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            rb.linearVelocity = Vector3.zero; 
        }
    }

    void Update()
    {
        if (isDashing) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.z = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space) && canDash && canDashGlobally)
        {
            StartCoroutine(PerformDash(moveInput != Vector3.zero ? moveInput.normalized : transform.forward));
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.linearVelocity = new Vector3(moveInput.normalized.x * currentMoveSpeed, rb.linearVelocity.y, moveInput.normalized.z * currentMoveSpeed);
        }
    }

    IEnumerator PerformDash(Vector3 direction)
    {
        isDashing = true;
        canDash = false;
        
        if (playerRenderer != null) playerRenderer.material.color = dashColor;
        
        rb.linearVelocity = direction * dashForce;
        
        yield return new WaitForSeconds(dashDuration);
        
        isDashing = false;
        rb.linearVelocity = Vector3.zero;
        if (playerRenderer != null) playerRenderer.material.color = normalColor;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void ForceAttackDash()
    {
        if (canDashGlobally)
        {
            StartCoroutine(PerformDash(transform.forward));
        }
    }
}