using KSP.Sim.impl;

namespace CommunityFixes.Fix.VesselLandedState;

[Fix("Vessel Landed State")]
public class VesselLandedStateFix: BaseFix
{
    private void LateUpdate()
    {
        var gameStateConfiguration = Game?.GlobalGameState?.GetGameState();
        if (gameStateConfiguration != null && gameStateConfiguration.IsFlightMode)
        {
            var vessel = Game?.ViewController?.GetActiveSimVessel();
            if (vessel != null && vessel.Situation == VesselSituations.Landed && vessel.AltitudeFromTerrain > 50)
            {
                vessel.Landed = false;
            }
        }
    }
}
