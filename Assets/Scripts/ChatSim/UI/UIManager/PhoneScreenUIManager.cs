// ════════════════════════════════════════════════════════════════════════
// Assets/Scripts/UI/PhoneScreenUIManager.cs
// Phone Chat Simulation Game - Home Screen & App Launcher
// ════════════════════════════════════════════════════════════════════════

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ChatSim.Core;

namespace ChatSim.UI.UIManager
{
    [System.Serializable]
    public class AppButton
    {
        public bool enabled = true;
        public string appName;
        public Button button;
        public string targetScene;
        public GameObject targetPanel;
    }

    public class PhoneScreenUIManager : MonoBehaviour
    {
        public static PhoneScreenUIManager Instance { get; private set; }

        [Header("Home Screen Panel")]
        [SerializeField] private GameObject homeScreenPanel;
        
        [Header("App Buttons")]
        [SerializeField] private List<AppButton> apps = new List<AppButton>();

        [Header("Navigation Buttons")]
        [SerializeField] private Button quitButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button backButton;

        [Header("Quit Confirmation")]
        [SerializeField] private GameObject quitConfirmationPanel;
        [SerializeField] private Button yesQuitButton;
        [SerializeField] private Button noQuitButton;

        private GameObject currentPanel;
        private Stack<GameObject> panelHistory = new Stack<GameObject>();

        private void Awake()
        {
            if (Instance == null) 
                Instance = this;
            else 
                Destroy(gameObject);
        }

        private void Start()
        {
            InitializeNavigation();
            InitializeApps();
            ShowHomeScreen();
        }

        private void InitializeNavigation()
        {
            // Quit button
            if (quitButton != null)
                quitButton.onClick.AddListener(() => quitConfirmationPanel?.SetActive(true));
            else
                Debug.LogWarning("[PhoneScreenUIManager] QuitButton not assigned!");

            // Quit confirmation
            if (yesQuitButton != null)
                yesQuitButton.onClick.AddListener(OnConfirmQuit);
            
            if (noQuitButton != null)
                noQuitButton.onClick.AddListener(() => quitConfirmationPanel?.SetActive(false));

            // Home button
            if (homeButton != null)
                homeButton.onClick.AddListener(GoHome);
            else
                Debug.LogWarning("[PhoneScreenUIManager] HomeButton not assigned!");

            // Back button
            if (backButton != null)
                backButton.onClick.AddListener(GoBack);
            else
                Debug.LogWarning("[PhoneScreenUIManager] BackButton not assigned!");

            // Hide quit panel initially
            if (quitConfirmationPanel != null)
                quitConfirmationPanel.SetActive(false);
        }

        private void OnConfirmQuit()
        {
            Debug.Log("[PhoneScreenUIManager] Quitting game...");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void InitializeApps()
        {
            // Hide all app panels initially
            foreach (var app in apps)
            {
                if (app.targetPanel != null)
                    app.targetPanel.SetActive(false);
            }

            // Setup app buttons
            foreach (var app in apps)
            {
                if (!app.enabled) 
                { 
                    app.button.gameObject.SetActive(false); 
                    continue; 
                }
                
                // Capture variables for closure
                string scene = app.targetScene;
                GameObject panel = app.targetPanel;
                string name = app.appName;
                
                app.button.onClick.AddListener(() => {
                    GameEvents.TriggerAppOpened(name);
                    
                    if (!string.IsNullOrEmpty(scene))
                    {
                        // Load new scene
                        GameBootstrap.SceneFlow.LoadScene(scene);
                    }
                    else if (panel != null)
                    {
                        // Open panel in current scene
                        OpenPanel(panel);
                    }
                });
            }
            
            Debug.Log($"[PhoneScreenUIManager] Initialized {apps.Count} app buttons");
        }

        private void ShowHomeScreen()
        {
            if (homeScreenPanel != null)
            {
                homeScreenPanel.SetActive(true);
                currentPanel = homeScreenPanel;
            }
            else
            {
                Debug.LogWarning("[PhoneScreenUIManager] HomeScreenPanel not assigned!");
            }
        }

        private void OpenPanel(GameObject panel)
        {
            if (currentPanel != null && currentPanel != homeScreenPanel)
            {
                panelHistory.Push(currentPanel);
                currentPanel.SetActive(false);
            }
            else if (currentPanel == homeScreenPanel)
            {
                homeScreenPanel.SetActive(false);
            }

            panel.SetActive(true);
            currentPanel = panel;
            
            Debug.Log($"[PhoneScreenUIManager] Opened panel: {panel.name}");
        }

        public void GoHome()
        {
            if (currentPanel != null && currentPanel != homeScreenPanel)
                currentPanel.SetActive(false);
            
            CloseAllPanels();
            
            if (homeScreenPanel != null)
            {
                homeScreenPanel.SetActive(true);
                currentPanel = homeScreenPanel;
            }
            
            Debug.Log("[PhoneScreenUIManager] Returned to home screen");
        }

        public void GoBack()
        {
            if (panelHistory.Count == 0)
            {
                // If no history, just go home
                GoHome();
                return;
            }

            if (currentPanel != null)
                currentPanel.SetActive(false);

            currentPanel = panelHistory.Pop();
            currentPanel.SetActive(true);

            Debug.Log($"[PhoneScreenUIManager] Back to panel: {currentPanel.name}");
        }

        private void CloseAllPanels()
        {
            while (panelHistory.Count > 0)
            {
                var panel = panelHistory.Pop();
                if (panel != null) panel.SetActive(false);
            }
        }
    }
}