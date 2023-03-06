using HarmonyLib;
using KSP.Sim.impl;

namespace CommunityFixes.Fix.KSP2SaveFix;

[HarmonyPatch(typeof(VesselComponent), nameof(VesselComponent.GetState))]
public class KSP2SaveFix_GetState
{
    // Called before VesselComponent.GetState
    // This is the part that crashes during serialization

    // Last 3 functions in the call stack after a save:
    // [EXC 20:35:06.678] NullReferenceException: Object reference not set to an instance of an object
    //  KSP.Sim.impl.VesselComponent.GetState() (at<8c5bdf369a8c45f68951f69eb825ef73>:0)
    //  KSP.Game.Serialization.SerializationUtility.VesselToSerializable(KSP.Sim.impl.SimulationObjectModel vessel, System.Boolean isAutosave) (at<8c5bdf369a8c45f68951f69eb825ef73>:0)
    //  KSP.Game.Load.CollectFlightDataFlowAction.CollectVesselComponents(System.Byte playerID) (at<8c5bdf369a8c45f68951f69eb825ef73>:0)

    // Occurs because the controlOwner hasn't been correctly set after decoupling / undocking
    // After saving in a file or in memory and loading afterwards (this can also be triggered after reverting to VAB), the faulty controlOwner is set to null
    // Once controlOwner is set to null, it crashes during the GetState() call

    // Ideal fix would be to fix the decoupling / docking, I will look into it later if needed but this is a decent workaround until the patch comes out
    public static void Prefix(VesselComponent __instance)
    {
        // Get control owner for the vessel
        PartComponent controlOwner = __instance.GetControlOwner();
        if (controlOwner is null)
        {
            KSP2SaveFix.Instance.Logger.LogInfo("Control 0wner not found for  " + __instance.GlobalId);
            // Gather command modules
            List<PartComponentModule_Command> partModules = __instance.SimulationObject.PartOwner.GetPartModules<PartComponentModule_Command>();
            // Set ownership to the first command module
            if (partModules.Count > 0)
            {
                KSP2SaveFix.Instance.Logger.LogInfo("Set control to " + partModules[0].Part.GlobalId);
                __instance.SetControlOwner(partModules[0].Part);
            }
            else
            {
                // Otherwise try to set it to the root part, whatever it is
                if (__instance.SimulationObject.PartOwner != null)
                {
                    KSP2SaveFix.Instance.Logger.LogInfo("Set control to " + __instance.SimulationObject.PartOwner.RootPart.GlobalId);
                    __instance.SetControlOwner(__instance.SimulationObject.PartOwner.RootPart);
                }
            }
        }
    }
}