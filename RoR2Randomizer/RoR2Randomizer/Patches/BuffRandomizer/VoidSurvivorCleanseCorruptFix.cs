﻿using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.Buff;
using RoR2Randomizer.Utility;
using RoR2Randomizer.Utility.Patching;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.Patches.BuffRandomizer
{
    [PatchClass]
    static class VoidFiendCleanseCorruptFix
    {
        static void Apply()
        {
            IL.RoR2.DotController.RemoveAllDots += DotController_RemoveAllDots;
        }

        static void Cleanup()
        {
            IL.RoR2.DotController.RemoveAllDots -= DotController_RemoveAllDots;
        }

        static void DotController_RemoveAllDots(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<Dictionary<int, DotController>>(_ => _.TryGetValue(default, out Discard<DotController>.Value))),
                              x => x.MatchBrfalse(out _)))
            {
                int dotControllerLocIndex = 0;
                foundCursors[0].GotoPrev(x => x.MatchLdloca(out dotControllerLocIndex));

                ILCursor brFalseCursor = foundCursors[1];
                if (brFalseCursor.Next.Operand is ILLabel brFalseLbl)
                {
                    c.Index = brFalseCursor.Index + 1;

                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldloc, dotControllerLocIndex);
                    c.EmitDelegate(static (GameObject victimObject, DotController controller) =>
                    {
                        if (victimObject && controller && BuffRandomizerController.IsActive)
                        {
                            if (victimObject.GetComponent<VoidSurvivorController>())
                            {
                                bool containsCorruptModeDOT(out int corruptModeDOTIndex)
                                {
                                    if (BuffRandomizerController.TryGetReplacementBuffIndex(DLC1Content.Buffs.VoidSurvivorCorruptMode.buffIndex, out BuffIndex corruptModeReplacement))
                                    {
                                        for (int i = 0; i < controller.dotStackList.Count; i++)
                                        {
                                            DotController.DotStack stack = controller.dotStackList[i];
                                            if (stack != null && stack.dotDef != null && stack.dotDef.associatedBuff && stack.dotDef.associatedBuff.buffIndex == corruptModeReplacement)
                                            {
                                                corruptModeDOTIndex = i;
                                                return true;
                                            }
                                        }
                                    }

                                    corruptModeDOTIndex = -1;
                                    return false;
                                }

                                if (containsCorruptModeDOT(out int corruptModeDOTIndex))
                                {
                                    for (int i = controller.dotStackList.Count - 1; i >= 0; i--)
                                    {
                                        if (i != corruptModeDOTIndex)
                                        {
                                            controller.RemoveDotStackAtServer(i);
                                        }
                                    }

                                    return true;
                                }
                            }
                        }

                        return false;
                    });

                    c.Emit(OpCodes.Brtrue, brFalseLbl);
                }
                else
                {
                    Log.Warning($"{nameof(OpCodes.Brfalse)} operand is not {nameof(ILLabel)}");
                }
            }
            else
            {
                Log.Warning("unable to find patch location");
            }
        }
    }
}
