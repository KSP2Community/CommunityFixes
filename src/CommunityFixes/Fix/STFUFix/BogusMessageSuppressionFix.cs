namespace CommunityFixes.Fix.STFUFix;

[Fix("Suppress bogus and unhelpful log messages")]
public class SuppressTransmissionsFalselyUrgentFix : BaseFix
{
    public override void OnInitialized()
    {
        HarmonyInstance.PatchAll(typeof(STFUPatches));
    }
}
