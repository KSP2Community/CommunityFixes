using BepInEx.Configuration;
using CommunityFixes;

namespace CommunityFixes.Fix.BetterNodeCheckFix;

[Fix("Better Node Check Fix")]
public class BetterNodeCheckFix : BaseFix
{
  public static BetterNodeCheckFix Instance;

  // Community Fix config parameters specific to this fix
  internal static ConfigEntry<bool> _allowAllNodes;

  public BetterNodeCheckFix()
  {
    Instance = this;
  }
  public override void OnInitialized()
  {
    // Community Fix config parameters specific to this fix
    _allowAllNodes = CommunityFixesMod.Config.File.Bind<bool>("Better Node Check Fix", "Allow All", false, "If enabled, all new maneuver nodes are allowed. If disabled new and rebuilt nodes are limited by the combined engine and RCS DeltaV");

    HarmonyInstance.PatchAll(typeof(ManeuverPlanComponent_patches));
  }
}
