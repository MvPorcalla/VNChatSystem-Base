# BubbleSpinner — .bub Format Reference

---

## Syntax Legend

| Symbol | Meaning |
|--------|---------|
| `title:` | Declares a new node |
| `---` | Opens node content — must follow `title:` |
| `===` | Closes a node |
| `...` | Pure pacing pause — tap to continue, nothing sent |
| `Speaker: "text"` | Message bubble |
| `Player: "text"` | Implicit pause point — tap sends message, then NPC continues |
| `System: "text"` | Non-chat system message (timestamps, scene breaks) |
| `>> media` | Image bubble command |
| `>> choice` | Opens a choice block |
| `>> endchoice` | Closes a choice block (required) |
| `-> "text"` | Choice button — must be inside `>> choice` |
| `<<jump NodeName>>` | Jump to a node |
| `//` | Comment — inline or full line |

---

## File Structure

```
contact: CharacterName

title: NodeName
---
[content here]
===

title: NextNode
---
[content here]
===
```

---

## Commands

### `contact: Name`
Optional metadata. Validated against `ConversationAsset.characterName` at parse time. Mismatch logs a warning but does not stop the game.

---

### `title: NodeName`
Declares a dialogue node. Must be unique within the file. First node is typically `Start`.

> Cross-chapter nodes use the `_Ch2`, `_Ch3` suffix — e.g. `title: Start_Ch2`

---

### `---`
Opens node content. Must appear directly after `title:`. Parser warns if found elsewhere.

---

### `===`
Closes a node. Parser warns if a `>> choice` block is still open when `===` is reached — always close choice blocks with `>> endchoice` first.

---

### `[Speaker]: "Text"`
Text message bubble. Quotes are optional — parser strips them.

```
Sofia: "Hey!"
Player: "Hi!"
System: "9:42 AM"
```

---

### `Player: "text"`
Implicit pause point. Shows the continue button — tapping sends the message first, then NPC continues.

```
Sofia: "What do you think?"
Player: "I think it's great."
Sofia: "Really?"
```

Player lines can appear anywhere in a node. No `...` needed before them.

---

### `...`
Pure pacing pause. Shows the continue button — tapping resumes NPC flow, nothing is sent.

```
Sofia: "I have something to tell you."
...
Sofia: "I like you."
```

> Cannot be inside `>> choice`. Produces a warning and is ignored.

---

### `System: "Text"`
Non-chat system message (timestamps, scene breaks). Case-insensitive. Can appear anywhere in a node.

```
System: "Later that Day."
```

---

### `>> media [Speaker] type:image path:[key]`
Image bubble. `path:` must be a valid Addressables key. Place `path:` last.

```
>> media npc type:image path:Sofia/happy
```

### `>> media [Speaker] type:image unlock:true path:[key]`
Same as above but also unlocks the CG to the gallery and fires `OnCGUnlocked`. `unlock:true` must come before `path:`.

```
>> media npc type:image unlock:true path:Sofia/CG1
```

---

### `>> choice` / `>> endchoice`
Opens and closes a choice block. `>> endchoice` is required.

```
>> choice
    -> "Option A"
        <<jump NodeA>>

    -> "Option B"
        <<jump NodeB>>
>> endchoice
```

- Nested choice blocks not supported
- `...` inside a choice block is ignored with a warning
- Missing `<<jump>>` on a choice logs a warning
- Player message for a choice belongs at the top of the target node — not inside the choice block

```
// In the choice block — just the jump
>> choice
    -> "Ask how she's feeling"
        <<jump Node_Concern>>
>> endchoice

// In the target node — player message is the first line
title: Node_Concern
---
Player: "You sound troubled. What's on your mind?"
Sofia: "..."
===
```

---

### `-> "Choice Text"`
One choice button. Must be inside `>> choice`. Text in double quotes.

```
-> "Let's go to the park"
```

---

### `<<jump NodeName>>`
Jump to another node. If not found in the current file, BubbleSpinner advances to the next chapter file.

```
<<jump EndNode>>
<<jump Node_Concern>>
<<jump Start_Ch2>>
```

---

### `//`
Comment. Inline or full line. Also used as section dividers between nodes.

```
Sofia: "Hi!" // greeting

//=====================================
// CONCERN NODE
//=====================================
title: Node_Concern
```

---

## Full Node Example

```
title: Start
---
System: "9:42 AM"

Sofia: "Hey, good morning."
Sofia: "I wasn't sure if you'd be up yet."

>> media npc type:image unlock:true path:Sofia/CG1

...

Sofia: "I've been thinking about something."
Sofia: "Can I ask you something personal?"

Player: "Of course."

Sofia: "Do you ever feel like you're just going through the motions?"

>> choice
    -> "Sometimes, yeah."
        <<jump Node_Honest>>

    -> "Not really."
        <<jump Node_Deflect>>
>> endchoice

===

title: Node_Honest
---
Player: "Sometimes, yeah. More than I'd like to admit."

Sofia: "Yeah."
Sofia: "Me too."

...

<<jump EndNode>>

===

title: Node_Deflect
---
Player: "Not really. Why do you ask?"

Sofia: "No reason."
Sofia: "Never mind."

...

<<jump EndNode>>

===

title: EndNode
---
Sofia: "Thanks for listening."

System: "Later that evening."

<<jump Start_Ch2>>

===
```

---

## Known Limitations

- `type:audio` not implemented — falls through to unrecognized line warning
- Variables (`<<set>>`) and conditionals (`<<if>>`) not yet implemented
- Cross-chapter warning suppression only covers Ch2–Ch5
- Timestamps assigned at parse time, not display time
- Message history grows with playtime — save file grows accordingly
- `contact:` mismatch is warning only, game continues
- Nested choice blocks not supported
- Two choices jumping to the same node with different player messages require separate routing nodes