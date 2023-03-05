namespace CommunityFixes.Fix.StickyOrbitMarkers;

[Fix("Sticky Orbit Markers")]
public class StickyOrbitMarkersFix : BaseFix
{
    public override void OnInitialized()
    {
        _harmony.PatchAll(typeof(Map3DOrbitalMarker_OnManeuverGizmoStateChange));
        _harmony.PatchAll(typeof(Map3DOrbitalMarker_UpdateDisplayState));
    }
}