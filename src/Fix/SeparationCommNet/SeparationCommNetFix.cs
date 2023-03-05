namespace CommunityFixes.Fix.SeparationCommNet;

public class SeparationCommNetFix: BaseFix
{
    public override string Name => "Separation CommNet Fix";

    public static SeparationCommNetFix Instance;

    public SeparationCommNetFix()
    {
        Instance = this;
    }

    public override void OnInitialized()
    {
        _harmony.PatchAll(typeof(TelemetryComponent_OnUpdate));
    }
}