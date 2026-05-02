using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;

namespace BingusNametagsPlusPlus.Utilities;

public static class NetworkingManager
{
	public static void SetNetworkedProperties()
	{
		if (Config.Current.CustomNametags)
		{
            var color = Config.Current.NetworkColor.First() == '#' ? Config.Current.NetworkColor[1..] : Config.Current.NetworkColor;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
			{
				{
					"BingusNametags++", new Dictionary<string, object>
					{
						{ "Color", color },
						{ "isBold", Config.Current.NetworkBold },
						{ "isItalic", Config.Current.NetworkItalic },
						{ "isUnderlined", Config.Current.NetworkUnderline },
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