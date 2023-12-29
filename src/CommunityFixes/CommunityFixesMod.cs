using System.Reflection;
using BepInEx;
using CommunityFixes.Fix;
using JetBrains.Annotations;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace CommunityFixes;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class CommunityFixesMod : BaseSpaceWarpPlugin
{
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    private static readonly Assembly Assembly = typeof(CommunityFixesMod).Assembly;
    internal new static Configuration Config;

    private readonly List<BaseFix> _fixes = new();

    private void Awake()
    {
        Type[] types;
        try
        {
            types = Assembly.GetTypes();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Could not get types: ${ex.Message}");
            return;
        }

        Config = new Configuration(base.Config);

        foreach (var type in types)
        {
            if (type.IsAbstract || !type.IsSubclassOf(typeof(BaseFix)))
            {
                continue;
            }

            try
            {
                var isLoaded = LoadFix(type);
                Logger.LogInfo($"Fix {type.Name} is " + (isLoaded ? "enabled" : "disabled"));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading fix {type.FullName}: {ex}");
            }
        }

        Logger.LogInfo($"{ModName} finished loading.");
    }

    public override void OnInitialized()
    {
        foreach (var fix in _fixes)
        {
            fix.OnInitialized();
        }
    }

    private bool LoadFix(Type type)
    {
        if (!Config.IsFixEnabled(type))
        {
            return false;
        }

        var fix = gameObject.AddComponent(type) as BaseFix;
        if (fix == null)
        {
            throw new Exception($"Could not instantiate fix {type.Name}.");
        }

        fix.transform.parent = transform;
        _fixes.Add(fix);

        return true;
    }
}