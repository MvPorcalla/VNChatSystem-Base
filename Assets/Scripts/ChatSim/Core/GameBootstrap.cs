// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/GameBootstrap.cs
// Phone Chat Simulation Game - Core Initialization (No Cutscene/Config)
// ════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using UnityEngine;
using ChatSim.Data;
using BubbleSpinner.Core;

namespace ChatSim.Core
{
    /// <summary>
    /// Persistent initialization system - lives in Bootstrap scene (01_Bootstrap)
    /// Initializes all core managers and loads first scene
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        #region Singleton
        public static GameBootstrap Instance { get; private set; }
        #endregion

        #region Manager References - ASSIGN IN INSPECTOR
        [Header("Core Managers")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private SceneFlowManager sceneFlowManager;

        [Header("Game Systems")]
        [SerializeField] private ConversationManager conversationManager;
        #endregion

        #region Public Static Accessors
        public static SaveManager Save { get; private set; }
        public static SceneFlowManager SceneFlow { get; private set; }
        public static ConversationManager Conversation { get; private set; }
        #endregion

        #region State
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Clear any stale events from previous sessions
            GameEvents.ClearAllEvents();

            Debug.Log("GameBootstrap created - will persist across scenes");
        }

        private IEnumerator Start()
        {
            Debug.Log("=== BOOTSTRAP INITIALIZATION START ===");

            try
            {
                ValidateManagerReferences();
                AssignStaticReferences();
                InitializeManagers();

                _isInitialized = true;
                Debug.Log("All systems initialized");
            }
            catch (Exception e)
            {
                Debug.LogError($"FATAL: Bootstrap initialization failed!\n{e}");
                QuitApplication();
                yield break;
            }

            string nextScene = DetermineNextScene();
            Debug.Log($"Loading next scene: {nextScene}");

            // Wait for scene to load
            bool sceneLoadComplete = false;
            System.Action<string> onSceneLoaded = null;
            
            onSceneLoaded = (sceneName) =>
            {
                if (sceneName == nextScene)
                {
                    sceneLoadComplete = true;
                    GameEvents.OnSceneLoaded -= onSceneLoaded;
                }
            };
            
            GameEvents.OnSceneLoaded += onSceneLoaded;
            SceneFlow.LoadScene(nextScene);
            
            yield return new WaitUntil(() => sceneLoadComplete);

            Debug.Log("=== BOOTSTRAP INITIALIZATION COMPLETE ===");
        }
        #endregion

        #region Manager Setup

        private void ValidateManagerReferences()
        {
            Debug.Log("Validating manager references...");

            if (saveManager == null)
                throw new InvalidOperationException("SaveManager not assigned in Inspector!");

            if (sceneFlowManager == null)
                throw new InvalidOperationException("SceneFlowManager not assigned in Inspector!");

            if (conversationManager == null)
                throw new InvalidOperationException("ConversationManager not assigned in Inspector!");

            Debug.Log("All manager references valid");
        }

        private void AssignStaticReferences()
        {
            Debug.Log("Assigning static references...");

            Save = saveManager;
            SceneFlow = sceneFlowManager;
            Conversation = conversationManager;

            Debug.Log("Static references assigned");
        }

        private void InitializeManagers()
        {
            Debug.Log("Initializing managers...");

            // PHASE 1: Core Systems (no dependencies)
            Save.Init();
            Debug.Log("✓ SaveManager initialized");

            SceneFlow.Init();
            Debug.Log("✓ SceneFlowManager initialized");

            // PHASE 2: Game Systems (depend on Save)
            Conversation.Init();
            Debug.Log("✓ ConversationManager initialized");

            // PHASE 3: Ensure save data exists
            EnsureSaveDataExists();

            Debug.Log("All managers initialized");
        }

        /// <summary>
        /// Ensures a valid save file exists before game starts
        /// Creates new save if none exists
        /// </summary>
        private void EnsureSaveDataExists()
        {
            Debug.Log("Ensuring save data exists...");
            
            SaveData saveData = Save.GetOrCreateSaveData();
            
            if (saveData == null)
            {
                throw new InvalidOperationException("FATAL: Failed to create or load save data!");
            }
            
            Debug.Log("✓ Save data ready");
        }

        #endregion

        #region Scene Flow Logic

        private string DetermineNextScene()
        {
            // Always start at lock screen
            Debug.Log("Loading LockScreen (default entry point)");
            return SceneNames.LOCKSCREEN;
        }

        #endregion

        #region Utilities

        private void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Editor Tools
#if UNITY_EDITOR
        [ContextMenu("Validate Bootstrap")]
        private void ValidateBootstrap()
        {
            Debug.Log("=== BOOTSTRAP VALIDATION ===");

            try
            {
                ValidateManagerReferences();
                Debug.Log("✓ All validations passed");
            }
            catch (Exception e)
            {
                Debug.LogError($"ERROR: Validation failed: {e.Message}");
            }

            Debug.Log($"Initialized: {_isInitialized}");
            Debug.Log("===========================");
        }

        [ContextMenu("Log GameEvents Subscribers")]
        private void LogEventSubscribers()
        {
            GameEvents.LogSubscriberCounts();
        }

        [ContextMenu("Simulate Returning Player")]
        private void SimulateReturningPlayer()
        {
            if (!Save.SaveExists())
            {
                SaveData testSave = Save.CreateNewSave();
                Save.SaveGame(testSave);
            }
            
            Debug.Log("✓ Created save - next run will start at LockScreen");
        }
#endif
        #endregion
    }
}
