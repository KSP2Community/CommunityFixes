using System.Reflection;
using BepInEx;
using CommunityFixes.Fix;
using JetBrains.Annotations;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace CommunityFixes;

[BepInPlugin(ModGuid, ModName, ModVer)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class CommunityFixesMod : BaseSpaceWarpPlugin
{
    [PublicAPI] public const string ModGuid = "CommunityFixes";
    [PublicAPI] public const string ModName = "Community Fixes";
    [PublicAPI] public const string ModVer = "0.6.0";

    private static readonly Assembly Assembly = typeof(CommunityFixesMod).Assembly;
    internal new static CommunityFixesConfig Config;

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

        Config = new CommunityFixesConfig(base.Config);

        foreach (var type in types)
        {
            if (type.IsAbstract || !HasFixType(type))
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
        var fixName = GetFixName(type);

        if (!Config.LoadConfig(type, fixName))
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

    private static string GetFixName(Type type)
    {
        var attributes = Attribute.GetCustomAttributes(type);
        foreach (var attribute in attributes)
        {
            if (attribute is FixAttribute fix)
            {
                return fix.Name;
            }
        }

        throw new Exception($"The attribute {typeof(FixAttribute).FullName} has to be declared on a fix class.");
    }

    private static bool HasFixType(Type type)
    {
        if (type == null)
        {
            return false;
        }

        // return all inherited types
        var currentBaseType = type.BaseType;
        while (currentBaseType != null)
        {
            if (currentBaseType == typeof(BaseFix))
            {
                return true;
            }

            currentBaseType = currentBaseType.BaseType;
        }

        return false;
    }
}