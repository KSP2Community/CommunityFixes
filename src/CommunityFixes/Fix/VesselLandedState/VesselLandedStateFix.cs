using KSP.Sim.impl;

namespace CommunityFixes.Fix.VesselLandedState;

[Fix("Vessel Landed State")]
public class VesselLandedStateFix: BaseFix
{
    private void LateUpdate()
    {
        if (Game == null)
        {
            return;
        }

        var gameStateConfiguration = Game.GlobalGameState?.GetGameState();
        if (gameStateConfiguration is not { IsFlightMode: true })
        {
            return;
        }

        var vessel = Game.ViewController?.GetActiveSimVessel();
        if (vessel is { Situation: VesselSituations.Landed, AltitudeFromSurface: > 50, SrfSpeedMagnitude: > 5 })
        {
            vessel.Landed = false;
        }
    }
}
