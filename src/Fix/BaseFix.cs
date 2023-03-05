using HarmonyLib;
using KSP.Game;
using SpaceWarp.API.Logging;

namespace CommunityFixes.Fix;

public abstract class BaseFix : KerbalMonoBehaviour, IFix
{
    public abstract string Name { get; }
    public abstract void OnInitialized();
    
    protected Harmony _harmony;
    internal BaseModLogger Logger { get; set; }

    protected BaseFix()
    {
        _harmony = new Harmony(GetType().FullName);
    }
}