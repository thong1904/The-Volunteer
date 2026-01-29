using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
[CustomEditor(typeof(TrashItemSO))]
public class TrashItemSOEditor : Editor
{
    TrashItemSO item;

    const int CELL_SIZE = 26;
void OnEnable()
{
    item = (TrashItemSO)target;
    EnsureShapeMask();
    CleanupAuraCells(); // ✅ QUAN TRỌNG
}


   public override void OnInspectorGUI()
{
    serializedObject.Update();

    CleanupAuraCells(); // ✅ MỖI LẦN VẼ LÀ DỌN RÁC

    DrawPropertiesExcluding(
        serializedObject,
        "shapeMask",
        "auraCells"
    );

    GUILayout.Space(10);
    DrawShapeEditor();

    GUILayout.Space(15);
    DrawAuraEditor();

    serializedObject.ApplyModifiedProperties();

    if (GUI.changed)
        EditorUtility.SetDirty(item);
}

    // ======================================================
    // SHAPE
    // ======================================================

    void EnsureShapeMask()
    {
        int need = item.size.x * item.size.y;
        if (need <= 0) return;

        while (item.shapeMask.Count < need)
            item.shapeMask.Add(true);

        while (item.shapeMask.Count > need)
            item.shapeMask.RemoveAt(item.shapeMask.Count - 1);
    }

    bool IsShapeCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= item.size.x || y >= item.size.y)
            return false;

        int index = y * item.size.x + x;
        if (index < 0 || index >= item.shapeMask.Count)
            return false;

        return item.shapeMask[index];
    }

    void DrawShapeEditor()
    {
        EditorGUILayout.LabelField("Item Shape", EditorStyles.boldLabel);
        EnsureShapeMask();

        for (int y = item.size.y - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < item.size.x; x++)
            {
                int index = y * item.size.x + x;
                bool filled = item.shapeMask[index];

                GUI.backgroundColor = filled ? Color.green : Color.gray;

                if (GUILayout.Button("", GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE)))
                {
                    Undo.RecordObject(item, "Toggle Shape Cell");
                    item.shapeMask[index] = !item.shapeMask[index];
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;
    }

    // ======================================================
    // AURA
    // ======================================================

    bool IsValidAuraCell(int x, int y)
    {
        if (IsShapeCell(x, y))
            return false;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                if (IsShapeCell(x + dx, y + dy))
                    return true;
            }
        }

        return false;
    }

    TrashAura GetAuraAt(Vector2Int offset)
    {
        return item.auraCells.Find(a => a.offset == offset);
    }

    void DrawAuraEditor()
    {
        EditorGUILayout.LabelField("Aura Cells (Around Shape)", EditorStyles.boldLabel);

        int minX = -1;
        int minY = -1;
        int maxX = item.size.x;
        int maxY = item.size.y;

        for (int y = maxY; y >= minY; y--)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = minX; x <= maxX; x++)
            {
                Vector2Int offset = new(x, y);
                bool isShape = IsShapeCell(x, y);
                bool validAura = IsValidAuraCell(x, y);
                var aura = GetAuraAt(offset);

                if (isShape)
                    GUI.backgroundColor = Color.green;
                else if (!validAura)
                    GUI.backgroundColor = Color.black;
                else if (aura != null)
                    GUI.backgroundColor = Color.cyan;
                else
                    GUI.backgroundColor = Color.gray;

                GUI.enabled = validAura;

                if (GUILayout.Button("", GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE)))
                {
                    Undo.RecordObject(item, "Toggle Aura Cell");

                    if (aura == null)
                    {
                        item.auraCells.Add(new TrashAura
                        {
                            offset = offset,
                            rules = new List<AuraRule>
        {
            new AuraRule
            {
                targetCategory = TrashCategory.Organic,
                type = AuraType.Add,
                value = 10,
                icon = null
            }
        }
                        });
                    }
                    else
                    {
                        item.auraCells.Remove(aura);
                    }
                }

                GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);
        DrawAuraDetails(item);
    }

    void DrawAuraDetails(TrashItemSO item)
{
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Aura Details", EditorStyles.boldLabel);

    foreach (var aura in item.auraCells)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField($"Offset {aura.offset}", EditorStyles.boldLabel);

        if (aura.rules == null)
            aura.rules = new List<AuraRule>();

        for (int i = 0; i < aura.rules.Count; i++)
        {
            var rule = aura.rules[i];

            EditorGUILayout.BeginHorizontal();

                // Icon field
                rule.icon = (UnityEngine.Sprite)EditorGUILayout.ObjectField(rule.icon, typeof(UnityEngine.Sprite), false, GUILayout.Width(40), GUILayout.Height(40));
                rule.targetCategory = (TrashCategory)EditorGUILayout.EnumPopup(rule.targetCategory);
                rule.type = (AuraType)EditorGUILayout.EnumPopup(rule.type, GUILayout.Width(90));
                rule.value = EditorGUILayout.IntField(rule.value, GUILayout.Width(50));

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                aura.rules.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("+ Add Category Rule"))
        {
            aura.rules.Add(new AuraRule
            {
                targetCategory = TrashCategory.Organic,
                type = AuraType.Add,
                value = 10
            });
        }

        EditorGUILayout.EndVertical();
    }
}

void CleanupAuraCells()
{
    for (int i = item.auraCells.Count - 1; i >= 0; i--)
    {
        var aura = item.auraCells[i];
        if (!IsValidAuraCell(aura.offset.x, aura.offset.y))
        {
            item.auraCells.RemoveAt(i);
        }
    }
}

}
