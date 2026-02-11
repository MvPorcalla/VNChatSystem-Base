I need help refactoring my Unity dialogue system (BubbleSpinner) to be a pure, reusable module with no game-specific dependencies.

**CONTEXT:**
- BubbleSpinner is a dialogue engine that should work in ANY Unity project
- ChatSim is my phone chat game that USES BubbleSpinner
- Currently BubbleSpinner has dependencies on ChatSim (GameBootstrap, GameEvents, SaveManager)
- This breaks modularity - I can't reuse BubbleSpinner in other projects

**MY GOAL:**

Make BubbleSpinner 100% standalone while keeping all functionality working in ChatSim.

**CURRENT STRUCTURE:**
Assets/Scripts/
├── BubbleSpinner/                    # ✅ PURE STANDALONE MODULE
│   ├── Core/
│   │   ├── IBubbleSpinnerCallbacks.cs       # 
│   │   ├── BubbleSpinnerParser.cs           # 
│   │   ├── DialogueExecutor.cs              # 
│   │   └── ConversationManager.cs           #
│   │
│   └── Data/
│       ├── MessageData.cs                   #
│       ├── ConversationAsset.cs             #
│       └── CharacterDatabase.cs             #
│
├── ChatSim/ 
    ├── Core/             
    │   ├── BubbleSpinnerBridge.cs        
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
        │   │   ├── BubbleSpinnerBridge.cs      # NEW: Integration layer
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

---

**WHAT I NEED:**

1. **Refactor BubbleSpinner to be pure:**
   - Remove all references to `GameBootstrap`, `GameEvents`, `SaveManager`
   - Replace with callbacks/events that ChatSim can plug into
   - Keep the same public API so my UI code doesn't break much

2. **Create ChatSim integration layer:**
   - Add callbacks in GameBootstrap to connect BubbleSpinner to save system
   - Bridge BubbleSpinner events to GameEvents

3. **Maintain existing functionality:**
   - Conversation save/load still works
   - Multiple conversations still supported
   - CG unlocking still works
   - My ChatAppController.cs code changes should be minimal

**CONSTRAINTS:**
- I want to keep ConversationManager in BubbleSpinner (it manages dialogue sessions)
- I DON'T want a separate ChatSim/Dialogue/ folder - keep it simple
- My UI should still call: `GameBootstrap.Conversation.StartConversation(asset)`

**FILES I'LL PROVIDE:**
I'll paste the current versions of:
1. BubbleSpinner/Core/ConversationManager.cs (the problematic file)
2. BubbleSpinner/Core/DialogueExecutor.cs (minor GameEvents dependency)
3. ChatSim/Core/GameBootstrap.cs (where integration should happen)

**WHAT I NEED FROM YOU:**
1. Refactored BubbleSpinner files (pure versions)
2. Updated GameBootstrap.cs with integration code
3. Migration guide showing what changed in my UI code
4. Step-by-step instructions on what to replace

Ready? I'll paste the files now.