﻿using RoR2;
using RoR2.Projectile;
using RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches;
using RoR2Randomizer.RandomizerControllers.Projectile;
using RoR2Randomizer.RandomizerControllers.Projectile.BulletAttackHandling;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoR2Randomizer.Patches.ProjectileRandomizer.BulletAttacks
{
    [PatchClass]
    static class BulletAttack_FireHook
    {
        static void Apply()
        {
            On.RoR2.BulletAttack.FireSingle += BulletAttack_FireSingle;
        }

        static void Cleanup()
        {
            On.RoR2.BulletAttack.FireSingle -= BulletAttack_FireSingle;
        }

        static void BulletAttack_FireSingle(On.RoR2.BulletAttack.orig_FireSingle orig, BulletAttack self, Vector3 normal, int muzzleIndex)
        {
            const string LOG_PREFIX = $"{nameof(BulletAttack_FireHook)}.{nameof(BulletAttack_FireSingle)} ";

            if (ProjectileRandomizerController.TryReplaceFire(self, normal))
            {
#if DEBUG
                Log.Debug(LOG_PREFIX + $"direction: {normal}");
#endif

                return;
            }

            orig(self, normal, muzzleIndex);
        }
    }
}