#if MELONLOADER

using BingusNametagsPlusPlus;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Plugin), BingusNametagsPlusPlus.Constants.Name, BingusNametagsPlusPlus.Constants.Version, "SirKingBinx", "https://monkeforge.org/view/bnametags")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

namespace BingusNametagsPlusPlus;

public class Plugin : MelonMod
{
    public override void OnInitializeMelon()
    {
        GameObject bingusNametagsGameObject = new GameObject("BingusNametagsPlusPlus");
        bingusNametagsGameObject.AddComponent<Main>();
        Object.DontDestroyOnLoad(bingusNametagsGameObject);
    }
}

#endif