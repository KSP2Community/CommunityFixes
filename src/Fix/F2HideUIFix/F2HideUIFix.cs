using HarmonyLib;
using KSP.Game;
using KSP.UI.Flight;

namespace CommunityFixes.Fix.F2HideUIFix;

[Fix("F2 Hide UI Fix")]
public class F2HideUIFix: BaseFix
{
    public override void OnInitialized()
    {
        _harmony.PatchAll(typeof(F2HideUIFix));
    }

    [HarmonyPatch(typeof(UIFlightHud), nameof(UIFlightHud.OnFlightHudCanvasActiveChanged))]
    [HarmonyPostfix]
    public static void UIFlightHud_OnFlightHudCanvasActiveChanged(bool isVisible)
    {
        // Just disable the UI camera when F2 is pressed
        GameManager.Instance.Game.UI.GetRootCanvas().worldCamera.enabled = isVisible;
    }
}