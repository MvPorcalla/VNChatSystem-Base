Got it. Here’s your **full, merged BubbleSpinner documentation** including the unlockable CG feature, fully formatted and ready to use:

---

# BubbleSpinner Chat Script Legend

**Engine / Format Name:** BubbleSpinner (bub)
**File Extension:** `.bub`
**Purpose:** Script files in this format contain dialogue, media, and branching choices for **chat-focused visual novels**. Each file can represent a chapter, scene, or node sequence. Each .bub file usually represents one chapter/scene to make file organization explicit.

These files support:

* Text bubbles
* Image/audio media bubbles
* Player choices and branching paths
* Variables and conditional dialogue
* Unlockable CGs saved to the gallery

**Example File Name:** `C1_Intro.bub`

### Note:

All character profile images, CGs, and media assets are loaded via Unity’s Addressables system. Do not hard-reference sprites in prefabs. The parser and ChatManager should request assets by their addressable keys (from the .bub `path:` field), then asynchronously load them before displaying in chat bubbles. Make sure image loading is asynchronous, with fallback/error handling if an asset cannot be found.

---

## **Legend for Chat Script Commands**

| Symbol / Command                               | Purpose                         | Notes/Usage                                                                                                |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `contact: Name`                                | **Contact Assignment**          | Defines which **character/contact** this node belongs to. Place at the top of each .bub file.             |
| Example: `contact: Elena`                      |                                 | “Place contact: at the top of each .bub file to define which NPC or chat thread this file represents.”    |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `title: NodeName`                              | **Node Header**                 | Marks the start of a conversation or branch node.                                                          |
| Example: `title: C1_Start`                     |                                 |                                                                                                            |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `---`                                          | **Node Separator**              | Separates node header from its content. Required after `title:`                                            |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `[Speaker]:`                                   | **Text Bubble**                 | Marks a line of dialogue. The parser displays it as a text bubble.                                         |
| Example: `NPC: "Hi! Let's go out"`             |                                 |                                                                                                            |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `>> media [Speaker] type:image path:`          | **CG/Image Bubble**             | Marks a line that shows a character image in a bubble. Can optionally **unlock the image for gallery**.    |
| -------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| Example: `>> media npc type:image path:NPC_CG1.png`                              | Standard image — shown in chat only, not saved to gallery.                                                 |
| Example: `>> media npc type:image unlock:true path:CGs/NPC_CG2.png`              | Unlockable CG — shown in chat and added to gallery.                                                        |
| -------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `-> ...`                                       | **Tap-to-Continue**             | Creates a **single continue button** (like a choice) that the player must tap to proceed.                  |
| Example: `-> ...`                              |                                 |                                                                                                            |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `>> choice`                                    | **Player Choice Block**         | Signals the start of a branching choice menu. Buttons are generated for each option.                       |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `-> "Choice Text"`                             | **Individual Choice**           | Defines a button with the text the player sees. The indented block after it is executed when chosen.       |
| Example: `-> "Let's meet at the park"`         |                                 |                                                                                                            |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `# Player:`                                    | **Player Dialogue**             | Lines that start with a speaker label (# Player:)                                                          |
|                                                |                                 | immediately following a choice are messages displayed in chat as if typed by the player.                   |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `<<jump NodeName>>`                            | **Jump to Node**                | Moves the script to another node. Used for branching paths or ending nodes.                                |
| Example: `<<jump C1_ParkNode>>`                |                                 |                                                                                                            |
| ---------------------------------------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `//`                                           | **Comment**                     | Anything after `//` is ignored by the parser. Useful for notes or instructions.                            |
| Example: `// Shows NPC happy in the bubble`    |                                 |                                                                                                            |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
---

## **Media Command Breakdown**

| Part            | Meaning                                                                                                                            |
| --------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `>> media`      | Marks this line as a **command to display media content** (image, audio, etc.) inside a chat bubble.                               |
| `[Speaker]`     | Specifies **who the bubble belongs to** — e.g., `NPC` or `Player`. Determines where the media is displayed.                        |
| `type:`         | Defines the **type of media** being shown. Examples: `image`, `audio`.                                                             |
| `path:`         | The **file path of the media file** to load and display/play.                                                                      |
| `unlock:true`   | Optional flag for images: <br>• Shows the image in chat<br>• Saves it to `PlayerData.unlockedCGs`<br>• Adds it to the gallery view |
| `npc_happy.png` | Example of an **image file path** (for an NPC’s image bubble).                                                                     |
| `npc_line1.ogg` | Example of an **audio file path** (for an NPC’s voice or sound bubble).                                                            |
| ---------------------------------------------------------------------------------------------------------------------------------------------------- |

---

## **Example bub File With Standard Image and CG Unlock**

```bub
contact: NPC_Elena

title: C1_Start
---
System: "4:50 PM"

NPC: "Hi! Let's go out."
NPC: "I want coffee."
>> media npc type:image unlock:true path:npc_CG1.png                    // Unlockable CG image
>> media npc type:image path:npc_happy.png                  // Standard image

-> ...                                                      // Tap to continue

Player: "Ok, let's go."
Player: "Blah blah."
>> media player type:image path:player_confident.png        // Player image

NPC: "Look at this sunset! 🌅"
>> media npc type:image unlock:true path:CGs/Date_Beach_Sunset.png  // Unlockable CG
NPC: "I'll never forget this moment."

>> choice
    -> "Let's meet at the park"
        # Player: "Let's meet at the park."
        <<jump C1_ParkNode>>

    -> "Let's meet at the cafe"
        # Player: "Let's meet at the cafe."
        <<jump C1_CafeNode>>

===

//=====================================
// PARK NODE
//=====================================

title: C1_ParkNode
---
NPC: "Tea drinkers are classy. 🍵"
NPC: "You have my respect."

-> ...                                                      // Tap to continue

Player: "Thank you."
NPC: "Anytime!"
>> media npc type:image path:npc_smug.png                   // Standard image

<<jump EndNode>>

===

//=====================================
// CAFE NODE
//=====================================

title: C1_CafeNode
---
NPC: "Coffee lovers unite! ☕"
NPC: "Here, take this cup of joy."

-> ...                                                      // Tap to continue

Player: "Thanks!"
NPC: "You're welcome!"
>> media npc type:image path:npc_winking.png                // Standard image

<<jump EndNode>>

===

//=====================================
// END NODE
//=====================================

title: EndNode
---
NPC: "Anyway, that’s the end of this little chat. Thanks for playing! 🎉"

```

---

### Notes

1. **`unlock:true` is optional**; if omitted, the image behaves as a standard chat-only media Image CG.
2. All paths should match Unity Addressables keys to avoid runtime errors.
3. Comments (`//`) can document each media bubble, including whether it’s a CG unlock.
4. Keep each node self-contained for clarity; each `.bub` file usually represents a single scene or chapter.

---