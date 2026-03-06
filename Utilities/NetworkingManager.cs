using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;

namespace BingusNametagsPlusPlus.Utilities;

public static class NetworkingManager
{
	public static void SetNetworkedProperties()
	{
		if (ConfigManager.CustomNametags)
		{
            var color = ConfigManager.NetworkColor.First() == '#' ? ConfigManager.NetworkColor[1..] : ConfigManager.NetworkColor;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
			{
				{
					"BingusNametags++", new Dictionary<string, object>
					{
						{ "Color", color },
						{ "isBold", ConfigManager.NetworkBold },
						{ "isItalic", ConfigManager.NetworkItalic },
						{ "isUnderlined", ConfigManager.NetworkUnderline },
						{ "version", Constants.Version}
					}
				}
			});
        } else
		{
			PhotonNetwork.LocalPlayer.CustomProperties.Remove("BingusNametags++");
		}
	}
}