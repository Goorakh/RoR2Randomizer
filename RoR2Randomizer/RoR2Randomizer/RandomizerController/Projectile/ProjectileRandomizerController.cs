﻿using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.ProjectileRandomizer;
using RoR2Randomizer.RandomizerController.Boss;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Projectile
{
    public class ProjectileRandomizerController : Singleton<ProjectileRandomizerController>
    {
        static readonly InitializeOnAccess<int[]> _projectileIndicesToRandomize = new InitializeOnAccess<int[]>(() =>
        {
            return ProjectileCatalog.projectilePrefabProjectileControllerComponents
                                    .Where(projectile =>
                                    {
                                        if (!projectile)
                                            return false;

                                        if (projectile.TryGetComponent<ProjectileFireChildren>(out ProjectileFireChildren projectileFireChildren)
                                            && !projectileFireChildren.childProjectilePrefab)
                                        {
#if DEBUG
                                            Log.Debug($"Projectile Randomizer: Excluding {projectile.name} due to invalid {nameof(ProjectileFireChildren)} setup");
#endif

                                            return false;
                                        }

                                        return true;
                                    })
                                    .Select(p => p.catalogIndex)
                                    .ToArray();
        });

        static readonly RunSpecific<bool> _hasReceivedProjectileReplacementsFromServer = new RunSpecific<bool>();

        static readonly RunSpecific<ReplacementDictionary<int>> _projectileIndicesReplacements = new RunSpecific<ReplacementDictionary<int>>((out ReplacementDictionary<int> result) =>
        {
            if (NetworkServer.active && ConfigManager.ProjectileRandomizer.Enabled)
            {
                result = ReplacementDictionary<int>.CreateFrom(_projectileIndicesToRandomize.Get);

#if DEBUG
                Log.Debug($"Sending {nameof(SyncProjectileReplacements)} to clients");
#endif

                new SyncProjectileReplacements(result).Send(NetworkDestination.Clients);

                return true;
            }

            result = default;
            return false;
        });

        static bool shouldBeActive => (NetworkServer.active && ConfigManager.ProjectileRandomizer.Enabled) || _hasReceivedProjectileReplacementsFromServer;

        public static void OnProjectileReplacementsReceivedFromServer(ReplacementDictionary<int> replacements)
        {
            _projectileIndicesReplacements.Value = replacements;
            _hasReceivedProjectileReplacementsFromServer.Value = true;
        }

        void OnDestroy()
        {
            _projectileIndicesReplacements.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool getProjectileReplacement(int original, out int replacement)
        {
#if DEBUG
            switch ((BossRandomizerController.DebugMode)ConfigManager.ProjectileRandomizer.DebugMode)
            {
                case BossRandomizerController.DebugMode.Manual:
                    replacement = _forcedProjectileIndex;
                    return true;
                case BossRandomizerController.DebugMode.Forced:
                    replacement = int.Parse(ConfigManager.ProjectileRandomizer.ForcedProjectileIndex);
                    return true;
            }
#endif

            return _projectileIndicesReplacements.Value.TryGetReplacement(original, out replacement);
        }

        public static void TryOverrideProjectilePrefab(ref GameObject prefab)
        {
            if (shouldBeActive && _projectileIndicesReplacements.HasValue)
            {
                int originalIndex = ProjectileCatalog.GetProjectileIndex(prefab);
                if (getProjectileReplacement(originalIndex, out int replacementIndex))
                {
                    GameObject replacementPrefab = ProjectileCatalog.GetProjectilePrefab(replacementIndex);
                    if (replacementPrefab)
                    {
#if DEBUG
                        Log.Debug($"Projectile randomizer: Replaced projectile: {prefab.name} ({originalIndex}) -> {replacementPrefab.name} ({replacementIndex})");
#endif

                        prefab = replacementPrefab;
                    }
                }
            }
        }

        public static bool TryGetOriginalProjectileIndex(int replacementIndex, out int originalIndex)
        {
            if (shouldBeActive && _projectileIndicesReplacements.HasValue && _projectileIndicesReplacements.Value.TryGetOriginal(replacementIndex, out originalIndex))
            {
                return true;
            }

            originalIndex = -1;
            return false;
        }

        public static bool TryGetOriginalProjectilePrefab(GameObject replacementPrefab, out GameObject originalPrefab)
        {
            if (TryGetOriginalProjectileIndex(ProjectileCatalog.GetProjectileIndex(replacementPrefab), out int originalIndex))
            {
                originalPrefab = ProjectileCatalog.GetProjectilePrefab(originalIndex);
                return (bool)originalPrefab;
            }

            originalPrefab = null;
            return false;
        }

#if DEBUG
        static int _forcedProjectileIndex = 0;

        void Update()
        {
            if (ConfigManager.ProjectileRandomizer.Enabled && ConfigManager.ProjectileRandomizer.DebugMode == BossRandomizerController.DebugMode.Manual)
            {
                bool changedProjectileIndex = false;
                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    if (++_forcedProjectileIndex >= ProjectileCatalog.projectilePrefabCount)
                        _forcedProjectileIndex = 0;

                    changedProjectileIndex = true;
                }
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    if (--_forcedProjectileIndex < 0)
                        _forcedProjectileIndex = ProjectileCatalog.projectilePrefabCount - 1;

                    changedProjectileIndex = true;
                }

                if (changedProjectileIndex)
                {
                    Log.Debug($"Current projectile override: {ProjectileCatalog.GetProjectilePrefab(_forcedProjectileIndex).name} ({_forcedProjectileIndex})");
                }
            }
        }
#endif
    }
}
