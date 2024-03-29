﻿using EntityStates.Missions.Goldshores;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2Randomizer.RandomizerControllers.Boss;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using UnityEngine;

namespace RoR2Randomizer.Patches.BossRandomizer.Aurelionite
{
    [PatchClass]
    public class AurelioniteFightTracker : BossTracker<AurelioniteFightTracker>
    {
        static void ApplyPatches()
        {
            new AurelioniteFightTracker().applyPatches();
        }

        static void CleanupPatches()
        {
            Instance?.cleanupPatches();
        }

        protected override void applyPatches()
        {
            base.applyPatches();

            BossRandomizerController.Aurelionite.AurelioniteReplacementReceivedClient += AurelioniteReplacementReceivedClient;

            IL.EntityStates.Missions.Goldshores.GoldshoresBossfight.SpawnBoss += GoldshoresBossfight_SpawnBoss;

            On.EntityStates.Missions.Goldshores.Exit.OnEnter += Exit_OnEnter;
        }

        protected override void cleanupPatches()
        {
            base.cleanupPatches();

            BossRandomizerController.Aurelionite.AurelioniteReplacementReceivedClient -= AurelioniteReplacementReceivedClient;

            IL.EntityStates.Missions.Goldshores.GoldshoresBossfight.SpawnBoss -= GoldshoresBossfight_SpawnBoss;

            On.EntityStates.Missions.Goldshores.Exit.OnEnter -= Exit_OnEnter;
        }

        void GoldshoresBossfight_SpawnBoss(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryFindNext(out ILCursor[] cursors,
                              x => x.MatchLdsfld<GoldshoresBossfight>(nameof(GoldshoresBossfight.combatEncounterPrefab)),
                              x => x.MatchCall(SymbolExtensions.GetMethodInfo(() => UnityEngine.Object.Instantiate<GameObject>(default))),
                              x => x.MatchStfld<GoldshoresBossfight>(nameof(GoldshoresBossfight.scriptedCombatEncounter))))
            {
                ILCursor last = cursors[cursors.Length - 1];

                // Move after the entire match
                last.Index++;

                last.Emit(OpCodes.Ldarg_0);
                last.EmitDelegate((GoldshoresBossfight instance) =>
                {
                    if (instance.scriptedCombatEncounter && !SpawnCardTracker.AurelioniteSpawnCard)
                    {
                        SpawnCardTracker.AurelioniteSpawnCard = instance.scriptedCombatEncounter.spawns[0].spawnCard;
                    }

                    IsInFight.Value = true;
                });
            }
            else
            {
                Log.Warning("failed to find patch location");
            }
        }

        void AurelioniteReplacementReceivedClient(AurelioniteReplacement replacement)
        {
            IsInFight.Value = true;
        }

        void Exit_OnEnter(On.EntityStates.Missions.Goldshores.Exit.orig_OnEnter orig, Exit self)
        {
            IsInFight.Value = false;
            orig(self);
        }
    }
}
