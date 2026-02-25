using BingusNametagsPlusPlus.Models;
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
			var nametagData = new BGNametagData {
				Color = color,
			};

			if (ConfigManager.NetworkBold)
				nametagData.Style |= BGNametagStyle.Bold;
			
			if (ConfigManager.NetworkItalic)
				nametagData.Style |= BGNametagStyle.Italic;

			if (ConfigManager.NetworkUnderline)
				nametagData.Style |= BGNametagStyle.Underline;

            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
				{{"BingusNametags++", nametagData }}
			);
        } else
		{
			PhotonNetwork.LocalPlayer.CustomProperties.Remove("BingusNametags++");
		}
	}
}