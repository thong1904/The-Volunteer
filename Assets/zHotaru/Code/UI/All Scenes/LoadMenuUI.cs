using UnityEngine;
using UnityEngine.UI;

public class LoadMenuUI : MonoBehaviour
{
    [Header("=== Load Buttons ===")]
    [SerializeField] private Button autoSaveButton;
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;

    [Header("=== Button Texts (Legacy Text) ===")]
    [SerializeField] private Text autoSaveText;
    [SerializeField] private Text slot1Text;
    [SerializeField] private Text slot2Text;
    [SerializeField] private Text slot3Text;

    [Header("=== Default Names ===")]
    [SerializeField] private string autoSaveDefaultName = "Auto Save";
    [SerializeField] private string slot1DefaultName = "Slot 1";
    [SerializeField] private string slot2DefaultName = "Slot 2";
    [SerializeField] private string slot3DefaultName = "Slot 3";

    [Header("=== References ===")]
    [SerializeField] private SaveLoadManager saveLoadManager;

    private void Start()
    {
        if (saveLoadManager == null)
            saveLoadManager = FindFirstObjectByType<SaveLoadManager>();

        SetupButtons();
        RefreshAllSlots();
    }

    private void OnEnable()
    {
        RefreshAllSlots();
    }

    private void SetupButtons()
    {
        autoSaveButton?.onClick.AddListener(OnAutoSaveClicked);
        slot1Button?.onClick.AddListener(() => OnSlotClicked(1));
        slot2Button?.onClick.AddListener(() => OnSlotClicked(2));
        slot3Button?.onClick.AddListener(() => OnSlotClicked(3));
    }

    #region Button Click Events
    public void OnAutoSaveClicked()
    {
        if (saveLoadManager == null) return;

        if (saveLoadManager.HasSaveData(-1))
        {
            saveLoadManager.LoadAutoSave();
        }
        else
        {
            Debug.Log("No Auto Save data found!");
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (saveLoadManager == null) return;

        if (saveLoadManager.HasSaveData(slotIndex))
        {
            saveLoadManager.LoadFromSlot(slotIndex);
        }
        else
        {
            Debug.Log($"No save data in Slot {slotIndex}!");
        }
    }

    public void OnSlot1Clicked() => OnSlotClicked(1);
    public void OnSlot2Clicked() => OnSlotClicked(2);
    public void OnSlot3Clicked() => OnSlotClicked(3);
    #endregion

    #region Refresh UI
    public void RefreshAllSlots()
    {
        if (saveLoadManager == null) return;

        RefreshSlotUI(-1, autoSaveText, autoSaveDefaultName, autoSaveButton);
        RefreshSlotUI(1, slot1Text, slot1DefaultName, slot1Button);
        RefreshSlotUI(2, slot2Text, slot2DefaultName, slot2Button);
        RefreshSlotUI(3, slot3Text, slot3DefaultName, slot3Button);
    }

    private void RefreshSlotUI(int slotIndex, Text buttonText, string defaultName, Button button)
    {
        if (buttonText == null) return;

        SaveSlotInfo info = saveLoadManager.GetSlotInfo(slotIndex);

        if (info != null && !info.isEmpty)
        {
            string displayText = FormatSlotDisplay(info);
            buttonText.text = displayText;
            
            if (button != null)
                button.interactable = true;
        }
        else
        {
            buttonText.text = defaultName;
            
            if (button != null)
                button.interactable = false;
        }
    }

    private string FormatSlotDisplay(SaveSlotInfo info)
    {
        // Format: "dd/MM/yyyy - HH:mm"
        string dateDisplay = info.saveDateTime;
        
        // Parse và format lại ngày
        if (System.DateTime.TryParse(info.saveDateTime, out System.DateTime saveDate))
        {
            dateDisplay = saveDate.ToString("dd/MM/yyyy - HH:mm");
        }

        // Format thời gian chơi
        string playTimeDisplay = FormatPlayTime(info.playTime);

        return $"{dateDisplay}\n{playTimeDisplay}";
    }

    private string FormatPlayTime(float seconds)
    {
        System.TimeSpan time = System.TimeSpan.FromSeconds(seconds);
        
        if (time.TotalHours >= 1)
        {
            return $"Đã chơi: {(int)time.TotalHours}h {time.Minutes}m";
        }
        else if (time.TotalMinutes >= 1)
        {
            return $"Đã chơi: {time.Minutes}m {time.Seconds}s";
        }
        else
        {
            return $"Đã chơi: {time.Seconds}s";
        }
    }
    #endregion

    private void OnDestroy()
    {
        autoSaveButton?.onClick.RemoveAllListeners();
        slot1Button?.onClick.RemoveAllListeners();
        slot2Button?.onClick.RemoveAllListeners();
        slot3Button?.onClick.RemoveAllListeners();
    }
}
