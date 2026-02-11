
```
Assets/Scripts/
├── BubbleSpinner/                           # PURE STANDALONE MODULE
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
        │   │   ├── BubbleSpinnerBridge.cs      # Integration layer
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