using BepInEx;
using UnityEngine;

namespace BingusNametagsPlusPlus;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class PluginBepInEx : BaseUnityPlugin
{
    private void Start()
    {
        GameObject bingusNametagsGameObject = new GameObject($"BingusNametagsPlusPlus");
        bingusNametagsGameObject.AddComponent<Main>();

        DontDestroyOnLoad(bingusNametagsGameObject);

        Constants.Loader = ModLoader.BepInEx;
    }
}
