using HarmonyLib;
using KSP.Map;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using KSP.Sim.impl;
using Castle.Components.DictionaryAdapter.Xml;
using static KSP.Api.UIDataPropertyStrings.View;

namespace CommunityFixes.Fix.TrackingStationDebrisNameFix
{
    [Fix("Fix debris name in the tracking station")]
    internal class TrackingStationDebrisNameFix : BaseFix
    {
        private static SpaceWarp.API.Logging.ILogger _logger;
        public override void OnInitialized()
        {
            _logger = Logger;
            HarmonyInstance.PatchAll(typeof(TrackingStationDebrisNameFix));
        }

        /**
         * Postfix the display of debris in the Tracking Station. Instead of displaying 'Debris: <GUID>', we display its formal name.
         ***/
        [HarmonyPatch(typeof(MapUI), nameof(MapUI.HandleDebrisObjectEntryConfigurations))]
        [HarmonyPostfix]
        public static void HandleDebrisObjectEntryConfigurationsPostfix(
            MapItem item,
            MapUISelectableEntry obj
        )
        {
            obj.Name = ((object)item._itemName).ToString();
        }

        /**
         * Postfix the creation of a new vessel. If it's a debris, we give it an appropriate name.
         **/
        [HarmonyPatch(typeof(SpaceSimulation), nameof(SpaceSimulation.CreateVesselSimObjectFromPart))]
        [HarmonyPostfix]
        public static void CreateVesselSimObjectFromPartPostfix(
            PartComponent rootPart,
            ref SimulationObjectModel __result
        )
        {
            System.Diagnostics.Debug.Write("requin");
            VesselComponent vessel = __result.FindComponent<VesselComponent>();
            if (!vessel._hasCommandModule)
            {
                renameVessel(vessel, "Unknown Debris");
            }
        }

        /**
         * Postfix the decoupling of a vessel into two subvessels, renaming the debris (if such vessel exists) and keeping the original name for the subvessel with a command module (in case the original root part ends up being a debris).
         **/
        [HarmonyPatch(typeof(SpaceSimulation), nameof(SpaceSimulation.SplitCombinedVesselSimObject))]
        [HarmonyPostfix]
        public static void SplitCombinedVesselSimObjectPostfix(
            VesselComponent combinedVessel, // the vessel with the root part 
            IGGuid detachingPartId,
            ref SimulationObjectModel __result
        )
        {
            System.Diagnostics.Debug.Write("albatros");
            VesselComponent vessel = __result.FindComponent<VesselComponent>(); // the new vessel splited from the vessel with the root part
            String originalVesselName = combinedVessel.Name.Replace("Debris of ", ""); // recreating the original vessel name by removing 'Debris of' (in case more than one linear decouplings happened at the same time)
            renameDebrisVessel(vessel, originalVesselName);
            renameDebrisVessel(combinedVessel, originalVesselName);
            if (vessel._hasCommandModule)
            {
                renameVessel(vessel, originalVesselName); // if a command module happens to be in the splitted vessel, we give it the name of the original vessel
            }
        }

        /**
         * Rename the vessel as 'Debris of xxx' if it's a debris.
         **/
        private static void renameDebrisVessel(VesselComponent vessel, string originalVesselName)
        {
            if (vessel._hasCommandModule) return;
            renameVessel(vessel, "Debris of " + originalVesselName);
        }

        /**
         * Rename the vessel with the specified name.
         ***/
        private static void renameVessel(VesselComponent vessel, string newName)
        {
            System.Diagnostics.Debug.Write("Renaming " + vessel.SimulationObject.Name + " to " + newName);
            vessel.SimulationObject.Name = newName;
        }
    }
}
