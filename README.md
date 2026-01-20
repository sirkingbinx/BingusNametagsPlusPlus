# BingusNametags++ [![Downloads](https://img.shields.io/github/downloads/sirkingbinx/BingusNametagsPlusPlus/total)](https://github.com/sirkingbinx/BingusNametagsPlusPlus/releases/latest) [![Static Badge](https://img.shields.io/badge/chat-discord-%235865f2)](https://discord.gg/TYvMnt9KtC)

BingusNametags++ is a simple, clean, but customizable nametag mod for Gorilla Tag. You can customize it to your needs with a very cool featureset:

- customize your own nametag with **bold** *italicized*, underlined, and colored text
- customize the font used by pasting a `.ttf` or `.otf` file into the data directory
- change visibility of certain items (such as platform/special user icons and the nametags themselves)
- change offset, scale, and visibility of nametags
- change where the nametag is visible (for example, show in VR but not on the PC monitor)

It's also open-source, free, and doesn't take up half of your screen.

## Installation
[Download the latest release (`BingusNametags++.dll`) here](https://github.com/sirkingbinx/BingusNametagsPlusPlus/releases/latest) and copy it into `(Your Gorilla Tag Install Directory)\BepInEx\plugins`.
It has no dependencies, just paste it in and start using it.

## Configuration
> Custom fonts and plugins can only be customized manually, not through the GUI.

### UI (recommended)
Press the `Right Shift` key to open the GUI. You can change every setting from the GUI.
- **Nametag**: Nametag size and offset settings
- **Icons**: Change which icons are displayed
- **Network**: Change how your nametag looks to other people
- **Plugins**: Toggle your installed plugins on/off
- **About**: Mod version information, you'll probably never have to touch this.

### Manual
When BingusNametags++ is ran for the first time, it will create a directory (folder) for data (which we refer to as the "data directory" / "data folder"). This is located at `(Your Gorilla Tag Installation Folder)\BingusNametags++`. The directory layout looks like this:
- `BingusNametags++`: holds all this data
	- `nametags`: This is where custom nametags go, unless it's a standalone mod (meaning it does more than just a nametag)
 	- `logs`: This is where "logs" go, which store error information incase something goes wrong.
    - `config.cfg`: This is where your preferences are stored. It is recommended to use the in-game UI to change this.

To add a custom font, paste a font with the `.ttf` or `.otf` extension into the data directory. If running the game, press Refresh on the UI. You should see your font be used instead of the default (which is called JetBrains Mono for anyone wondering)

To add a custom nametag, if the file extension is `.nametag`, 99% of the time, that is a custom nametag and should be placed in the `nametags` folder inside of your data folder.

## Plugin System (1.3.3+)
> [!NOTE]
> To make it simpler for non-technical people to install nametags, I recommend you to change the file extension to `.nametag` after building to make it clear that it isn't a standalone mod.

```cs
// As long as your plugin is loaded into the application domain, plugins should be detected and load automatically.
// If you use BepInEx and you have a BaseUnityPlugin, then you shouldn't have to worry about that.

//                    [   plugin name   ][author][                                         description                                     ][offset][ unsupported ]
[BingusNametagsPlugin("Your Plugin Name", "Me", "Write a short description of your plugin here. It is shown to the user on the Plugins tab.", 0.5f, [  "Default"  ])]
public class MyNametag : IBaseNametag
{
	// Called every frame to update a nametag.
	public void UpdateNametag(PlayerNametag nametag)
	{
		// Styles are automatically applied when the tag text is set.
		// If you have styling tricks, do them before setting the nametag text with nametag.Text.
		// You can see a full list of tags here: https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html

		// Add a styling tag with TextMeshPro, eg. "b" = "bold"
		nametag.AddStyle("b");
		nametag.AddStyle("color", "#2b2b2b"); // for tags like <color=*>, use this syntax instead

		// remove a style if you don't need it anymore
		nametag.RemoveStyle("color");
		nametag.RemoveStyle("b");

		// Nametag size relative to the user's set nametag size. To use the default nametag size, set this to 1f.
		// Internally, this is multiplied by the user's nametag size to properly conform to customization settings.
		// It's better to think of this as a percentage of the normal nametag scale instead of the font size.
		nametag.Size = 0.85f;

		// Sprite sheets are used to add icons to a nametag.
		// https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/Sprites.html
		nametag.SpriteSheet = mySpriteSheet;

		// Set the nametag text like this.
		nametag.Text = nametag.Owner.OwningNetPlayer.NickName;

		/*
		Nametag visibility is automatically determined by BingusNametags++.
		You can turn certain nametags (including the built-in nametag) on/off with the Plugins tab in settings.
		*/
	}
}
```
