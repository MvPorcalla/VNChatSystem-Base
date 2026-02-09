// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/PoolingManager.cs
// Phone Chat Simulation Game - Object Pooling System (FIXED)
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;

namespace ChatSim.Core
{
    /// <summary>
    /// Simple object pooling system for message bubbles and typing indicators.
    /// Access via: GameBootstrap.Pool (if integrated) or attach to ChatAppController
    /// </summary>
    public class PoolingManager : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ POOL STORAGE
        // ═══════════════════════════════════════════════════════════
        
        private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
        private Dictionary<GameObject, GameObject> activeObjects = new Dictionary<GameObject, GameObject>();
        private Transform poolRoot;
        
        // ═══════════════════════════════════════════════════════════
        // ░ INITIALIZATION
        // ═══════════════════════════════════════════════════════════
        
        private void Awake()
        {
            poolRoot = new GameObject("_PooledObjects").transform;
            poolRoot.SetParent(transform);
            poolRoot.gameObject.SetActive(false);
            
            Debug.Log("[PoolingManager] Initialized");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Get an object from pool or create new one
        /// </summary>
        public GameObject Get(GameObject prefab, Transform parent = null, bool activateOnGet = false)
        {
            if (prefab == null)
            {
                Debug.LogError("[PoolingManager] Cannot get object from null prefab");
                return null;
            }
            
            // Create pool if doesn't exist
            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new Queue<GameObject>();
            }
            
            GameObject obj;
            
            // Reuse from pool if available
            if (pools[prefab].Count > 0)
            {
                obj = pools[prefab].Dequeue();
                
                if (parent != null)
                {
                    obj.transform.SetParent(parent, false);
                }
            }
            else
            {
                // Create new instance
                obj = Instantiate(prefab, parent);
                
                // Add PooledObject component to track prefab
                var pooledObject = obj.GetComponent<PooledObject>();
                if (pooledObject == null)
                {
                    pooledObject = obj.AddComponent<PooledObject>();
                }
                pooledObject.SetPrefab(prefab);
            }
            
            // Track as active
            activeObjects[obj] = prefab;
            
            // Reset transform
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            
            if (activateOnGet)
            {
                obj.SetActive(true);
            }
            
            return obj;
        }
        
        /// <summary>
        /// Return object to pool
        /// </summary>
        public void Recycle(GameObject obj)
        {
            if (obj == null) return;
            
            var pooledObject = obj.GetComponent<PooledObject>();
            if (pooledObject == null || pooledObject.Prefab == null)
            {
                Debug.LogWarning($"[PoolingManager] Cannot recycle {obj.name} - no PooledObject component");
                Destroy(obj);
                return;
            }
            
            GameObject prefab = pooledObject.Prefab;
            
            // Remove from active tracking
            if (activeObjects.ContainsKey(obj))
            {
                activeObjects.Remove(obj);
            }
            
            // Only clear dynamic content if needed
            if (ShouldClearContent(pooledObject))
            {
                ClearDynamicContent(obj);
            }
            
            // Deactivate and reparent
            obj.SetActive(false);
            obj.transform.SetParent(poolRoot, false);
            
            // Add back to pool
            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new Queue<GameObject>();
            }
            
            pools[prefab].Enqueue(obj);
        }
        
        /// <summary>
        /// Pre-warm pool with objects
        /// </summary>
        public void PreWarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0) return;
            
            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new Queue<GameObject>();
            }
            
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab, poolRoot);
                obj.SetActive(false);
                
                var pooledObject = obj.GetComponent<PooledObject>();
                if (pooledObject == null)
                {
                    pooledObject = obj.AddComponent<PooledObject>();
                }
                pooledObject.SetPrefab(prefab);
                
                pools[prefab].Enqueue(obj);
            }
            
            Debug.Log($"[PoolingManager] Pre-warmed {count} instances of {prefab.name}");
        }
        
        /// <summary>
        /// Clear all pools and destroy objects
        /// </summary>
        public void ClearAllPools()
        {
            int totalDestroyed = 0;
            
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj != null)
                    {
                        Destroy(obj);
                        totalDestroyed++;
                    }
                }
            }
            
            pools.Clear();
            activeObjects.Clear();
            
            Debug.Log($"[PoolingManager] Cleared all pools - destroyed {totalDestroyed} objects");
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ HELPER METHODS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Check if content should be cleared based on PooledObject settings
        /// </summary>
        private bool ShouldClearContent(PooledObject pooledObject)
        {
            // Don't clear if PreserveContent is enabled
            return !pooledObject.PreserveContent;
        }
        
        /// <summary>
        /// Clear text and button listeners from recycled objects
        /// </summary>
        private void ClearDynamicContent(GameObject obj)
        {
            // Clear text
            var textComponents = obj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var text in textComponents)
            {
                text.text = string.Empty;
            }
            
            // Clear button listeners
            var button = obj.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CLEANUP
        // ═══════════════════════════════════════════════════════════
        
        private void OnDestroy()
        {
            ClearAllPools();
        }
    }
}