// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/GameEvents.cs
// Phone Chat Simulation Game - Central Event Hub
// ════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;

namespace ChatSim.Core
{
    /// <summary>
    /// Central event hub - all game state changes flow through here
    /// Provides decoupled communication between systems
    /// 
    /// [WARNING] MEMORY LEAK [WARNING]:
    /// Always unsubscribe from events in OnDestroy() or OnDisable()
    /// 
    /// Example:
    ///   void OnEnable() => GameEvents.OnSceneLoaded += HandleSceneLoaded;
    ///   void OnDestroy() => GameEvents.OnSceneLoaded -= HandleSceneLoaded;
    /// </summary>
    public static class GameEvents
    {
        #region Logging Control
        
        #if UNITY_EDITOR
        private const bool ENABLE_EVENT_LOGGING = true;
        #else
        private const bool ENABLE_EVENT_LOGGING = false;
        #endif
        
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void Log(string message)
        {
            Debug.Log($"[GameEvents] {message}");
        }
        
        #endregion

        // ════════════════════════════════════════════════════════════════
        // SCENE EVENTS
        // ════════════════════════════════════════════════════════════════
        
        public static event Action<string> OnSceneLoaded;
        public static event Action<string> OnSceneChanging;
        
        // ════════════════════════════════════════════════════════════════
        // SAVE/LOAD EVENTS
        // ════════════════════════════════════════════════════════════════
        
        public static event Action OnNewGameStarted;
        public static event Action OnGameSaved;
        public static event Action OnGameLoaded;
        public static event Action OnSaveDeleted;
        
        // ════════════════════════════════════════════════════════════════
        // PHONE STATE EVENTS
        // ════════════════════════════════════════════════════════════════
        
        public static event Action OnPhoneLocked;
        public static event Action OnPhoneUnlocked;
        public static event Action<string> OnAppOpened;     // appId
        public static event Action<string> OnAppClosed;     // appId
        
        // ════════════════════════════════════════════════════════════════
        // CONVERSATION EVENTS
        // ════════════════════════════════════════════════════════════════
        
        public static event Action<string> OnConversationStarted;       // conversationId
        public static event Action<string> OnConversationUnlocked;      // conversationId
        public static event Action<string, string> OnMessageReceived;   // conversationId, messageId
        public static event Action<string, string> OnMessageSent;       // conversationId, messageId
        public static event Action<string> OnConversationCompleted;     // conversationId
        
        // ════════════════════════════════════════════════════════════════
        // CG GALLERY EVENTS
        // ════════════════════════════════════════════════════════════════
        
        public static event Action<string> OnCGUnlocked;    // cgAddressableKey
        
        // ════════════════════════════════════════════════════════════════
        // NOTIFICATION EVENTS
        // ════════════════════════════════════════════════════════════════
        
        public static event Action<string, string> OnNotificationReceived;  // appId, message
        public static event Action OnNotificationCleared;
        
        // ════════════════════════════════════════════════════════════════
        // CONTACT EVENTS
        // ════════════════════════════════════════════════════════════════
        
        public static event Action<string> OnContactAdded;      // contactId
        public static event Action<string> OnContactUpdated;    // contactId
        
        // ════════════════════════════════════════════════════════════════
        // PROGRESSION EVENTS
        // ════════════════════════════════════════════════════════════════
        
        public static event Action<string> OnStoryFlagSet;      // flagName
        public static event Action<string> OnStoryFlagCleared;  // flagName
        public static event Action<int> OnChapterUnlocked;      // chapterNumber
        
        // ════════════════════════════════════════════════════════════════
        // EVENT LIFECYCLE MANAGEMENT
        // ════════════════════════════════════════════════════════════════
        
        public static void ClearAllEvents()
        {
            // Scene events
            OnSceneLoaded = null;
            OnSceneChanging = null;
            
            // Save/Load events
            OnNewGameStarted = null;
            OnGameSaved = null;
            OnGameLoaded = null;
            OnSaveDeleted = null;
            
            // Phone state events
            OnPhoneLocked = null;
            OnPhoneUnlocked = null;
            OnAppOpened = null;
            OnAppClosed = null;
            
            // Conversation events
            OnConversationStarted = null;
            OnConversationUnlocked = null;
            OnMessageReceived = null;
            OnMessageSent = null;
            OnConversationCompleted = null;
            
            // CG events
            OnCGUnlocked = null;
            
            // Notification events
            OnNotificationReceived = null;
            OnNotificationCleared = null;
            
            // Contact events
            OnContactAdded = null;
            OnContactUpdated = null;
            
            // Progression events
            OnStoryFlagSet = null;
            OnStoryFlagCleared = null;
            OnChapterUnlocked = null;
            
            Log("All events cleared");
        }
        
        public static void LogSubscriberCounts()
        {
            Debug.Log("=== GameEvents Subscriber Counts ===");
            
            // Scene
            LogEventCount("OnSceneLoaded", OnSceneLoaded);
            LogEventCount("OnSceneChanging", OnSceneChanging);
            
            // Save/Load
            LogEventCount("OnNewGameStarted", OnNewGameStarted);
            LogEventCount("OnGameSaved", OnGameSaved);
            LogEventCount("OnGameLoaded", OnGameLoaded);
            LogEventCount("OnSaveDeleted", OnSaveDeleted);
            
            // Phone State
            LogEventCount("OnPhoneLocked", OnPhoneLocked);
            LogEventCount("OnPhoneUnlocked", OnPhoneUnlocked);
            LogEventCount("OnAppOpened", OnAppOpened);
            LogEventCount("OnAppClosed", OnAppClosed);
            
            // Conversations
            LogEventCount("OnConversationStarted", OnConversationStarted);
            LogEventCount("OnConversationUnlocked", OnConversationUnlocked);
            LogEventCount("OnMessageReceived", OnMessageReceived);
            LogEventCount("OnMessageSent", OnMessageSent);
            LogEventCount("OnConversationCompleted", OnConversationCompleted);
            
            // CG Events
            LogEventCount("OnCGUnlocked", OnCGUnlocked);
            
            // Notifications
            LogEventCount("OnNotificationReceived", OnNotificationReceived);
            LogEventCount("OnNotificationCleared", OnNotificationCleared);
            
            // Contacts
            LogEventCount("OnContactAdded", OnContactAdded);
            LogEventCount("OnContactUpdated", OnContactUpdated);
            
            // Progression
            LogEventCount("OnStoryFlagSet", OnStoryFlagSet);
            LogEventCount("OnStoryFlagCleared", OnStoryFlagCleared);
            LogEventCount("OnChapterUnlocked", OnChapterUnlocked);
            
            Debug.Log("====================================");
        }
        
        private static void LogEventCount(string eventName, Delegate eventDelegate)
        {
            int count = eventDelegate?.GetInvocationList()?.Length ?? 0;
            if (count > 0)
            {
                Debug.Log($"  {eventName}: {count} subscriber(s)");
            }
        }
        
        // ════════════════════════════════════════════════════════════════
        // EVENT TRIGGERS - Scene
        // ════════════════════════════════════════════════════════════════
        
        public static void TriggerSceneLoaded(string sceneName)
        {
            OnSceneLoaded?.Invoke(sceneName);
            Log($"Scene loaded: {sceneName}");
        }
        
        public static void TriggerSceneChanging(string sceneName)
        {
            OnSceneChanging?.Invoke(sceneName);
            Log($"Scene changing to: {sceneName}");
        }
        
        // ════════════════════════════════════════════════════════════════
        // EVENT TRIGGERS - Save/Load
        // ════════════════════════════════════════════════════════════════
        
        public static void TriggerNewGameStarted()
        {
            OnNewGameStarted?.Invoke();
            Log("New game started");
        }
        
        public static void TriggerGameSaved()
        {
            OnGameSaved?.Invoke();
            Log("Game saved");
        }
        
        public static void TriggerGameLoaded()
        {
            OnGameLoaded?.Invoke();
            Log("Game loaded");
        }
        
        public static void TriggerSaveDeleted()
        {
            OnSaveDeleted?.Invoke();
            Log("Save deleted");
        }
        
        // ════════════════════════════════════════════════════════════════
        // EVENT TRIGGERS - Phone State
        // ════════════════════════════════════════════════════════════════
        
        public static void TriggerPhoneLocked()
        {
            OnPhoneLocked?.Invoke();
            Log("Phone locked");
        }
        
        public static void TriggerPhoneUnlocked()
        {
            OnPhoneUnlocked?.Invoke();
            Log("Phone unlocked");
        }
        
        public static void TriggerAppOpened(string appId)
        {
            OnAppOpened?.Invoke(appId);
            Log($"App opened: {appId}");
        }
        
        public static void TriggerAppClosed(string appId)
        {
            OnAppClosed?.Invoke(appId);
            Log($"App closed: {appId}");
        }
        
        // ════════════════════════════════════════════════════════════════
        // EVENT TRIGGERS - Conversations
        // ════════════════════════════════════════════════════════════════
        
        public static void TriggerConversationStarted(string conversationId)
        {
            OnConversationStarted?.Invoke(conversationId);
            Log($"Conversation started: {conversationId}");
        }
        
        public static void TriggerConversationUnlocked(string conversationId)
        {
            OnConversationUnlocked?.Invoke(conversationId);
            Log($"Conversation unlocked: {conversationId}");
        }
        
        public static void TriggerMessageReceived(string conversationId, string messageId)
        {
            OnMessageReceived?.Invoke(conversationId, messageId);
            Log($"Message received: {conversationId}/{messageId}");
        }
        
        public static void TriggerMessageSent(string conversationId, string messageId)
        {
            OnMessageSent?.Invoke(conversationId, messageId);
            Log($"Message sent: {conversationId}/{messageId}");
        }
        
        public static void TriggerConversationCompleted(string conversationId)
        {
            OnConversationCompleted?.Invoke(conversationId);
            Log($"Conversation completed: {conversationId}");
        }
        
        // ════════════════════════════════════════════════════════════════
        // EVENT TRIGGERS - CG Gallery
        // ════════════════════════════════════════════════════════════════
        
        public static void TriggerCGUnlocked(string cgAddressableKey)
        {
            OnCGUnlocked?.Invoke(cgAddressableKey);
            Log($"🎨 CG unlocked: {cgAddressableKey}");
        }
        
        // ════════════════════════════════════════════════════════════════
        // EVENT TRIGGERS - Notifications
        // ════════════════════════════════════════════════════════════════
        
        public static void TriggerNotificationReceived(string appId, string message)
        {
            OnNotificationReceived?.Invoke(appId, message);
            Log($"Notification: [{appId}] {message}");
        }
        
        public static void TriggerNotificationCleared()
        {
            OnNotificationCleared?.Invoke();
            Log("Notifications cleared");
        }
        
        // ════════════════════════════════════════════════════════════════
        // EVENT TRIGGERS - Contacts
        // ════════════════════════════════════════════════════════════════
        
        public static void TriggerContactAdded(string contactId)
        {
            OnContactAdded?.Invoke(contactId);
            Log($"Contact added: {contactId}");
        }
        
        public static void TriggerContactUpdated(string contactId)
        {
            OnContactUpdated?.Invoke(contactId);
            Log($"Contact updated: {contactId}");
        }
        
        // ════════════════════════════════════════════════════════════════
        // EVENT TRIGGERS - Progression
        // ════════════════════════════════════════════════════════════════
        
        public static void TriggerStoryFlagSet(string flagName)
        {
            OnStoryFlagSet?.Invoke(flagName);
            Log($"Story flag set: {flagName}");
        }
        
        public static void TriggerStoryFlagCleared(string flagName)
        {
            OnStoryFlagCleared?.Invoke(flagName);
            Log($"Story flag cleared: {flagName}");
        }
        
        public static void TriggerChapterUnlocked(int chapterNumber)
        {
            OnChapterUnlocked?.Invoke(chapterNumber);
            Log($"Chapter unlocked: {chapterNumber}");
        }
    }
}