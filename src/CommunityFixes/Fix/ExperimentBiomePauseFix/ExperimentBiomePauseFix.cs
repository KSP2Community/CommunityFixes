using HarmonyLib;
using KSP.Game.Science;
using KSP.Sim.impl;
using KSP.Modules;
using System.Reflection;

namespace CommunityFixes.Fix.ExperimentBiomePauseFix;

[Fix("Fix experiments pausing when switching biome for scenarios where biome is irrelevant")]
public class ExperimentBiomePauseFix : BaseFix
{
    public static ExperimentBiomePauseFix Instance;

    // This fix makes assumptions about the game's code and reads/writes private state, which can end up in save files.
    // In order to avoid accidentally breaking anything, we only apply the patch to known-broken versions of the game.
    public static readonly HashSet<string> KNOWN_BROKEN_VERSIONS = new() { "0.2.0.0.30291" };
    public static string GameVersion = typeof(VersionID).GetField("VERSION_TEXT", BindingFlags.Static | BindingFlags.Public)
            ?.GetValue(null) as string;

    public override void OnInitialized()
    {
        Instance = this;
        if (KNOWN_BROKEN_VERSIONS.Contains(GameVersion))
        {
            HarmonyInstance.PatchAll(typeof(ExperimentBiomePauseFix));
        }
        else
        {
            Logger.LogError($"Not enabling experiment biome pause fix - game version {GameVersion} may not be broken");
        }
    }

    // RefreshLocationsValidity has a bug where it unconditionally pauses experiments when switching biome, even if the
    // experiment is still valid and doesn't care about the biome. To fix this, we prevent the function from being
    // called if the part has running experiments, the experiments don't care about the biome, and the other parameters
    // of the experiment are unchanged.
    // To avoid breaking things when we skip RefreshLocationsValidity, we also update some private state that the method
    // is responsible for when skipping it.
    [HarmonyPatch(typeof(PartComponentModule_ScienceExperiment), "RefreshLocationsValidity")]
    [HarmonyPrefix]
    public static bool RefreshLocationsValidityPrefix(
        ref VesselComponent ____vesselComponent,
        ref ResearchLocation ____currentLocation,
        ref Data_ScienceExperiment ___dataScienceExperiment
    )
    {
        if (____vesselComponent?.mainBody == null || ____vesselComponent?.VesselScienceRegionSituation.ResearchLocation == null)
        {
            return true;
        }

        var newLocation = new ResearchLocation(
            requiresRegion: true, // Placeholder, assigned per-experiment below
            bodyName: ____vesselComponent.mainBody.bodyName,
            scienceSituation: ____vesselComponent.VesselScienceRegionSituation.ResearchLocation.ScienceSituation,
            scienceRegion: ____vesselComponent.VesselScienceRegionSituation.ResearchLocation.ScienceRegion
        );

        bool safeToSkip = true;
        var newRegions = new List<string>();
        for (int i = 0; i < ___dataScienceExperiment.ExperimentStandings.Count; i++)
        {
            var standing = ___dataScienceExperiment.ExperimentStandings[i];
            if (standing.CurrentExperimentState == ExperimentState.RUNNING &&
                !standing.RegionRequired &&
                standing.ExperimentLocation.BodyName == newLocation.BodyName &&
                standing.ExperimentLocation.ScienceSituation == newLocation.ScienceSituation)
            {
                newLocation.RequiresRegion = standing.RegionRequired;
                newRegions.Append(newLocation.ScienceRegion);
                continue;
            }
            safeToSkip = false;
        }

        if (safeToSkip)
        {
            Instance.Logger.LogInfo("Skipping PartComponentModule_ScienceExperiment.RefreshLocationsValidity - experiment is still valid.");
            ____currentLocation = newLocation;
            for (int i = 0; i < newRegions.Count; i++)
            {
                ___dataScienceExperiment.ExperimentStandings[i].ExperimentLocation.SetScienceRegion(newRegions[i]);
            }
        }
        return !safeToSkip;
    }
}
