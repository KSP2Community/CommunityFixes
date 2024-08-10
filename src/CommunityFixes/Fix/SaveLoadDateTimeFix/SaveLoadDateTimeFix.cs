namespace CommunityFixes.Fix.SaveLoadDateTimeFix;

[Fix("Save/Load Date/Time Fix")]
public class SaveLoadDateTimeFix : BaseFix
{
    public override void OnInitialized()
    {
        HarmonyInstance.PatchAll(typeof(SaveLoadDateTimeFix_Patch));
    }
}
