﻿using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Patches.BossRandomizer;
using RoR2Randomizer.PrefabMarkers;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using UnityEngine;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class VoidCradleInfestors_SpawnHook
    {
        static void Apply()
        {
            On.RoR2.ScriptedCombatEncounter.Spawn += ScriptedCombatEncounter_Spawn;
        }

        static void Cleanup()
        {
            On.RoR2.ScriptedCombatEncounter.Spawn -= ScriptedCombatEncounter_Spawn;
        }

        static void ScriptedCombatEncounter_Spawn(On.RoR2.ScriptedCombatEncounter.orig_Spawn orig, ScriptedCombatEncounter self, ref ScriptedCombatEncounter.SpawnInfo spawnInfo)
        {
            if (ExplicitSpawnRandomizerController.IsActive && ConfigManager.ExplicitSpawnRandomizer.RandomizeVoidInfestors)
            {
                if (self.GetComponent<VoidCradleMarker>())
                {
                    if (ExplicitSpawnRandomizerController.TryReplaceSummon(ref spawnInfo.spawnCard.prefab, out GameObject originalPrefab))
                    {
                        SpawnCard card = spawnInfo.spawnCard;
                        void onSpawned(SpawnCard.SpawnResult result)
                        {
                            if (result.spawnRequest != null && result.spawnRequest.spawnCard == card)
                            {
                                result.spawnRequest.spawnCard.prefab = originalPrefab;

                                GenericScriptedSpawnHook.OnSpawned -= onSpawned;

                                if (result.success && result.spawnedInstance)
                                {
                                    if (originalPrefab && originalPrefab.TryGetComponent<CharacterMaster>(out CharacterMaster originalMasterPrefab))
                                    {
                                        ExplicitSpawnRandomizerController.RegisterSpawnedReplacement(result.spawnedInstance, originalMasterPrefab.masterIndex);
                                    }
                                }
                            }
                        }

                        GenericScriptedSpawnHook.OnSpawned += onSpawned;
                    }
                }
            }

            orig(self, ref spawnInfo);
        }
    }
}
