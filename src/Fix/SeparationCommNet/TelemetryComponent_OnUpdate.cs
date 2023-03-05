using HarmonyLib;
using KSP.Sim.impl;

namespace CommunityFixes.Fix.SeparationCommNet
{
    [HarmonyPatch(typeof(TelemetryComponent), nameof(TelemetryComponent.OnUpdate))]
    public class TelemetryComponent_OnUpdate
    {
        public static void Prefix(TelemetryComponent __instance)
        {
            if (__instance.SimulationObject?.Vessel is null)
            {
                return;
            }

            try
            {
                var args = new object[] { null };
                var getRange = AccessTools.Method(typeof(TelemetryComponent), "GetMaxTransmitterDistanceFromParts");
                var maxRange = (double)getRange.Invoke(__instance, args);
                var isActive = (bool)args[0];

                var node = __instance.CommNetNode;

                if (node.IsActive == isActive && !(Math.Abs(node.MaxRange - maxRange) > 0.1))
                {
                    return;
                }

                var comManager = KSP.Game.GameManager.Instance.Game.SessionManager.CommNetManager;

                SeparationCommNetFix.Instance.Logger.Info(
                    $"Refreshed CommNet from {node.MaxRange}/{node.IsActive} to {maxRange}/{isActive}"
                );

                node.MaxRange = maxRange;
                node.IsActive = isActive;
                comManager.UnregisterNode(node);
                comManager.RegisterNode(node);
            }
            catch (Exception ex)
            {
                SeparationCommNetFix.Instance.Logger.Error($"Could not refresh: {ex.Message}");
            }
        }
    }
}