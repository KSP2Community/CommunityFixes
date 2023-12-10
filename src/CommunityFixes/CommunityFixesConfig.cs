using BepInEx.Configuration;

namespace CommunityFixes;

public class CommunityFixesConfig
{
    private const string TogglesSection = "Toggle fixes";

    private readonly Dictionary<Type, ConfigEntry<bool>> _fixesEnabled = new();
    internal ConfigFile File { get; }

    public CommunityFixesConfig(ConfigFile fileFileFile)
    {
        File = fileFileFile;
    }

    public bool LoadConfig(Type type, string name)
    {
        // If the toggle value for a fix class is already defined, we return it
        if (_fixesEnabled.TryGetValue(type, out var isEnabled))
        {
            return isEnabled.Value;
        }

        // Otherwise create a new config entry for the fix class and return its default value (true)
        var configEntry = File.Bind(TogglesSection, type.Name, true, name);
        _fixesEnabled.Add(type, configEntry);
        return configEntry.Value;
    }
}