using KSP.Sim.impl;

namespace CommunityFixes.Fix.VesselLandedState;

[Fix("Vessel Landed State")]
public class VesselLandedStateFix: BaseFix
{
    private void LateUpdate()
    {
        var gameStateConfiguration = Game?.GlobalGameState?.GetGameState();
        if (gameStateConfiguration is { IsFlightMode: true })
        {
            var vessel = Game?.ViewController?.GetActiveSimVessel();
            if (vessel is { Situation: VesselSituations.Landed, AltitudeFromTerrain: > 50, SrfSpeedMagnitude: > 5 })
            {
                vessel.Landed = false;
            }
        }
    }
}
