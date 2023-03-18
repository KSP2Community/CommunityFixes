using System.Reflection;
using BepInEx;
using CommunityFixes.Fix;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace CommunityFixes;

[BepInPlugin(ModGuid, ModName, ModVer)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class CommunityFixesMod : BaseSpaceWarpPlugin
{
    public const string ModGuid = "com.github.communityfixes";
    public const string ModName = "Community Fixes";
    public const string ModVer = "0.4.0";
    
    private static readonly Assembly Assembly = typeof(CommunityFixesMod).Assembly;
    private CommunityFixesConfig _config;
    
    public override void OnInitialized()
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

        _config = new CommunityFixesConfig(Config);

        foreach (var type in types)
        {
            if (type.IsAbstract || !type.GetInterfaces().Contains(typeof(IFix)))
            {
                continue;
            }
            
            try
            {
                var isLoaded = LoadFix(type);
                Logger.LogInfo($"Fix {type.Name} is " + (isLoaded ? "disabled" : "enabled"));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading fix {type.FullName}: {ex.Message}");
            }
        }
        
        Logger.LogInfo($"{ModName} finished loading.");
    }

    private bool LoadFix(Type type)
    {
        var fixName = GetFixName(type);
        
        if (!_config.LoadConfig(type, fixName))
        {
            return false;
        }

        IFix fix;
        if (type.BaseType == typeof(BaseFix))
        {
            var baseFix = gameObject.AddComponent(type) as BaseFix;
            baseFix!.transform.parent = transform;
            baseFix.Logger = BepInEx.Logging.Logger.CreateLogSource($"CF/{fixName}");
            fix = baseFix;
        }
        else
        {
            fix = Assembly.CreateInstance(type.FullName!) as IFix;
        }

        if (fix == null)
        {
            throw new Exception($"Could not instantiate fix {type.Name}.");
        }

        fix.OnInitialized();
        
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
}