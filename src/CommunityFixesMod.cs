using System.Reflection;
using CommunityFixes.Fix;
using KSP;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods;
using UnityEngine;

namespace CommunityFixes;

[MainMod]
public class CommunityFixesMod : Mod
{
    private static readonly Assembly _assembly = typeof(CommunityFixesMod).Assembly;
    
    public override void OnInitialized()
    {
        Type[] types;
        try
        {
            types = _assembly.GetTypes();
        }
        catch (Exception ex)
        {
            Logger.Error($"Could not get types: ${ex.Message}");
            return;
        }

        foreach (var type in types)
        {
            if (type.IsAbstract || !type.GetInterfaces().Contains(typeof(IFix)))
            {
                continue;
            }
            
            try
            {
                LoadFix(type);
                Logger.Info($"Initialized fix {type.Name}.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error loading fix {type.FullName}: {ex.Message}");
            }
        }
        
        Logger.Info($"{Info.name} finished loading.");
    }

    internal void LoadFix(Type type)
    {
        IFix fix;
        if (type.BaseType == typeof(BaseFix))
        {
            var baseFix = gameObject.AddComponent(type) as BaseFix;
            baseFix!.transform.parent = transform;
            baseFix.Logger = new ModLogger($"{baseFix.Name}");
            fix = baseFix;
        }
        else
        {
            fix = _assembly.CreateInstance(type.FullName!) as IFix;
        }

        if (fix == null)
        {
            throw new Exception($"Could not instantiate fix {type.Name}.");
        }

        fix.OnInitialized();
    }
}