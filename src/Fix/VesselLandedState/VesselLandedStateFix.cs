using KSP.Sim.impl;

namespace CommunityFixes.Fix.VesselLandedState;

[Fix("Vessel Landed State")]
public class VesselLandedStateFix: BaseFix
{
    private void LateUpdate()
    {
        GameStateConfiguration gameStateConfiguration = GameManager.Instance.Game.GlobalGameState.GetGameState();
        if (gameStateConfiguration.IsFlightMode)
            var vessel = Game?.ViewController?.GetActiveSimVessel();
            if (vessel != null && vessel.Situation == VesselSituations.Landed && vessel.AltitudeFromTerrain > 50)
            {
                vessel.Landed = false;
            }
        }  
    }
}
