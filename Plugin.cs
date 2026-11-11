using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SableDSIcons;

public class Plugin : MonoBehaviour
{
    internal static bool iconsAtlasReplaced = false;

    private static readonly string resourcePath = "Textures/UI/ButtonIconPacks/";
    
    internal static readonly Dictionary<string, string> texturePaths = new()
    {
        {$"{resourcePath}Xbox One/XboxOne_A", $"{resourcePath}PS5/PS5_Cross"},
        {$"{resourcePath}Xbox One/XboxOne_B", $"{resourcePath}PS5/PS5_Circle"},
        {$"{resourcePath}Xbox One/XboxOne_Dpad_Down", $"{resourcePath}PS5_Alt/PS5Icon_Dpad_Down"},
        {$"{resourcePath}Xbox One/XboxOne_Dpad_Left", $"{resourcePath}PS5_Alt/PS5Icon_Dpad_Left"},
        {$"{resourcePath}Xbox One/XboxOne_Dpad_Right", $"{resourcePath}PS5_Alt/PS5Icon_Dpad_Right"},
        {$"{resourcePath}Xbox One/XboxOne_Dpad_Up", $"{resourcePath}PS5_Alt/PS5Icon_Dpad_Up"},
        {$"{resourcePath}Xbox One/XboxOne_LB", $"{resourcePath}PS5_Alt/PS5Icon_LB"},
        // {$"{resourcePath}Xbox One/XboxOne_Left_Stick", $"{resourcePath}PS5_Alt/PS5Icon_Left_Stick"},
        {$"{resourcePath}Xbox One/XboxOne_LT", $"{resourcePath}PS5_Alt/PS5Icon_LT"},
        {$"{resourcePath}Xbox One/XboxOne_Menu", $"{resourcePath}PS5_Alt/PS5Icon_Menu"},
        {$"{resourcePath}Xbox One/XboxOne_RB", $"{resourcePath}PS5_Alt/PS5Icon_RB"},
        // {$"{resourcePath}Xbox One/XboxOne_Right_Stick", $"{resourcePath}PS5_Alt/PS5Icon_Right_Stick"},
        {$"{resourcePath}Xbox One/XboxOne_RT", $"{resourcePath}PS5_Alt/PS5Icon_RT"},
        {$"{resourcePath}Xbox One/XboxOne_Windows", $"{resourcePath}PS5_Alt/PS5Icon_Windows"},
        {$"{resourcePath}Xbox One/XboxOne_X", $"{resourcePath}PS5/PS5_Square"},
        {$"{resourcePath}Xbox One/XboxOne_Y", $"{resourcePath}PS5/PS5_Triangle"}
    };

    private static readonly Harmony harmony = new($"{MyPluginInfo.PLUGIN_GUID}");

    private static readonly MethodInfo originalResourcesLoad = typeof(Resources).GetMethod(nameof(Resources.Load), [typeof(string), typeof(Il2CppSystem.Type)]);
    private static readonly MethodInfo patchedResourcesLoad = typeof(Patches).GetMethod(nameof(Patches.ResourcesLoadPrefix));

    private UnityAction<Scene, LoadSceneMode> onSceneLoaded;

    public void OnEnable()
    {
        onSceneLoaded = (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
        SceneManager.sceneLoaded += onSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        harmony.PatchAll();
        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    internal static void PatchResourcesLoad()
    {
        harmony.Patch(originalResourcesLoad, prefix: new HarmonyMethod(patchedResourcesLoad));
    }

    internal static void UnpatchResourcesLoad()
    {
        harmony.Unpatch(originalResourcesLoad, patchedResourcesLoad);
    }

    // ingame action icons or icons in dialogs reffering to ingame actions; icons in the controls setting 
    // (the game loads two identical XboxControllerIconsAtlas for some reason… better modify both)
    internal static void ReplaceIconsAtlas()
    {            
        var spriteAssets = Resources.FindObjectsOfTypeAll<TMP_SpriteAsset>();

        List<TMP_SpriteAsset> xboxIconAtlas = []; 
        TMP_SpriteAsset playstationIconAtlas = null; 

        for(int counter = 0; counter < spriteAssets.Length; ++counter)
        {
            switch(spriteAssets[counter].name)
            {
                case "XboxControllerIconsAtlas":
                    xboxIconAtlas.Add(spriteAssets[counter]);
                    break;

                case "PlaystationControllerIconsAtlas":
                    playstationIconAtlas = spriteAssets[counter];
                    break;
            }
        }

        for(int counter = 0; counter < xboxIconAtlas.Count; ++counter)
        {
            xboxIconAtlas[counter].hashCode = playstationIconAtlas.hashCode;
            xboxIconAtlas[counter].material = playstationIconAtlas.material;
            xboxIconAtlas[counter].materialHashCode = playstationIconAtlas.materialHashCode;
            xboxIconAtlas[counter].fallbackSpriteAssets = playstationIconAtlas.fallbackSpriteAssets;
            xboxIconAtlas[counter].spriteInfoList = playstationIconAtlas.spriteInfoList;
            xboxIconAtlas[counter].spriteSheet = playstationIconAtlas.spriteSheet;
            xboxIconAtlas[counter].spriteCharacterLookupTable = playstationIconAtlas.spriteCharacterLookupTable;
            xboxIconAtlas[counter].spriteCharacterTable = playstationIconAtlas.spriteCharacterTable;
            xboxIconAtlas[counter].spriteGlyphTable = playstationIconAtlas.spriteGlyphTable;
        }

        PluginLoader.Log.LogInfo("Action Icons replaced.");
    }
}
