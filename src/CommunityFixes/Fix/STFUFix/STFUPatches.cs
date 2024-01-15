using HarmonyLib;
using KSP.Map;
using KSP.Sim.impl;
using SpaceWarp.API.Game;

namespace CommunityFixes.Fix.STFUFix;

internal class STFUPatches
{
    [HarmonyPatch(typeof(Map3DTrajectoryEvents), nameof(Map3DTrajectoryEvents.UpdateViewForOrbiter))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static bool BetterUpdateViewForOrbiter(Map3DTrajectoryEvents __instance, OrbiterComponent orbiter)
    {
        return orbiter.PatchedConicSolver != null || Vehicle.ActiveSimVessel?.TargetObject?.IsCelestialBody is not true;
    }
}