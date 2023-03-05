using BepInEx.Configuration;

namespace CommunityFixes;

public class CommunityFixesConfig
{
    private const string TogglesSection = "Toggle fixes";
    
    private Dictionary<Type, ConfigEntry<bool>> _fixesEnabled = new();
    private ConfigFile _configFile;

    public CommunityFixesConfig(ConfigFile configFileFile)
    {
        _configFile = configFileFile;
    }


    public bool LoadConfig(Type type, string name)
    {
        // If the toggle value for a fix class is already defined, we return it
        if (_fixesEnabled.TryGetValue(type, out var isEnabled))
        {
            return isEnabled.Value;
        }

        // Otherwise create a new config entry for the fix class and return its default value (true)
        var configEntry = _configFile.Bind(TogglesSection, type.Name, true, name);
        _fixesEnabled.Add(type, configEntry);
        return configEntry.Value;
    }
}