﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class ProjectileSpawnMaster_SpawnHook
    {
        static CharacterSpawnCard _cscMinorConstructOnKill;

        [SystemInitializer]
        static void Init()
        {
            AsyncOperationHandle<CharacterSpawnCard> cscMinorConstructOnKillRequest = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/MajorAndMinorConstruct/cscMinorConstructOnKill.asset");
            cscMinorConstructOnKillRequest.Completed += static handle =>
            {
                _cscMinorConstructOnKill = handle.Result;

#if DEBUG
                Log.Debug($"loaded {_cscMinorConstructOnKill}");
#endif
            };
        }

        static void Apply()
        {
            IL.RoR2.Projectile.ProjectileSpawnMaster.SpawnMaster += ProjectileSpawnMaster_SpawnMaster;
        }

        static void Cleanup()
        {
            IL.RoR2.Projectile.ProjectileSpawnMaster.SpawnMaster += ProjectileSpawnMaster_SpawnMaster;
        }

        static void ProjectileSpawnMaster_SpawnMaster(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject))))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);

                c.EmitDelegate(static (DirectorSpawnRequest spawnRequest, ProjectileSpawnMaster instance) =>
                {
                    if (!instance.spawnCard)
                    {
#if DEBUG
                        Log.Debug("spawncard is null");
#endif
                    }
                    else if (instance.spawnCard == _cscMinorConstructOnKill)
                    {
                        if (!ConfigManager.ExplicitSpawnRandomizer.RandomizeDefenseNucleusAlphaConstruct)
                        {
                            return;
                        }
                    }
                    else
                    {
                        Log.Warning($"unhandled spawncard {instance.spawnCard}");
                    }

                    ExplicitSpawnRandomizerController.TryReplaceDirectorSpawnRequest(spawnRequest);
                });
            }
            else
            {
                Log.Warning("failed to find any patch locations");
            }
        }
    }
}
