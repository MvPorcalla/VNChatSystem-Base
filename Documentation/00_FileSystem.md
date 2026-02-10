```
Assets/Scripts/
├── BubbleSpinner/                    # ✅ NEW: Standalone dialogue module
│   ├── Core/
│   │   ├── BubbleSpinnerParser.cs           # Parses .bub files
│   │   ├── DialogueExecutor.cs              # Executes nodes, handles flow
│   │   └── ConversationManager.cs           # Integrates with GameBootstrap
│   │
│   ├── Data/
│   │   ├── DialogueNode.cs                  # Node structure
│   │   ├── MessageData.cs                   # Message/Choice/CG data
│   │   └── ConversationAsset.cs             # ScriptableObject (replaces NPCChatData)
│   │
│   └── Events/
│       └── DialogueEvents.cs                # Dialogue-specific events (Dont have this yet)
│
├── Core/                              
│   ├── GameBootstrap.cs              
│   ├── GameEvents.cs                 
│   ├── SaveManager.cs                 
│   ├── SceneFlowManager.cs
│   └── Scenename.cs
│
├── Data/
│   ├── Config/
│   │   └── 
│   └── SaveData.cs
│   
└── UI/
    ├── ChatAppUI/
    │   ├── Components/
    │   │   ├── AutoResizeText
    │   │   ├── ChoiceButton.cs             <- Individual choice button
    │   │   └── MessageBubble.cs            <- Individual bubble behavior
    │   │
    │   ├── Core/
    │   │   ├── BubbleSpinnerBridge.cs      # ✅ NEW: Integration layer
    │   │   ├── ChatAppController.cs        <- Main controller (interfaces with BubbleSpinner)
    │   │   ├── ChatAutoScroll.cs           <- 
    │   │   ├── ChatChoiceDisplay.cs        <- Handles choice button spawning
    │   │   ├── ChatMessageDisplay.cs       <- Handles message bubble spawning/animation
    │   │   ├── ChatTimingController.cs     <- 
    │   │   ├── PooledObject.cs             <- 
    │   │   └── PoolingManager.cs           <- 
    │   │
    │   └── Panels/
    │       ├── ContactListPanel.cs         <- Contact list UI
    │       ├── ChatAppPanel.cs             <- future
    │       └── CharacterButton.cs          <- Contact list item

    ├── UIManager
    │   ├── ChatAppUIManager.cs
    │   ├── LockScreenUIManager.cs
    │   └── PhoneScreenManager.cs

    └── DisclaimerController.cs
```