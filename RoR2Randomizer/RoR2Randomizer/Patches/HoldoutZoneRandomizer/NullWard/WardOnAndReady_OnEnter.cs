﻿#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates.Missions.Arena.NullWard;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.HoldoutZone;
using UnityEngine;

namespace RoR2Randomizer.Patches.HoldoutZoneRandomizer.NullWard
{
    [PatchClass]
    static class WardOnAndReady_NullRefFix
    {
        static void Apply()
        {
            IL.EntityStates.Missions.Arena.NullWard.WardOnAndReady.OnEnter += WardOnAndReady_OnEnter;
        }

        static void Cleanup()
        {
            IL.EntityStates.Missions.Arena.NullWard.WardOnAndReady.OnEnter -= WardOnAndReady_OnEnter;
        }

        static void WardOnAndReady_OnEnter(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(WardOnAndReady_NullRefFix)}.{nameof(WardOnAndReady_OnEnter)}";

            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertySetter(typeof(SphereZone), nameof(SphereZone.Networkradius)))))
            {
                c.Emit(OpCodes.Ldarg_0);

                c.Remove();
                c.Emit(OpCodes.Call, RandomizedHoldoutZoneController.SetSphereZoneRadius_MI);
            }
            else
            {
                Log.Warning($"{LOG_PREFIX}: SphereZone.Networkradius Patch failed");
            }

            if (c.TryFindNext(out ILCursor[] foundCursors,
                              x => x.MatchLdfld<NullWardBaseState>(nameof(NullWardBaseState.sphereZone)),
                              x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertyGetter(typeof(Behaviour), nameof(Behaviour.enabled)))))
            {
                ILCursor iLCursor = foundCursors[foundCursors.Length - 1];
                iLCursor.Emit(OpCodes.Ldarg_0);
                iLCursor.Emit(OpCodes.Call, RandomizedHoldoutZoneController.getZone_MI);
            }
            else
            {
                Log.Warning($"{LOG_PREFIX}: Behaviour.enabled Patch failed");
            }
        }
    }
}
#endif