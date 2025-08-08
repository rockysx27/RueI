namespace RueI.Patches;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using Hints;
using Mirror;
using RueI.API;

using static HarmonyLib.AccessTools;

/// <summary>
/// Patches <see cref="HintDisplay.Show(Hint)"/> to update the display after the hint is over.
/// </summary>
[HarmonyPatch(typeof(HintDisplay), nameof(HintDisplay.Show))]
internal static class SendHintPatch
{
#pragma warning disable SA1600 // Elements should be documented
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
#pragma warning restore SA1600 // Elements should be documented
    {
        LocalBuilder local = generator.DeclareLocal(typeof(ReferenceHub));
        Label label = generator.DefineLabel();

        MethodInfo method = typeof(ReferenceHub).GetMethods().First(
            x => x.Name == nameof(ReferenceHub.TryGetHub) && x.GetParameters().Any(x => x.ParameterType == typeof(NetworkConnection)));

#pragma warning disable SA1114 // Parameter list should follow declaration
#pragma warning disable SA1115 // Parameter should follow comma
#pragma warning disable SA1515 // Single-line comma should be preceded by blank line
        CodeMatcher matcher = new CodeMatcher(instructions, generator)
            .MatchEndForward(
                // if (hint == null) throw new ArgumentNullException("hint")
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Callvirt, Method(typeof(HashSet<NetworkConnection>), nameof(HashSet<NetworkConnection>.Contains))),
                new CodeMatch(x => x.opcode == OpCodes.Brtrue_S))
            .Advance(1)
            .InsertAndAdvance(
                // if (ReferenceHub.TryGetHub(base.netIdentity.connectionToClient, out ReferenceHub hub))
                new CodeInstruction(OpCodes.Ldloc_0),
                new(OpCodes.Ldloca, local.LocalIndex),
                new(OpCodes.Call, method),
                new(OpCodes.Brfalse_S, label),

                // Display.Get(hub).SetUpdateIn(hint.DurationScalar);
                new(OpCodes.Ldloc, local.LocalIndex),
                new(OpCodes.Call, Method(typeof(RueDisplay), nameof(RueDisplay.Get), new Type[] { typeof(ReferenceHub) })),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Callvirt, PropertyGetter(typeof(DisplayableObject<SharedHintData>), nameof(DisplayableObject<SharedHintData>.DurationScalar))),
                new(OpCodes.Callvirt, Method(typeof(RueDisplay), nameof(RueDisplay.SetUpdateIn))))
#pragma warning restore SA1114 // Parameter list should follow declaration
#pragma warning restore SA1115 // Parameter should follow comma
#pragma warning restore SA1515 // Single-line comma should be preceded by blank line
            .Advance(1)
            .AddLabels(new Label[] { label });

        return matcher.InstructionEnumeration();
    }
}