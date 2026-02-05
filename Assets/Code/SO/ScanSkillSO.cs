using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Scan Skill")]
public class ScanSkillSO : ScriptableObject
{
    [Header("Base Stats (Level 1)")]
    public float baseRadius = 10f;
    public float baseDuration = 4f;
    public float baseCooldown = 12f;

    [Header("Upgrade Per Level")]
    public float radiusUpgrade = 2f;
    public float durationUpgrade = 1f;
    public float cooldownReduce = 2f;

    [Header("Level")]
    public int maxLevel = 5;
}
