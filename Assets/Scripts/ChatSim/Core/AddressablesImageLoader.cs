// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/Core/AddressablesImageLoader.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ChatSim.Core
{
    /// <summary>
    /// Handles async loading of sprites from Addressables with caching.
    /// </summary>
    public static class AddressablesImageLoader
    {
        // ═══════════════════════════════════════════════════════════
        // ░ CACHE
        // ═══════════════════════════════════════════════════════════
        
        private static Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
        private static Dictionary<string, AsyncOperationHandle<Sprite>> ongoingLoads = new Dictionary<string, AsyncOperationHandle<Sprite>>();
        
        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Load a sprite from Addressables asynchronously.
        /// Callbacks: onLoaded(sprite), onFailed(error)
        /// </summary>
        public static void LoadSpriteAsync(
            string addressableKey, 
            Action<Sprite> onLoaded, 
            Action<string> onFailed = null)
        {
            if (string.IsNullOrEmpty(addressableKey))
            {
                Debug.LogError("[AddressablesImageLoader] Empty addressable key!");
                onFailed?.Invoke("Empty addressable key");
                return;
            }
            
            // Check cache first
            if (cachedSprites.TryGetValue(addressableKey, out Sprite cached))
            {
                Debug.Log($"[AddressablesImageLoader] ✓ Cache hit: {addressableKey}");
                onLoaded?.Invoke(cached);
                return;
            }
            
            // Check if already loading
            if (ongoingLoads.ContainsKey(addressableKey))
            {
                Debug.Log($"[AddressablesImageLoader] Already loading: {addressableKey}");
                
                // Subscribe to existing load operation
                var existingHandle = ongoingLoads[addressableKey];
                existingHandle.Completed += (op) => HandleLoadComplete(op, addressableKey, onLoaded, onFailed);
                return;
            }
            
            // Start new load
            Debug.Log($"[AddressablesImageLoader] Loading: {addressableKey}");
            
            var handle = Addressables.LoadAssetAsync<Sprite>(addressableKey);
            ongoingLoads[addressableKey] = handle;
            
            handle.Completed += (op) => HandleLoadComplete(op, addressableKey, onLoaded, onFailed);
        }
        
        /// <summary>
        /// Preload a sprite into cache (fire and forget)
        /// </summary>
        public static void PreloadSprite(string addressableKey)
        {
            LoadSpriteAsync(addressableKey, null, null);
        }
        
        /// <summary>
        /// Check if a sprite is already cached
        /// </summary>
        public static bool IsCached(string addressableKey)
        {
            return cachedSprites.ContainsKey(addressableKey);
        }
        
        /// <summary>
        /// Clear all cached sprites (use sparingly - causes reloads)
        /// </summary>
        public static void ClearCache()
        {
            Debug.Log($"[AddressablesImageLoader] Clearing cache ({cachedSprites.Count} sprites)");
            
            // Release all handles
            foreach (var kvp in ongoingLoads)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }
            
            cachedSprites.Clear();
            ongoingLoads.Clear();
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ LOAD COMPLETION HANDLER
        // ═══════════════════════════════════════════════════════════
        
        private static void HandleLoadComplete(
            AsyncOperationHandle<Sprite> operation, 
            string addressableKey,
            Action<Sprite> onLoaded,
            Action<string> onFailed)
        {
            // Remove from ongoing loads
            ongoingLoads.Remove(addressableKey);
            
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                var sprite = operation.Result;
                
                if (sprite != null)
                {
                    // Cache the sprite
                    cachedSprites[addressableKey] = sprite;
                    
                    Debug.Log($"[AddressablesImageLoader] ✓ Loaded: {addressableKey}");
                    onLoaded?.Invoke(sprite);
                }
                else
                {
                    string error = $"Sprite is null: {addressableKey}";
                    Debug.LogError($"[AddressablesImageLoader] ✗ {error}");
                    onFailed?.Invoke(error);
                }
            }
            else
            {
                string error = operation.OperationException?.Message ?? "Unknown error";
                Debug.LogError($"[AddressablesImageLoader] ✗ Failed to load '{addressableKey}': {error}");
                onFailed?.Invoke(error);
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ EDITOR TOOLS
        // ═══════════════════════════════════════════════════════════
        
        #if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            // Clear static state when entering play mode (Editor only)
            cachedSprites.Clear();
            ongoingLoads.Clear();
        }
        #endif
    }
}