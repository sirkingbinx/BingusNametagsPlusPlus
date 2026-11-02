# Localization
BingusNametags++ supports several languages because a surprising number of this mod's users aren't in English-speaking countries. I will accept basically any language additions, see the guide below.

## Language Selection
When determining what country languages to support, we often choose the majority or official language of that country. As an example, Norway's official language is Norwegian, however it has two major dialects: bokmål and nynorsk. Bokmål was selected as it is the most widely spoken dialect (85%) while nynorsk is much smaller in comparison (around 10-15%). The first countries were chosen based on userbase statistics; all the countries chosen were in the top 10 most active BingusNametags++ users.

Usage statistics were determined based on unique visitors to the [update server](https://updates.sirkingbinx.dev/version/nametags) in the past 30 days, as Cloudflare provides traffic analytics for all visitors. Most users leave Auto-Update enabled, which makes the analytics selected surprisingly informative about which countries contribute most to the traffic.

## Adding The Languages
Each language pack's file is named according to their [ISO 639 language code](https://en.wikipedia.org/wiki/List_of_ISO_639_language_codes). Copy [`UIStrings-en.resx`](https://github.com/sirkingbinx/BingusNametagsPlusPlus/blob/master/Localization/UIStrings-en.resx) to a new file and skip to [line 120](https://github.com/sirkingbinx/BingusNametagsPlusPlus/blob/master/Localization/UIStrings-en.resx#L120). Under the `<value>` element of each string, translate it to the target language. Keep your translations simple & stupid; informal language is recommended since it makes the mod seem less professional, since it really isn't.

If you don't understand the context, open the game and see where the string is used. Every string is used for the GUI panel (click the `Shift` key on the right of your keyboard), so you don't require a VR headset to check it's usage.

Once you're done, you can save it to the `Localization/` folder with a name following the format `UIStrings-##.resx`, where the `##` placeholder is replaced with your ISO 639 language code.

## Submitting the Pull Request
A **pull request** is a change request sent to project maintainers where the changes you want have been made and they are ready to be added to the project. Think of it as a contribution, like "hey i just did this thing, you can use it if you want."

Pull requests are submitted via GitHub. Account registration is free and only requires an email.

## Panel and preview strings
New panel strings use descriptive keys (`UI.*` and `Preview.*`) in the English resource.
Add the same keys to any language resource to translate them; missing keys fall back to
English. Preserve format placeholders such as `{0}` in `UI.Author`. New keys are not
split on hyphens. Legacy `t*` keys use `label - tooltip` (including spaces) where needed.
The canvas uses a compact, draggable window starting at the top left, scrollable
settings, and wrapping labels. The transparent nametag preview sits outside the
window to its right and follows when the window moves; it has no surrounding labels.
`Canvas.*` keys describe the new Nametag Studio interface. The original IMGUI
implementation has been replaced with Unity UI objects and mouse/keyboard input.

The live preview runs every enabled plugin's nametag callbacks against the local
player in isolated TextMeshPro objects. It uses the actual font and sprite assets,
plugin sizes, global and per-plugin offsets, and live player data. Custom default
nametag styles are applied immediately from the editor. Plugins that require a
remote player can fail independently; their preview shows an error while the rest
of the stack remains available. The view expands to fit large stacks. Camera
visibility settings apply in game, so they do not hide the editor preview.
