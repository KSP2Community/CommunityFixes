using BepInEx.Logging;
using HarmonyLib;
using KSP.Game;

namespace CommunityFixes.Fix;

public abstract class BaseFix : KerbalMonoBehaviour, IFix
{
    public virtual void OnInitialized()
    {
    }

    protected Harmony _harmony;
    internal ManualLogSource Logger { get; set; }

    protected BaseFix()
    {
        _harmony = new Harmony(GetType().FullName);
    }
}