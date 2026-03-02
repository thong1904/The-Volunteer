using UnityEngine;
using System.Collections;

public class PlayerScanSkill : MonoBehaviour
{
    public ScanSkillSO data;
    public LayerMask trashLayer;

    [Header("Level")]
    public int level = 1; // ✅ mặc định level 1

    bool isScanning;
    bool isCooldown;

    float scanTimer;
    float cooldownTimer;

    Coroutine scanRoutine;

    // ===== RUNTIME STATS =====
    float scanRadius;
    float scanDuration;
    float cooldown;

    public bool IsScanning => isScanning;
    public bool IsCooldown => isCooldown;
    public float ScanTimeRemaining => scanTimer;
    public float CooldownRemaining => cooldownTimer;
    public float ScanDuration => scanDuration;
    public float ScanRadius => scanRadius;
    public float Cooldown => cooldown;

    public bool CanScan => !isScanning && !isCooldown;

    void Awake()
    {
        ApplyStats();
    }

    // ================= STATS =================

    void ApplyStats()
    {
        scanRadius =
            data.baseRadius + data.radiusUpgrade * (level - 1);

        scanDuration =
            data.baseDuration + data.durationUpgrade * (level - 1);

        cooldown =
            Mathf.Max(
                1f,
                data.baseCooldown - data.cooldownReduce * (level - 1)
            );
    }

    // ================= SCAN =================

    public void TryScan()
    {
        if (!CanScan) return;
        scanRoutine = StartCoroutine(ScanRoutine());
    }

    IEnumerator ScanRoutine()
    {
        // ===== SCAN =====
        isScanning = true;
        scanTimer = scanDuration;

        SetOutline(true);

        while (scanTimer > 0f)
        {
            scanTimer -= Time.deltaTime;
            ScanTrash();
            yield return null;
        }

        SetOutline(false);
        isScanning = false;

        // ===== COOLDOWN =====
        isCooldown = true;
        cooldownTimer = cooldown;

        while (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            yield return null;
        }

        isCooldown = false;
    }

    // ================= OUTLINE =================

    void ScanTrash()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            scanRadius,
            trashLayer
        );

        foreach (var hit in hits)
        {
            var outline = hit.GetComponentInChildren<TrashOutline>();
            if (outline != null)
                outline.SetScan(true);
        }
    }

    void SetOutline(bool value)
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            scanRadius,
            trashLayer
        );

        foreach (var hit in hits)
        {
            var outline = hit.GetComponentInChildren<TrashOutline>();
            if (outline != null)
                outline.SetScan(value);
        }
    }

    // ================= UPGRADE =================

    public bool CanUpgrade()
    {
        return level < data.maxLevel;
    }

    public bool Upgrade()
    {
        if (!CanUpgrade()) return false;

        level++;
        ApplyStats();
        return true;
    }
}
