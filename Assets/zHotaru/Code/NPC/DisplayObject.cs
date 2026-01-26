using UnityEngine;

public class DisplayObject : MonoBehaviour
{
    [SerializeField] private string displayName;
    [SerializeField] private QuestionData[] questions;

    public string DisplayName => displayName;

    public QuestionData GetRandomQuestion()
    {
        if (questions == null || questions.Length == 0) return null;
        return questions[Random.Range(0, questions.Length)];
    }
}
