# Changelog

## [Unreleased]

## [2.1.0]

### Forge version notes
- New: a search field in the flea market's Add Offer window (My Offers, + ADD OFFER). It sits above your stash grid next to AUTOSELECT SIMILAR. While you search, the grid shows only the matching items from your stash, gathered at the top; clear the field to get your full stash back. Your selection and the chosen category icon are kept.
- While you type in that field, ENTER and SPACE no longer post the offer, even with a mod such as UIFixes that binds them to posting. ESC clears the field instead of closing the window.

### Added
- Search field in the flea market's Add Offer window (`AddOfferWindow`), cloned from the game's stash search field and placed left of AUTOSELECT SIMILAR. While a query is set, the window's grid view shows a `ReferenceGrid` in a fake stash holding only the matching top-level stash items, the construction the game's own stash search uses; clearing the query shows the stash grid again and restores its scroll position. Rebuilds are debounced and follow stash changes made while the window is open.
- The offer selection survives the grid swaps: `RagfairNewOfferContext.DeselectItem`, which every killed offer item view calls, is skipped only while the search swaps grids. The selected category icon is restored with a tab click after each swap.
- Typing safety for that field: ESC clears and leaves the field instead of closing the window (`Window<DialogWindowContext>.TranslateCommand`, restricted to the Add Offer window), other game commands are blocked while it has focus, and `AddOfferWindow.AddOffer` is refused while it has focus or when it lost focus on the same ENTER/SPACE press.

### Changed
- The trader and Add Offer fields share one base controller for cloning the field, matching, focus tracking and re-placement; the trader search behaves as before.
- The letter/number keybind suppression now applies to both search fields.
- Harmony patch classes carry a `TraderSearch` prefix. SPT uses the class name as the Harmony id, and UIFixes has a patch class with the same former name on the same method.
- Patch registration continues with the remaining patches if one patch class fails to load.
- README updated for SPT 4.1; `CHANGELOG.md`, `LICENSE` and Forge sources added to the repository.

## [2.0.0]

### Forge version notes
- Supports SPT 4.1. No functional changes.

### Changed
- Ported to SPT 4.1 and its deobfuscated game assembly. The alias file for obfuscated type names is no longer needed.

## [1.0.0]

### Forge version notes
- Initial release: a search field in the trader window that filters the buy grid by item name.

### Added
- Search field between the assortment refresh button and the loyalty tabs, cloned from the game's stash search field.
- Filtering through the trader grid's own category filter, so the grid re-flows like it does for a category, composing with the loyalty tabs and categories.
- Typing safety: SPACE does not buy the selected item, ESC clears the field instead of closing the screen, letter/number keybinds do not fire while typing.
