﻿using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer.Mithrix;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.Boss
{
    public partial class BossRandomizerController : MonoBehaviour
    {
        public static class Mithrix
        {
            public static readonly SerializableEntityStateType MithrixHurtInitialState = new SerializableEntityStateType(typeof(SpellChannelEnterState));

            public static readonly InitializeOnAccess<ReturnStolenItemsOnGettingHit> MithrixReturnItemsComponent = new InitializeOnAccess<ReturnStolenItemsOnGettingHit>(() =>
            {
                return Caches.MasterPrefabs["BrotherHurtMaster"].bodyPrefab.GetComponent<ReturnStolenItemsOnGettingHit>();
            });

            public static void Initialize()
            {
                if (MithrixPhaseTracker.Instance != null)
                {
                    MithrixPhaseTracker.Instance.IsInFight.OnChanged += IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive += SyncBossReplacementCharacter_OnReceive;
            }

            public static void Uninitialize()
            {
                if (MithrixPhaseTracker.Instance != null)
                {
                    MithrixPhaseTracker.Instance.IsInFight.OnChanged -= IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive -= SyncBossReplacementCharacter_OnReceive;
            }

            static void IsInFight_OnChanged(bool isInFight)
            {
                if (NetworkServer.active)
                {
                    if (isInFight)
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = (ref SpawnCard card, out GenericScriptedSpawnHook.ResetCardDelegate resetCardFunc) =>
                        {
                            resetCardFunc = null;

                            if (ConfigManager.BossRandomizer.Enabled)
                            {
                                if ((ConfigManager.BossRandomizer.RandomizeMithrix && (card == SpawnCardTracker.MithrixNormalSpawnCard || card == SpawnCardTracker.MithrixHurtSpawnCard))
                                 || (ConfigManager.BossRandomizer.RandomizeMithrixPhase2 && SpawnCardTracker.IsPartOfMithrixPhase2(card)))
                                {
                                    GameObject originalPrefab = card.prefab;

                                    resetCardFunc = (ref SpawnCard c) => c.prefab = originalPrefab;

                                    CharacterReplacements.TryReplaceMasterPrefab(ref card.prefab);
                                }
                            }
                        };

                        GenericScriptedSpawnHook.OnSpawned += handleSpawnedMithrixCharacterServer;
                    }
                    else
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = null;
                        GenericScriptedSpawnHook.OnSpawned -= handleSpawnedMithrixCharacterServer;
                    }
                }
            }

            static void SyncBossReplacementCharacter_OnReceive(GameObject masterObject, BossReplacementType replacementType)
            {
                if (MithrixPhaseTracker.Instance == null || !MithrixPhaseTracker.Instance.IsInFight)
                    return;

                switch (replacementType)
                {
                    case BossReplacementType.MithrixNormal:
                    case BossReplacementType.MithrixHurt:
                    case BossReplacementType.MithrixPhase2:
#if DEBUG
                        Log.Debug($"Running {nameof(handleSpawnedMithrixCharacterClient)}");
#endif
                        handleSpawnedMithrixCharacterClient(masterObject, replacementType);
                        break;
                }
            }

            static void handleSpawnedMithrixCharacterClient(GameObject masterObject, BossReplacementType type)
            {
                BaseMithrixReplacement baseMithrixReplacement;
                if (type == BossReplacementType.MithrixPhase2)
                {
#if DEBUG
                    Log.Debug($"Adding {nameof(MithrixPhase2EnemiesReplacement)} component to {masterObject}");
#endif
                    baseMithrixReplacement = masterObject.AddComponent<MithrixPhase2EnemiesReplacement>();
                }
                else
                {
                    bool isHurt;
                    if ((isHurt = type == BossReplacementType.MithrixHurt) || type == BossReplacementType.MithrixNormal)
                    {
#if DEBUG
                        Log.Debug($"Adding {nameof(MainMithrixReplacement)} component to {masterObject}, isHurt={isHurt}");
#endif

                        MainMithrixReplacement mainMithrixReplacement;
                        baseMithrixReplacement = mainMithrixReplacement = masterObject.AddComponent<MainMithrixReplacement>();
                        mainMithrixReplacement.IsHurt = isHurt;
                    }
                    else
                    {
                        Log.Warning($"Invalid mithrix replacement type '{type}'");
                        return;
                    }
                }

                baseMithrixReplacement.Initialize();
            }

            static void handleSpawnedMithrixCharacterServer(SpawnCard.SpawnResult spawnResult)
            {
                if (ConfigManager.BossRandomizer.AnyMithrixRandomizerEnabled && MithrixPhaseTracker.Instance != null && MithrixPhaseTracker.Instance.IsInFight)
                {
                    BaseMithrixReplacement baseMithrixReplacement = null;
                    if (MithrixPhaseTracker.Instance.Phase == 2)
                    {
                        if (ConfigManager.BossRandomizer.RandomizeMithrixPhase2 && SpawnCardTracker.IsPartOfMithrixPhase2(spawnResult.spawnRequest.spawnCard))
                        {
                            baseMithrixReplacement = spawnResult.spawnedInstance.AddComponent<MithrixPhase2EnemiesReplacement>();
                        }
                    }
                    else
                    {
                        if (ConfigManager.BossRandomizer.RandomizeMithrix)
                        {
                            bool isHurtMithrixReplacement;
                            if ((isHurtMithrixReplacement = spawnResult.spawnRequest.spawnCard == SpawnCardTracker.MithrixHurtSpawnCard) ||
                                spawnResult.spawnRequest.spawnCard == SpawnCardTracker.MithrixNormalSpawnCard)
                            {
                                MainMithrixReplacement mainMithrixReplacement = spawnResult.spawnedInstance.AddComponent<MainMithrixReplacement>();
                                mainMithrixReplacement.IsHurt = isHurtMithrixReplacement;

                                baseMithrixReplacement = mainMithrixReplacement;
                            }
                        }
                    }

                    if (baseMithrixReplacement)
                    {
                        baseMithrixReplacement.Initialize();
                    }
                }
            }

            public static bool IsReplacedMithrix(GameObject master)
            {
                return master && master.GetComponent<MainMithrixReplacement>();
            }

            public static bool IsReplacedMithrixPhase2Spawn(GameObject master)
            {
                return master && master.GetComponent<MithrixPhase2EnemiesReplacement>();
            }

            public static bool IsReplacedPartOfMithrixFight(GameObject master)
            {
                return master && master.GetComponent<BaseMithrixReplacement>();
            }
        }
    }
}