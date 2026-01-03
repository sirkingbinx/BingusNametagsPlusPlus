using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;

namespace BingusNametagsPlusPlus.Utilities;

public static class Networking
{
	public static bool DoNetworking = true;

	public static void SetNetworkedProperties()
	{
		if (DoNetworking)
		{
            var color = Config.NametagColor.First() == '#' ? Config.NametagColor[1..] : Config.NametagColor;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
			{
				{
					"BingusNametags++", new Dictionary<string, object>
					{
						{ "Color", color },
						{ "isBold", Config.NetworkBold },
						{ "isItalic", Config.NetworkItalic },
						{ "isUnderlined", Config.NetworkUnderline },
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