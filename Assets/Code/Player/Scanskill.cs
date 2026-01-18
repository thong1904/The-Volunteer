using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ScanSkill : MonoBehaviour
{
    [Header("Scan")]
    public float scanRadius = 15f;
    public float scanDuration = 3f;
    public LayerMask pickupLayer;

    [Header("Cooldown")]
    public float cooldownTime = 8f;

    [Header("UI Layers")]
    public Image readyWhite;     // lớp trắng sáng (READY)
    public Image usedDark;       // lớp trắng tối (IN USE / COOLDOWN)
    public Image cooldownFill;   // lớp xoay tròn
    public TMP_Text cooldownText;

    bool isCooldown;

    PlayerInputActions input;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Player.Enable();
        input.Player.Scan.performed += OnScanPerformed;
    }

    void OnDisable()
    {
        input.Player.Scan.performed -= OnScanPerformed;
        input.Player.Disable();
    }

    void Start()
    {
        // ===== TRẠNG THÁI BAN ĐẦU =====
        readyWhite.gameObject.SetActive(true);
        usedDark.gameObject.SetActive(false);
        cooldownFill.gameObject.SetActive(false);
        cooldownFill.fillAmount = 0f;

        cooldownText.gameObject.SetActive(false);
    }

    void OnScanPerformed(InputAction.CallbackContext ctx)
    {
        TryActivateScan();
    }

    void TryActivateScan()
    {
        if (isCooldown) return;

        StartCoroutine(ScanAndCooldown());
    }

    IEnumerator ScanAndCooldown()
    {
        isCooldown = true;

        // ===== UI KHI DÙNG SKILL =====
        readyWhite.gameObject.SetActive(false);
        usedDark.gameObject.SetActive(true);

        cooldownFill.gameObject.SetActive(true);
        cooldownFill.fillAmount = 0f;

        cooldownText.gameObject.SetActive(true);

        // ===== SCAN =====
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            scanRadius,
            pickupLayer
        );

        foreach (Collider col in hits)
        {
            Outline outline = col.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = true;
        }

        yield return new WaitForSeconds(scanDuration);

        foreach (Collider col in hits)
        {
            Outline outline = col.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;
        }

        // ===== COOLDOWN =====
        float timer = 0f;

        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;

            float percent = timer / cooldownTime;
            float remaining = cooldownTime - timer;

            cooldownFill.fillAmount = percent;
            cooldownText.text = Mathf.Ceil(remaining).ToString();

            yield return null;
        }

        // ===== READY LẠI =====
        cooldownFill.fillAmount = 1f;
        cooldownFill.gameObject.SetActive(false);

        cooldownText.text = "";
        cooldownText.gameObject.SetActive(false);

        usedDark.gameObject.SetActive(false);
        readyWhite.gameObject.SetActive(true);

        isCooldown = false;
    }
}
