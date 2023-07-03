using HarmonyLib;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Game;

namespace CommunityFixes.Fix.BetterNodeCheckFix;

public class ManeuverPlanComponent_patches
{
  [HarmonyPatch(typeof(ManeuverPlanComponent), nameof(ManeuverPlanComponent.EnoughDeltaVToAddNode))]
  [HarmonyPrefix]
  public static bool BetterEnoughDeltaVToAddNode(ManeuverPlanComponent __instance, ref bool __result)
  {
    // New nodes are always allowed, no matter what the fuel state it. If we wanted to do a better job we would
    // compare the sum of all DeltaV needed from all the nodes to this.SimulationObject.VesselDeltaV.TotalDeltaVActual
    // + the total DeltaV available from RCS. Not sure how to get that second quantity, so for now we simply return
    // true to permit all new nodes regardless.

    // This replaces the return value the original method would have provided
    // Assume we can afford the whole requested burn (no reduction)
    __result = true;

    if (BetterNodeCheckFix._allowAllNodes.Value)
    {
      BetterNodeCheckFix.Instance.Logger.LogInfo($"BetterEnoughDeltaVToAddNode called: returning {__result}: Allow any node.");
      return false;
    }

    // Get the vessel component for this SimulationObject
    VesselComponent thisVessel = __instance.SimulationObject.Vessel;

    // Gather command modules - TODO: Math this cather all the monoprop?
    List<PartComponentModule_Command> partModules = __instance.SimulationObject.PartOwner.GetPartModules<PartComponentModule_Command>();

    double totalDeltaVNeeded = TotalDeltaVNeeded(__instance._currentNodes);
    double RcsDeltaV = RCSDeltaV(thisVessel);
    // Get the remaining DeltaV available that isn't already allocated to other pre-existing nodes
    double remainingDeltaV = __instance.SimulationObject.VesselDeltaV.TotalDeltaVActual + RcsDeltaV - totalDeltaVNeeded;

    // This will prevent the creation of the node if we don't have enough gas for it
    __result = remainingDeltaV > 0.001;

    BetterNodeCheckFix.Instance.Logger.LogInfo($"BetterEnoughDeltaVToAddNode called: returning {__result}: remainingDeltaV = {remainingDeltaV} m/s, RcsDeltaV = {RcsDeltaV} m/s, totalDeltaVNeeded = {totalDeltaVNeeded} m/s");

    // Returning true indicates the patched method should be called after this, and false indicates it should not be called
    return false;
  }

  [HarmonyPatch(typeof(ManeuverPlanComponent), nameof(ManeuverPlanComponent.EnoughDeltaVToAddNodeOnRebuild))]
  [HarmonyPrefix]
  public static bool BetterEnoughDeltaVToAddNodeOnRebuild(ManeuverPlanComponent __instance, ref bool __result, ManeuverNodeData node, out Vector3d reducedBurnVectorChange)
  {
    // Rebuilt nodes are always allowed, no matter what the fuel state it. If we wanted to do a better job we would
    // compare the sum of all DeltaV needed from all the nodes to this.SimulationObject.VesselDeltaV.TotalDeltaVActual
    // + the total DeltaV available from RCS. Not sure how to get that second quantity, so for now we simply return
    // true to permit all new nodes regardless.

    // Assume we can afford the whole requested burn (no reduction)
    reducedBurnVectorChange = node.BurnVector;
    __result = true;

    if (BetterNodeCheckFix._allowAllNodes.Value)
    {
      BetterNodeCheckFix.Instance.Logger.LogInfo($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: Allow any node.");
      return false;
    }
    // Get the vessel component for this SimulationObject
    VesselComponent thisVessel = __instance.SimulationObject.Vessel;

    double totalDeltaVNeeded = TotalDeltaVNeeded(__instance._currentNodes);
    double RcsDeltaV = RCSDeltaV(thisVessel);
    // Get the remaining DeltaV available that isn't already allocated to other pre-existing nodes
    double remainingDeltaV = __instance.SimulationObject.VesselDeltaV.TotalDeltaVActual + RcsDeltaV - totalDeltaVNeeded;

    if (remainingDeltaV <= 0.001) // No DeltaV available, no node for you!
    {
      reducedBurnVectorChange = Vector3d.zero;
      __result = false;
    }

    else if (remainingDeltaV < node.BurnRequiredDV) // Some DeltaV available, you may have only this much!
    {
      Vector3d temp = node.BurnVector;
      temp = temp.normalized;
      temp.x *= remainingDeltaV;
      temp.y *= remainingDeltaV;
      temp.z *= remainingDeltaV;
      reducedBurnVectorChange = temp - node.BurnVector;
      __result = true;
    }

    BetterNodeCheckFix.Instance.Logger.LogInfo($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: remainingDeltaV = {remainingDeltaV} m/s, RcsDeltaV = {RcsDeltaV} m/s, totalDeltaVNeeded = {totalDeltaVNeeded} m/s");

    // Returning true indicates the patched method should be called after this, and false indicates it should not be called
    return false;
  }

  static double TotalDeltaVNeeded(List<ManeuverNodeData> nodes)
  {
    // Get the total Delta V needed for all currently existing nodes to execute
    double totalDeltaVNeeded = 0.0;
    for (int index = 0; index < nodes.Count; ++index)
      totalDeltaVNeeded += nodes[index].BurnRequiredDV;
    return totalDeltaVNeeded;
  }

  static double RCSDeltaV(VesselComponent thisVessel)
  {
    // Get the total mass of the vessel at this time, and the mass of the monopropellant
    double initialMass;
    double monoPropMass;
    double finalMass;
    double Isp = 260.0; // Assumption - should check
    // __instance.SimulationObject.Part;

    // thisVessel.fuelCapacity[]
    var foo = thisVessel.fuelCapacity[GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName("monopropellant")];
    monoPropMass = foo.StoredUnits;
    initialMass = thisVessel.totalMass;
    finalMass = initialMass - monoPropMass;

    // Standard Gravity = PhysicsSettings.STANDARD_GRAVITY_EARTH = 9.80665 m/s^2

    // Return the RCS DeltaV available, where DeltaV = Isp * g * ln(initialMass / finalMass)
    return Isp * PhysicsSettings.STANDARD_GRAVITY_EARTH * Math.Log(initialMass / finalMass);
  }
}
