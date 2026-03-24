// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/ChatApp/Components/ImageMessageBubble.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using BubbleSpinner.Data;
using ChatSim.Core;

namespace ChatSim.UI.ChatApp.Components
{
    /// <summary>
    /// Component for message bubbles that display images (CGs).
    /// Handles loading the image from Addressables, displaying it in the bubble, and opening a fullscreen viewer when clicked.
    /// Attach to: NpcImageBubble and PlayerImageBubble prefabs
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ImageMessageBubble : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES
        // ═══════════════════════════════════════════════════════════
        
        [Header("Image Display")]
        [SerializeField] private Image cgImage;
        
        [Header("Fullscreen Viewer")]
        [Tooltip("Reference to FullscreenCGViewer (find at runtime if null)")]
        [SerializeField] private FullscreenCGViewer fullscreenViewer;
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private MessageData messageData;
        private Sprite loadedSprite;
        private Button clickButton;
        
        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════
        
        private void Awake()
        {
            clickButton = GetComponent<Button>();
            clickButton.onClick.AddListener(OnImageClicked);
            
            // Find fullscreen viewer if not assigned
            if (fullscreenViewer == null)
            {
                fullscreenViewer = FindObjectOfType<FullscreenCGViewer>(true);
            }
        }
        
        /// <summary>
        /// Initialize the image bubble with message data
        /// </summary>
        public void Initialize(MessageData msg, bool instant = false)
        {
            messageData = msg;
            
            // Load image from Addressables
            LoadImage(msg.imagePath);
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ IMAGE LOADING
        // ═══════════════════════════════════════════════════════════
        
        private void LoadImage(string addressableKey)
        {
            if (string.IsNullOrEmpty(addressableKey))
            {
                LogError("No image path specified");
                return;
            }

            Log($"Loading: {addressableKey}");

            AddressablesImageLoader.LoadSpriteAsync(
                addressableKey,
                onLoaded: OnImageLoaded,
                onFailed: OnImageLoadFailed
            );
        }
        
        private void OnImageLoaded(Sprite sprite)
        {
            if (sprite == null)
            {
                LogError("Loaded sprite is null");
                return;
            }

            loadedSprite = sprite;

            if (cgImage != null)
            {
                cgImage.sprite = sprite;
                cgImage.enabled = true;
            }

            Log($"✓ Image loaded: {messageData.imagePath}");
        }
        
        private void OnImageLoadFailed(string error)
        {
            LogError($"✗ Load failed: {messageData.imagePath}\n{error}");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ FULLSCREEN INTERACTION
        // ═══════════════════════════════════════════════════════════
        
        private void OnImageClicked()
        {
            if (loadedSprite == null)
            {
                LogWarning("Cannot show fullscreen - sprite not loaded");
                return;
            }

            if (fullscreenViewer == null)
            {
                LogError("FullscreenCGViewer not found!");
                return;
            }

            Log($"Opening fullscreen: {messageData.imagePath}");
            fullscreenViewer.ShowFullscreen(loadedSprite, messageData.imagePath);
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CLEANUP
        // ═══════════════════════════════════════════════════════════
        
        private void OnDestroy()
        {
            if (clickButton != null)
            {
                clickButton.onClick.RemoveListener(OnImageClicked);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ░ LOGGING
        // ═══════════════════════════════════════════════════════════

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void Log(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.imageMessageBubbleDebugLogs) return;
            UnityEngine.Debug.Log($"[ImageMessageBubble] {message}");
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void LogWarning(string message)
        {
            if (GameBootstrap.Config == null || !GameBootstrap.Config.imageMessageBubbleDebugLogs) return;
            UnityEngine.Debug.LogWarning($"[ImageMessageBubble] WARNING: {message}");
        }

        private void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[ImageMessageBubble] ERROR: {message}");
        }
    }
}