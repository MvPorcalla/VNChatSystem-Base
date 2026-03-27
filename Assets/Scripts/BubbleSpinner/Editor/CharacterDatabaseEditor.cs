#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using BubbleSpinner.Data;

[CustomEditor(typeof(CharacterDatabase))]
public class CharacterDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CharacterDatabase db = (CharacterDatabase)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Auto-Find All Characters"))
        {
            AutoFind(db);
        }

        if (GUILayout.Button("Clear List"))
        {
            Undo.RecordObject(db, "Clear Characters");
            db.allCharacters.Clear();
            EditorUtility.SetDirty(db);
        }
    }

    private void AutoFind(CharacterDatabase db)
    {
        Undo.RecordObject(db, "Auto Find Characters");

        db.allCharacters.Clear();

        string[] guids = AssetDatabase.FindAssets("t:ConversationAsset");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ConversationAsset>(path);

            if (asset != null)
                db.allCharacters.Add(asset);
        }

        EditorUtility.SetDirty(db);

        Debug.Log($"[CharacterDatabase] Found {db.allCharacters.Count} characters");
    }
}
#endif