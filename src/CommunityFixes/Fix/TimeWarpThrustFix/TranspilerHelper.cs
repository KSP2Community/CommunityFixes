using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace CommunityFixes.Fix.TimeWarpThrustFix;

internal static class TranspilerHelper
{
    public sealed class ILLookupKey
    {
        public OpCode OpCode { get; set; }
        [CanBeNull] public object Operand { get; set; }
    }

    public static IEnumerable<CodeInstruction> Replace(
        IEnumerable<CodeInstruction> oldBody,
        ILLookupKey[] lookupKeys,
        Func<IReadOnlyCollection<CodeInstruction>, IEnumerable<CodeInstruction>> repl,
        Range expectedTimes
    )
    {
        var queue = new Queue<CodeInstruction>(lookupKeys.Length);
        var foundCount = 0;

        foreach (var instruction in oldBody)
        {
            queue.Enqueue(instruction);
            if (queue.Count < lookupKeys.Length) continue;

            if (queue.Zip(lookupKeys, (instr, lookup) =>
                    instr.opcode == lookup.OpCode &&
                    (lookup.Operand is null || Equals(instr.operand, lookup.Operand))
                ).All(b => b))
            {
                foundCount++;
                foreach (var modInstr in repl(queue))
                {
                    yield return modInstr;
                }

                queue.Clear();
                continue;
            }

            yield return queue.Dequeue();
        }

        foreach (var instr in queue)
        {
            yield return instr;
        }

        if (foundCount < expectedTimes.Start.Value || foundCount > expectedTimes.End.Value)
        {
            throw new InvalidOperationException($"Found expected IL {foundCount} times, instead of {expectedTimes}");
        }
    }
}