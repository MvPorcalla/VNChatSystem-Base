// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/BubbleSpinner/Core/BSDebug.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;

namespace BubbleSpinner.Core
{
    public static class BSDebug
    {
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message)
        {
            Debug.Log(message);
        }

        public static void LogWarning(string message) => Debug.LogWarning(message);
        public static void LogError(string message)   => Debug.LogError(message);
    }
}