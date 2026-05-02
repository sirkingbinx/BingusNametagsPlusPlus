using BepInEx;
using UnityEngine;

namespace BingusNametagsPlusPlus;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class PluginBepInEx : BaseUnityPlugin
{
    private void Start()
    {
        new GameObject($"BingusNametags++ {Constants.Version} [BepInEx]").AddComponent<Main>();
        Constants.Loader = ModLoader.BepInEx;
    }
}
