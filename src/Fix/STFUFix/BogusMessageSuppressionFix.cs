using BepInEx.Configuration;
using CommunityFixes;

namespace CommunityFixes.Fix.STFUFix;

[Fix("Suspress bogus and unhelpful log messages")]
public class SuppressTransmissionsFalselyUrgentFix : BaseFix
{
    public static SuppressTransmissionsFalselyUrgentFix Instance;

    // Community Fix config parameters specific to this fix
    internal static ConfigEntry<bool> _allowAllNodes;

    public SuppressTransmissionsFalselyUrgentFix()
    {
        Instance = this;
    }

    public override void OnInitialized()
    {
        // Community Fix config parameters specific to this fix
        // _allowAllNodes = CommunityFixesMod.Config.File.Bind<bool>("Better Node Check Fix", "Allow All", false, "If enabled, all new maneuver nodes are allowed. If disabled new and rebuilt nodes are limited by the combined engine and RCS DeltaV");

        // HarmonyInstance.PatchAll(typeof(BetterNodeCheckPatches));
        HarmonyInstance.PatchAll(typeof(STFUPatches));
    }
}
