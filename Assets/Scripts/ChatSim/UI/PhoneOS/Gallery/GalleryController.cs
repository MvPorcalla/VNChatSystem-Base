// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/PhoneOS/Gallery/Controllers/GalleryController.cs
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChatSim.Core;
using BubbleSpinner.Data;
using ChatSim.Data;

namespace ChatSim.UI.PhoneOS.Gallery
{
    /// <summary>
    /// Main controller for the CG Gallery panel.
    /// Responsibilities:
    /// - Dynamically builds the gallery UI based on CharacterDatabase and save data
    /// - Handles thumbnail clicks to open fullscreen viewer
    /// - Displays overall progress of unlocked CGs
    /// - Provides editor tools for debugging and validation
    /// Attach to: GalleryController GameObject (child of GalleryPanel)
    /// </summary>
    public class GalleryController : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES
        // ═══════════════════════════════════════════════════════════
        
        [Header("Gallery UI")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private TextMeshProUGUI progressText;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject characterSectionPrefab;
        [SerializeField] private GameObject thumbnailPrefab;
        
        [Header("Character Data")]
        [Tooltip("Drag your CharacterDatabase ScriptableObject here")]
        [SerializeField] private CharacterDatabase characterDatabase;
        
        [Header("Display Options")]
        [SerializeField] private bool showLockedCGs = true;
        [SerializeField] private bool showEmptySections = false;
        [SerializeField] private Sprite lockedCGSprite;
        
        [Header("Fullscreen Viewer")]
        [SerializeField] private GalleryFullscreenViewer fullscreenViewer;
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private SaveData currentSaveData;
        
        // ═══════════════════════════════════════════════════════════
        // ░ LIFECYCLE
        // ═══════════════════════════════════════════════════════════
        
        private void OnEnable()
        {
            RefreshGallery();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Rebuild the entire gallery from save data
        /// </summary>
        public void RefreshGallery()
        {
            ClearGallery();
            
            if (characterDatabase == null)
            {
                Debug.LogError("[GalleryController] CharacterDatabase not assigned!");
                return;
            }
            
            currentSaveData = GameBootstrap.Save?.GetOrCreateSaveData();
            
            if (currentSaveData == null)
            {
                Debug.LogError("[GalleryController] Failed to load save data!");
                return;
            }
            
            var allCharacters = characterDatabase.GetAllCharacters();
            
            if (allCharacters == null || allCharacters.Count == 0)
            {
                Debug.LogWarning("[GalleryController] CharacterDatabase is empty! Use 'Auto-Find All Characters' in the database inspector.");
                return;
            }
            
            int totalUnlocked = 0;
            int totalCGs = 0;
            
            foreach (var convAsset in allCharacters)
            {
                if (convAsset == null) continue;
                
                var unlockedCGs = GetUnlockedCGsForConversation(convAsset.ConversationId);
                
                if (convAsset.cgAddressableKeys == null || convAsset.cgAddressableKeys.Count == 0)
                {
                    Debug.LogWarning($"[GalleryController] {convAsset.characterName} has no CGs defined");
                    continue;
                }
                
                if (!showEmptySections && unlockedCGs.Count == 0)
                {
                    Debug.Log($"[GalleryController] Skipping {convAsset.characterName} (0 CGs unlocked)");
                    continue;
                }
                
                totalUnlocked += unlockedCGs.Count;
                totalCGs += convAsset.cgAddressableKeys.Count;
                
                CreateCharacterSection(convAsset, unlockedCGs);
            }
            
            UpdateProgressDisplay(totalUnlocked, totalCGs);
            
            Debug.Log($"[GalleryController] Gallery refreshed: {totalUnlocked}/{totalCGs} CGs unlocked from {allCharacters.Count} characters");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CHARACTER SECTION CREATION
        // ═══════════════════════════════════════════════════════════
        
        private void CreateCharacterSection(ConversationAsset convAsset, HashSet<string> unlockedCGs)
        {
            if (characterSectionPrefab == null || contentContainer == null)
            {
                Debug.LogError("[GalleryController] Missing characterSectionPrefab or contentContainer!");
                return;
            }
            
            GameObject section = Instantiate(characterSectionPrefab, contentContainer);
            spawnedObjects.Add(section);
            
            TextMeshProUGUI headerText = section.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (headerText != null)
            {
                int unlocked = unlockedCGs.Count;
                int total = convAsset.cgAddressableKeys.Count;
                headerText.text = $"{convAsset.characterName} — {unlocked}/{total}";
            }
            
            Transform gridContainer = section.transform.GetChild(1);
            
            foreach (string cgKey in convAsset.cgAddressableKeys)
            {
                bool isUnlocked = unlockedCGs.Contains(cgKey);
                
                if (!showLockedCGs && !isUnlocked)
                    continue;
                
                CreateThumbnail(cgKey, isUnlocked, gridContainer);
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ THUMBNAIL CREATION
        // ═══════════════════════════════════════════════════════════
        
        private void CreateThumbnail(string cgKey, bool isUnlocked, Transform parent)
        {
            if (thumbnailPrefab == null)
            {
                Debug.LogError("[GalleryController] Missing thumbnailPrefab!");
                return;
            }
            
            GameObject thumbnail = Instantiate(thumbnailPrefab, parent);
            spawnedObjects.Add(thumbnail);
            
            // Setup thumbnail component
            GalleryThumbnailItem item = thumbnail.GetComponent<GalleryThumbnailItem>();
            if (item != null)
            {
                item.Initialize(cgKey, isUnlocked, lockedCGSprite, OnThumbnailClicked);
            }
            else
            {
                Debug.LogError("[GalleryController] Thumbnail prefab missing GalleryThumbnailItem component!");
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ THUMBNAIL CLICK HANDLER
        // ═══════════════════════════════════════════════════════════
        
        private void OnThumbnailClicked(string cgKey, Sprite sprite)
        {
            if (fullscreenViewer == null)
            {
                Debug.LogWarning("[GalleryController] FullscreenCGViewer not assigned!");
                return;
            }
            
            string cgName = System.IO.Path.GetFileName(cgKey);
            
            fullscreenViewer.Show(sprite, cgName);
            
            Debug.Log($"[GalleryController] Opened fullscreen: {cgKey}");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ SAVE DATA QUERIES
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Get all unlocked CGs for a specific conversation from save data
        /// </summary>
        private HashSet<string> GetUnlockedCGsForConversation(string conversationId)
        {
            var unlockedSet = new HashSet<string>();
            
            if (currentSaveData?.conversationStates == null)
                return unlockedSet;
            
            foreach (var convState in currentSaveData.conversationStates)
            {
                if (convState.conversationId == conversationId)
                {
                    if (convState.unlockedCGs != null)
                    {
                        foreach (string cgKey in convState.unlockedCGs)
                        {
                            unlockedSet.Add(cgKey);
                        }
                    }
                    break;
                }
            }
            
            return unlockedSet;
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ PROGRESS DISPLAY
        // ═══════════════════════════════════════════════════════════
        
        private void UpdateProgressDisplay(int unlocked, int total)
        {
            if (progressText == null) return;
            
            float percentage = total > 0 ? (unlocked / (float)total) * 100f : 0f;
            progressText.text = $"{unlocked}/{total} ({percentage:F0}%)";
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CLEANUP
        // ═══════════════════════════════════════════════════════════
        
        private void ClearGallery()
        {
            foreach (var obj in spawnedObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            spawnedObjects.Clear();
        }
        
        private void OnDisable()
        {
            // Optional: Clear gallery when panel is hidden to free memory
            // ClearGallery();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ EDITOR TOOLS
        // ═══════════════════════════════════════════════════════════
        
        #if UNITY_EDITOR
        [ContextMenu("Debug/Refresh Gallery")]
        private void DebugRefresh()
        {
            RefreshGallery();
            Debug.Log("✓ Gallery manually refreshed");
        }
        
        [ContextMenu("Debug/Print Gallery Stats")]
        private void DebugPrintStats()
        {
            if (characterDatabase == null)
            {
                Debug.LogError("CharacterDatabase not assigned!");
                return;
            }
            
            var saveData = GameBootstrap.Save?.GetOrCreateSaveData();
            if (saveData == null) return;
            
            Debug.Log("╔═══════════════ GALLERY STATS ═══════════════╗");
            
            var allCharacters = characterDatabase.GetAllCharacters();
            
            foreach (var convAsset in allCharacters)
            {
                if (convAsset == null) continue;
                
                var unlocked = GetUnlockedCGsForConversation(convAsset.ConversationId);
                int total = convAsset.cgAddressableKeys?.Count ?? 0;
                float percentage = total > 0 ? (unlocked.Count / (float)total) * 100f : 0f;
                
                Debug.Log($"║ {convAsset.characterName}");
                Debug.Log($"║   {unlocked.Count}/{total} ({percentage:F1}%)");
                
                if (unlocked.Count > 0)
                {
                    Debug.Log($"║   Unlocked: {string.Join(", ", unlocked)}");
                }
                
                Debug.Log("╠═════════════════════════════════════════════╣");
            }
            
            Debug.Log("╚═════════════════════════════════════════════╝");
        }
        
        [ContextMenu("Debug/Validate References")]
        private void DebugValidateReferences()
        {
            Debug.Log("╔═══════ VALIDATING GALLERY REFERENCES ═══════╗");
            
            bool allValid = true;
            
            if (contentContainer == null)
            {
                Debug.LogError("║ ❌ Content Container not assigned!");
                allValid = false;
            }
            
            if (characterSectionPrefab == null)
            {
                Debug.LogError("║ ❌ Character Section Prefab not assigned!");
                allValid = false;
            }
            
            if (thumbnailPrefab == null)
            {
                Debug.LogError("║ ❌ Thumbnail Prefab not assigned!");
                allValid = false;
            }
            
            if (fullscreenViewer == null)
            {
                Debug.LogWarning("║ ⚠️ Fullscreen Viewer not assigned!");
            }
            
            if (characterDatabase == null)
            {
                Debug.LogError("║ ❌ CharacterDatabase not assigned!");
                allValid = false;
            }
            else
            {
                var allCharacters = characterDatabase.GetAllCharacters();
                Debug.Log($"║ ✓ CharacterDatabase assigned ({allCharacters.Count} characters)");
                
                for (int i = 0; i < allCharacters.Count; i++)
                {
                    var asset = allCharacters[i];
                    if (asset == null)
                    {
                        Debug.LogError($"║   [{i}] ❌ NULL reference in database!");
                        allValid = false;
                    }
                    else
                    {
                        int cgCount = asset.cgAddressableKeys?.Count ?? 0;
                        Debug.Log($"║   [{i}] ✓ {asset.characterName} ({cgCount} CGs)");
                    }
                }
            }
            
            Debug.Log("╚═════════════════════════════════════════════╝");
            
            if (allValid)
            {
                Debug.Log("✓ All references valid!");
            }
        }
        #endif
    }
}