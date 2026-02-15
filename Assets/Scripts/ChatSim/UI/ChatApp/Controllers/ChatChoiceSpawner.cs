// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Core/ChatChoiceSpawner.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BubbleSpinner.Data;
using ChatSim.UI.ChatApp.Components;

namespace ChatSim.UI.ChatApp.Controllers
{
    /// <summary>
    /// Handles spawning and displaying choice buttons.
    /// Attach to: ChatChoices GameObject
    /// </summary>
    public class ChatChoiceSpawner : MonoBehaviour
    {
        // ═══════════════════════════════════════════════════════════
        // ░ INSPECTOR REFERENCES
        // ═══════════════════════════════════════════════════════════
        
        [Header("Prefabs")]
        [SerializeField] private GameObject choiceButtonPrefab;
        
        [Header("Optional: Separate Continue Button Styling")]
        [Tooltip("Leave empty to use choiceButtonPrefab for continue/end buttons")]
        [SerializeField] private GameObject continueButtonPrefab;
        
        [Header("Container")]
        [SerializeField] private RectTransform choiceContainer;
        
        // ═══════════════════════════════════════════════════════════
        // ░ STATE
        // ═══════════════════════════════════════════════════════════
        
        private Action<ChoiceData> onChoiceSelected;
        
        // ═══════════════════════════════════════════════════════════
        // ░ PUBLIC API
        // ═══════════════════════════════════════════════════════════
        
        public void DisplayChoices(List<ChoiceData> choices, Action<ChoiceData> callback)
        {
            ClearChoices();
            
            onChoiceSelected = callback;
            
            foreach (var choice in choices)
            {
                SpawnChoiceButton(choice);
            }
            
            gameObject.SetActive(true);
            
            // Rebuild layout
            StartCoroutine(RebuildLayoutDelayed());
        }
        
        /// <summary>
        /// Show continue button (for pause points)
        /// </summary>
        public void ShowContinueButton(Action callback)
        {
            ClearChoices();
            
            GameObject prefabToUse = continueButtonPrefab != null ? continueButtonPrefab : choiceButtonPrefab;
            
            if (prefabToUse == null)
            {
                Debug.LogError("[ChatChoiceSpawner] No button prefab assigned!");
                return;
            }
            
            GameObject continueObj = Instantiate(prefabToUse, choiceContainer);
            
            var button = continueObj.GetComponent<ChoiceButton>();
            if (button != null)
            {
                button.Initialize("...", callback);
            }
            else
            {
                Debug.LogError("[ChatChoiceSpawner] ChoiceButton component missing on continue button prefab!");
            }
            
            gameObject.SetActive(true);
            
            // Rebuild layout
            StartCoroutine(RebuildLayoutDelayed());
        }
        
        /// <summary>
        /// Show end button (e.g. "Continue to Next Chapter" or "Return to Contacts")
        /// Used for:
        /// - "Continue to Next Chapter" (when more chapters exist)
        /// - "Return to Contacts" (when conversation is complete)
        /// </summary>
        public void ShowEndButton(string buttonText, Action callback)
        {
            ClearChoices();
            
            GameObject prefabToUse = continueButtonPrefab != null ? continueButtonPrefab : choiceButtonPrefab;
            
            if (prefabToUse == null)
            {
                Debug.LogError("[ChatChoiceSpawner] No button prefab assigned!");
                return;
            }
            
            GameObject endObj = Instantiate(prefabToUse, choiceContainer);
            
            var button = endObj.GetComponent<ChoiceButton>();
            if (button != null)
            {
                button.Initialize(buttonText, callback);
            }
            else
            {
                Debug.LogError("[ChatChoiceSpawner] ChoiceButton component missing on end button prefab!");
            }
            
            gameObject.SetActive(true);
            
            // Rebuild layout
            StartCoroutine(RebuildLayoutDelayed());
            
            Debug.Log($"[ChatChoiceSpawner] Showing end button: {buttonText}");
        }
        
        public void ClearChoices()
        {
            if (choiceContainer == null)
            {
                Debug.LogError("[ChatChoiceSpawner] choiceContainer is null!");
                return;
            }

            foreach (Transform child in choiceContainer)
            {
                Destroy(child.gameObject);
            }
            
            gameObject.SetActive(false);
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ CHOICE SPAWNING
        // ═══════════════════════════════════════════════════════════
        
        private void SpawnChoiceButton(ChoiceData choice)
        {
            if (choiceButtonPrefab == null)
            {
                Debug.LogError("[ChatChoiceSpawner] choiceButtonPrefab is null!");
                return;
            }

            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
            
            var button = buttonObj.GetComponent<ChoiceButton>();
            if (button != null)
            {
                button.Initialize(choice.choiceText, () => OnChoiceClicked(choice));
            }
            else
            {
                Debug.LogError("[ChatChoiceSpawner] ChoiceButton component missing on choice button prefab!");
            }
        }
        
        private void OnChoiceClicked(ChoiceData choice)
        {
            onChoiceSelected?.Invoke(choice);
        }
        
        // ═══════════════════════════════════════════════════════════
        // ░ LAYOUT REBUILD
        // ═══════════════════════════════════════════════════════════
        
        private IEnumerator RebuildLayoutDelayed()
        {
            yield return new WaitForEndOfFrame();

            if (choiceContainer != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(choiceContainer);
            }
        }
    }
}