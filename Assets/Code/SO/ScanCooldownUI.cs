using UnityEngine;
using UnityEngine.UI;

public class ScanCooldownUI : MonoBehaviour
{
    public PlayerScanSkill scanSkill;
    public Image fillImage;

    void Update()
    {
        if (scanSkill == null) return;

        if (scanSkill.IsScanning)
        {
            float remain = scanSkill.ScanTimeRemaining;
            float total = scanSkill.ScanDuration;

            fillImage.fillAmount = remain / total;
        }
        else if (scanSkill.IsCooldown)
        {
            float remain = scanSkill.CooldownRemaining;
            float total = scanSkill.Cooldown;

            fillImage.fillAmount = 1f - (remain / total);
        }
        else
        {
            fillImage.fillAmount = 1f;
        }
    }
}
