using HarmonyLib;
using KSP.Map;

namespace CommunityFixes.Fix.StickyOrbitMarkers;

[HarmonyPatch(typeof(Map3DOrbitalMarker), "UpdateDisplayState")]
public class Map3DOrbitalMarker_UpdateDisplayState
{
    public static void Prefix(ref bool ____maneuverGizmoActive, ref bool __state)
    {
        __state = ____maneuverGizmoActive;
        ____maneuverGizmoActive = false;
    }
    
    public static void Postfix(ref bool ____maneuverGizmoActive, bool __state)
    {
        ____maneuverGizmoActive = __state;
    }
}