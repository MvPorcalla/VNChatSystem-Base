// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/ChatApp/Components/ChoiceButton.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChatSim.UI.ChatApp.Components
{
    /// <summary>
    /// Attached to each choice button prefab
    /// </summary>
    public class ChoiceButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;
        
        public void Initialize(string text, Action onClick)
        {
            if (buttonText != null)
            {
                buttonText.text = text;
            }
            
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick?.Invoke());
            }
        }
    }
}