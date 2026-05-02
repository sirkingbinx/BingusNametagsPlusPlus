using BingusNametagsPlusPlus;
using MelonLoader;
using UnityEngine;


[assembly: MelonInfo(typeof(PluginMelonLoader), BingusNametagsPlusPlus.Constants.Name, BingusNametagsPlusPlus.Constants.Version, BingusNametagsPlusPlus.Constants.Author)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

namespace BingusNametagsPlusPlus;

public class PluginMelonLoader : MelonMod
{
    public override void OnLateInitializeMelon()
    {
        new GameObject($"BingusNametags++ {Constants.Version} [MelonLoader]").AddComponent<Main>();
        Constants.Loader = ModLoader.MelonLoader;
    }
}
