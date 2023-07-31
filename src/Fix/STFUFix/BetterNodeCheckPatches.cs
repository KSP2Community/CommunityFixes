using HarmonyLib;
using KSP.Game;
using KSP.Messages;
using KSP.Sim;
using KSP.Sim.DeltaV;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.Sim.ResourceSystem;
using Unity.Mathematics;
using UnityEngine;

namespace CommunityFixes.Fix.BetterNodeCheckFix;

public class BetterNodeCheckPatches
{

    //[HarmonyPatch(typeof(GlobalLog), nameof(GlobalLog.Warn), typeof(object))]
    //[HarmonyPrefix]
    //private static bool GlobalLog_Warn(object message)
    //{
    //  return message is not string strMessage || !strMessage.Contains("has not defined a binding for id");
    //}

    [HarmonyPatch(typeof(ManeuverPlanComponent), nameof(ManeuverPlanComponent.EnoughDeltaVToAddNode))]
    [HarmonyPrefix]
    public static bool BetterEnoughDeltaVToAddNode(ManeuverPlanComponent __instance, ref bool __result)
    {
        // This method offers two options for the player. Either allow all nodes to be made regardless of the DeltaV capability of the vessel,
        // or prevent node creation based on there being insufficient available DeltaV including contribuitions from use of RCS thrusters

        // __result replaces the return value the original method would have provided

        // Assume we can afford the whole requested burn (no reduction)
        __result = true;

        //if (BetterNodeCheckFix._allowAllNodes.Value)
        //{
        //    // All nodes are allowed without any check for available DeltaV
        //    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: Allow any node.");
        //    return false;
        //}

        // Get the vessel component for this SimulationObject and related vessel and node info
        VesselComponent thisVessel = __instance.SimulationObject.Vessel;
        double previousDeltaVNeeded = TotalDeltaVNeeded(__instance._currentNodes); // The DeltaV that's been commited for any existing nodes
        double RcsDeltaV = RCSDeltaV(thisVessel); // The DeltaV we've got available if we use RCS too

        // Get the remaining DeltaV available that isn't already allocated to other pre-existing nodes
        double remainingDeltaV = __instance.SimulationObject.VesselDeltaV.TotalDeltaVActual + RcsDeltaV - previousDeltaVNeeded;

        // This will prevent the creation of the node if we don't have enough gas for it
        __result = remainingDeltaV > 0.001;

        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: TotalDeltaVActual    = {__instance.SimulationObject.VesselDeltaV.TotalDeltaVActual} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: RcsDeltaV            = {RcsDeltaV} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: previousDeltaVNeeded = {previousDeltaVNeeded} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNode called: returning {__result}: remainingDeltaV = TotalDeltaVActual + RcsDeltaV - previousDeltaVNeeded = {remainingDeltaV} m/s");

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

        //if (BetterNodeCheckFix._allowAllNodes.Value)
        //{
        //    // All nodes are allowed without any check for available DeltaV
        //    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: Allow any node.");
        //    return false;
        //}

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

        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: TotalDeltaVActual       = {__instance.SimulationObject.VesselDeltaV.TotalDeltaVActual} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: RcsDeltaV               = {RcsDeltaV} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: previousDeltaVNeeded       = {totalDeltaVNeeded} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: remainingDeltaV         = TotalDeltaVActual + RcsDeltaV - previousDeltaVNeeded = {remainingDeltaV} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: RequestedDeltaV         = {node.BurnVector.magnitude} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToAddNodeOnRebuild called: returning {__result}: reducedBurnVectorChange = {reducedBurnVectorChange.magnitude} m/s");

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

        //if (BetterNodeCheckFix._allowAllNodes.Value)
        //{
        //    // All nodes are allowed without any check for available DeltaV
        //    BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: Allow any node.");
        //    return false;
        //}

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
            newChange.x *= (float)num3;
            newChange.y *= (float)num3;
            newChange.z *= (float)num3;
            Vector3 vector3 = change;
            string str1 = vector3.ToString();
            vector3 = newChange;
            string str2 = vector3.ToString();
            Debug.Log((object)("change " + str1 + " reduced to newChange = " + str2));
            node.showOutOfFuelMessage = true;
            __result = true;
        }
        __result = remainingDeltaV < (totalDeltaVactual - newBurnVec.magnitude);

        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: TotalDeltaVActual       = {totalDeltaVactual} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: RcsDeltaV               = {RcsDeltaV} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: totalDeltaVNeededOther  = {totalDeltaVNeededOther} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: remainingDeltaV         = TotalDeltaVActual + RcsDeltaV - node.BurnRequiredDV = {remainingDeltaV} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: RequestedDeltaV         = {change.magnitude} m/s");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterEnoughDeltaVToChangeNode called: returning {__result}: reducedBurnVectorChange = {newChange.magnitude} m/s");

        // Returning true indicates the patched method should be called after this, and false indicates it should not be called
        return false;
    }

    static double TotalDeltaVNeeded(List<ManeuverNodeData> nodes, bool excludeCurrentNode = false, ManeuverNodeData currentNode = null)
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
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"RCSDeltaV: monoPropMass = {monoPropMass:N3}");

        // Assume we use all available monoprop for a maneuver
        finalMass = initialMass - monoPropMass;

        // Return the RCS DeltaV available, where DeltaV = Isp * g * ln(initialMass / finalMass)
        // For DeltaV/Isp calculations, g = PhysicsSettings.STANDARD_GRAVITY_EARTH = 9.80665 m/s^2
        return Isp * PhysicsSettings.STANDARD_GRAVITY_EARTH * Math.Log(initialMass / finalMass);
    }

    static double RCSFlowRate(VesselComponent thisVessel)
    {
        // Get the total mass of the vessel at this time, and the mass of the monopropellant
        //double Isp = 260.0; // Assumption - should check
        //double initialMass = thisVessel.totalMass;
        double massFlowRate = 0;
        //double finalMass;

        // Get the resource ID for monopropellant
        // ResourceDefinitionID monopropID = GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName("monopropellant");

        // Get a list of all the parts on this vessel
        var partList = GameManager.Instance.Game.ViewController.GetActiveVehicle().GetSimulationObject().PartOwner.Parts.ToList();

        // Add up all the monoprop found on the vessel
        partList.ForEach((partdata) =>
        {
            partdata.TryGetModule<PartComponentModule_RCS>(out var mRCS); //.   SimulationObject.GetComponents<Module_RCS>(); //  GetComponentType()  ; // TryGetModule<Module_RCS>(out var mRCS);  //GetComponentType() // .GetComponentInPart<Module_RCS>();
            if (mRCS is not null)
            {
                // monoPropMass += partdata.PartResourceContainer.GetResourceContainedData(monopropID).StoredUnits;
                massFlowRate += mRCS.dataRCS.OABGetMaxFuelFlow();
            }
        });

        // Log the monoprop mass found
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"RCSFlowRate: massFlowRate = {massFlowRate:N3}");

        return massFlowRate;
    }

    [HarmonyPatch(typeof(ManeuverPlanSolver), nameof(ManeuverPlanSolver.CalculatePatchConicList))]
    [HarmonyPrefix]
    public static bool PreCalculatePatchConicList(ManeuverPlanSolver __instance, int maneuverNumber)
    {
        // Log some stuff to see if we can figure out what's causing the out of range error
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: maneuverNumber           = {maneuverNumber}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _numConics               = {__instance._numConics}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _numNBody                = {__instance._numNBody}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _patchConicsLimit        = {__instance._patchConicsLimit}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _hasMorePatchesAhead     = {__instance._hasMorePatchesAhead}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _odeManeuverNodes.Count  = {__instance._odeManeuverNodes.Count}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: PatchedConicsList.Count  = {__instance.PatchedConicsList.Count}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: ManeuverTrajectory.Count = {__instance.ManeuverTrajectory.Count}");

        // With gas in the tank and an engine, attempting to make the first node
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: maneuverNumber = 0
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _numConics = 0
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _numNBody = 1
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _patchConicsLimit = 4
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _hasMorePatchesAhead = False
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _odeManeuverNodes.Count = 1
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: PatchedConicsList.Count = 20
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: ManeuverTrajectory.Count = 20

        // With only RCS, attempting to make the first node
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: maneuverNumber = 0
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _numConics = 0
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _numNBody = 0
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _patchConicsLimit = 4
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _hasMorePatchesAhead = False
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: _odeManeuverNodes.Count = 1
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: PatchedConicsList.Count = 20
        //[Info: CF/BetterNodeCheckFix] PreCalculatePatchConicList called: ManeuverTrajectory.Count = 20

        __instance.InitializeFirstPatchConic(maneuverNumber);
        bool flag = __instance._odeManeuverNodes.Count > maneuverNumber + 1;
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: flag = {flag}");

        return (__instance._numConics + __instance._numNBody - 1) >= 0;
    }

    [HarmonyPatch(typeof(ManeuverPlanSolver), nameof(ManeuverPlanSolver.GetBurnDuration))]
    [HarmonyPostfix]
    public static bool BetterGetBurnDuration(ManeuverPlanSolver __instance, ref double __result, double deltaV, OdeSolverStageComponent deltaVStage)
    {
        // Log some stuff to see if we can figure out what's causing the out of range error
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: maneuverNumber           = {maneuverNumber}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _numConics               = {__instance._numConics}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _numNBody                = {__instance._numNBody}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _patchConicsLimit        = {__instance._patchConicsLimit}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _hasMorePatchesAhead     = {__instance._hasMorePatchesAhead}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: _odeManeuverNodes.Count  = {__instance._odeManeuverNodes.Count}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: PatchedConicsList.Count  = {__instance.PatchedConicsList.Count}");
        //BetterNodeCheckFix.Instance.Logger.LogInfo($"PreCalculatePatchConicList called: ManeuverTrajectory.Count = {__instance.ManeuverTrajectory.Count}");

        //double num = math.length(deltaVStage.ExhaustVelocity);
        //__result = deltaVStage.InitialAssemblyMass / deltaVStage.MassFlux * (1.0 - math.exp(-deltaV / num));

        //BetterNodeCheckFix.Instance.Logger.LogInfo($"BetterGetBurnDuration called with __result = {__result}");

        if (__result == 0)
        {
            // BetterNodeCheckFix.Instance.Logger.LogDebug($"BetterGetBurnDuration called with __result = {__result}");

            // Get the vessel component for this SimulationObject and related vessel and node info
            VesselComponent thisVessel = __instance._orbiter.SimulationObject.Vessel;
            double RcsDeltaV = RCSDeltaV(thisVessel);
            //BetterNodeCheckFix.Instance.Logger.LogInfo($"BetterGetBurnDuration called with __result = {__result}, RcsDeltaV = {RcsDeltaV}");
            if (RcsDeltaV > 0)
            {
                double Isp = 260;
                double exhaustVel = Isp * 9.80665;
                // double maxFlowRate = maxThrust / exhaustVel;
                double RcsFlowRate = RCSFlowRate(thisVessel);
                double num = math.length(exhaustVel); // TODO: Fix this, it can't be right...
                __result = deltaVStage.InitialAssemblyMass / RcsFlowRate * (1.0 - math.exp(-deltaV / num));
                //BetterNodeCheckFix.Instance.Logger.LogInfo($"BetterGetBurnDuration calculated __result = {__result}, RcsFlowRate = {RcsFlowRate}, num = {num}");
            }
        }

        // Returning true indicates the patched method should be called after this, and false indicates it should not be called
        return false;
    }

    [HarmonyPatch(typeof(ManeuverPlanSolver), nameof(ManeuverPlanSolver.CalculateOdeManeuverNodes))]
    [HarmonyPrefix]
    public static bool BetterCalculateOdeManeuverNodes(ManeuverPlanSolver __instance, List<ManeuverNodeData> maneuverNodes)
    {
        // Log some stuff to see if we can figure out what's causing the out of range error
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"PreCalculatePatchConicList called: maneuverNumber           = {maneuverNumber}");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"PreCalculatePatchConicList called: _numConics               = {__instance._numConics}");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"PreCalculatePatchConicList called: _numNBody                = {__instance._numNBody}");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"PreCalculatePatchConicList called: _patchConicsLimit        = {__instance._patchConicsLimit}");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"PreCalculatePatchConicList called: _hasMorePatchesAhead     = {__instance._hasMorePatchesAhead}");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"PreCalculatePatchConicList called: _odeManeuverNodes.Count  = {__instance._odeManeuverNodes.Count}");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"PreCalculatePatchConicList called: PatchedConicsList.Count  = {__instance.PatchedConicsList.Count}");
        //BetterNodeCheckFix.Instance.Logger.LogDebug($"PreCalculatePatchConicList called: ManeuverTrajectory.Count = {__instance.ManeuverTrajectory.Count}");

        //BetterNodeCheckFix.Instance.Logger.LogInfo($"BetterCalculateOdeManeuverNodes: maneuverNodes.Count = {maneuverNodes.Count}");

        __instance.RemoveAllOdeManeuverNodes();
        VesselDeltaVComponent vesselDeltaV = __instance._orbiter.SimulationObject.VesselDeltaV;
        int num1 = DeltaVExtensions.CurrentStage(vesselDeltaV);
        int num2 = 0;
        int capacity = num1 + 1;
        __instance._deltaVStageStack.Clear();
        __instance._lastStageIndexWithFuel = num1;
        for (int inStage = num1; inStage >= num2; --inStage)
        {
            __instance._deltaVStageStack.Add(new List<OdeSolverStageComponent>());
            List<OdeSolverStageComponent> solverStageComponents = vesselDeltaV.TryGetOdeSolverStageComponents(inStage);
            if (solverStageComponents != null)
            {
                if (solverStageComponents.Count > 0)
                    __instance._lastStageIndexWithFuel = capacity - inStage - 1;
                foreach (OdeSolverStageComponent solverStageComponent in solverStageComponents)
                    __instance._deltaVStageStack[__instance._deltaVStageStack.Count - 1].Add(solverStageComponent);
            }
        }
        foreach (ManeuverNodeData maneuverNode in maneuverNodes)
        {
            Vector nodeBurnDirVector = __instance._orbiter.SimulationObject.ManeuverPlan.GetManeuverNodeBurnDirVector(maneuverNode.NodeID);
            double magnitude = maneuverNode.BurnVector.magnitude;
            Vector deltaV1 = MathDP.Scale(magnitude, nodeBurnDirVector);
            maneuverNode.BurnRequiredDV = magnitude;
            OdeManeuverNode odeManeuverNode = new OdeManeuverNode(deltaV1, magnitude, new List<OdeStage>(capacity), maneuverNode.Time);
            for (int index = 0; index < capacity; ++index)
            {
                OdeStage odeStage = new OdeStage(new List<OdeSolverStageComponent>());
                odeManeuverNode.OdeStages.Add(odeStage);
            }
            __instance._odeManeuverNodes.Add(odeManeuverNode);
            double num3 = 0.0;
            double num4 = 0.0;
            bool flag = true;
            int index1 = 0;
            OdeSolverStageComponent solverStageComponent1 = new OdeSolverStageComponent();
            while (flag && index1 < __instance._deltaVStageStack.Count)
            {
                List<OdeSolverStageComponent> deltaVstage = __instance._deltaVStageStack[index1];
                for (int index2 = 0; index2 < deltaVstage.Count; ++index2)
                {
                    OdeSolverStageComponent deltaVStage = deltaVstage[index2];
                    if (deltaVStage.DeltaV >= 0.0)
                    {
                        double deltaV2 = magnitude - num4;
                        double3 exhaustVelocity = (double3)(math.length(deltaVStage.ExhaustVelocity) * nodeBurnDirVector.vector);
                        OdeSolverStageComponent solverStageComponent2;
                        if (deltaVStage.DeltaV >= deltaV2)
                        {
                            num4 += deltaV2;
                            deltaVStage.DeltaV -= deltaV2;
                            deltaVstage[index2] = deltaVStage;
                            double burnTime = math.max(ManeuverPlanSolver.GetBurnDuration(deltaV2, deltaVStage), 0.001);
                            double startUt = maneuverNode.Time + num3;
                            double endUt = maneuverNode.Time + num3 + burnTime;
                            num3 += burnTime;
                            solverStageComponent2 = new OdeSolverStageComponent(deltaVStage.StageID, startUt, endUt, exhaustVelocity, deltaVStage.InitialAssemblyMass, deltaVStage.MassFlux, burnTime, deltaVStage.MaxPossibleBurnTime);
                            odeManeuverNode.OdeStages[index1].OdeSubStages.Add(solverStageComponent2);
                            flag = false;
                            break;
                        }
                        num4 += deltaVStage.DeltaV;
                        deltaVStage.DeltaV = 0.0;
                        deltaVstage[index2] = deltaVStage;
                        double burnTime1 = math.min(ManeuverPlanSolver.GetBurnDuration(deltaV2, deltaVStage), deltaVStage.MaxPossibleBurnTime);
                        double startUt1 = maneuverNode.Time + num3;
                        double endUt1 = maneuverNode.Time + num3 + burnTime1;
                        num3 += burnTime1;
                        solverStageComponent2 = new OdeSolverStageComponent(deltaVStage.StageID, startUt1, endUt1, exhaustVelocity, deltaVStage.InitialAssemblyMass, deltaVStage.MassFlux, burnTime1, deltaVStage.MaxPossibleBurnTime);
                        solverStageComponent1 = solverStageComponent2;
                        odeManeuverNode.OdeStages[index1].OdeSubStages.Add(solverStageComponent2);
                    }
                }
                if (flag)
                {
                    ++index1;
                    if (index1 == __instance._deltaVStageStack.Count)
                    {
                        double num5 = magnitude - num4;
                        double initialAssemblyMass = solverStageComponent1.InitialAssemblyMass - solverStageComponent1.MassFlux * solverStageComponent1.MaxPossibleBurnTime;
                        double burnTime = initialAssemblyMass * num5 / (math.length(solverStageComponent1.ExhaustVelocity) * solverStageComponent1.MassFlux);
                        OdeStage odeStage = new OdeStage(new List<OdeSolverStageComponent>());
                        odeManeuverNode.OdeStages.Add(odeStage);
                        OdeSolverStageComponent solverStageComponent3 = new OdeSolverStageComponent(solverStageComponent1.StageID, solverStageComponent1.EndUT, solverStageComponent1.EndUT + burnTime, solverStageComponent1.ExhaustVelocity, initialAssemblyMass, solverStageComponent1.MassFlux, burnTime, double.PositiveInfinity);
                        odeManeuverNode.OdeStages[odeManeuverNode.OdeStages.Count - 1].OdeSubStages.Add(solverStageComponent3);
                    }
                }
            }
            maneuverNode.BurnDuration = num3;
        }

        // Returning true indicates the patched method should be called after this, and false indicates it should not be called
        return false;
    }
}
