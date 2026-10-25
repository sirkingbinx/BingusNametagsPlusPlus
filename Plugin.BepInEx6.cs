#if BEPINEX6_MONO

using BepInEx;
using UnityEngine;

namespace BingusNametagsPlusPlus;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        GameObject bingusNametagsGameObject = new GameObject("BingusNametagsPlusPlus");
        bingusNametagsGameObject.AddComponent<Main>();
        DontDestroyOnLoad(bingusNametagsGameObject);
    }
}

#endif