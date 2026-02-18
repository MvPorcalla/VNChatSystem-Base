03_PhoneScreen
└── Canvas (Screen Space - Overlay)
    ├── PhoneRoot
    │   ├── WallpaperContainer
    │   │   └── WallpaperImage
    │   │
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
    │   │   │
    │   │   ├── GalleryPanel                            ← ATTACH [GalleryController.cs] (Inactive in scene)
    │   │   │   ├── Header
    │   │   │   ├── ProgressText
    │   │   │   ├── ScrollView                          (active in scene)
    │   │   │   │   └── Viewport
    │   │   │   │       └── Content
    │   │   │   │           └── CGContainer
    │   │   │   │               ├── CharacterName
    │   │   │   │               └── CGGrid
    │   │   │   │                   └── CGThumbnail     ← ATTACH [GalleryThumbnailItem.cs] HERE (in prefab)
    │   │   │   │                       ├── Background
    │   │   │   │                       ├── ThumbnailImage
    │   │   │   │                       └── LockedOverlay
    │   │   │   └── GalleryFullscreenViewer             ← ATTACH [GalleryFullscreenViewer.cs] (Do not Put this panel Inactive) (active in scene)
    │   │   │       ├── BackgroundOverlay
    │   │   │       ├── ImageContainer
    │   │   │       │   └── CGImage
    │   │   │       └── TopBar
    │   │   │           ├── CloseButton
    │   │   │           └── CGNameText
    │   │   │
    │   │   ├── ContactsPanel                           ← ATTACH [ContactsAppPanel.cs] (Inactive in scene)
                ├── Header                              ← Empty GameObject (layout)
                │   ├── BackButton
                │   └── TitleText                       ← TextMeshProUGUI, text = "Contacts"
                │
                ├── ScrollView                          (active in scene)
                │   └── Viewport
                │       └── Content
                            └── ContactsAppItem         ← ATTACH [ContactsAppItem.cs]
                                ├── ProfileImage
                                ├── InfoGroup   
                                │   ├── NameText
                                │   └── BioText 
                                └── ResetButton 
                                    └── Text 

                ├── ContactsAppDetailPanel          ← ContactsAppDetailPanel.cs (active in scene) [FUTURE]
                │   ├── Overlay
                │   └── DetailCard
                │       ├── CloseButton
                │       ├── ProfileImage
                │       ├── NameText
                │       ├── InfoGroup
                │       │   ├── AgeText
                │       │   ├── BirthdateText
                │       │   ├── BioText
                │       │   └── DescriptionText
                │       └── ResetButton
                │           └── Text ("Reset Story")   
                │
                └── ResetConfirmationDialog             ← ATTACH [ResetConfirmationDialog.cs] (Do not Put this panel Inactive) (active in scene)
                    └── ConfirmationDialog
                        └── ContentPanel   
                            ├── TitleText
                            ├── MessageText 
                            ├── CancelButton
                            │   └── Text
                            └── ResetButton 
                                └── Text    

    │   │   └── SettingsPanel
    │   │
    │   ├── NavigationBar
    │   │   ├── QuitButton
    │   │   ├── HomeButton
    │   │   └── BackButton
    │   │
    │   ├── Overlays (CAN STACK) (FUTURE IMPLEMENTATION)
    │   │   ├── NotificationPopup
    │   │   ├── ConfirmationDialog
    │   │   └── Tooltip
    │   │
    │   ├── Transitions (FUTURE IMPLEMENTATION)
    │   │   ├── FadeOverlay
    │   │   └── ScreenBlocker
    │   │
    │   └── QuitConfirmationPanel ← CREATE THIS
    │       ├── Overlay (Image - Black with 50% alpha)
    │       ├── ConfirmPanel (Image - White background)
    │       │   ├── TitleText ("Quit Game?")
    │       │   ├── YesButton
    │       │   │   └── ButtonText ("Yes")
    │       │   └── NoButton
    │       │       └── ButtonText ("No")
    │       └── [QuitConfirmationManager] ← Attach script here
    │
    └── EventSystem
