using System.Reflection;
using HarmonyLib;
using KSP.Game;
using KSP.Logging;
using KSP.Map;
using KSP.Sim.impl;

namespace CommunityFixes.Fix.STFUFix;
internal class STFUPatches
{
    private const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;
    private static Type TYPE = typeof(Map3DTrajectoryEvents);
    private static FieldInfo mapCameraField = TYPE.GetField("_mapCamera", FLAGS);
    private static MethodInfo updateForCurrent = TYPE.GetMethod("UpdateViewForCurrentTrajectory", FLAGS);
    private static MethodInfo updateForManeuver = TYPE.GetMethod("UpdateViewForManeuverTrajectory", FLAGS);
    private static MethodInfo updateForTargeter = TYPE.GetMethod("UpdateViewForTargeter", FLAGS);

    private static MapCamera GetCamera(Map3DTrajectoryEvents inst)
    {
        return mapCameraField.GetValue(inst) as MapCamera;
    }

    private static void UpdateViews(Map3DTrajectoryEvents inst, OrbiterComponent orbiter, IGGuid globalId)
    {
        updateForCurrent.Invoke(inst, [orbiter, globalId]);
        updateForManeuver.Invoke(inst, [orbiter, globalId]);
        updateForTargeter.Invoke(inst, [orbiter.OrbitTargeter, orbiter, globalId]);
    }

    [HarmonyPatch(typeof(Map3DTrajectoryEvents), "UpdateViewForOrbiter")]
    [HarmonyPrefix]
    public static bool BetterUpdateViewForOrbiter(Map3DTrajectoryEvents __instance, OrbiterComponent orbiter)
    {
        if (orbiter == null)
            GlobalLog.Warn("GenerateEventsForVessel() called with vessel.Orbiter == null! Events will not be updated");
        else if (orbiter.PatchedConicSolver == null)
        {
            var activeVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
            var currentTarget = activeVessel?.TargetObject;
            if (!currentTarget.IsCelestialBody)
                GlobalLog.Warn("GenerateEventsForVessel() called with vessel.Orbiter.patchedConicSolver == null. Events will not be updated");
        }
        else if (GetCamera(__instance)?.UnityCamera == null)
        {
            GlobalLog.Warn("GenerateEventsForVessel() called with a null map camera. Events will not be updated");
        }
        else
        {
            UpdateViews(__instance, orbiter, orbiter.SimulationObject.GlobalId);
        }
        return false;
    }
}
