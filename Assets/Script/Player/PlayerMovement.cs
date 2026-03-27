using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
using FMODUnity;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashEnergyCost = 25f;
    public ParticleSystem startDashParticle;

    [Header("Audio (FMOD)")]
    [Tooltip("Zvuk, který se přehraje při startu dashe (např. Whoosh)")]
    public EventReference dashSound;

    [Header("Camera LookAhead")] // 🔥 PŘIDÁNO: Posun kamery podle pohybu
    public Transform lookAheadTarget;
    public float lookDistance = 4f;
    public float lookSmoothSpeed = 5f;
    private bool isDynamicCamera;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isDashing = false;

    public MMF_Player dashFeedback;

    [Header("Animation")]
    public Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start() // 🔥 UPRAVENO: Načtení nastavení a automatické hledání targetu
    {
        isDynamicCamera = PlayerPrefs.GetInt("DynamicCamera", 1) == 1;

        // Pokud není target přiřazen ručně v Inspektoru, zkusíme ho najít podle tagu
        if (lookAheadTarget == null)
        {
            GameObject foundTarget = GameObject.FindGameObjectWithTag("LookAheadTarget");
            if (foundTarget != null)
            {
                lookAheadTarget = foundTarget.transform;
            }
            else
            {
                Debug.LogWarning("PlayerMovement: Objekt s tagem 'LookAheadTarget' nebyl nalezen ve scéně. Kamera se nebude posouvat dopředu.");
            }
        }
    }

    void Update()
    {
        // Pokud dashujeme, ignorujeme input
        if (isDashing) return;

        // Načtení pohybu
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.Normalize();

        // Kontrola staminy a spuštění Dashe
        if (Input.GetKeyDown(KeyCode.Space) && movement != Vector2.zero)
        {
            // Zeptáme se Banky (PlayerStats), jestli máme dost energie
            if (PlayerStats.Instance != null && PlayerStats.Instance.HasStamina(dashEnergyCost))
            {
                StartCoroutine(Dash());
            }
        }

        UpdateAnimations();
        UpdateLookAheadTarget(); // 🔥 PŘIDÁNO: Aktualizace pozice cíle
    }

    void FixedUpdate()
    {
        // Hýbeme hráčem pouze pokud nedashuje
        if (!isDashing)
        {
            rb.velocity = movement * moveSpeed;
        }
    }

    // 🔥 PŘIDÁNO: Logika pro posun cíle kamery
    private void UpdateLookAheadTarget()
    {
        if (lookAheadTarget == null) return;

        // POKUD JE VYPNUTO: Cíl se okamžitě zamkne na pozici hráče (žádné zpoždění, žádné houpání)
        if (!isDynamicCamera)
        {
            lookAheadTarget.position = transform.position;
            return;
        }

        // POKUD JE ZAPNUTO: Klasické posouvání dopředu
        Vector3 targetPosition = transform.position;

        if (movement.magnitude > 0.1f)
        {
            targetPosition += (Vector3)(movement * lookDistance);
        }

        // Plynulý dojezd cíle na vypočítanou pozici
        lookAheadTarget.position = Vector3.Lerp(lookAheadTarget.position, targetPosition, lookSmoothSpeed * Time.deltaTime);
    }

    // 🔥 PŘIDÁNO: Funkce pro UI Toggle v nastavení
    public void SetDynamicCamera(bool isOn)
    {
        isDynamicCamera = isOn;
        PlayerPrefs.SetInt("DynamicCamera", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    IEnumerator Dash()
    {
        isDashing = true;

        // --- 🔥 AUDIO START: Přehrajeme zvuk dashe ---
        if (!dashSound.IsNull)
        {
            // Hrajeme zvuk na pozici hráče (pro případný 3D efekt)
            RuntimeManager.PlayOneShot(dashSound, transform.position);
        }
        // --- AUDIO END ---

        // Vypočítáme rotaci partiklů
        if (startDashParticle != null && movement != Vector2.zero)
        {
            float angle = Mathf.Atan2(movement.y, movement.x);
            angle = angle * Mathf.Rad2Deg;
            startDashParticle.transform.rotation = Quaternion.Euler(angle - 0f, -90f, 90f);
        }

        // Spuštění particle efektu
        if (startDashParticle != null)
        {
            startDashParticle.Play();
        }

        // Utratíme staminu
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.UseStamina(dashEnergyCost);
        }

        // Aplikace rychlosti pro dash
        rb.velocity = movement * dashSpeed;

        // Efekt (Feedbacks)
        if (dashFeedback != null)
        {
            dashFeedback.PlayFeedbacks();
        }

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        // Na konci dashe zastavíme setrvačnost (pokud stále běžíme)
        rb.velocity = Vector2.zero;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    // DŮLEŽITÁ OPRAVA PRO KNOCKBACK
    private void OnDisable()
    {
        // 1. Okamžitě zastavíme Dash coroutinu, aby nepřepsala fyziku odhození
        StopAllCoroutines();

        // 2. Resetujeme stav, abychom po zapnutí nebyly zaseklí v "isDashing"
        isDashing = false;
    }
}