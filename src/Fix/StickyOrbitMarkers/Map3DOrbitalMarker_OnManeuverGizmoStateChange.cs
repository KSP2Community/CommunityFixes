using HarmonyLib;
using KSP.Api.CoreTypes;
using KSP.Map;

namespace CommunityFixes.Fix.StickyOrbitMarkers;

[HarmonyPatch(typeof(Map3DOrbitalMarker), "OnManeuverGizmoStateChange")]
public class Map3DOrbitalMarker_OnManeuverGizmoStateChange
{
    public static void Prefix(bool ____isPinned, ref bool __state)
    {
        __state = ____isPinned;
    }

    public static void Postfix(ref bool ____isPinned, ref Property<bool> ____isExpandedProp, bool __state)
    {
        ____isPinned = __state;
        ____isExpandedProp.SetValue(__state);
    }
}