# 04_ChatApp scene

Canvas
│
├── ChatAppController [ChatAppController.cs] [ChatTimingController.cs] [ChatAutoScroller.cs]
└── PhoneRoot
    │
    ├── ContactListPanel [ContactListPanel.cs]
    │   │
    │   ├── Header
    │   │   └── Title
    │   │
    │   └── ContactScroll
    │       └── Viewport
    │           └── Content
    │               └── ContactListItem
    │                   ├── ProfileIMG
    │                   ├── ProfileName
    │                   └── Badge
    │
    └── ChatAppPanel
        │
        ├── ChatHeader
        │   ├── ChatBackButton
        │   ├── ChatProfileContainer
        │   │   └── ChatProfileIMG
        │   ├── ChatProfileName
        │   └── ChatModeToggle         <- Fast mode and normal speed mode toggle
        │       └── Icon
        │
        ├── ChatPanel
        │   ├── Viewport
        │   │   └── Content
        │   │       ├── SystemContainer             (Prefab)
        │   │       │   └── SystemBubble
        │   │       │       └── SystemMessage
        │   │       │
        │   │       ├── NpcChatContainer            (Prefab)
        │   │       │   └── NpcBubble
        │   │       │       └── NpcMessage
        │   │       │
        │   │       ├── NpcCGContainer              (Prefab)
        │   │       │   └── NpcBubble
        │   │       │       └── NpcImage
        │   │       │
        │   │       ├── TypingIndicator             [PooledObject.cs]
        │   │       │   └── TypingBubble
        │   │       │       └── TypingText
        │   │       │
        │   │       ├── PlayerChatContainer         (Prefab)
        │   │       │   └── PlayerBubble
        │   │       │       └── PlayerMessage
        │   │       │
        │   │       └── PlayerCGContainer           (Prefab)
        │   │           └── PlayerBubble
        │   │               └── PlayerImage
        │   │
        │   └── NewMessageIndicator
        │       ├── IndicatorBackground
        │       └── IndicatorText
        │
        └── ChatChoices                            
            └── ChoiceButton                       (Prefab)
                └── ButtonText

NavigationBar
└── ActionButton
    ├── QuitButton
    ├── HomeButton  
    └── BackButton
