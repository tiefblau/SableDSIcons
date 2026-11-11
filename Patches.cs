using HarmonyLib;
using UnityEngine;

namespace SableDSIcons;

[HarmonyPatch]
public class Patches
{   
    private static int textureCounter = 0;

    [HarmonyPatch(typeof(SableGameManager), nameof(SableGameManager.LoadTitleCanvas)), HarmonyPostfix]
    public static void SableGameManagerLoadTitleCanvasPostfix()
    {
        if(Plugin.iconsAtlasReplaced == false)
        {
            Plugin.ReplaceIconsAtlas();
            Plugin.iconsAtlasReplaced = true;
        }
        else
        {
            Plugin.PatchResourcesLoad();
        }
    }

    // menu icons (ingame and title); dialog icons; icons in quest notifications
    [HarmonyPatch(typeof(Resources), nameof(Resources.Load), [typeof(string), typeof(Il2CppSystem.Type)]), HarmonyPrefix]
    public static void ResourcesLoadPrefix(ref string path)
    {
        if(Plugin.texturePaths.ContainsKey(path) == true)
        {
            path = Plugin.texturePaths[path];
            ++textureCounter;
        }

        if(textureCounter == Plugin.texturePaths.Count)
        {
            PluginLoader.Log.LogInfo("Menu icons replaced.");
            textureCounter = 0;
            Plugin.UnpatchResourcesLoad();
        }
    }

    // controls setting menu icon
    [HarmonyPatch(typeof(ConditionalImageSprite), nameof(ConditionalImageSprite.Awake)), HarmonyPostfix]
    public static void ConditionalImageSpriteAwakePostfix(ConditionalImageSprite __instance)
    {
        __instance.target.sprite = __instance.conditions[0].sprite;
        PluginLoader.Log.LogInfo("Settings icon replaced.");
    }
}
