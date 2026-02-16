// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/UI/ChatApp/Components/ImageMessageBubble.cs
// SIMPLIFIED - Direct image loading without loading indicators
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;
using UnityEngine.UI;
using BubbleSpinner.Data;
using ChatSim.Core;

namespace ChatSim.UI.ChatApp.Components
{
    /// <summary>
    /// Displays an image/CG in a chat bubble.
    /// Features: Direct image loading, click-to-fullscreen.
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
                Debug.LogError("[ImageMessageBubble] No image path specified");
                return;
            }
            
            Debug.Log($"[ImageMessageBubble] Loading: {addressableKey}");
            
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
                Debug.LogError("[ImageMessageBubble] Loaded sprite is null");
                return;
            }
            
            loadedSprite = sprite;
            
            // Display image
            if (cgImage != null)
            {
                cgImage.sprite = sprite;
                cgImage.enabled = true;
            }
            
            Debug.Log($"[ImageMessageBubble] ✓ Image loaded: {messageData.imagePath}");
        }
        
        private void OnImageLoadFailed(string error)
        {
            Debug.LogError($"[ImageMessageBubble] ✗ Load failed: {messageData.imagePath}\n{error}");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ FULLSCREEN INTERACTION
        // ═══════════════════════════════════════════════════════════
        
        private void OnImageClicked()
        {
            if (loadedSprite == null)
            {
                Debug.LogWarning("[ImageMessageBubble] Cannot show fullscreen - sprite not loaded");
                return;
            }
            
            if (fullscreenViewer == null)
            {
                Debug.LogError("[ImageMessageBubble] FullscreenCGViewer not found!");
                return;
            }
            
            Debug.Log($"[ImageMessageBubble] Opening fullscreen: {messageData.imagePath}");
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
    }
}