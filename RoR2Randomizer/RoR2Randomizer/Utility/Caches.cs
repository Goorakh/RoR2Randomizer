﻿using HG;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.Utility
{
    public static class Caches
    {
        public static readonly InitializeOnAccessDictionary<string, CharacterMaster> MasterPrefabs = new InitializeOnAccessDictionary<string, CharacterMaster>(name => MasterCatalog.FindMasterPrefab(name)?.GetComponent<CharacterMaster>());

        public static readonly InitializeOnAccessDictionary<GameObject, float> CharacterBodyRadius = new InitializeOnAccessDictionary<GameObject, float>((GameObject bodyPrefab, out float radius) =>
        {
            static bool tryGetRadius(Collider collider, out float radius)
            {
                if (collider is SphereCollider sphereCollider)
                {
                    radius = sphereCollider.radius;
                    return true;
                }
                else if (collider is CapsuleCollider capsuleCollider)
                {
                    radius = Mathf.Max(capsuleCollider.radius, capsuleCollider.height);
                    return true;
                }
                else
                {
                    radius = -1f;
                    return false;
                }
            }

            if (!bodyPrefab.TryGetComponent<CharacterBody>(out CharacterBody body) || body.bodyIndex == BodyIndex.None ||
                // The following characters have Colliders that don't match their actual collision size, so force them to be calculated from the model bounds
                (body.bodyIndex != Bodies.BeetleQueenBodyIndex &&
                 body.bodyIndex != Bodies.ClayBossBodyIndex &&
                 body.bodyIndex != Bodies.LunarGolemBodyIndex))
            {
                foreach (Collider collider in bodyPrefab.GetComponents<Collider>())
                {
                    if (collider.enabled && tryGetRadius(collider, out radius))
                    {
                        return true;
                    }
                }
            }

            if (body.TryGetComponent<ModelLocator>(out ModelLocator modelLocator))
            {
                Transform model = modelLocator.modelTransform;
                if (model)
                {
                    if (model.TryGetComponent<CharacterModel>(out CharacterModel characterModel) && characterModel.TryGetModelBounds(out Bounds modelBounds))
                    {
                        Vector3 position = ((MonoBehaviour)body).transform.position;
                        Vector3 max = modelBounds.max;
                        Vector3 min = modelBounds.min;

                        radius = (Mathf.Abs(position.x - max.x) + Mathf.Abs(position.x - min.x) +
                                  Mathf.Abs(position.y - max.y) + Mathf.Abs(position.y - min.y) +
                                  Mathf.Abs(position.z - max.z) + Mathf.Abs(position.z - min.z)) / 6f;

                        return true;
                    }
                }
            }

            radius = -1f;
            return false;
        });

        public static class Masters
        {
            public static MasterCatalog.MasterIndex Gup { get; private set; } = MasterCatalog.MasterIndex.none;

            public static MasterCatalog.MasterIndex Heretic { get; private set; } = MasterCatalog.MasterIndex.none;

            public static MasterCatalog.MasterIndex MalachiteUrchin { get; private set; } = MasterCatalog.MasterIndex.none;

            public static MasterCatalog.MasterIndex VoidInfestor { get; private set; } = MasterCatalog.MasterIndex.none;

            public static MasterCatalog.MasterIndex SoulWisp { get; private set; } = MasterCatalog.MasterIndex.none;

            public static MasterCatalog.MasterIndex HealingCore { get; private set; } = MasterCatalog.MasterIndex.none;

            public static MasterCatalog.MasterIndex ShopkeeperNewt { get; private set; } = MasterCatalog.MasterIndex.none;

            [SystemInitializer(typeof(MasterCatalog))]
            static void Init()
            {
                Gup = MasterCatalog.FindMasterIndex(Constants.MasterNames.GUP_NAME);
#if DEBUG
                if (!Gup.isValid) Log.Warning($"Unable to find master index '{Constants.MasterNames.GUP_NAME}'");
#endif

                Heretic = MasterCatalog.FindMasterIndex(Constants.MasterNames.HERETIC_NAME);
#if DEBUG
                if (!Heretic.isValid) Log.Warning($"Unable to find master index '{Constants.MasterNames.HERETIC_NAME}'");
#endif

                MalachiteUrchin = MasterCatalog.FindMasterIndex(Constants.MasterNames.MALACHITE_URCHIN_NAME);
#if DEBUG
                if (!MalachiteUrchin.isValid) Log.Warning($"Unable to find master index '{Constants.MasterNames.MALACHITE_URCHIN_NAME}'");
#endif

                VoidInfestor = MasterCatalog.FindMasterIndex(Constants.MasterNames.VOID_INFESTOR_NAME);
#if DEBUG
                if (!VoidInfestor.isValid) Log.Warning($"Unable to find master index '{Constants.MasterNames.VOID_INFESTOR_NAME}'");
#endif

                SoulWisp = MasterCatalog.FindMasterIndex(Constants.MasterNames.SOUL_WISP_NAME);
#if DEBUG
                if (!SoulWisp.isValid) Log.Warning($"Unable to find master index '{Constants.MasterNames.SOUL_WISP_NAME}'");
#endif

                HealingCore = MasterCatalog.FindMasterIndex(Constants.MasterNames.HEALING_CORE_NAME);
#if DEBUG
                if (!HealingCore.isValid) Log.Warning($"Unable to find master index '{Constants.MasterNames.HEALING_CORE_NAME}'");
#endif

                ShopkeeperNewt = MasterCatalog.FindMasterIndex(Constants.MasterNames.SHOPKEEPER_NEWT_NAME);
#if DEBUG
                if (!ShopkeeperNewt.isValid) Log.Warning($"Unable to find master index '{Constants.MasterNames.SHOPKEEPER_NEWT_NAME}'");
#endif
            }
        }

        public static class Bodies
        {
            public static BodyIndex VoidlingBaseBodyIndex { get; private set; } = BodyIndex.None;

            public static BodyIndex VoidlingPhase1BodyIndex { get; private set; } = BodyIndex.None;

            public static BodyIndex VoidlingPhase2BodyIndex { get; private set; } = BodyIndex.None;

            public static BodyIndex VoidlingPhase3BodyIndex { get; private set; } = BodyIndex.None;

            public static BodyIndex BeetleQueenBodyIndex { get; private set; } = BodyIndex.None;

            public static BodyIndex ClayBossBodyIndex { get; private set; } = BodyIndex.None;

            public static BodyIndex LunarGolemBodyIndex { get; private set; } = BodyIndex.None;

            public static BodyIndex HereticBodyIndex { get; private set; } = BodyIndex.None;

            public static BodyIndex SquidTurretBodyIndex { get; private set; } = BodyIndex.None;

            public static BodyIndex MinorConstructOnKillBodyIndex { get; private set; } = BodyIndex.None;

            [SystemInitializer(typeof(BodyCatalog))]
            static void Init()
            {
                VoidlingBaseBodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.VOIDLING_BASE_NAME);
#if DEBUG
                if (VoidlingBaseBodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.VOIDLING_BASE_NAME}'");
#endif

                VoidlingPhase1BodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.VOIDLING_PHASE_1_NAME);
#if DEBUG
                if (VoidlingPhase1BodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.VOIDLING_PHASE_1_NAME}'");
#endif

                VoidlingPhase2BodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.VOIDLING_PHASE_2_NAME);
#if DEBUG
                if (VoidlingPhase2BodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.VOIDLING_PHASE_2_NAME}'");
#endif

                VoidlingPhase3BodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.VOIDLING_PHASE_3_NAME);
#if DEBUG
                if (VoidlingPhase3BodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.VOIDLING_PHASE_3_NAME}'");
#endif

                BeetleQueenBodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.BEETLE_QUEEN_NAME);
#if DEBUG
                if (BeetleQueenBodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.BEETLE_QUEEN_NAME}'");
#endif

                ClayBossBodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.CLAY_BOSS_NAME);
#if DEBUG
                if (ClayBossBodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.CLAY_BOSS_NAME}'");
#endif

                LunarGolemBodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.LUNAR_GOLEM_NAME);
#if DEBUG
                if (LunarGolemBodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.LUNAR_GOLEM_NAME}'");
#endif

                HereticBodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.HERETIC_NAME);
#if DEBUG
                if (HereticBodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.HERETIC_NAME}'");
#endif

                SquidTurretBodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.SQUID_TURRET_NAME);
#if DEBUG
                if (SquidTurretBodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.SQUID_TURRET_NAME}'");
#endif

                MinorConstructOnKillBodyIndex = BodyCatalog.FindBodyIndex(Constants.BodyNames.MINOR_CONSTRUCT_ON_KILL_NAME);
#if DEBUG
                if (MinorConstructOnKillBodyIndex == BodyIndex.None) Log.Warning($"Unable to find body index '{Constants.BodyNames.MINOR_CONSTRUCT_ON_KILL_NAME}'");
#endif
            }
        }

        public static class Scene
        {
            public static SceneIndex ArtifactTrialSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex NewtShopSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex GoldShoresSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex ObliterateSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex LunarScavFightSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex CommencementSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex VoidlingFightSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex VoidLocusSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex AbandonedAqueductSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex OldCommencementSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex AITestSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex TestSceneSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static ReadOnlyArray<SceneIndex> SimulacrumStageIndices { get; private set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsSimulacrumStage(SceneDef scene)
            {
                return IsSimulacrumStage(scene.sceneDefIndex);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsSimulacrumStage(SceneIndex index)
            {
                return ReadOnlyArray<SceneIndex>.BinarySearch(SimulacrumStageIndices, index) >= 0;
            }

            public static ReadOnlyArray<SceneIndex> PossibleStartingStagesIndices { get; private set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsPossibleStartingStage(SceneDef scene)
            {
                return IsPossibleStartingStage(scene.sceneDefIndex);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsPossibleStartingStage(SceneIndex index)
            {
                return ReadOnlyArray<SceneIndex>.BinarySearch(PossibleStartingStagesIndices, index) >= 0;
            }

            [SystemInitializer(typeof(SceneCatalog), typeof(GameModeCatalog))]
            static void Init()
            {
                ArtifactTrialSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.ARTIFACT_TRIAL_SCENE_NAME);
#if DEBUG
                if (ArtifactTrialSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.ARTIFACT_TRIAL_SCENE_NAME}'");
#endif

                NewtShopSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.NEWT_SHOP_SCENE_NAME);
#if DEBUG
                if (NewtShopSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.NEWT_SHOP_SCENE_NAME}'");
#endif

                GoldShoresSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.GOLD_SHORES_SCENE_NAME);
#if DEBUG
                if (GoldShoresSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.GOLD_SHORES_SCENE_NAME}'");
#endif

                ObliterateSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.OBLITERATE_SCENE_NAME);
#if DEBUG
                if (ObliterateSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.OBLITERATE_SCENE_NAME}'");
#endif

                LunarScavFightSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.LUNAR_SCAV_FIGHT_SCENE_NAME);
#if DEBUG
                if (LunarScavFightSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.LUNAR_SCAV_FIGHT_SCENE_NAME}'");
#endif

                CommencementSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.COMMENCEMENT_SCENE_NAME);
#if DEBUG
                if (CommencementSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.COMMENCEMENT_SCENE_NAME}'");
#endif

                VoidlingFightSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.VOIDLING_FIGHT_SCENE_NAME);
#if DEBUG
                if (VoidlingFightSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.VOIDLING_FIGHT_SCENE_NAME}'");
#endif

                VoidLocusSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.VOID_LOCUS_SCENE_NAME);
#if DEBUG
                if (VoidLocusSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.VOID_LOCUS_SCENE_NAME}'");
#endif

                AbandonedAqueductSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.ABANDONED_AQUEDUCT_SCENE_NAME);
#if DEBUG
                if (AbandonedAqueductSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.ABANDONED_AQUEDUCT_SCENE_NAME}'");
#endif

                OldCommencementSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.OLD_COMMENCEMENT_SCENE_NAME);
#if DEBUG
                if (OldCommencementSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.OLD_COMMENCEMENT_SCENE_NAME}'");
#endif

                AITestSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.AI_TEST_SCENE_NAME);
#if DEBUG
                if (AITestSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.AI_TEST_SCENE_NAME}'");
#endif

                TestSceneSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.TESTSCENE_SCENE_NAME);
#if DEBUG
                if (TestSceneSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.TESTSCENE_SCENE_NAME}'");
#endif

                HashSet<SceneIndex> simulacrumStageIndices = new HashSet<SceneIndex>();

                const string IT_RUN_NAME = "InfiniteTowerRun";
                Run itRunPrefab = GameModeCatalog.FindGameModePrefabComponent(IT_RUN_NAME);
                if (itRunPrefab)
                {
                    SceneCollection startingSceneGroup = itRunPrefab.startingSceneGroup;
                    if (startingSceneGroup)
                    {
                        static void handleCollection(SceneCollection collection, in HashSet<SceneCollection> alreadyHandledCollections, in HashSet<SceneIndex> stageIndices)
                        {
                            if (alreadyHandledCollections.Add(collection))
                            {
                                foreach (SceneCollection.SceneEntry entry in collection.sceneEntries)
                                {
                                    SceneDef scene = entry.sceneDef;
                                    if (scene)
                                    {
                                        stageIndices.Add(scene.sceneDefIndex);

                                        SceneCollection destinationsGroup = scene.destinationsGroup;
                                        if (destinationsGroup)
                                        {
                                            handleCollection(destinationsGroup, alreadyHandledCollections, stageIndices);
                                        }
                                    }
                                }
                            }
                        }

                        handleCollection(startingSceneGroup, new HashSet<SceneCollection>(), simulacrumStageIndices);
                    }
                }
                else
                {
                    Log.Warning($"Unable to find run prefab '{IT_RUN_NAME}'");
                }

                SimulacrumStageIndices = new ReadOnlyArray<SceneIndex>(simulacrumStageIndices.Where(i => i != SceneIndex.Invalid).OrderBy(i => i).ToArray());
                
                SceneIndex[] possibleStartingStages = Array.Empty<SceneIndex>();

                const string CLASSIC_RUN_NAME = "ClassicRun";
                Run runPrefab = GameModeCatalog.FindGameModePrefabComponent(CLASSIC_RUN_NAME);
                if (runPrefab)
                {
                    SceneCollection startingSceneGroup = runPrefab.startingSceneGroup;
                    if (startingSceneGroup)
                    {
                        ReadOnlyArray<SceneCollection.SceneEntry> sceneEntries = startingSceneGroup.sceneEntries;
                        possibleStartingStages = new SceneIndex[sceneEntries.Length];
                        for (int i = 0; i < sceneEntries.Length; i++)
                        {
                            possibleStartingStages[i] = sceneEntries[i].sceneDef.sceneDefIndex;
                        }
                    }
                }
                else
                {
                    Log.Warning($"Unable to find run prefab '{CLASSIC_RUN_NAME}'");
                }

                PossibleStartingStagesIndices = new ReadOnlyArray<SceneIndex>(possibleStartingStages.Where(i => i != SceneIndex.Invalid).Distinct().OrderBy(i => i).ToArray());
            }
        }

        public static class DLC
        {
            public static ExpansionDef SOTV { get; private set; }

            [SystemInitializer(typeof(ExpansionCatalog))]
            static void Init()
            {
                SOTV = ExpansionCatalog.expansionDefs.FirstOrDefault(static e => e.name == Constants.DLCNames.SOTV_NAME);
#if DEBUG
                if (!SOTV) Log.Warning($"Unable to find {nameof(ExpansionDef)} {Constants.DLCNames.SOTV_NAME}");
#endif
            }
        }
    }
}
