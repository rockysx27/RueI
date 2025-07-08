namespace RueI.Patches;

using System;
using System.Collections.Generic;
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

#pragma warning disable SA1114 // Parameter list should follow declaration
#pragma warning disable SA1115 // Parameter should follow comma
        return new CodeMatcher(instructions, generator)
            .MatchEndForward(

                // if (hint == null) throw new ArgumentNullException("hint")
                new CodeMatch(OpCodes.Ldstr, "hint"),
                new CodeMatch(x => x.opcode == OpCodes.Newobj),
                new CodeMatch(OpCodes.Throw))
            .Advance(1)
            .InsertAndAdvance(

                // if (ReferenceHub.TryGetHub(base.netIdentity.connectionToClient, out ReferenceHub hub))
                new CodeInstruction(OpCodes.Ldarg_0),
                new(OpCodes.Call, PropertyGetter(typeof(NetworkBehaviour), nameof(NetworkBehaviour.netIdentity))),
                new(OpCodes.Callvirt, PropertyGetter(typeof(NetworkIdentity), nameof(NetworkIdentity.connectionToClient))),
                new(OpCodes.Ldloca, local.LocalIndex),
                new(OpCodes.Call, Method(typeof(ReferenceHub), nameof(ReferenceHub.TryGetHub), new Type[] { typeof(NetworkConnection), typeof(IntPtr) })),
                new(OpCodes.Brfalse_S, label),

                // Display.Get(hub).SetUpdateIn(hint.DurationScalar);
                new(OpCodes.Ldloc, local.LocalIndex),
                new(OpCodes.Call, Method(typeof(Display), nameof(Display.Get), new Type[] { typeof(ReferenceHub) })),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Callvirt, PropertyGetter(typeof(DisplayableObject<SharedHintData>), nameof(DisplayableObject<SharedHintData>.DurationScalar))),
                new(OpCodes.Callvirt, Method(typeof(Display), nameof(Display.SetUpdateIn))))
#pragma warning restore SA1114 // Parameter list should follow declaration
#pragma warning restore SA1115 // Parameter should follow comma
            .Advance(1)
            .AddLabels(new Label[] { label })
            .InstructionEnumeration();
    }
}