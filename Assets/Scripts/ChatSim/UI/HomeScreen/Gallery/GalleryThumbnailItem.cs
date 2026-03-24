// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/HomeScreen/Gallery/Components/GalleryThumbnailItem.cs
// Individual CG thumbnail - handles display, loading, and click events
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ChatSim.Core;

namespace ChatSim.UI.HomeScreen.Gallery
{
    /// <summary>
    /// Represents a single CG thumbnail in the gallery.
    /// Loads the thumbnail sprite from Addressables and handles click events.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class GalleryThumbnailItem : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES
        // ═══════════════════════════════════════════════════════════
        
        [Header("UI References")]
        [SerializeField] private Image thumbnailImage;
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private Button button;
        private string cgKey;
        private bool isUnlocked;
        private Sprite loadedSprite;
        private AsyncOperationHandle<Sprite> loadHandle;
        private Action<string, Sprite> onClickCallback;
        
        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════
        
        private void Awake()
        {
            button = GetComponent<Button>();
        }
        
        /// <summary>
        /// Initialize the thumbnail with CG data
        /// </summary>
        public void Initialize(
            string addressableKey, 
            bool unlocked, 
            Sprite lockedSprite, 
            Action<string, Sprite> clickCallback)
        {
            cgKey = addressableKey;
            isUnlocked = unlocked;
            onClickCallback = clickCallback;
            
            // Setup button
            button.onClick.RemoveAllListeners();
            
            if (isUnlocked)
            {
                StartCoroutine(LoadCGSprite());
                
                button.onClick.AddListener(OnClicked);
                button.interactable = true;
            }
            else
            {
                if (thumbnailImage != null && lockedSprite != null)
                {
                    thumbnailImage.sprite = lockedSprite;
                    thumbnailImage.color = Color.white;
                }
                else if (thumbnailImage != null)
                {
                    thumbnailImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                }
                
                button.interactable = false;
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ IMAGE LOADING
        // ═══════════════════════════════════════════════════════════
        
        private IEnumerator LoadCGSprite()
        {
            if (string.IsNullOrEmpty(cgKey))
            {
                LogError("Cannot load: cgKey is null/empty!");
                yield break;
            }

            // Guard against double load — if handle is already valid, skip
            if (loadHandle.IsValid())
            {
                LogWarning($"Load already in progress for: {cgKey}");
                yield break;
            }

            // Load via Addressables
            loadHandle = Addressables.LoadAssetAsync<Sprite>(cgKey);
            yield return loadHandle;

            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedSprite = loadHandle.Result;

                if (thumbnailImage != null)
                {
                    thumbnailImage.sprite = loadedSprite;
                    thumbnailImage.color = Color.white;
                }

                Log($"Loaded: {cgKey}");
            }
            else
            {
                LogError($"Failed to load: {cgKey}");

                if (thumbnailImage != null)
                {
                    thumbnailImage.color = new Color(1f, 0.3f, 0.3f, 1f);
                }
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CLICK HANDLER
        // ═══════════════════════════════════════════════════════════
        
        private void OnClicked()
        {
            if (!isUnlocked || loadedSprite == null)
            {
                LogWarning("Cannot open: CG not unlocked or not loaded");
                return;
            }
            
            onClickCallback?.Invoke(cgKey, loadedSprite);
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CLEANUP
        // ═══════════════════════════════════════════════════════════
        
        private void OnDestroy()
        {
            // Release Addressables handle
            if (loadHandle.IsValid())
            {
                Addressables.Release(loadHandle);
            }
            
            // Clear button listener
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ LOGGING
        // ═══════════════════════════════════════════════════════════

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void Log(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.galleryAppDebugLogs) return;
            UnityEngine.Debug.Log($"[GalleryThumbnailItem] {message}");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void LogWarning(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.galleryAppDebugLogs) return;
            UnityEngine.Debug.LogWarning($"[GalleryThumbnailItem] WARNING: {message}");
        }

        private void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[GalleryThumbnailItem] ERROR: {message}");
        }
    }
}