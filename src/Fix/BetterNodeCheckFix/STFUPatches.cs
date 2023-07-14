using HarmonyLib;
using KSP.Game;
using KSP.Logging;
using KSP.Map;
using KSP.Sim.impl;

namespace CommunityFixes.Fix.BetterNodeCheckFix;
internal class STFUPatches
{
  [HarmonyPatch(typeof(Map3DTrajectoryEvents), nameof(Map3DTrajectoryEvents.UpdateViewForOrbiter))]
  [HarmonyPrefix]
  public static bool BetterUpdateViewForOrbiter(Map3DTrajectoryEvents __instance, OrbiterComponent orbiter)
  {
    if (orbiter == null)
      GlobalLog.Warn((object)"GenerateEventsForVessel() called with vessel.Orbiter == null! Events will not be updated");
    else if (orbiter.PatchedConicSolver == null)
    {
      var activeVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
      var currentTarget = activeVessel?.TargetObject;
      if (!currentTarget.IsCelestialBody)
        GlobalLog.Warn((object)"GenerateEventsForVessel() called with vessel.Orbiter.patchedConicSolver == null. Events will not be updated");
    }
    else if ((UnityEngine.Object)__instance._mapCamera?.UnityCamera == (UnityEngine.Object)null)
    {
      GlobalLog.Warn((object)"GenerateEventsForVessel() called with a null map camera. Events will not be updated");
    }
    else
    {
      IGGuid globalId = orbiter.SimulationObject.GlobalId;
      __instance.UpdateViewForCurrentTrajectory(orbiter, globalId);
      __instance.UpdateViewForManeuverTrajectory(orbiter, globalId);
      __instance.UpdateViewForTargeter(orbiter.OrbitTargeter, orbiter, globalId);
    }
    return false;
  }
}
