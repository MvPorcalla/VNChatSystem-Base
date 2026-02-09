03_PhoneScreen
└── Canvas (Screen Space - Overlay)
    ├── PhoneRoot
    │   ├── WallpaperContainer
    │   │   └── WallpaperImage
    │
    │   ├── Screens (ONLY ONE ACTIVE AT A TIME)
    │   │   ├── HomeScreenPanel
    │   │   │   ├── AppGrid
    │   │   │   │   ├── AppButton_Chat
    │   │   │   │   ├── AppButton_Contacts
    │   │   │   │   ├── AppButton_Gallery
    │   │   │   │   └── AppButton_Settings
    │   │   │   └── Dock
    │   │   │       ├── AppButton_Phone
    │   │   │       └── AppButton_Messages
    │   │   ├── ChatAppPanel
    │   │   ├── ContactsPanel
    │   │   ├── GalleryPanel
    │   │   └── SettingsPanel
    │
    │   ├── NavigationBar
    │   │   ├── QuitButton
    │   │   ├── HomeButton
    │   │   └── BackButton
    │
    │   ├── Overlays (CAN STACK) (FUTURE IMPLEMENTATION)
    │   │   ├── NotificationPopup
    │   │   ├── ConfirmationDialog
    │   │   └── Tooltip
    │
    │   ├── Transitions (FUTURE IMPLEMENTATION)
    │   │   ├── FadeOverlay
    │   │   └── ScreenBlocker
    │
    └── EventSystem


    └── QuitConfirmationPanel ← CREATE THIS
        ├── Overlay (Image - Black with 50% alpha)
        ├── ConfirmPanel (Image - White background)
        │   ├── TitleText ("Quit Game?")
        │   ├── YesButton
        │   │   └── ButtonText ("Yes")
        │   └── NoButton
        │       └── ButtonText ("No")
        └── [QuitConfirmationManager] ← Attach script here
