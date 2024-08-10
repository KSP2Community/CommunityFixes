using System.Reflection.Emit;
using HarmonyLib;
using KSP.Sim.impl;
using SpaceWarp.API.Logging;

namespace CommunityFixes.Fix.TimeWarpThrustFix;

[Fix("Fixes time warp thrust rounding error.")]
public class TimeWarpThrustFix : BaseFix
{
    private static ILogger _logger;

    public override void OnInitialized()
    {
        _logger = Logger;
        HarmonyInstance.PatchAll(typeof(TimeWarpThrustFix));
    }

    private static bool CheckNeedRoundingError(double positionError, double velocityError)
    {
        var needError = positionError >= 0.01 || velocityError >= 0.01;
        if (needError)
        {
            _logger.LogDebug($"pos_err={positionError}, vel_err={velocityError}");
        }

        return false;
    }

    [HarmonyPatch(typeof(VesselComponent), nameof(VesselComponent.HandleOrbitalPhysicsUnderThrustStart))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> VesselComponent_HandleOrbitalPhysicsUnderThrustStart(
        IEnumerable<CodeInstruction> bodyToReplace
    )
    {
        var propGetSqrMagnitude = typeof(Vector3d).GetProperty("sqrMagnitude")!.GetGetMethod();
        var res = TranspilerHelper.Replace(
            bodyToReplace,
            [
                new TranspilerHelper.ILLookupKey { OpCode = OpCodes.Ldloca_S },
                new TranspilerHelper.ILLookupKey { OpCode = OpCodes.Call, Operand = propGetSqrMagnitude },
                new TranspilerHelper.ILLookupKey { OpCode = OpCodes.Ldc_R8 },
                new TranspilerHelper.ILLookupKey { OpCode = OpCodes.Bge_Un },
                new TranspilerHelper.ILLookupKey { OpCode = OpCodes.Ldloca_S },
                new TranspilerHelper.ILLookupKey { OpCode = OpCodes.Call, Operand = propGetSqrMagnitude },
                new TranspilerHelper.ILLookupKey { OpCode = OpCodes.Ldc_R8 },
                new TranspilerHelper.ILLookupKey { OpCode = OpCodes.Bge_Un },
            ],
            oldBody =>
            {
                var checkNeedRoundingError = CheckNeedRoundingError;
                var instructions = oldBody.ToArray();
                return
                [
                    instructions[0], instructions[1],
                    instructions[4], instructions[5],
                    new CodeInstruction(OpCodes.Call, checkNeedRoundingError.Method),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    instructions[7], new CodeInstruction(OpCodes.Nop),
                ];
            },
            1..1
        );

        return res;
    }
}