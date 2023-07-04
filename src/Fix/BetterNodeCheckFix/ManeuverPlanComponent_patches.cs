using HarmonyLib;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Game;
using KSP.Sim.ResourceSystem;
using KSP.Messages;
using UnityEngine;


namespace CommunityFixes.Fix.BetterNodeCheckFix;

public class ManeuverPlanComponent_patches
{
  [HarmonyPatch(typeof(ManeuverPlanComponent), nameof(ManeuverPlanComponent.EnoughDeltaVToAddNode))]
  [HarmonyPrefix]
  public static bool BetterEnoughDeltaVToAddNode(ManeuverPlanComponent __instance, ref bool __result)
  {
    // This method offers two options for the player. Either allow all nodes to be made regardless of the DeltaV capability of the vessel,
    // or prevent node creation based on there being insufficient available DeltaV including contribuitions from use of RCS thrusters

    // __result replaces the return value the original method would have provided

    // Assume we can afford the whole requested burn (no reduction)
    __result = true;

    if (BetterNodeCheckFix._allowAllNodes.Value)
    {
      // All nodes are allowed without any check for available DeltaV
      BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: Allow any node.");
      return false;
    }

    // Get the vessel component for this SimulationObject and related vessel and node info
    VesselComponent thisVessel = __instance.SimulationObject.Vessel;
    double totalDeltaVNeeded = TotalDeltaVNeeded(__instance._currentNodes);
    double RcsDeltaV = RCSDeltaV(thisVessel);

    // Get the remaining DeltaV available that isn't already allocated to other pre-existing nodes
    double remainingDeltaV = __instance.SimulationObject.VesselDeltaV.TotalDeltaVActual + RcsDeltaV - totalDeltaVNeeded;

    // This will prevent the creation of the node if we don't have enough gas for it
    __result = remainingDeltaV > 0.001;

    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: TotalDeltaVActual = {__instance.SimulationObject.VesselDeltaV.TotalDeltaVActual} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: RcsDeltaV         = {RcsDeltaV} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: totalDeltaVNeeded = {totalDeltaVNeeded} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: remainingDeltaV = TotalDeltaVActual + RcsDeltaV - totalDeltaVNeeded = {remainingDeltaV} m/s");

    // Returning true indicates the patched method should be called after this, and false indicates it should not be called
    return false;
  }

  [HarmonyPatch(typeof(ManeuverPlanComponent), nameof(ManeuverPlanComponent.EnoughDeltaVToAddNodeOnRebuild))]
  [HarmonyPrefix]
  public static bool BetterEnoughDeltaVToAddNodeOnRebuild(ManeuverPlanComponent __instance, ref bool __result, ManeuverNodeData node, out Vector3d reducedBurnVectorChange)
  {
    // This method offers two options for the player. Either allow all nodes to be made regardless of the DeltaV capability of the vessel,
    // or limit node creation based on the available DeltaV including any contribuition possible from use of RCS thrusters

    // __result replaces the return value the original method would have provided
    // reducedBurnVectorChange should similarly be returned by the harmony method in place of the patched method's output

    // Assume we can afford the whole requested burn (no reduction)
    reducedBurnVectorChange = node.BurnVector;
    __result = true;

    if (BetterNodeCheckFix._allowAllNodes.Value)
    {
      // All nodes are allowed without any check for available DeltaV
      BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: Allow any node.");
      return false;
    }

    // Check for available DeltaV including DeltaV possible by burning RCS thrusters

    // Get the vessel component for this SimulationObject and related vessel and node info
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

    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: TotalDeltaVActual       = {__instance.SimulationObject.VesselDeltaV.TotalDeltaVActual} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: RcsDeltaV               = {RcsDeltaV} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: totalDeltaVNeeded       = {totalDeltaVNeeded} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: remainingDeltaV         = TotalDeltaVActual + RcsDeltaV - totalDeltaVNeeded = {remainingDeltaV} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: RequestedDeltaV         = {node.BurnVector.magnitude} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: reducedBurnVectorChange = {reducedBurnVectorChange.magnitude} m/s");

    // Returning true indicates the patched method should be called after this, and false indicates it should not be called
    return false;
  }

  [HarmonyPatch(typeof(ManeuverPlanComponent), nameof(ManeuverPlanComponent.EnoughDeltaVToChangeNode))]
  [HarmonyPrefix]
  public static bool BetterEnoughDeltaVToChangeNode(ManeuverPlanComponent __instance, ref bool __result, ManeuverNodeData node, Vector3 change, out Vector3 newChange)
  {
    // This method offers two options for the player. Either allow all nodes to be updated regardless of the DeltaV capability of the vessel,
    // or limit node update of nodes based on the available DeltaV including any contribuition possible from use of RCS thrusters

    // __result replaces the return value the original method would have provided
    // reducedBurnVectorChange should similarly be returned by the harmony method in place of the patched method's output

    // Assume we can afford the whole requested burn (no reduction)
    newChange = change; // node.BurnVector;
    __result = true;

    if (BetterNodeCheckFix._allowAllNodes.Value)
    {
      // All nodes are allowed without any check for available DeltaV
      BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: Allow any node.");
      return false;
    }

    // Check for available DeltaV including DeltaV possible by burning RCS thrusters

    // Get the vessel component for this SimulationObject and related vessel and node info
    VesselComponent thisVessel = __instance.SimulationObject.Vessel;
    double totalDeltaVNeededOther = TotalDeltaVNeeded(__instance._currentNodes, true, node);
    double RcsDeltaV = RCSDeltaV(thisVessel);
    double totalDeltaVactual = __instance.SimulationObject.VesselDeltaV.TotalDeltaVActual - totalDeltaVNeededOther;

    Vector3d newBurnVec = node.BurnVector + change;

    // If there's plenty of fuel - don't even need to touch RCS, just permit it
    if (totalDeltaVactual + RcsDeltaV >= newBurnVec.magnitude)
    {
      node.showOutOfFuelMessage = true;
      return false;
    }

    // If the engine is in airbreathing mode
    if (__instance.IsEngineInAirbreathingMode(__instance.SimulationObject.VesselDeltaV))
    {
      GameManager.Instance.Game.Messages.Publish<EngineInAirBreathingModeMessage>(GameManager.Instance.Game.Messages.CreateMessage<EngineInAirBreathingModeMessage>());
      node.showOutOfFuelMessage = false;
      __result = false;
      return false;
    }

    // Get the remaining DeltaV available that isn't already allocated to other pre-existing nodes
    double remainingDeltaV = totalDeltaVactual + RcsDeltaV - node.BurnRequiredDV;

    if (remainingDeltaV >= 0.001)
    {
      double num3 = remainingDeltaV;
      newChange = change.normalized;
      newChange.x *= (float) num3;
      newChange.y *= (float) num3;
      newChange.z *= (float) num3;
      Vector3 vector3 = change;
      string str1 = vector3.ToString();
      vector3 = newChange;
      string str2 = vector3.ToString();
      Debug.Log((object)("change " + str1 + " reduced to newChange = " + str2));
      node.showOutOfFuelMessage = true;
      __result = true;
    }
    __result = remainingDeltaV < (totalDeltaVactual - newBurnVec.magnitude);

    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: TotalDeltaVActual       = {totalDeltaVactual} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: RcsDeltaV               = {RcsDeltaV} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: totalDeltaVNeededOther  = {totalDeltaVNeededOther} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: remainingDeltaV         = TotalDeltaVActual + RcsDeltaV - node.BurnRequiredDV = {remainingDeltaV} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: RequestedDeltaV         = {change.magnitude} m/s");
    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: reducedBurnVectorChange = {newChange.magnitude} m/s");

    // Returning true indicates the patched method should be called after this, and false indicates it should not be called
    return false;
  }

  static double TotalDeltaVNeeded(List<ManeuverNodeData> nodes, bool excludeCurrentNode=false, ManeuverNodeData currentNode = null)
  {
    // Get the total Delta V needed for all currently existing nodes to execute
    double totalDeltaVNeeded = 0.0;
    for (int index = 0; index < nodes.Count; ++index)
      if (excludeCurrentNode)
      { 
        if (currentNode != nodes[index])
          totalDeltaVNeeded += nodes[index].BurnRequiredDV;
      }
      else
        totalDeltaVNeeded += nodes[index].BurnRequiredDV;
    return totalDeltaVNeeded;
  }

  static double RCSDeltaV(VesselComponent thisVessel)
  {
    // Get the total mass of the vessel at this time, and the mass of the monopropellant
    double Isp = 260.0; // Assumption - should check
    double initialMass = thisVessel.totalMass;
    double monoPropMass = 0;
    double finalMass;

    // Get the resource ID for monopropellant
    ResourceDefinitionID monopropID = GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName("monopropellant");

    // Get a list of all the parts on this vessel
    var partList = GameManager.Instance.Game.ViewController.GetActiveVehicle().GetSimulationObject().PartOwner.Parts.ToList();

    // Add up all the monoprop found on the vessel
    partList.ForEach((partdata) =>
    {
      int dataIndexFromId = partdata.PartResourceContainer.GetDataIndexFromID(monopropID);
      if (partdata.PartResourceContainer.IsValidDataIndex(dataIndexFromId))
      {
        monoPropMass += partdata.PartResourceContainer.GetResourceContainedData(monopropID).StoredUnits;
      }
    });

    // Log the monoprop mass found
    BetterNodeCheckFix.Instance.Logger.LogDebug($"RCSDeltaV: monoPropMass = {monoPropMass:N3}");

    // Assume we use all available monoprop for a maneuver
    finalMass = initialMass - monoPropMass;

    // Return the RCS DeltaV available, where DeltaV = Isp * g * ln(initialMass / finalMass)
    // For DeltaV/Isp calculations, g = PhysicsSettings.STANDARD_GRAVITY_EARTH = 9.80665 m/s^2
    return Isp * PhysicsSettings.STANDARD_GRAVITY_EARTH * Math.Log(initialMass / finalMass);
  }
}
