using HarmonyLib;
using KSP.Game;
using KSP.Messages;
using KSP.UI.Flight;

namespace CommunityFixes.Fix.F2HideUIFix;

[Fix("F2 Hide UI Fix")]
public class F2HideUIFix: BaseFix
{
    public override void OnInitialized()
    {
        _harmony.PatchAll(typeof(F2HideUIFix));
        Game.Messages.Subscribe<GameStateEnteredMessage>(msg =>
        {
            var message = (GameStateEnteredMessage)msg;
            if (message.StateBeingEntered != GameState.FlightView && message.StateBeingEntered != GameState.Map3DView)
            {
                Game.UI.GetRootCanvas().worldCamera.enabled = true;
            }
        });
    }

    [HarmonyPatch(typeof(UIFlightHud), nameof(UIFlightHud.OnFlightHudCanvasActiveChanged))]
    [HarmonyPostfix]
    public static void UIFlightHud_OnFlightHudCanvasActiveChanged(bool isVisible)
    {
        // Just disable the UI camera when F2 is pressed
        GameManager.Instance.Game.UI.GetRootCanvas().worldCamera.enabled = isVisible;
    }
}