// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/ChatSim/Data/GameConfig.cs
// ════════════════════════════════════════════════════════════════════════

using UnityEngine;

namespace ChatSim.Data
{
    // ════════════════════════════════════════════════════════════════
    // ENUMS
    // ════════════════════════════════════════════════════════════════

    public enum TimeFormat
    {
        HH_mm,          // 14:30  (24-hour)
        h_mm_tt,        // 2:30 PM (12-hour)
        HH_mm_ss,       // 14:30:00 (24-hour with seconds)
        h_mm_ss_tt,     // 2:30:00 PM (12-hour with seconds)
    }

    public enum DateFormat
    {
        dddd_MMMM_dd,   // Monday, January 01
        MMMM_dd_yyyy,   // January 01 2025
        dd_MMMM_yyyy,   // 01 January 2025
        MM_dd_yyyy,     // 01/01/2025
        yyyy_MM_dd,     // 2025-01-01
    }

    /// <summary>
    /// Central ScriptableObject config for all game settings.
    /// Create via: Right-click → Create → ChatSim → Game Config
    ///
    /// Sections:
    ///   - LockScreen
    ///   (add more sections here as the game grows)
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "ChatSim/Game Config")]
    public class GameConfig : ScriptableObject
    {
        // ════════════════════════════════════════════════════════════════
        // LOCK SCREEN
        // ════════════════════════════════════════════════════════════════

        [Header("── Lock Screen ──────────────────────────")]

        [Header("Swipe")]
        [Tooltip("Minimum swipe distance in pixels to trigger unlock")]
        public float swipeThreshold = 300f;

        [Tooltip("How many pixels of upward swipe = fully faded out")]
        public float fadeSwipeRange = 400f;

        [Header("Notifications")]
        [Tooltip("Max individual notifications shown on lock screen. 0 = disabled.")]
        public int maxIndividualNotifications = 3;

        [Header("Time & Date")]
        public TimeFormat timeFormat = TimeFormat.HH_mm;
        public DateFormat dateFormat = DateFormat.dddd_MMMM_dd;

        // ════════════════════════════════════════════════════════════════
        // DEBUG
        // ════════════════════════════════════════════════════════════════

        [Header("── Debug ────────────────────────────────")]

        [Tooltip("Enable debug logs for LockScreen")]
        public bool lockScreenDebugLogs = true;

        // ════════════════════════════════════════════════════════════════
        // HELPERS — converts enum to C# DateTime format string
        // ════════════════════════════════════════════════════════════════

        public string GetTimeFormatString()
        {
            switch (timeFormat)
            {
                case TimeFormat.HH_mm:       return "HH:mm";
                case TimeFormat.h_mm_tt:     return "h:mm tt";
                case TimeFormat.HH_mm_ss:    return "HH:mm:ss";
                case TimeFormat.h_mm_ss_tt:  return "h:mm:ss tt";
                default:                     return "HH:mm";
            }
        }

        public string GetDateFormatString()
        {
            switch (dateFormat)
            {
                case DateFormat.dddd_MMMM_dd:  return "dddd, MMMM dd";
                case DateFormat.MMMM_dd_yyyy:  return "MMMM dd yyyy";
                case DateFormat.dd_MMMM_yyyy:  return "dd MMMM yyyy";
                case DateFormat.MM_dd_yyyy:    return "MM/dd/yyyy";
                case DateFormat.yyyy_MM_dd:    return "yyyy-MM-dd";
                default:                       return "dddd, MMMM dd";
            }
        }

        // ════════════════════════════════════════════════════════════════
        // FUTURE SECTIONS
        // ════════════════════════════════════════════════════════════════

        // [Header("── Phone Screen ─────────────────────────")]
        // [Header("── Chat App ──────────────────────────────")]
        // [Header("── Dialogue ──────────────────────────────")]
    }
}