using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TrashAura
{
    public Vector2Int offset;

    public List<AuraRule> rules = new();
}

