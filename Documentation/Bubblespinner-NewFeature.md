Good point. Let me revise the TODO list with that context in mind — some of those features are more relevant than others for a phone chat sim specifically.

---

# BubbleSpinner — Future Features TODO
### (Phone Chat Messenger Simulation)

---

## High Priority — Core Chat Sim Feel

**Typing Indicators & Delays**
- `>> typing 2.0` — show "Elena is typing..." for X seconds before next bubble
- `>> delay 1.5` — silent pause between messages, no indicator
- Feels essential for making it feel like a real messenger

**Contact / Notification System**
- `>> status online/offline` — character appears online/offline in contact list
- `>> notify "Preview text"` — shows message preview on contact list screen
- `>> unread` — marks conversation as having unread messages
- This is what makes the phone OS feel alive between conversations

**Message Timestamps**
- Fix current issue where timestamps use parse time not display time
- `system: "9:42 AM"` already works but real clock feel needs display-time stamping

---

## Medium Priority — Story Depth

**Variables & State**
- `<<set $varName = value>>` — track affection, story flags, choices made
- `<<set $affection += 1>>` — relationship scoring
- Needs a `VariableStore` with save/load

**Conditionals**
- `<<if $varName>> ... <<endif>>` — show different lines based on past choices
- `<<else>>` — fallback branch
- Makes replays feel different, rewards player choices

**Choice Enhancements**
- Affection-gated choices — only appear if `$affection >= 3`
- One-time choices — disappear after selected once

---

## Lower Priority — Polish

**Audio**
- `type:audio` in `>> media` command
- Voice messages, notification sounds

**Metadata Tags**
- `#mood: happy` — NPC avatar expression change
- `#bgm: track_name` — background music trigger

**Multi-Character Chat**
- Group chat scenes with multiple NPCs in one `.bub` file
- Useful for friend group chats, story scenes

---

## Editor Tools
- `BubbleSpinnerValidator` — scan all `.bub` files, catch errors before runtime
- Auto-wiring — assign `.bub` files to correct `ConversationAsset` automatically

---

The top section is what separates a chat sim from a regular VN. Everything else is bonus.