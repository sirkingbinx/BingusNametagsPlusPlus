using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;

namespace BingusNametagsPlusPlus.Utilities;

public static class Networking
{
	public static bool DoNetworking = false;

	public static void SetNetworkedProperties()
	{
		if (DoNetworking)
		{
            var color = ConfigManager.NametagColor.First() == '#' ? ConfigManager.NametagColor[1..] : ConfigManager.NametagColor;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
			{
				{
					"BingusNametags++", new Dictionary<string, object>
					{
						{ "Color", color },
						{ "isBold", ConfigManager.NetworkBold },
						{ "isItalic", ConfigManager.NetworkItalic },
						{ "isUnderlined", ConfigManager.NetworkUnderline },
						{ "version", Main.Instance?.Info.Metadata.Version.ToString() ?? "0.0.0" }
					}
				}
			});
        } else
		{
			PhotonNetwork.LocalPlayer.CustomProperties.Remove("BingusNametags++");
		}
	}
}