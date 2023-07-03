using BepInEx.Configuration;
using CommunityFixes;

namespace CommunityFixes.Fix.AddNodeFix;

[Fix("Add Node Fix")]
public class AddNodeFix : BaseFix
{
  public static AddNodeFix Instance;

  // Config parameters
  internal static ConfigEntry<bool> _allowAllNodes;

  public AddNodeFix()
  {
    Instance = this;
  }
  public override void OnInitialized()
  {
    _allowAllNodes = CommunityFixesMod.Config.File.Bind<bool>("Add Nodes Fix", "Allow All", false, "If enabled, all new maneuver nodes are allowed. If disabled new and rebuilt nodes are limited by the combined engine and RCS DeltaV");

    HarmonyInstance.PatchAll(typeof(ManeuverPlanComponent_patches));
  }
}
