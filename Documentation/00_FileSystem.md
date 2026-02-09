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

---

Assets/Scripts/
│
├── BubbleSpinner/                          # ✅ STANDALONE MODULE (keep separate)
│   ├── Core/
│   │   ├── BubbleSpinnerParser.cs          # ✅ KEEP (brand name makes sense now)
│   │   ├── DialogueExecutor.cs             # ⚠️ Consider: DialogueFlowExecutor.cs
│   │   └── ConversationManager.cs          # ⚠️ PROBLEM: Too tightly coupled to ChatSim
│   │
│   └── Data/
│       ├── DialogueNode.cs                 # ✅ KEEP
│       ├── MessageData.cs                  # ✅ KEEP
│       ├── ChoiceData.cs                   # ✅ KEEP (split from MessageData.cs)
│       ├── ConversationState.cs            # ✅ KEEP (split from MessageData.cs)
│       ├── ConversationAsset.cs            # ✅ KEEP (this IS the right name)
│       └── CharacterDatabase.cs            # ❌ MOVE (this is ChatSim-specific)
│
├── ChatSim/                                # Your game-specific code
│   ├── Core/
│   │   ├── GameBootstrap.cs
│   │   ├── GameEvents.cs
│   │   ├── SaveManager.cs
│   │   ├── SceneFlowManager.cs
│   │   └── SceneNames.cs
│   │
│   ├── Dialogue/                           # ✨ NEW: ChatSim's BubbleSpinner integration layer
│   │   ├── ConversationStateManager.cs     # ✨ MOVE ConversationManager here & rename
│   │   └── CharacterDatabase.cs            # ✨ MOVE from BubbleSpinner
│   │
│   ├── Data/
│   │   └── SaveData.cs
│   │
│   └── UI/
│       ├── Chat/
│       │   ├── Components/
│       │   │   ├── MessageBubble.cs
│       │   │   ├── ChoiceButton.cs
│       │   │   └── AutoResizeText.cs
│       │   │
│       │   ├── Controllers/
│       │   │   ├── ChatViewController.cs           # Rename from ChatAppController
│       │   │   ├── MessageDisplayController.cs     # Rename from ChatMessageDisplay
│       │   │   ├── ChoiceDisplayController.cs      # Rename from ChatChoiceDisplay
│       │   │   ├── ChatAutoScroll.cs
│       │   │   └── ChatTimingController.cs
│       │   │
│       │   ├── Pooling/
│       │   │   ├── PooledObject.cs
│       │   │   └── PoolingManager.cs
│       │   │
│       │   └── Screens/
│       │       ├── ContactListScreen.cs            # Rename from ContactListPanel
│       │       ├── ChatScreen.cs
│       │       └── ContactButton.cs                # Rename from CharacterButton
│       │
│       ├── Phone/
│       │   ├── LockScreenController.cs             # Rename from LockScreenUIManager
│       │   ├── PhoneHomeController.cs              # Rename from PhoneScreenManager
│       │   └── ChatAppUIBridge.cs                  # Rename from ChatAppUIManager
│       │
│       └── DisclaimerController.cs