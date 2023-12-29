using HarmonyLib;
using KSP.UI;
using UnityEngine;

namespace CommunityFixes.Fix.ResourceManagerUIFix;

[Fix("Resource Manager UI Fix")]
public class ResourceManagerUIFix : BaseFix
{
    public override void OnInitialized()
    {
        HarmonyInstance.PatchAll(typeof(ResourceManagerUIFix));
    }

    [HarmonyPatch(typeof(ResourceManagerUI), nameof(ResourceManagerUI.Initialize))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void PatchInitialize(ResourceManagerUI __instance)
    {
        __instance._partFamiliesTransform = GameObject.Find(
            "/GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Popup Canvas/ResourceManagerApp(Clone)/" +
            "KSP2UIWindow/Root/Window-App/GRP-Body/MainContent/LeftContent/Parts List Section"
        ).GetComponent<RectTransform>();
    }
}