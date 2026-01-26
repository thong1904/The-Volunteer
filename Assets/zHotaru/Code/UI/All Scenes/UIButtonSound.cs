using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Component để custom âm thanh cho Button cụ thể.
/// Attach script này vào Button muốn có sound khác mặc định.
/// Button có component này sẽ KHÔNG dùng sound mặc định của SoundManager.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerClickHandler
{
    [Header("Sound Settings")]
    [SerializeField] private string soundName = "confirm";
    [SerializeField, Range(0f, 1f)] private float volumeScale = 1f;
    
    private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            PlaySound();
        }
    }
    
    public void PlaySound()
    {
        if (SoundManager.Instance != null && !string.IsNullOrEmpty(soundName))
        {
            SoundManager.Instance.PlaySFX(soundName, volumeScale);
        }
    }
    
    public void SetSoundName(string newSoundName) => soundName = newSoundName;
    public void SetVolumeScale(float volume) => volumeScale = Mathf.Clamp01(volume);
}
