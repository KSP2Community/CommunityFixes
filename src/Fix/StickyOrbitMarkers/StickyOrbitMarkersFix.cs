namespace CommunityFixes.Fix.StickyOrbitMarkers;

public class StickyOrbitMarkersFix : BaseFix
{
    public override string Name => "Sticky Orbit Markers";

    public override void OnInitialized()
    {
        _harmony.PatchAll(typeof(Map3DOrbitalMarker_OnManeuverGizmoStateChange));
        _harmony.PatchAll(typeof(Map3DOrbitalMarker_UpdateDisplayState));
    }
}