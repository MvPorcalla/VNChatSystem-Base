#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using BubbleSpinner.Data;

namespace BubbleSpinner.EditorTools
{
    [CustomEditor(typeof(ConversationAsset))]
    public class ConversationAssetEditor : Editor
    {
        private int tabIndex;

        private SerializedProperty profileImageProp;
        private SerializedProperty conversationIdProp;
        private SerializedProperty characterNameProp;
        private SerializedProperty chaptersProp;
        private SerializedProperty cgKeysProp;

        // Profile properties
        private SerializedProperty characterAgeProp;
        private SerializedProperty birthdateProp;
        private SerializedProperty occupationProp;
        private SerializedProperty relationshipStatusProp;
        private SerializedProperty bioProp;
        private SerializedProperty descriptionProp;
        private SerializedProperty personalityTraitsProp;

        private ReorderableList chapterList;
        private ReorderableList cgList;

        // =========================
        // STYLES
        // =========================
        private GUIStyle cardStyle;
        private GUIStyle sectionStyle;
        private GUIStyle headerStyle;
        private GUIStyle subLabelStyle;

        private void InitStyles()
        {
            if (cardStyle == null)
            {
                cardStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(12, 12, 10, 10),
                    margin  = new RectOffset(0, 0, 6, 6)
                };
            }

            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize   = 11,
                    fontStyle  = FontStyle.Bold,
                    normal     = { textColor = new Color(0.7f, 0.7f, 0.7f) }
                };
            }

            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16
                };
            }

            if (subLabelStyle == null)
            {
                subLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
                };
            }
        }

        private void OnEnable()
        {
            // Core
            profileImageProp   = serializedObject.FindProperty("profileImage");
            conversationIdProp = serializedObject.FindProperty("conversationId");
            characterNameProp  = serializedObject.FindProperty("characterName");
            chaptersProp       = serializedObject.FindProperty("chapters");
            cgKeysProp         = serializedObject.FindProperty("cgAddressableKeys");

            // Profile
            characterAgeProp        = serializedObject.FindProperty("characterAge");
            birthdateProp           = serializedObject.FindProperty("birthdate");
            occupationProp          = serializedObject.FindProperty("occupation");
            relationshipStatusProp  = serializedObject.FindProperty("relationshipStatus");
            bioProp                 = serializedObject.FindProperty("bio");
            descriptionProp         = serializedObject.FindProperty("description");
            personalityTraitsProp   = serializedObject.FindProperty("personalityTraits");

            InitStyles();
            BuildChapterList();
            BuildCGList();
        }

        public override void OnInspectorGUI()
        {
            // Safety net — EditorStyles can be null on first OnEnable in some Unity versions
            if (cardStyle == null) InitStyles();

            var asset = (ConversationAsset)target;

            serializedObject.Update();

            DrawHeader(asset);
            DrawTabs();

            GUILayout.Space(10);

            switch (tabIndex)
            {
                case 0: DrawChapters(asset); break;
                case 1: DrawProfile(asset); break;
                case 2: DrawCG(asset); break;
                case 3: DrawTools(asset); break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        // =========================
        // HEADER
        // =========================
        private void DrawHeader(ConversationAsset asset)
        {
            GUILayout.Space(5);

            EditorGUILayout.LabelField(
                string.IsNullOrEmpty(asset.characterName) ? "Unnamed Character" : asset.characterName,
                headerStyle
            );

            EditorGUILayout.LabelField("Conversation Asset", subLabelStyle);

            GUILayout.Space(8);
            DrawLine();
            GUILayout.Space(8);
        }

        // =========================
        // TABS
        // =========================

        private void DrawTabs()
        {
            tabIndex = GUILayout.Toolbar(
                tabIndex,
                new string[]
                {
                    Labels.TAB_CHAPTERS,
                    Labels.TAB_PROFILE,
                    Labels.TAB_CG,
                    Labels.TAB_TOOLS
                },
                GUILayout.Height(28)
            );
        }

        // =========================
        // CHAPTERS
        // =========================

        private void DrawChapters(ConversationAsset asset)
        {
            DrawSection(Labels.SECTION_CHARACTER);

            DrawCard(() =>
            {
                EditorGUILayout.PropertyField(characterNameProp, new GUIContent("Character Name"));
                EditorGUILayout.PropertyField(profileImageProp);
                EditorGUILayout.PropertyField(conversationIdProp);
            });

            GUILayout.Space(10);
            DrawSection(Labels.SECTION_CHAPTERS);
            chapterList.DoLayoutList();
        }

        private void BuildChapterList()
        {
            chapterList = new ReorderableList(
                serializedObject,
                chaptersProp,
                draggable:    true,
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true
            );

            chapterList.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "Chapters");

            chapterList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element  = chaptersProp.GetArrayElementAtIndex(index);
                SerializedProperty idProp   = element.FindPropertyRelative("chapterId");
                SerializedProperty fileProp = element.FindPropertyRelative("file");

                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing    = EditorGUIUtility.standardVerticalSpacing;

                float y = rect.y + spacing;

                if (index == 0)
                {
                    // Row 1 — Entry Point label
                    EditorGUI.LabelField(
                        new Rect(rect.x, y, rect.width, lineHeight),
                        "Entry Point",
                        EditorStyles.boldLabel
                    );

                    y += lineHeight + spacing;

                    // Row 2 — Chapter ID
                    EditorGUI.PropertyField(
                        new Rect(rect.x, y, rect.width, lineHeight),
                        idProp,
                        new GUIContent("Chapter ID")
                    );

                    y += lineHeight + spacing;

                    // Row 3 — File
                    EditorGUI.PropertyField(
                        new Rect(rect.x, y, rect.width, lineHeight),
                        fileProp,
                        new GUIContent("File")
                    );
                }
                else
                {
                    // Row 1 — Chapter ID
                    EditorGUI.PropertyField(
                        new Rect(rect.x, y, rect.width, lineHeight),
                        idProp,
                        new GUIContent("Chapter ID")
                    );

                    y += lineHeight + spacing;

                    // Row 2 — File
                    EditorGUI.PropertyField(
                        new Rect(rect.x, y, rect.width, lineHeight),
                        fileProp,
                        new GUIContent("File")
                    );
                }
            };

            chapterList.elementHeightCallback = index =>
            {
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float spacing    = EditorGUIUtility.standardVerticalSpacing;

                if (index == 0)
                {
                    // 3 rows
                    return (lineHeight * 3) + (spacing * 4);
                }

                // 2 rows
                return (lineHeight * 2) + (spacing * 3);
            };

            chapterList.onAddCallback = list =>
            {
                Undo.RecordObject((ConversationAsset)target, "Add Chapter");
                list.serializedProperty.arraySize++;
                serializedObject.ApplyModifiedProperties();
            };

            chapterList.onRemoveCallback = list =>
            {
                // Block removal of index 0 (entry point)
                if (list.index == 0)
                {
                    EditorUtility.DisplayDialog(
                        "Cannot Remove Entry Point",
                        "The first chapter is always the entry point and cannot be removed.",
                        "OK"
                    );
                    return;
                }

                Undo.RecordObject((ConversationAsset)target, "Remove Chapter");
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                serializedObject.ApplyModifiedProperties();
            };
        }

        // =========================
        // PROFILE
        // =========================

        private void DrawProfile(ConversationAsset asset)
        {
            DrawSection(Labels.SECTION_PROFILE);

            DrawCard(() =>
            {
                // Name is read-only here — edited in Chapters tab
                EditorGUILayout.LabelField("Name", asset.characterName);

                GUILayout.Space(4);

                EditorGUILayout.PropertyField(characterAgeProp,       new GUIContent("Age"));
                EditorGUILayout.PropertyField(birthdateProp,          new GUIContent("Birthdate"));
                EditorGUILayout.PropertyField(occupationProp,         new GUIContent("Occupation"));
                EditorGUILayout.PropertyField(relationshipStatusProp, new GUIContent("Relationship"));

                GUILayout.Space(6);

                EditorGUILayout.LabelField("Bio");
                EditorGUILayout.PropertyField(bioProp, GUIContent.none);

                GUILayout.Space(4);

                EditorGUILayout.LabelField("Description");
                EditorGUILayout.PropertyField(descriptionProp, GUIContent.none);

                GUILayout.Space(4);

                EditorGUILayout.LabelField("Personality Traits");
                EditorGUILayout.PropertyField(personalityTraitsProp, GUIContent.none);
            });
        }

        // =========================
        // CG
        // =========================

        private void BuildCGList()
        {
            cgList = new ReorderableList(
                serializedObject,
                cgKeysProp,
                draggable:    true,
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true
            );

            cgList.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "CG Addressable Keys");

            cgList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = cgKeysProp.GetArrayElementAtIndex(index);

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight),
                    element,
                    GUIContent.none
                );
            };

            cgList.onAddCallback = list =>
            {
                list.serializedProperty.arraySize++;
                serializedObject.ApplyModifiedProperties();
            };

            cgList.onRemoveCallback = list =>
            {
                Undo.RecordObject((ConversationAsset)target, "Remove CG Key");
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                serializedObject.ApplyModifiedProperties();
            };
        }
        
        private void DrawCG(ConversationAsset asset)
        {
            DrawSection(Labels.SECTION_CG);

            DrawCard(() =>
            {
                cgList.DoLayoutList();
                
                if (GUILayout.Button("Clear All"))
                {
                    Undo.RecordObject(asset, "Clear CG Keys");
                    cgKeysProp.ClearArray();
                    serializedObject.ApplyModifiedProperties();
                }
            });
        }

        // =========================
        // TOOLS
        // =========================

        private void DrawTools(ConversationAsset asset)
        {
            DrawSection(Labels.SECTION_TOOLS);

            DrawCard(() =>
            {
                if (GUILayout.Button("Auto-fill Chapter IDs"))
                {
                    Undo.RecordObject(asset, "Auto-fill Chapter IDs");
                    foreach (var entry in asset.chapters)
                    {
                        if (entry.file != null && string.IsNullOrEmpty(entry.chapterId))
                            entry.chapterId = ConversationAssetEditorUtils.ReadFirstTitleFromBub(entry.file);
                    }
                    EditorUtility.SetDirty(asset);
                }

                if (GUILayout.Button("Auto-Fill CG from Folder"))
                {
                    string path = EditorUtility.OpenFolderPanel("Select CG Folder", "Assets", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        Undo.RecordObject(asset, "Auto-Fill CG from Folder");
                        ConversationAssetEditorUtils.FillCGFromFolder(asset, path);
                        EditorUtility.SetDirty(asset);
                    }
                }

                if (GUILayout.Button("Validate"))
                {
                    var warnings = ConversationAssetEditorUtils.Validate(asset);

                    if (warnings.Count == 0)
                    {
                        Debug.Log($"[ConversationAsset] '{asset.characterName}' — Validation passed. No issues found.");
                    }
                    else
                    {
                        foreach (var w in warnings)
                            Debug.LogWarning($"[ConversationAsset] '{asset.characterName}' — {w}");
                    }
                }
            });
        }

        // =========================
        // UI HELPERS
        // =========================
        private void DrawCard(System.Action content)
        {
            Rect rect = EditorGUILayout.BeginVertical(cardStyle);
            EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f));

            GUILayout.Space(4);
            content?.Invoke();
            GUILayout.Space(2);

            EditorGUILayout.EndVertical();
        }

        private void DrawSection(string title)
        {
            GUILayout.Space(6);
            EditorGUILayout.LabelField(title, sectionStyle);
            DrawLine();
            GUILayout.Space(4);
        }

        private void DrawLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.05f));
        }
    }
}
#endif