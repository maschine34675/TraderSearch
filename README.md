# TraderSearch

Adds the search bar the trader window has always been missing. The stash and the flea market let you search for items by name - the trader buy screen does not. This mod puts a native-looking search field into the trader window header, between the assortment update button and the loyalty-level filter tabs.

## What it does

- Clones EFT's own stash search input field (native styling, native font) into the `TraderDealScreen` header between the **UpdateButton** and the **Loyalty Filter Panel**.
- Typing filters the trader's buy grid live by localized item name and short name (case-insensitive substring match, same semantics as the vanilla stash search).
- The search composes with the vanilla filters: the loyalty-level tabs and the handbook category tree are applied first, the search narrows their result.
- Filtering rides the game's own `FilterPanel` pipeline, so the grid re-flows compactly exactly like it does when you click a category - no gaps, no grid rebuilds per keystroke.
- The query is cleared automatically when you switch traders or close the trader screen.
- While the search field is focused, game input is suppressed: SPACE cannot accidentally buy the selected item, ESC clears/defocuses the field instead of closing the screen, and letter/number keybinds do not fire.

## Configuration

None. Click the field, type, done.

## Installation

Extract the release zip over your SPT game root (the folder containing `EscapeFromTarkov.exe`). It contains a single file:

```
BepInEx/plugins/maschine-TraderSearch.dll
```

## Limitations / fragility

- Client-only mod; no server component. Safe to add or remove at any time.
- The search filters the **buy** grid only. The sell side (your own items) already has the vanilla magnifier search.
- Obfuscated type names (`GClass2412`) are build-specific and isolated in `Aliases.cs`; an EFT/SPT update will likely require re-mapping them. All patches log registration failures to the BepInEx console/log instead of breaking the game.
- Compatible with UIFixes: its out-of-stock filter uses the same filter hook (both narrow the result independently) and its textbox keybind suppression overlaps harmlessly with this mod's.
