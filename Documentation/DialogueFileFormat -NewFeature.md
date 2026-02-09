## **Legend for Chat Script Commands**

| Symbol / Command    | Purpose                 | Notes / Usage                                                                               |
|-------------------------------------------------------------------------------------------------------------------------------------------  |

# Future Feature Improvement
Optional: Variables can be used later for relationship tracking, conditional dialogue, or story flags.

| `<<set $variable = value>>`                            | **Set Variable**      | Assigns a value to a variable. Used to track relationships, choices, or events.
| `<<set $variable += 1>>`                               | **Modify Variable**   | Increments or changes a variable’s value.
| `<<if $variable == value>> ... <<else>> ... <<endif>>` | **Conditional Check** | Shows dialogue depending on a variable’s value. Can branch story dynamically.
| `>> media [Speaker] type:audio path:`                  | **Audio/Voice Bubble** | Plays an audio clip for the specified speaker. Typically used for NPC voice messages.
| Example: `>> media npc type:audio path:npc_line1.ogg`                                                                                       |

---


# Future Feature example

### Variables and Conditions

This feature allows your game to remember player choices and change the story accordingly. It's the most effective way to make a **linear chat experience feel personal and dynamic**. You can use a simple variable system to track things like a relationship score or a key story event.

  * **Syntax:**
      * **Setting a variable:** `<<set $variableName = value>>`
      * **Changing a variable:** `<<set $variableName += 1>>`
      * **Conditional dialogue:** `<<if $variableName == value>>`
  * **Purpose:** The parser checks the condition (`if`) and only shows the dialogue inside the block if the condition is true. The `else` block is used for when the condition is false. This allows you to have a single conversation node that can display different text depending on previous player actions.

**Example:**

```
>> choice
    -> "I had a great day!"
        <<set $relationship += 1>>
        Player: "It was great! Thanks for asking."

...

title: End of day
---
<<if $relationship > 0>>
    NPC: "It sounds like you had a fun day. I'm happy for you."
<<else>>
    NPC: "I hope tomorrow is better for you."
<<endif>>
```

-----

# unity chatapp heirarchy
## Note:
- Performance: For the ScrollView, consider adding a ScrollRect.movementType = Clamped and potentially object pooling if you expect very long conversations.

- Dynamic Max Width Based on Screen Size
put in autosize text script 
## You could make the max width responsive:

```

Start()
{
    // Adjust max width based on screen width
    float screenWidth = Screen.width;
    maxWidth = Mathf.Min(maxWidth, screenWidth * 0.8f); // 80% of screen width
    
    SetupLayoutElement();
    UpdateWidth();
}

```

Here’s a clean note you can keep as a **future reference** so you can jump back to this feature idea later:

---

## 📌 Multi-Character Chat Feature — MugiNovel System

**Goal:** Allow `.mugi` chat scripts to support multiple NPCs in the same story, with each NPC tracked and displayed in the correct bubble style without creating separate ChatAppPanels.

---

### **Key Changes Needed**

1. **Update `.mugi` format**

   * Add explicit speaker tags:

     ```
     npc:   // First NPC
     npc2:  // Second NPC
     npc3:  // Additional NPCs if needed
     player:
     ```
   * Keep `contact:` as **story identifier** rather than a single character.

2. **Update `NPCChatData.cs`**

   * Store multiple NPCs in the same story asset:

     ```csharp
     public string storyName;
     public string npc1Name;
     public AssetReference profileImageNpc1;
     public string npc2Name;
     public AssetReference profileImageNpc2;
     public List<TextAsset> mugiChapters;
     ```

3. **Update `ChatManager.cs` parser**

   * Map each speaker tag to the correct NPC profile name and image.
   * Example:

     ```csharp
     if (line.StartsWith("npc:"))
         CreateBubble(chatData.npc1Name, chatData.profileImageNpc1, content);
     else if (line.StartsWith("npc2:"))
         CreateBubble(chatData.npc2Name, chatData.profileImageNpc2, content);
     else if (line.StartsWith("player:"))
         CreateBubble("Player", playerProfileImage, content);
     ```

4. **Story Example (.mugi)**

   ```
   contact: Story_BardAndMuse

   title: Node_Wow
   ---
   npc: "Blah blah"
   npc: "Blah blah"
   player: "Blah blah"
   npc: "Blah blah"

   <<jump Node_Friend>>

   ===

   title: Node_Friend
   ---
   npc2: "Blah blah"
   npc2: "Blah blah"
   player: "Blah blah"
   npc2: "Blah blah"

   <<jump Node_Coco>>

   ===
   ```

---

### **Benefits**

* Supports multi-character stories without duplicate panels.
* Keeps all chapters of a story in one place.
* Makes `.mugi` flexible and extensible for future expansions.

---

💡 **Future Improvements:**

* Support dynamic NPC count (`npc3`, `npc4`, etc.) via dictionary mapping instead of hardcoding.
* Add metadata in `.mugi` for NPC roles and emotions.

---

If you want, I can also make a **short, searchable title for this note** so it’s easier to find later in your project notes. That way, you can quickly search `"MugiNovel multi-character system"` and find this instantly.

Do you want me to do that?
