using HarmonyLib;
using KSP.Map;
using System.Text.RegularExpressions;

namespace CommunityFixes.Fix.TrackingStationDebrisNameFix
{
    [Fix("Fix debris name in the tracking station")]
    internal class TrackingStationDebrisNameFix : BaseFix
    {
        public override void OnInitialized()
        {
            HarmonyInstance.PatchAll(typeof(TrackingStationDebrisNameFix));
        }

        [HarmonyPatch(typeof(MapUI), nameof(MapUI.HandleDebrisObjectEntryConfigurations))]
        [HarmonyPostfix]
        public static void HandleDebrisObjectEntryConfigurationsPostfix(
            MapItem item,
            MapUISelectableEntry obj
        )
        {
            string debrisName = ((object)item._itemName).ToString();
            var match = Regex.Match(debrisName, @"-(\d+)$");
            var newName = match.Success
                ? Regex.Replace(debrisName, @"-\d+$", "")
                : debrisName;
            obj.Name = string.Format("Debris of {0}", newName);
        }

    }
}
