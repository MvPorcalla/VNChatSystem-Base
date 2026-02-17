// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/Core/SaveManager.cs
// ════════════════════════════════════════════════════════════════════════

using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using ChatSim.Data;

namespace ChatSim.Core
{
    /// <summary>
    /// Handles save/load operations for game state
    /// Access via: GameBootstrap.Save
    /// 
    /// Features:
    /// - Atomic saves (temp file → swap)
    /// - Automatic backup on overwrite
    /// - Backup recovery on corrupted save
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        #region Settings
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        
        [Header("Save Settings")]
        [SerializeField] private bool prettyPrintJson = true;
        #endregion

        #region File Paths
        private const string ROOT_SAVE_FOLDER = "Saves";
        private const string GAME_DATA_FOLDER = "ChatSimData";
        private const string SAVE_FILE = "game_save.json";
        private const string BACKUP_SUFFIX = ".bak";
        private const string TEMP_SUFFIX = ".tmp";

        private string RootSavePath => Path.Combine(Application.persistentDataPath, ROOT_SAVE_FOLDER);
        private string GameDataPath => Path.Combine(RootSavePath, GAME_DATA_FOLDER);
        private string SaveFilePath => Path.Combine(GameDataPath, SAVE_FILE);
        private string BackupFilePath => SaveFilePath + BACKUP_SUFFIX;
        private string TempFilePath => SaveFilePath + TEMP_SUFFIX;
        #endregion

        #region State
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        #endregion

        #region Initialization
        /// <summary>
        /// Called by GameBootstrap during initialization
        /// </summary>
        public void Init()
        {
            Log("Initializing SaveManager...");

            try
            {
                CreateFolderStructure();
                ValidateFileSystem();
                
                _isInitialized = true;
                Log("✓ SaveManager initialized successfully");
                LogSavePath();
            }
            catch (Exception e)
            {
                LogError($"SaveManager initialization failed: {e.Message}");
                throw;
            }
        }
        #endregion

        #region File System Setup
        private void CreateFolderStructure()
        {
            Log("Creating save folder structure...");

            if (!Directory.Exists(RootSavePath))
            {
                Directory.CreateDirectory(RootSavePath);
                Log($"  ✓ Created root folder: {RootSavePath}");
            }

            if (!Directory.Exists(GameDataPath))
            {
                Directory.CreateDirectory(GameDataPath);
                Log($"  ✓ Created game data folder: {GameDataPath}");
            }
            
            Log("✓ Folder structure ready");
        }

        private void ValidateFileSystem()
        {
            Log("Validating file system access...");

            string testFile = Path.Combine(RootSavePath, ".test");
            
            try
            {
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                Log("✓ File system access confirmed");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Cannot write to save directory: {e.Message}");
            }
        }

        private void LogSavePath()
        {
            Log($"Save location: {RootSavePath}");
            
            #if UNITY_EDITOR
            Log("Press F12 to open save folder in explorer");
            #endif
        }
        #endregion

        #region Public API - Save/Load
        
        /// <summary>
        /// Check if a save file exists
        /// </summary>
        public bool SaveExists()
        {
            bool exists = File.Exists(SaveFilePath);
            Log($"Save exists check: {exists}");
            return exists;
        }

        /// <summary>
        /// Get existing save data or create new one if none exists
        /// GUARANTEES a valid SaveData is returned
        /// </summary>
        public SaveData GetOrCreateSaveData()
        {
            // Try to load existing save
            SaveData saveData = LoadGame();
            
            if (saveData != null)
            {
                Log("✓ Loaded existing save data");
                return saveData;
            }
            
            // No save exists - create new one
            Log("No save found - creating new save data");
            SaveData newSave = CreateNewSave();
            
            // Save it immediately to disk
            SaveGame(newSave);
            
            Log("✓ New save data created and saved");
            return newSave;
        }

        /// <summary>
        /// Save game data to disk with atomic write and backup
        /// </summary>
        /// <param name="gameData">Game data to save</param>
        /// <param name="playtime">Current playtime in seconds (optional)</param>
        /// <returns>True if save successful</returns>
        public bool SaveGame(SaveData gameData, float playtime = 0f)
        {
            if (gameData == null)
            {
                LogError("Cannot save null game data!");
                return false;
            }

            try
            {
                // Create save data wrapper (includes metadata)
                SaveDataWrapper saveWrapper = new SaveDataWrapper
                {
                    gameData = gameData,
                    saveVersion = gameData.saveVersion,
                    saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    playtimeSeconds = playtime
                };

                // Serialize to JSON using Newtonsoft.Json
                string json = JsonConvert.SerializeObject(saveWrapper, 
                    prettyPrintJson ? Formatting.Indented : Formatting.None);

                // ATOMIC SAVE PROCESS:
                // 1. Write to temp file first
                File.WriteAllText(TempFilePath, json);
                
                // 2. Backup existing save (if it exists)
                if (File.Exists(SaveFilePath))
                {
                    File.Copy(SaveFilePath, BackupFilePath, overwrite: true);
                    Log("  ✓ Previous save backed up");
                }
                
                // 3. Replace save file with temp file
                // Note: File.Move doesn't support overwrite in .NET Standard 2.0
                if (File.Exists(SaveFilePath))
                    File.Delete(SaveFilePath);
                
                File.Move(TempFilePath, SaveFilePath);

                Log($"✓ Game saved (version {gameData.saveVersion})");
                
                // Trigger event
                GameEvents.TriggerGameSaved();
                
                return true;
            }
            catch (Exception e)
            {
                LogError($"Failed to save game: {e.Message}");
                
                // Clean up temp file if it exists
                if (File.Exists(TempFilePath))
                {
                    try { File.Delete(TempFilePath); } 
                    catch { /* Ignore cleanup errors */ }
                }
                
                return false;
            }
        }

        /// <summary>
        /// Load game data from disk with automatic backup recovery
        /// </summary>
        /// <returns>SaveData if successful, null if failed</returns>
        public SaveData LoadGame()
        {
            // Try to load from primary save
            SaveData saveData = LoadFromFile(SaveFilePath, "primary save");
            
            if (saveData != null)
                return saveData;
            
            // Primary failed - try backup
            LogWarning("Primary save corrupted or missing, attempting backup recovery...");
            saveData = LoadFromFile(BackupFilePath, "backup save");
            
            if (saveData != null)
            {
                LogWarning("✓ Recovered from backup!");
                
                // Restore backup as primary save
                try
                {
                    File.Copy(BackupFilePath, SaveFilePath, overwrite: true);
                    Log("  ✓ Backup restored as primary save");
                }
                catch (Exception e)
                {
                    LogError($"Failed to restore backup: {e.Message}");
                }
                
                return saveData;
            }
            
            // Both failed
            Log("Both primary and backup saves are corrupted or missing!");
            return null;
        }

        /// <summary>
        /// Delete save file and backup
        /// </summary>
        public bool DeleteSave()
        {
            bool deleted = false;

            // Delete primary save
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    File.Delete(SaveFilePath);
                    Log("✓ Primary save deleted");
                    deleted = true;
                }
                catch (Exception e)
                {
                    LogError($"Failed to delete primary save: {e.Message}");
                }
            }

            // Delete backup
            if (File.Exists(BackupFilePath))
            {
                try
                {
                    File.Delete(BackupFilePath);
                    Log("✓ Backup save deleted");
                    deleted = true;
                }
                catch (Exception e)
                {
                    LogError($"Failed to delete backup: {e.Message}");
                }
            }

            if (deleted)
            {
                GameEvents.TriggerSaveDeleted();
            }
            else
            {
                LogWarning("No save files to delete");
            }

            return deleted;
        }

        /// <summary>
        /// Create a new default save data
        /// </summary>
        public SaveData CreateNewSave()
        {
            Log("Creating new save data...");
            
            SaveData newSave = new SaveData
            {
                saveVersion = 1
            };
            
            return newSave;
        }
        #endregion

        #region Private Helpers
        
        /// <summary>
        /// Load save data from a specific file
        /// </summary>
        private SaveData LoadFromFile(string filePath, string fileDescription)
        {
            if (!File.Exists(filePath))
            {
                Log($"{fileDescription} not found");
                return null;
            }

            try
            {
                // Read file
                string json = File.ReadAllText(filePath);

                // Deserialize using Newtonsoft.Json
                SaveDataWrapper saveWrapper = JsonConvert.DeserializeObject<SaveDataWrapper>(json);

                if (saveWrapper?.gameData == null)
                {
                    LogError($"{fileDescription} is corrupted or invalid!");
                    return null;
                }

                Log($"✓ Loaded {fileDescription}");
                Log($"  Save version: {saveWrapper.saveVersion}");
                Log($"  Saved on: {saveWrapper.saveTimestamp}");
                Log($"  Playtime: {FormatPlaytime(saveWrapper.playtimeSeconds)}");
                
                // Trigger event only when loading primary save
                if (filePath == SaveFilePath)
                {
                    GameEvents.TriggerGameLoaded();
                }
                
                return saveWrapper.gameData;
            }
            catch (Exception e)
            {
                LogError($"Failed to load {fileDescription}: {e.Message}");
                return null;
            }
        }
        
        #endregion

        #region Save Data Wrapper
        
        /// <summary>
        /// Wrapper for save data with metadata
        /// </summary>
        [Serializable]
        private class SaveDataWrapper
        {
            public SaveData gameData;
            public int saveVersion;
            public string saveTimestamp;
            public float playtimeSeconds;
        }
        
        #endregion

        #region Utilities
        
        private string FormatPlaytime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";
            else if (time.TotalMinutes >= 1)
                return $"{time.Minutes}m {time.Seconds}s";
            else
                return $"{time.Seconds}s";
        }
        
        #endregion

        #region Editor Debug Tools
        #if UNITY_EDITOR
        
        private void Update()
        {
            // F12: Open save folder
            if (Input.GetKeyDown(KeyCode.F12))
            {
                OpenSaveFolder();
            }
            
            // F11: Delete save (for testing)
            if (Input.GetKeyDown(KeyCode.F11))
            {
                if (UnityEditor.EditorUtility.DisplayDialog(
                    "Delete Save?",
                    "This will delete the save file and backup. Continue?",
                    "Delete",
                    "Cancel"))
                {
                    DeleteSave();
                }
            }
        }

        [ContextMenu("Open Save Folder")]
        public void OpenSaveFolder()
        {
            if (!Directory.Exists(RootSavePath))
            {
                CreateFolderStructure();
            }

            Application.OpenURL("file://" + RootSavePath);
            Log("Opened save folder in explorer");
        }

        [ContextMenu("Delete Save File")]
        private void EditorDeleteSave()
        {
            DeleteSave();
        }

        [ContextMenu("Print Save Info")]
        private void PrintSaveInfo()
        {
            Debug.Log("=== SAVE MANAGER INFO ===");
            Debug.Log($"Initialized: {_isInitialized}");
            Debug.Log($"Root Path: {RootSavePath}");
            Debug.Log($"Game Data Path: {GameDataPath}");
            Debug.Log($"Save File: {SaveFilePath}");
            Debug.Log($"Backup File: {BackupFilePath}");
            Debug.Log($"Primary Save Exists: {File.Exists(SaveFilePath)}");
            Debug.Log($"Backup Exists: {File.Exists(BackupFilePath)}");
            
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    SaveDataWrapper wrapper = JsonConvert.DeserializeObject<SaveDataWrapper>(json);
                    Debug.Log($"Save version: {wrapper.saveVersion}");
                    Debug.Log($"Saved: {wrapper.saveTimestamp}");
                    Debug.Log($"Playtime: {FormatPlaytime(wrapper.playtimeSeconds)}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to read save: {e.Message}");
                }
            }
            
            Debug.Log("========================");
        }

        [ContextMenu("Create Test Save")]
        private void CreateTestSave()
        {
            SaveData testData = CreateNewSave();
            SaveGame(testData, 3600f); // 1 hour playtime
            Debug.Log("✓ Test save created");
        }

        [ContextMenu("Test Backup Recovery")]
        private void TestBackupRecovery()
        {
            if (!File.Exists(BackupFilePath))
            {
                Debug.LogError("No backup file exists to test recovery!");
                return;
            }

            // Temporarily corrupt primary save
            string corruptData = "CORRUPTED_DATA_TEST";
            File.WriteAllText(SaveFilePath, corruptData);
            Debug.Log("✓ Corrupted primary save for testing");

            // Try to load - should recover from backup
            SaveData recovered = LoadGame();
            
            if (recovered != null)
            {
                Debug.Log("✓ Backup recovery successful!");
            }
            else
            {
                Debug.LogError("ERROR: Backup recovery failed!");
            }
        }

        #endif
        #endregion

        #region Logging
        private void Log(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[SaveManager] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SaveManager] WARNING: {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SaveManager] ERROR: {message}");
        }
        #endregion
    }
}