#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using BubbleSpinner.Data;

[CustomEditor(typeof(ConversationAsset))]
public class ConversationAssetEditor : Editor
{
    // ══════════════════════════════════════════════
    // COLORS
    // ══════════════════════════════════════════════

    private static readonly Color BG_DARK        = new Color(0.18f, 0.18f, 0.18f);
    private static readonly Color BG_CARD        = new Color(0.22f, 0.22f, 0.22f);
    private static readonly Color BG_ENTRY       = new Color(0.18f, 0.28f, 0.42f);
    private static readonly Color BG_ENTRY_HDR   = new Color(0.22f, 0.38f, 0.60f);
    private static readonly Color BG_FIELD       = new Color(0.14f, 0.14f, 0.14f);
    private static readonly Color BG_BADGE       = new Color(0.28f, 0.28f, 0.28f);
    private static readonly Color BG_BTN_ADD     = new Color(0.20f, 0.20f, 0.20f);

    private static readonly Color BORDER_DEFAULT = new Color(0.30f, 0.30f, 0.30f);
    private static readonly Color BORDER_ENTRY   = new Color(0.35f, 0.52f, 0.75f);
    private static readonly Color BORDER_DASHED  = new Color(0.35f, 0.35f, 0.35f);

    private static readonly Color TEXT_PRIMARY   = new Color(0.92f, 0.92f, 0.92f);
    private static readonly Color TEXT_SECONDARY = new Color(0.55f, 0.55f, 0.55f);
    private static readonly Color TEXT_BADGE     = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color TEXT_ENTRY_LBL = new Color(0.55f, 0.80f, 1.00f);
    private static readonly Color TEXT_ALWAYS    = new Color(0.55f, 0.80f, 1.00f);
    private static readonly Color TEXT_AUTO      = new Color(0.45f, 0.45f, 0.45f);
    private static readonly Color TEXT_SECTION   = new Color(0.60f, 0.60f, 0.60f);
    private static readonly Color TEXT_HINT      = new Color(0.40f, 0.40f, 0.40f);

    private static readonly Color DOT_GREEN      = new Color(0.25f, 0.75f, 0.35f);
    private static readonly Color DOT_EMPTY      = new Color(0.35f, 0.35f, 0.35f);
    private static readonly Color BTN_X_BG       = new Color(0.28f, 0.28f, 0.28f);
    private static readonly Color BTN_X_TEXT     = new Color(0.70f, 0.70f, 0.70f);
    private static readonly Color DIVIDER_LINE   = new Color(0.28f, 0.28f, 0.28f);
    private static readonly Color DRAG_HANDLE    = new Color(0.38f, 0.38f, 0.38f);

    // ══════════════════════════════════════════════
    // STYLES (lazy init)
    // ══════════════════════════════════════════════

    private GUIStyle _styleFieldText;
    private GUIStyle _styleLabel;
    private GUIStyle _styleLabelSecondary;
    private GUIStyle _styleBadge;
    private GUIStyle _styleAuto;
    private GUIStyle _styleSectionLabel;
    private GUIStyle _styleHint;
    private GUIStyle _styleAlwaysFirst;
    private GUIStyle _styleEntryLabel;
    private GUIStyle _styleDragHandle;
    private GUIStyle _styleDividerLabel;
    private GUIStyle _styleAddBtn;
    private GUIStyle _styleXBtn;
    private bool _stylesInit;

    // ══════════════════════════════════════════════
    // STATE
    // ══════════════════════════════════════════════

    private int tabIndex;
    private string searchFilter = "";
    private SerializedProperty profileImageProp;
    private SerializedProperty conversationIdProp;
    private ReorderableList cgList;
    private int removeIndex = -1;

    // ══════════════════════════════════════════════
    // ENABLE
    // ══════════════════════════════════════════════

    private void OnEnable()
    {
        profileImageProp    = serializedObject.FindProperty("profileImage");
        conversationIdProp  = serializedObject.FindProperty("conversationId");

        var asset = (ConversationAsset)target;

        cgList = new ReorderableList(
            asset.cgAddressableKeys,
            typeof(string),
            true, true, true, true
        );

        cgList.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "CG Addressable Keys");

        cgList.drawElementCallback = (rect, index, active, focused) =>
        {
            rect.y += 2;
            asset.cgAddressableKeys[index] =
                EditorGUI.TextField(rect, asset.cgAddressableKeys[index]);
        };
    }

    // ══════════════════════════════════════════════
    // STYLE INIT
    // ══════════════════════════════════════════════

    private void EnsureStyles()
    {
        if (_stylesInit) return;
        _stylesInit = true;

        _styleFieldText = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_PRIMARY },
            fontSize  = 12,
            alignment = TextAnchor.MiddleLeft,
            padding   = new RectOffset(8, 8, 0, 0)
        };

        _styleLabel = new GUIStyle(EditorStyles.label)
        {
            normal   = { textColor = TEXT_PRIMARY },
            fontSize = 12
        };

        _styleLabelSecondary = new GUIStyle(EditorStyles.label)
        {
            normal   = { textColor = TEXT_SECONDARY },
            fontSize = 11
        };

        _styleBadge = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_BADGE },
            fontSize  = 11,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft,
            padding   = new RectOffset(8, 8, 0, 0)
        };

        _styleAuto = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_AUTO },
            fontSize  = 10,
            alignment = TextAnchor.MiddleLeft
        };

        _styleSectionLabel = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_SECTION },
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };

        _styleHint = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_HINT },
            fontSize  = 10,
            wordWrap  = true,
            alignment = TextAnchor.MiddleLeft
        };

        _styleAlwaysFirst = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_ALWAYS },
            fontSize  = 11,
            alignment = TextAnchor.MiddleRight
        };

        _styleEntryLabel = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_ENTRY_LBL },
            fontSize  = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding   = new RectOffset(8, 8, 0, 0)
        };

        _styleDragHandle = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = DRAG_HANDLE },
            fontSize  = 13,
            alignment = TextAnchor.MiddleCenter
        };

        _styleDividerLabel = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_SECTION },
            fontSize  = 9,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        _styleAddBtn = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_PRIMARY },
            fontSize  = 12,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Normal
        };

        _styleXBtn = new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = BTN_X_TEXT },
            fontSize  = 12,
            alignment = TextAnchor.MiddleCenter
        };
    }

    // ══════════════════════════════════════════════
    // MAIN DRAW
    // ══════════════════════════════════════════════

    public override void OnInspectorGUI()
    {
        EnsureStyles();

        var asset = (ConversationAsset)target;
        serializedObject.Update();

        DrawHeader(asset);
        DrawTabs();

        EditorGUILayout.Space(10);

        switch (tabIndex)
        {
            case 0: DrawChapters(asset); break;
            case 1: DrawProfile(asset);  break;
            case 2: DrawCG(asset);       break;
            case 3: DrawTools(asset);    break;
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(asset);
    }

    // ══════════════════════════════════════════════
    // HEADER
    // ══════════════════════════════════════════════

    private void DrawHeader(ConversationAsset asset)
    {
        EditorGUILayout.Space(6);

        // Header row: CA icon + name + ScriptableObject label
        Rect headerRect = EditorGUILayout.GetControlRect(false, 36);
        EditorGUI.DrawRect(headerRect, BG_DARK);

        // CA badge
        Rect badgeRect = new Rect(headerRect.x + 6, headerRect.y + 6, 26, 24);
        EditorGUI.DrawRect(badgeRect, BG_ENTRY_HDR);
        EditorGUI.LabelField(badgeRect, "CA", new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = new Color(0.75f, 0.90f, 1f) },
            fontSize  = 10,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        });

        // Character name
        Rect nameRect = new Rect(badgeRect.xMax + 8, headerRect.y, headerRect.width - 160, headerRect.height);
        string displayName = string.IsNullOrEmpty(asset.characterName)
            ? "Unnamed Character"
            : $"{asset.characterName} (ConversationAsset)";
        EditorGUI.LabelField(nameRect, displayName, new GUIStyle(EditorStyles.boldLabel)
        {
            normal   = { textColor = TEXT_PRIMARY },
            fontSize = 13,
            alignment = TextAnchor.MiddleLeft
        });

        // ScriptableObject label
        Rect soRect = new Rect(headerRect.xMax - 120, headerRect.y, 114, headerRect.height);
        EditorGUI.LabelField(soRect, "ScriptableObject", new GUIStyle(EditorStyles.label)
        {
            normal    = { textColor = TEXT_SECONDARY },
            fontSize  = 10,
            alignment = TextAnchor.MiddleRight
        });

        EditorGUILayout.Space(6);
    }

    // ══════════════════════════════════════════════
    // TABS
    // ══════════════════════════════════════════════

    private void DrawTabs()
    {
        tabIndex = GUILayout.Toolbar(tabIndex, new[] { "Chapters", "Profile", "CG", "Tools" });
    }

    // ══════════════════════════════════════════════
    // CHAPTERS TAB
    // ══════════════════════════════════════════════

    private void DrawChapters(ConversationAsset asset)
    {
        // ── CHARACTER section ──────────────────────
        DrawSectionLabel("CHARACTER");
        EditorGUILayout.Space(4);

        DrawDarkFieldRow("Character name",   asset.characterName,
            v => asset.characterName = v);

        DrawReadOnlyFieldRow("Conversation ID",
            serializedObject.FindProperty("conversationId")?.stringValue ?? "");

        EditorGUILayout.Space(12);

        // ── CHAPTERS section ───────────────────────
        DrawSectionLabel("CHAPTERS");
        EditorGUILayout.Space(6);

        // Entry point card
        DrawEntryPointCard(asset);

        EditorGUILayout.Space(6);

        // Divider
        DrawRegistryDivider();

        EditorGUILayout.Space(6);

        // Search
        Rect searchRect = EditorGUILayout.GetControlRect(false, 22);
        EditorGUI.DrawRect(searchRect, BG_FIELD);
        DrawBorder(searchRect, BORDER_DEFAULT, 1);
        searchFilter = EditorGUI.TextField(searchRect, searchFilter, new GUIStyle(EditorStyles.label)
        {
            normal  = { textColor = TEXT_PRIMARY },
            fontSize = 11,
            padding  = new RectOffset(8, 8, 0, 0),
            alignment = TextAnchor.MiddleLeft
        });
        if (string.IsNullOrEmpty(searchFilter))
        {
            EditorGUI.LabelField(searchRect, "  Search chapter ID...", new GUIStyle(EditorStyles.label)
            {
                normal    = { textColor = TEXT_SECONDARY },
                fontSize  = 11,
                alignment = TextAnchor.MiddleLeft
            });
        }

        EditorGUILayout.Space(6);

        // Registry entries (skip index 0 — that's the entry point)
        removeIndex = -1;

        for (int i = 1; i < asset.chapters.Count; i++)
        {
            var entry = asset.chapters[i];
            if (!MatchesSearch(entry, i)) continue;
            DrawRegistryEntry(asset, entry, i);
            EditorGUILayout.Space(4);
        }

        if (removeIndex >= 0)
        {
            asset.chapters.RemoveAt(removeIndex);
            GUI.changed = true;
        }

        EditorGUILayout.Space(4);

        // Add chapter button
        DrawAddChapterButton(asset);

        EditorGUILayout.Space(8);

        // Hint
        EditorGUILayout.LabelField(
            "Key is read automatically from the first title: in the file. " +
            "Drag to reorder registry entries — order has no effect on gameplay.",
            _styleHint
        );

        EditorGUILayout.Space(4);
    }

    // ── Entry point card ────────────────────────────

    private void DrawEntryPointCard(ConversationAsset asset)
    {
        // Ensure index 0 exists
        if (asset.chapters.Count == 0)
            asset.chapters.Add(new ConversationAsset.ChapterEntry());

        var entry = asset.chapters[0];

        float cardHeight = 66f;
        Rect cardRect = EditorGUILayout.GetControlRect(false, cardHeight);
        EditorGUI.DrawRect(cardRect, BG_ENTRY);
        DrawBorder(cardRect, BORDER_ENTRY, 1);

        // Header row
        Rect hdrRect = new Rect(cardRect.x, cardRect.y, cardRect.width, 26);
        EditorGUI.DrawRect(hdrRect, BG_ENTRY_HDR);

        // "Entry point" badge
        Rect lblRect = new Rect(hdrRect.x + 8, hdrRect.y, 90, hdrRect.height);
        EditorGUI.LabelField(lblRect, "Entry point", _styleEntryLabel);

        // "Always loads first"
        Rect alwaysRect = new Rect(hdrRect.xMax - 120, hdrRect.y, 112, hdrRect.height);
        EditorGUI.LabelField(alwaysRect, "Always loads first", _styleAlwaysFirst);

        // File row
        Rect fileRowY = new Rect(cardRect.x, hdrRect.yMax, cardRect.width, cardRect.height - hdrRect.height);

        Rect fileLabelRect = new Rect(fileRowY.x + 10, fileRowY.y, 36, fileRowY.height);
        EditorGUI.LabelField(fileLabelRect, "File", _styleLabelSecondary);

        Rect fileFieldRect = new Rect(fileLabelRect.xMax + 4, fileRowY.y + 5, fileRowY.xMax - fileLabelRect.xMax - 14, 22);
        DrawFileField(fileFieldRect, entry, isEntryPoint: true);

        // Accept drag
        entry.file = (TextAsset)EditorGUI.ObjectField(
            new Rect(fileFieldRect.x, fileFieldRect.y, fileFieldRect.width, fileFieldRect.height),
            GUIContent.none,
            entry.file,
            typeof(TextAsset),
            false
        );

        // Auto-fill chapterId on assign
        if (entry.file != null && string.IsNullOrEmpty(entry.chapterId))
            entry.chapterId = ReadFirstTitleFromBub(entry.file);
    }

    // ── Registry entry row ──────────────────────────

    private void DrawRegistryEntry(ConversationAsset asset, ConversationAsset.ChapterEntry entry, int index)
    {
        float rowHeight = 62f;
        Rect rowRect = EditorGUILayout.GetControlRect(false, rowHeight);
        EditorGUI.DrawRect(rowRect, BG_CARD);
        DrawBorder(rowRect, BORDER_DEFAULT, 1);

        // Top row: drag handle + badge + auto + X
        Rect topRow = new Rect(rowRect.x, rowRect.y, rowRect.width, 28);

        // Drag handle
        Rect handleRect = new Rect(topRow.x + 6, topRow.y, 16, topRow.height);
        EditorGUI.LabelField(handleRect, "⠿", _styleDragHandle);

        // Badge background
        float badgeWidth = Mathf.Max(90, GUI.skin.label.CalcSize(new GUIContent(entry.chapterId ?? "")).x + 24);
        Rect badgeRect = new Rect(handleRect.xMax + 4, topRow.y + 5, badgeWidth, 18);
        EditorGUI.DrawRect(badgeRect, BG_BADGE);
        DrawRoundBorder(badgeRect, BORDER_DEFAULT, 1);
        EditorGUI.LabelField(badgeRect, entry.chapterId ?? "", _styleBadge);

        // "auto" label
        Rect autoRect = new Rect(badgeRect.xMax + 6, topRow.y, 30, topRow.height);
        EditorGUI.LabelField(autoRect, "auto", _styleAuto);

        // X button
        Rect xRect = new Rect(rowRect.xMax - 32, topRow.y + 4, 24, 20);
        EditorGUI.DrawRect(xRect, BTN_X_BG);
        DrawBorder(xRect, BORDER_DEFAULT, 1);
        if (GUI.Button(xRect, "✕", _styleXBtn))
            removeIndex = index;

        // Bottom row: File label + file field
        Rect botRow = new Rect(rowRect.x, topRow.yMax, rowRect.width, rowRect.height - topRow.height);

        Rect fileLabelRect = new Rect(botRow.x + 10, botRow.y, 36, botRow.height);
        EditorGUI.LabelField(fileLabelRect, "File", _styleLabelSecondary);

        Rect fileFieldRect = new Rect(fileLabelRect.xMax + 4, botRow.y + 5, botRow.xMax - fileLabelRect.xMax - 14, 22);
        DrawFileField(fileFieldRect, entry, isEntryPoint: false);

        // Object field overlay (transparent, just for drag/pick)
        var newFile = (TextAsset)EditorGUI.ObjectField(
            new Rect(fileFieldRect.x, fileFieldRect.y, fileFieldRect.width, fileFieldRect.height),
            GUIContent.none,
            entry.file,
            typeof(TextAsset),
            false
        );

        if (newFile != entry.file)
        {
            entry.file = newFile;
            if (entry.file != null && string.IsNullOrEmpty(entry.chapterId))
                entry.chapterId = ReadFirstTitleFromBub(entry.file);
        }
    }

    // ── File field (filled or empty/dashed) ─────────

    private void DrawFileField(Rect rect, ConversationAsset.ChapterEntry entry, bool isEntryPoint)
    {
        bool hasFile = entry.file != null;

        if (hasFile)
        {
            EditorGUI.DrawRect(rect, BG_FIELD);
            DrawBorder(rect, BORDER_DEFAULT, 1);

            // Green dot
            Rect dotRect = new Rect(rect.x + 8, rect.y + (rect.height - 8) / 2, 8, 8);
            EditorGUI.DrawRect(dotRect, DOT_GREEN);

            // Filename
            Rect nameRect = new Rect(dotRect.xMax + 6, rect.y, rect.xMax - dotRect.xMax - 8, rect.height);
            EditorGUI.LabelField(nameRect, entry.file.name + ".bub", _styleFieldText);
        }
        else
        {
            // Dashed empty state
            EditorGUI.DrawRect(rect, BG_FIELD);
            DrawDashedBorder(rect, BORDER_DASHED);

            // Empty dot
            Rect dotRect = new Rect(rect.x + 8, rect.y + (rect.height - 8) / 2, 8, 8);
            EditorGUI.DrawRect(dotRect, DOT_EMPTY);

            Rect hintRect = new Rect(dotRect.xMax + 6, rect.y, rect.xMax - dotRect.xMax - 8, rect.height);
            EditorGUI.LabelField(hintRect, "None — drag .bub file here", new GUIStyle(_styleFieldText)
            {
                normal = { textColor = TEXT_SECONDARY }
            });
        }
    }

    // ── Registry divider ────────────────────────────

    private void DrawRegistryDivider()
    {
        Rect divRect = EditorGUILayout.GetControlRect(false, 16);

        float lineY = divRect.y + divRect.height / 2;
        float labelW = 230;
        float labelX = divRect.x + (divRect.width - labelW) / 2;

        // Left line
        Rect leftLine = new Rect(divRect.x, lineY, labelX - divRect.x - 4, 1);
        EditorGUI.DrawRect(leftLine, DIVIDER_LINE);

        // Label
        Rect labelRect = new Rect(labelX, divRect.y, labelW, divRect.height);
        EditorGUI.LabelField(labelRect, "JUMP REGISTRY — ORDER DOES NOT MATTER", _styleDividerLabel);

        // Right line
        Rect rightLine = new Rect(labelX + labelW + 4, lineY, divRect.xMax - labelX - labelW - 4, 1);
        EditorGUI.DrawRect(rightLine, DIVIDER_LINE);
    }

    // ── Add chapter button ──────────────────────────

    private void DrawAddChapterButton(ConversationAsset asset)
    {
        Rect btnRect = EditorGUILayout.GetControlRect(false, 30);
        EditorGUI.DrawRect(btnRect, BG_BTN_ADD);
        DrawBorder(btnRect, BORDER_DEFAULT, 1);

        EditorGUI.LabelField(btnRect, "+ Add chapter", _styleAddBtn);

        if (GUI.Button(btnRect, GUIContent.none, GUIStyle.none))
        {
            asset.chapters.Add(new ConversationAsset.ChapterEntry());
            GUI.changed = true;
        }
    }

    // ══════════════════════════════════════════════
    // PROFILE TAB
    // ══════════════════════════════════════════════

    private void DrawProfile(ConversationAsset asset)
    {
        DrawSectionLabel("CHARACTER PROFILE");
        EditorGUILayout.Space(6);

        DrawCard(() =>
        {
            EditorGUILayout.LabelField("Name", asset.characterName);
            asset.characterAge  = EditorGUILayout.TextField("Age", asset.characterAge);
            asset.birthdate     = EditorGUILayout.TextField("Birthdate", asset.birthdate);
            asset.occupation    = EditorGUILayout.TextField("Occupation", asset.occupation);
            asset.relationshipStatus =
                (RelationshipStatus)EditorGUILayout.EnumPopup("Relationship", asset.relationshipStatus);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Bio");
            asset.bio = EditorGUILayout.TextArea(asset.bio, GUILayout.Height(60));

            EditorGUILayout.LabelField("Description");
            asset.description = EditorGUILayout.TextArea(asset.description, GUILayout.Height(90));

            EditorGUILayout.LabelField("Personality Traits");
            asset.personalityTraits = EditorGUILayout.TextArea(asset.personalityTraits, GUILayout.Height(50));
        });
    }

    // ══════════════════════════════════════════════
    // CG TAB
    // ══════════════════════════════════════════════

    private void DrawCG(ConversationAsset asset)
    {
        DrawSectionLabel("CG GALLERY");
        EditorGUILayout.Space(6);

        DrawCard(() =>
        {
            cgList.DoLayoutList();
            if (GUILayout.Button("Clear All CG"))
                asset.cgAddressableKeys.Clear();
        });
    }

    // ══════════════════════════════════════════════
    // TOOLS TAB
    // ══════════════════════════════════════════════

    private void DrawTools(ConversationAsset asset)
    {
        DrawSectionLabel("DEVELOPER TOOLS");
        EditorGUILayout.Space(6);

        DrawCard(() =>
        {
            if (GUILayout.Button("Auto-fill Chapter IDs"))
            {
                foreach (var entry in asset.chapters)
                {
                    if (entry.file != null && string.IsNullOrEmpty(entry.chapterId))
                        entry.chapterId = ReadFirstTitleFromBub(entry.file);
                }
                EditorUtility.SetDirty(asset);
            }

            if (GUILayout.Button("Auto-Fill CG from Folder"))
            {
                string path = EditorUtility.OpenFolderPanel("Select CG Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    FillCGFromFolder(asset, path);
                    EditorUtility.SetDirty(asset);
                }
            }

            if (GUILayout.Button("Validate Asset"))
                Validate(asset);
        });
    }

    // ══════════════════════════════════════════════
    // UI HELPERS
    // ══════════════════════════════════════════════

    private void DrawSectionLabel(string text)
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField(text, _styleSectionLabel);
        EditorGUILayout.Space(2);
    }

    private void DrawDarkFieldRow(string label, string value, System.Action<string> setter)
    {
        Rect row = EditorGUILayout.GetControlRect(false, 26);

        Rect labelRect = new Rect(row.x, row.y, 110, row.height);
        EditorGUI.LabelField(labelRect, label, _styleLabelSecondary);

        Rect fieldRect = new Rect(labelRect.xMax + 4, row.y + 2, row.xMax - labelRect.xMax - 4, 22);
        EditorGUI.DrawRect(fieldRect, BG_FIELD);
        DrawBorder(fieldRect, BORDER_DEFAULT, 1);

        string newVal = EditorGUI.TextField(fieldRect, value, new GUIStyle(EditorStyles.textField)
        {
            normal   = { textColor = TEXT_PRIMARY, background = null },
            focused  = { textColor = TEXT_PRIMARY, background = null },
            fontSize = 12,
            padding  = new RectOffset(8, 8, 0, 0),
            alignment = TextAnchor.MiddleLeft
        });

        if (newVal != value)
            setter(newVal);

        EditorGUILayout.Space(2);
    }

    private void DrawReadOnlyFieldRow(string label, string value)
    {
        Rect row = EditorGUILayout.GetControlRect(false, 26);

        Rect labelRect = new Rect(row.x, row.y, 110, row.height);
        EditorGUI.LabelField(labelRect, label, _styleLabelSecondary);

        Rect fieldRect = new Rect(labelRect.xMax + 4, row.y + 2, row.xMax - labelRect.xMax - 4, 22);
        EditorGUI.DrawRect(fieldRect, BG_FIELD);
        DrawBorder(fieldRect, BORDER_DEFAULT, 1);
        EditorGUI.LabelField(fieldRect, value, _styleFieldText);

        EditorGUILayout.Space(2);
    }

    private void DrawCard(System.Action content)
    {
        EditorGUILayout.BeginVertical("box");
        content?.Invoke();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawBorder(Rect rect, Color color, float thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x,                  rect.y,                   rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x,                  rect.yMax - thickness,    rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x,                  rect.y,                   thickness,  rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness,   rect.y,                   thickness,  rect.height), color);
    }

    private void DrawRoundBorder(Rect rect, Color color, float thickness)
    {
        DrawBorder(rect, color, thickness);
    }

    private void DrawDashedBorder(Rect rect, Color color)
    {
        float dash = 4f, gap = 3f;
        float t = 1f;

        // Top
        for (float x = rect.x; x < rect.xMax; x += dash + gap)
            EditorGUI.DrawRect(new Rect(x, rect.y, Mathf.Min(dash, rect.xMax - x), t), color);

        // Bottom
        for (float x = rect.x; x < rect.xMax; x += dash + gap)
            EditorGUI.DrawRect(new Rect(x, rect.yMax - t, Mathf.Min(dash, rect.xMax - x), t), color);

        // Left
        for (float y = rect.y; y < rect.yMax; y += dash + gap)
            EditorGUI.DrawRect(new Rect(rect.x, y, t, Mathf.Min(dash, rect.yMax - y)), color);

        // Right
        for (float y = rect.y; y < rect.yMax; y += dash + gap)
            EditorGUI.DrawRect(new Rect(rect.xMax - t, y, t, Mathf.Min(dash, rect.yMax - y)), color);
    }

    // ══════════════════════════════════════════════
    // SEARCH
    // ══════════════════════════════════════════════

    private bool MatchesSearch(ConversationAsset.ChapterEntry entry, int index)
    {
        if (string.IsNullOrEmpty(searchFilter)) return true;

        string s        = searchFilter.ToLower();
        string id       = (entry.chapterId ?? "").ToLower();
        string fileName = entry.file != null ? entry.file.name.ToLower() : "";

        return id.Contains(s) || fileName.Contains(s) || index.ToString().Contains(s);
    }

    // ══════════════════════════════════════════════
    // LOGIC
    // ══════════════════════════════════════════════

    private string ReadFirstTitleFromBub(TextAsset file)
    {
        if (file == null) return "";
        foreach (var line in file.text.Split('\n'))
        {
            var t = line.Trim();
            if (t.StartsWith("title:"))
                return t.Substring(6).Trim();
        }
        return file.name;
    }

    private void FillCGFromFolder(ConversationAsset asset, string fullPath)
    {
        if (!fullPath.Contains("Assets"))
        {
            Debug.LogWarning("Please select a folder inside Assets.");
            return;
        }

        string projectPath = fullPath.Substring(fullPath.IndexOf("Assets"));
        string[] guids = AssetDatabase.FindAssets("t:Texture2D t:Sprite", new[] { projectPath });

        asset.cgAddressableKeys.Clear();

        foreach (string guid in guids)
            asset.cgAddressableKeys.Add(AssetDatabase.GUIDToAssetPath(guid));

        Debug.Log($"CG Auto-Fill complete: {guids.Length} items");
    }

    private void Validate(ConversationAsset asset)
    {
        bool valid = true;

        if (asset.chapters.Count == 0)
        {
            Debug.LogWarning("[ConversationAsset] No chapters assigned.");
            valid = false;
        }
        else if (asset.chapters[0].file == null)
        {
            Debug.LogWarning("[ConversationAsset] Entry point (index 0) has no file.");
            valid = false;
        }

        for (int i = 1; i < asset.chapters.Count; i++)
        {
            var c = asset.chapters[i];
            if (string.IsNullOrEmpty(c.chapterId))
            {
                Debug.LogWarning($"[ConversationAsset] Chapter {i} missing ID.");
                valid = false;
            }
            if (c.file == null)
            {
                Debug.LogWarning($"[ConversationAsset] Chapter {i} missing file.");
                valid = false;
            }
        }

        if (valid)
            Debug.Log("[ConversationAsset] ✓ Validation passed.");
    }
}
#endif