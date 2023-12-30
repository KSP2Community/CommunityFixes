using BepInEx.Configuration;
using HarmonyLib;
using KSP.Game;
using SpaceWarp.API.Logging;

namespace CommunityFixes.Fix;

/// <summary>
/// Base class for all fixes.
/// </summary>
public abstract class BaseFix : KerbalMonoBehaviour
{
    private Configuration Config { get; }

    /// <summary>
    /// The logger for the fix.
    /// </summary>
    internal ILogger Logger { get; }

    /// <summary>
    /// The harmony instance for the fix.
    /// </summary>
    protected Harmony HarmonyInstance { get; }

    /// <summary>
    /// Creates a new fix.
    /// </summary>
    protected BaseFix()
    {
        Config = CommunityFixesMod.Config;
        Logger = new BepInExLogger(BepInEx.Logging.Logger.CreateLogSource($"CF/{GetType().Name}"));
        HarmonyInstance = new Harmony(GetType().FullName);
    }

    /// <summary>
    /// Binds a config entry to a fix class.
    /// </summary>
    /// <param name="key">The config entry key.</param>
    /// <param name="defaultValue">The config entry default value.</param>
    /// <param name="description">The config entry description.</param>
    /// <typeparam name="T">The config entry type.</typeparam>
    /// <returns>The config entry.</returns>
    private protected ConfigEntry<T> BindConfigValue<T>(string key, T defaultValue, string description = null)
    {
        return Config.BindFixValue(GetType(), key, defaultValue, description);
    }

    /// <summary>
    /// Called when the fix is initialized.
    /// </summary>
    public virtual void OnInitialized()
    {
    }
}