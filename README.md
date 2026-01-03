# BingusNametags++
BingusNametags++ is a simple, clean, but customizable nametag mod for Gorilla Tag. You can customize it to your needs with a very cool featureset:

- customize your own nametag with **bold** *italicized*, underlined, and colored text
- customize the font used by pasting a `.ttf` or `.otf` file into the same directory as the mod
- change visibility of certain items (such as platform/special user icons, the nametag itself)
- change offset, scale, and visibility of nametags
- change where the nametag is visible (for example, show in VR but not on the PC monitor)

It's also open-source, free, and doesn't take up half of your screen.

## Installation
[Download the latest release (`BingusNametags++.dll`) here](https://github.com/sirkingbinx/BingusNametagsPlusPlus/releases/latest) and copy it into `(Your Gorilla Tag Install Directory)\BepInEx\plugins`.
It has no dependencies, just paste it in and start using it.

## Customization
Press the `Right Shift` key to open the GUI. You can change every setting from the GUI.
- **Nametag**: Nametag behaviour settings
- **Text**: Text display settings
- **Icons**: Change how icons behave
- **Network**: Change how your nametag looks to other people
- **Plugins**: Toggle your installed plugins on/off.
- **About**: Mod version information, you'll probably never have to touch this.

## Plugin System (1.3.0+)
```cs
// As long as your plugin is loaded into the application domain, plugins should be detected and load automatically.
// If you use BepInEx and you have a BaseUnityPlugin, then you shouldn't have to worry about that.

public class MyNametag : IBaseNametag
{
	public string Name => "Your Plugin Name";
	public string Description => "Write a short description of your plugin here. It is shown to the user on the Plugins tab.";
	public string Author => "Me"; // this is you
	public float Offset => 0.5f; // This is the offset from the user's selected nametag offset. The default nametag's offset is 0f.
	public bool Enabled { get; set; } = true; // Whether your nametag is enabled.

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

		// Set the nametag text like this.
		nametag.Text = nametag.Owner.OwningNetPlayer.NickName;

		/*
		Nametag offset, size, and visibility is automatically determined by BingusNametags++.
		You can turn certain nametags (including the built-in nametag) on/off with the Plugins tab in settings.
		*/
	}
}
```