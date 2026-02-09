# VNChatSystem-Base

**Messenger-Style Visual Novel Framework for Unity**

VNChatSystem is a **modular, production-ready phone chat simulation framework** built in **Unity** for narrative-driven games.
It powers messenger-style storytelling with branching dialogue, CG unlocks, and persistent save states—designed specifically for mobile-first visual novels.

At its core is **BubbleSpinner**, a fully standalone dialogue engine, paired with a complete phone UI simulation (lock screen → home → chat app).

---

## 📦 Requirements

## Unity Project Requirements

* **Engine:** Unity 2022.3.62f2 LTS (2D)
* **Target Platform:** Mobile (primary), PC support may come later
* **Version Control:** GitHub (Git)

## Packages:
* **TextMeshPro**
* **Addressables**
* **Newtonsoft.Json**

---

## 🚀 Key Highlights

* 📱 **Authentic phone chat UX** (lock screen, contacts, messenger flow)
* 🎭 **Standalone dialogue engine** (BubbleSpinner)
* 🌿 **Branching, choice-driven narratives**
* 💾 **Persistent saves & CG gallery tracking**
* 🧩 **Highly modular & reusable architecture**
* ⚡ **Optimized for mobile performance**

---

## ✨ Features

### 🎭 BubbleSpinner Dialogue Engine (Standalone)

* Custom **`.bub` script format** (human-readable, version-control friendly)
* Branching dialogue with conditional jumps
* Multi-chapter conversation support
* Pause / continue control (`-> ...`)
* Message read/unread tracking
* CG trigger & unlock system
* Persistent conversation state
* **Zero game-specific dependencies**

> BubbleSpinner can be extracted and reused in any Unity project.

---

### 📱 Phone Chat UI System

* Messenger-style chat bubbles with animation
* Typing indicators & message delay simulation
* Fast-mode toggle for repeat playthroughs
* Choice buttons with pooling
* Smart auto-scroll + new message indicator
* Contact list with avatars
* Integrated lock screen & phone home flow

---

### 💾 Save System

* JSON-based persistence
* Auto-save on pause, focus loss, and quit
* Throttled saving for performance
* Multi-conversation tracking
* CG gallery persistence

---

### 🎨 Asset & Performance

* Addressables for dynamic loading
* Object pooling (no runtime instantiation spikes)
* ScriptableObject-driven configuration

---

### 🔧 Developer Architecture

* Event-driven (decoupled systems via `GameEvents`)
* Centralized scene flow manager
* Bootstrap pattern (`DontDestroyOnLoad`)
* Context menu debug tools for rapid inspection

---

## 📂 Project Structure

```
Assets/Scripts/
├── BubbleSpinner/              # Standalone dialogue engine
│   ├── Core/
│   │   ├── BubbleSpinnerParser.cs
│   │   ├── DialogueFlowExecutor.cs
│   │   └── ConversationManager.cs
│   └── Data/
│       ├── DialogueNode.cs
│       ├── MessageData.cs
│       ├── ChoiceData.cs
│       ├── ConversationState.cs
│       └── ConversationAsset.cs
│
├── ChatSim/                    # Phone chat game implementation
│   ├── Core/
│   │   ├── GameBootstrap.cs
│   │   ├── GameEvents.cs
│   │   ├── SaveManager.cs
│   │   └── SceneFlowManager.cs
│   ├── Data/
│   │   └── SaveData.cs
│   └── UI/
│       ├── Chat/
│       │   ├── Controllers/
│       │   ├── Components/
│       │   └── Screens/
│       └── Phone/
│           ├── LockScreenController.cs
│           └── PhoneHomeController.cs
```

---

## 🧠 Dialogue Script Example (`.bub`)

```bub
title: Start
---
Alice: Hey! How was your day?
-> ...

title: ChoicePoint
---
Alice: Want to grab coffee tomorrow?
>> choice
-> "Sure"
    #Player: Sounds good!
    <<jump Happy>>
-> "I'm busy"
    #Player: Maybe another time.
    <<jump Sad>>
>> endchoice

title: Happy
---
>> media Alice path:CG/alice_happy.png unlock:true
Alice: Great! See you at 2 PM.
<<jump End>>

title: Sad
---
Alice: Oh… okay.
<<jump End>>

title: End
===
```

---

## 🧩 Starting a Conversation (Code)

```csharp
using ChatSim.Core;
using BubbleSpinner.Data;

public void OpenChat(ConversationAsset asset)
{
    GameBootstrap.Conversation.StartConversation(asset);
}
```

UI components automatically subscribe to executor events.

---

## 🔌 Using BubbleSpinner Standalone

Copy:

```
Assets/Scripts/BubbleSpinner/
```

Minimal usage:

```csharp
var executor = new DialogueExecutor();
executor.Initialize(asset, state);

executor.OnMessagesReady += DisplayMessages;
executor.OnChoicesReady += ShowChoices;
executor.OnConversationEnd += HandleEnd;

executor.ContinueFromCurrentState();
```

---

## 🎮 Scene Flow

### Required Scenes

1. **01_Bootstrap** – Persistent managers
2. **02_LockScreen**
3. **03_PhoneHome**
4. **04_ChatApp**

### Bootstrap Hierarchy

```
GameBootstrap
├── SaveManager
├── SceneFlowManager
└── ConversationManager
```

---

## 📘 `.bub` Syntax Reference

| Command     | Purpose                |
| ----------- | ---------------------- |
| `title:`    | Define node            |
| `<<jump>>`  | Jump to node           |
| `-> ...`    | Pause                  |
| `>> choice` | Begin choice           |
| `-> "text"` | Choice option          |
| `#Speaker:` | Player reply           |
| `>> media`  | Show image / unlock CG |
| `===`       | End file               |
| `//`        | Comment                |

---

## 🎯 Project Goals

* Rapid narrative prototyping
* Scalable multi-character VN architecture
* Reusable dialogue engine
* Clean separation of systems

Built as a **foundation**, not a one-off game.

---

## 🛠 Customization Notes

* Message timing → `ChatTimingController.cs`
* New message types → extend `MessageData`
* Save path → `SaveManager.cs`

---

## 📄 License

**All rights reserved.**
No redistribution or commercial use without permission.

---

## 🤝 Contributing

This is an internal base framework.
If extending:

* Keep BubbleSpinner isolated
* Document `.bub` extensions
* Optimize for large dialogue graphs

---

## 📬 Contact

**Melvin Porcalla**
GitHub: [https://github.com/MvPorcalla](https://github.com/MvPorcalla)
Email: [scryptid1@gmail.com](mailto:scryptid1@gmail.com)

---

**Built for narrative-first developers who care about structure, performance, and clean systems.**