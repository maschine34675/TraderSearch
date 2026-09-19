# TraderSearch

Find items by name where the game gives you no search: in the trader buy window and in the flea market's Add Offer window.

## Features

- **Trader window:** a search field between the assortment refresh button and the loyalty-level tabs. Typing narrows the trader's buy grid to matching items, and the grid closes up so the results sit together at the top.
- **Flea market, Add Offer window:** a search field above your stash grid, next to AUTOSELECT SIMILAR. While you search, the grid shows only the matching items from your stash, gathered at the top; clear the field and your stash is back where you left it.
- Matching uses the item's name and short name in your game language, ignoring upper/lower case. Part of a name is enough: "m855" finds both M855 and M855A1.
- The search works together with the game's own filters: the trader's loyalty tabs and category list, and the Add Offer window's category icons (the selected icon stays selected while the results change).
- The search clears itself when you switch traders, close the trader screen, or open the Add Offer window again.
- While you type, the game does not react to your keys: letters and numbers do not trigger keybinds, SPACE does not buy the selected trader item, and a key another mod binds to posting an offer does not post it. ESC clears the field instead of closing the screen or window; press ESC again to close as usual.

## Requirements and compatibility

- SPT: 4.1.x, tested on SPT 4.1.5.
- Components: client only; nothing to install on the server.
- Dependencies: none.
- UIFixes: both searches were tested together with UIFixes. While the Add Offer search field has focus, UIFixes' Enter/Space shortcut for posting offers does not post (see Usage).
- Fika: compatible. The mod only changes local menu screens and sends nothing over the network.

## Installation

1. Extract the release ZIP into your SPT installation directory.
2. Verify that `BepInEx/plugins/maschine-TraderSearch.dll` exists.

## Updating

Overwrite the existing `BepInEx/plugins/maschine-TraderSearch.dll`. There are no other files to remove.

## Usage

- **Trader window:** open a trader's buy screen, click the search field left of the loyalty tabs and type.
- **Flea market:** go to My Offers, click **+ ADD OFFER**, click the search field above your stash grid and type. The results appear after a short pause in typing; select them as usual. Items you selected stay selected while you change or clear the search.
- **ESC** clears the field and leaves it. Clicking anywhere else also leaves the field and keeps the text.
- While the Add Offer search field has focus, ENTER and SPACE never post the offer, even with a mod that binds them to posting. Leave the field first (click outside or press ESC).

## Configuration

None.

## Known limitations

- The trader search filters the buy grid only. For your own items on the sell side, use the game's own stash search (magnifier icon).
- The Add Offer search checks the items lying directly in your stash. Items inside cases or backpacks cannot be picked in that window anyway, so a case is matched by its own name, not by its contents.
- The Add Offer results only list items; to rearrange your stash by dragging, clear the search first.
- An item you selected before searching stays selected even when the results do not show it.
- With UIFixes' stash scroll synchronization turned on, closing the Add Offer window while a search is active can leave the synchronized stash scroll position where the results list was. Clearing the search before closing avoids it.
- Only item names are searched, not item descriptions or categories.

## Support

Include the exact mod and SPT versions, expected and actual behavior, short reproduction steps, and the complete client log (`BepInEx/LogOutput.log`). Report problems on the mod's Forge page or as a GitHub issue at https://github.com/maschine34675/TraderSearch/issues.

## License and credits

MIT, see [LICENSE](LICENSE).
