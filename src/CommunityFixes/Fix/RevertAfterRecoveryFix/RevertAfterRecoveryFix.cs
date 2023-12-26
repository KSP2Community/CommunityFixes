using HarmonyLib;
using KSP.Game;
using KSP.UI;

namespace CommunityFixes.Fix.RevertAfterRecoveryFix;

[Fix("Fix reverting after recovery")]
public class RevertAfterRecoveryFix : BaseFix
{
    public override void OnInitialized()
    {
        HarmonyInstance.PatchAll(typeof(RevertAfterRecoveryFix));
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.HandleButtonStates))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void HandleButtonStatesPatch(UIManager __instance)
    {
        __instance._escapeMenu.GetComponentInChildren<ESCMenuUIController>().UpdateRevertAvailability();
    }
}