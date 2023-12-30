using BepInEx.Configuration;
using CommunityFixes.Fix;

namespace CommunityFixes;

internal class Configuration
{
    private const string TogglesSection = "Toggle fixes";

    private readonly Dictionary<Type, ConfigEntry<bool>> _fixesEnabled = new();
    private readonly ConfigFile _file;

    /// <summary>
    /// Creates a new config file object.
    /// </summary>
    /// <param name="file">The config file to use.</param>
    public Configuration(ConfigFile file)
    {
        _file = file;
    }

    /// <summary>
    /// Gets the toggle value for a fix class.
    /// </summary>
    /// <param name="type">Type of the fix class.</param>
    /// <returns>The toggle value for the fix class.</returns>
    public bool IsFixEnabled(Type type)
    {
        // If the toggle value for a fix class is already defined, we return it
        if (_fixesEnabled.TryGetValue(type, out var isEnabled))
        {
            return isEnabled.Value;
        }

        // Otherwise create a new config entry for the fix class and return its default value (true)
        var metadata = FixAttribute.GetForType(type);
        var configEntry = _file.Bind(TogglesSection, type.Name, true, metadata.Name);
        _fixesEnabled.Add(type, configEntry);
        return configEntry.Value;
    }

    /// <summary>
    /// Binds a config entry to a hack class.
    /// </summary>
    /// <param name="hackType">The hack class type.</param>
    /// <param name="key">The config entry key.</param>
    /// <param name="defaultValue">The config entry default value.</param>
    /// <param name="description">The config entry description.</param>
    /// <typeparam name="T">The config entry type.</typeparam>
    /// <returns>The config entry.</returns>
    public ConfigEntry<T> BindFixValue<T>(Type hackType, string key, T defaultValue, string description = null)
    {
        return _file.Bind($"{hackType.Name} settings", key, defaultValue, description);
    }
}