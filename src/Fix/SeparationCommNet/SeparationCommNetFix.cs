namespace CommunityFixes.Fix.SeparationCommNet;

[Fix("Separation CommNet Fix")]
public class SeparationCommNetFix: BaseFix
{
    public static SeparationCommNetFix Instance;

    public SeparationCommNetFix()
    {
        Instance = this;
    }

    public override void OnInitialized()
    {
        HarmonyInstance.PatchAll(typeof(TelemetryComponent_OnUpdate));
    }
}