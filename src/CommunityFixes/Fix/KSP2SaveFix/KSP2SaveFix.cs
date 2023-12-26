namespace CommunityFixes.Fix.KSP2SaveFix;

[Fix("KSP 2 Save Fix")]
public class KSP2SaveFix : BaseFix
{
    public static KSP2SaveFix Instance;

    public KSP2SaveFix()
    {
        Instance = this;
    }

    public override void OnInitialized()
    {
        HarmonyInstance.PatchAll(typeof(KSP2SaveFix_GetState));
    }
}