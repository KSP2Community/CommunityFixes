using KSP.Sim.impl;

namespace CommunityFixes.Fix.VesselLandedState;

[Fix("Vessel Landed State")]
public class VesselLandedStateFix: BaseFix
{
    private void LateUpdate()
    {
        if (Game?.GlobalGameState?.GetGameState() is not { IsFlightMode: true })
        {
            return;
        }

        var vessel = Game?.ViewController?.GetActiveSimVessel();
        if (vessel is
            {
                Situation: VesselSituations.Landed or VesselSituations.Splashed,
                AltitudeFromSurface: > 50, SrfSpeedMagnitude: > 5
            })
        {
            vessel.Landed = false;
            vessel.Splashed = false;
        }
    }
}