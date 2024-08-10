using System.Text.RegularExpressions;
using HarmonyLib;
using KSP.Messages;
using KSP.Sim.impl;

namespace CommunityFixes.Fix.FairingEjectSidewaysFix;

[Fix("Decoupled craft name fix")]
public class DecoupledCraftNameFix : BaseFix
{
    public override void OnInitialized()
    {
        Messages.Subscribe<DecoupleMessage>(msg => HandleDecoupleMessage((DecoupleMessage)msg));
        Messages.Subscribe<VesselUndockedMessage>(msg => HandleUndockMessage((VesselUndockedMessage)msg));

        HarmonyInstance.PatchAll(typeof(DecoupledCraftNameFix));
    }

    private void HandleDecoupleMessage(DecoupleMessage decoupleMessage)
    {
        var part1Guid = new IGGuid(Guid.Parse(decoupleMessage.PartGuid));
        var part2Guid = decoupleMessage.OtherPartGuid;

        var vessel1 = Game.UniverseModel.FindPartComponent(part1Guid)?.PartOwner?.SimulationObject?.Vessel;
        var vessel2 = Game.UniverseModel.FindPartComponent(part2Guid)?.PartOwner?.SimulationObject?.Vessel;

        HandleSeparationEvent(vessel1, vessel2);
    }

    private void HandleUndockMessage(VesselUndockedMessage undockMessage)
    {
        VesselComponent vessel1 = undockMessage.VesselOne == null ? null : undockMessage.VesselOne.Model;
        VesselComponent vessel2 = undockMessage.VesselTwo == null ? null : undockMessage.VesselTwo.Model;

        HandleSeparationEvent(vessel1, vessel2);
    }

    private void HandleSeparationEvent(VesselComponent vessel1, VesselComponent vessel2)
    {
        Logger.LogDebug($"Separated: {vessel1?.Name}, {vessel2?.Name}");

        if (vessel2 is not { Name: var newName } ||
            !newName.StartsWith("Default Name") ||
            string.IsNullOrEmpty(vessel1?.Name))
        {
            return;
        }

        var match = Regex.Match(vessel1!.Name, @"-(\d+)$");
        newName = match.Success
            ? Regex.Replace(vessel1.Name, @"-\d+$", $"-{int.Parse(match.Groups[1].Value) + 1}")
            : $"{vessel1.Name}-2";

        Logger.LogDebug($"Renaming {vessel2.Name} to {newName}");

        vessel2.SimulationObject.Name = newName;
    }

    [HarmonyPatch(typeof(SpaceSimulation), nameof(SpaceSimulation.CreateCombinedVesselSimObject))]
    [HarmonyPrefix]
    public static void CreateCombinedVesselSimObjectPrefix(
        // ReSharper disable once InconsistentNaming
        ref string __state,
        VesselComponent masterVessel
    )
    {
        __state = masterVessel.Name;
    }

    [HarmonyPatch(typeof(SpaceSimulation), nameof(SpaceSimulation.CreateCombinedVesselSimObject))]
    [HarmonyPostfix]
    public static void CreateCombinedVesselSimObjectPostfix(
        // ReSharper disable once InconsistentNaming
        string __state,
        // ReSharper disable once InconsistentNaming
        ref SimulationObjectModel __result
    )
    {
        __result.Name = __state;
    }
}