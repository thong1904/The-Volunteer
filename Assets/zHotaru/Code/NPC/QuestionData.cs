using System;
using UnityEngine;

[Serializable]
public class QuestionData
{
    public string questionText;
    public string[] answers = new string[4];
    public int correctAnswerIndex;
    public int pointsReward = 10;
}
