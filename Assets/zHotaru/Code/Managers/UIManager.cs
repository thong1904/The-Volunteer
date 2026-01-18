using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private Transform answersContainer;
    [SerializeField] private GameObject answerButtonPrefab;
    
    private Action<int> onAnswerSelected;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    void Start()
    {
        if (questionPanel != null)
            questionPanel.SetActive(false);
    }
    
    public void ShowQuestion(string npcName, string question, string[] answers, 
        int correctAnswerIndex, Action<int> onAnswerCallback)
    {
        if (questionPanel == null)
        {
            Debug.LogWarning("Question Panel chưa được gán!");
            return;
        }
        
        onAnswerSelected = onAnswerCallback;
        
        // Cập nhật tên NPC
        if (npcNameText != null)
            npcNameText.text = npcName;
        
        // Cập nhật câu hỏi
        if (questionText != null)
            questionText.text = question;
        
        // Xóa các nút cũ
        foreach (Transform child in answersContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Tạo các nút câu trả lời
        for (int i = 0; i < answers.Length; i++)
        {
            GameObject answerButton = Instantiate(answerButtonPrefab, answersContainer);
            
            TextMeshProUGUI answerButtonText = answerButton.GetComponentInChildren<TextMeshProUGUI>();
            if (answerButtonText != null)
                answerButtonText.text = answers[i];
            
            Button btn = answerButton.GetComponent<Button>();
            if (btn != null)
            {
                int answerIndex = i;
                btn.onClick.AddListener(() => OnAnswerSelected(answerIndex));
            }
        }
        
        // Hiển thị panel
        questionPanel.SetActive(true);
    }
    
    private void OnAnswerSelected(int answerIndex)
    {
        // Ẩn panel
        questionPanel.SetActive(false);
        
        // Gọi callback
        onAnswerSelected?.Invoke(answerIndex);
    }
    
    public void UpdateScore(int score)
    {
        // TODO: Cập nhật điểm trên UI
        Debug.Log("Điểm: " + score);
    }
}
