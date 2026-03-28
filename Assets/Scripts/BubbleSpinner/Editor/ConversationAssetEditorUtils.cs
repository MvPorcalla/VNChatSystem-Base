#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BubbleSpinner.Data;

namespace BubbleSpinner.EditorTools
{
    public static class Labels
    {
        // ─────────────────────────────────────────────
        // TABS
        // ─────────────────────────────────────────────
        public const string TAB_CHAPTERS = "Chapters";
        public const string TAB_PROFILE  = "Profile";
        public const string TAB_CG       = "CG Images";
        public const string TAB_TOOLS    = "Tools";

        // ─────────────────────────────────────────────
        // SECTION HEADERS
        // ─────────────────────────────────────────────
        public const string SECTION_CHARACTER = "CHARACTER";
        public const string SECTION_CHAPTERS  = "CHAPTERS";
        public const string SECTION_PROFILE   = "PROFILE";
        public const string SECTION_CG        = "CG";
        public const string SECTION_TOOLS     = "TOOLS";
    }

    public static class ConversationAssetEditorUtils
    {
        // ─────────────────────────────────────────────
        // FILE PARSING
        // ─────────────────────────────────────────────

        /// <summary>
        /// Reads the `chapter:` header from a .bub TextAsset.
        /// Returns the file name as fallback if no chapter: declaration is found.
        /// </summary>
        public static string ReadChapterIdFromBub(TextAsset file)
        {
            if (file == null) return "";

            foreach (var line in file.text.Split('\n'))
            {
                var t = System.Text.RegularExpressions.Regex.Replace(line.Trim(), @"\s+", " ");

                if (t.StartsWith("chapter :", StringComparison.OrdinalIgnoreCase))
                    return t.Substring("chapter :".Length).Trim();

                if (t.StartsWith("chapter:", StringComparison.OrdinalIgnoreCase))
                    return t.Substring("chapter:".Length).Trim();
            }

            Debug.LogWarning($"[ConversationAsset] No 'chapter:' declaration found in '{file.name}' — using file name as fallback");
            return file.name;
        }

        // ─────────────────────────────────────────────
        // FOLDER SCANNING
        // ─────────────────────────────────────────────

        /// <summary>
        /// Fills cgAddressableKeys from all Texture2D and Sprite assets in the given folder.
        /// fullPath must be inside the Assets directory.
        /// </summary>
        public static void FillCGFromFolder(ConversationAsset asset, string fullPath)
        {
            if (!fullPath.Contains("Assets"))
            {
                Debug.LogWarning("[ConversationAsset] Selected folder must be inside the Assets directory.");
                return;
            }

            string projectPath = fullPath.Substring(fullPath.IndexOf("Assets"));
            string[] guids     = AssetDatabase.FindAssets("t:Texture2D t:Sprite", new[] { projectPath });

            asset.cgAddressableKeys.Clear();

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                asset.cgAddressableKeys.Add(assetPath);
            }
        }

        // ─────────────────────────────────────────────
        // VALIDATION
        // ─────────────────────────────────────────────

        /// <summary>
        /// Validates a ConversationAsset and returns a list of warning strings.
        /// Empty list means the asset is valid.
        /// </summary>
        public static List<string> Validate(ConversationAsset asset)
        {
            var warnings = new List<string>();

            if (string.IsNullOrWhiteSpace(asset.characterName))
                warnings.Add("Character name is empty.");

            if (asset.chapters == null || asset.chapters.Count == 0)
            {
                warnings.Add("No chapters assigned.");
                return warnings; // No point checking further
            }

            if (asset.chapters[0].file == null)
                warnings.Add("Entry point chapter (index 0) has no file assigned.");

            for (int i = 0; i < asset.chapters.Count; i++)
            {
                var c = asset.chapters[i];

                if (string.IsNullOrEmpty(c.chapterId))
                    warnings.Add($"Chapter [{i}] is missing a Chapter ID.");

                if (c.file == null)
                    warnings.Add($"Chapter [{i}] \"{c.chapterId}\" has no file assigned.");
            }

            return warnings;
        }
    }
}
#endif